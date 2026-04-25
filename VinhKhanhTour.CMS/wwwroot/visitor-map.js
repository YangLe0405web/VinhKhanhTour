/* ══════════════════════════════════════════════════════════════
   VinhKhanhTour Web App — JS (V11.10: Fix QR Scan + Persistent Cooldown)
   ══════════════════════════════════════════════════════════════ */

const API = 'https://vinhkhanh-api.onrender.com';
const START_LAT = 10.7595, START_LNG = 106.7048;

const LOCALES = {
    vi: { map: "Bản đồ", list: "Danh sách", settings: "Cài đặt", start: "Bắt đầu", stop: "Dừng", tracking_on: "📍 Đang định vị...", tracking_off: "⏸ Tạm dừng" },
    en: { map: "Map", list: "List", settings: "Settings", start: "Start", stop: "Stop", tracking_on: "📍 Tracking...", tracking_off: "⏸ Paused" }
};

let config = { speed: 1.0, repeat: 1, cooldown: 3, radius: 25, lang: localStorage.getItem('vk_lang') || 'vi' };
let state = {
    deviceId: getDeviceId(), isTracking: false, userPos: [START_LAT, START_LNG],
    allPoi: [], currentPoi: null, entryTime: null, selectedPoi: null,
    audioStatus: 'Sẵn sàng',
    poiLastPlayed: loadCooldownState()
};
let map, userMarker, userCircle, poiMarkers = {}, poiCircles = {}, audioPlayer = new Audio();

// ── Device ID ──────────────────────────────────────────────
function getDeviceId() {
    let id = localStorage.getItem('vk_device_id');
    if (!id || id === 'undefined' || id === 'Web Browser') {
        id = 'Web_' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('vk_device_id', id);
    }
    return id;
}

function unlockAudio() {
    audioPlayer.play().then(() => { audioPlayer.volume = 1.0; audioPlayer.pause(); audioPlayer.currentTime = 0; console.log("🔊 Audio Unlocked"); }).catch(e => console.warn("Audio unlock blocked", e));
}

// ── KHỞI ĐỘNG ──────────────────────────────────────────────
window.onload = async () => {
    initMap();
    initUI();
    state.allPoi = getOfflinePois();
    renderMarkers();
    await loadData();
    handleDeepLink();
    if (!sessionStorage.getItem('vk_visited')) {
        logAction('log_visit', null, 'Web Visitor Online');
        sessionStorage.setItem('vk_visited', '1');
    }
    setInterval(() => sendTrace(), 30000);

    // Mở khóa âm thanh khi chạm bất kỳ đâu (Fix cho điện thoại)
    document.addEventListener('click', () => {
        unlockAudio();
    }, { once: true });
};

// ── API LOGGING ────────────────────────────────────────────
async function logAction(type, poi, note = '', duration = 0) {
    const devId = getDeviceId();
    const payload = {
        Action: type, PoiId: poi ? poi.Id : '', PoiName: poi ? poi.Name : note,
        Device: devId, Language: config.lang, Duration: parseInt(duration),
        Lat: parseFloat(state.userPos[0]), Lng: parseFloat(state.userPos[1]),
        Timestamp: new Date().toISOString()
    };
    fetch(`${API}/api/history`, {
        method: 'POST', mode: 'cors',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    }).catch(() => { });
}

async function logTourScan(tourId) {
    if (!tourId) return;
    const devId = getDeviceId();
    fetch(`${API}/api/tours/${tourId}/scan?deviceId=${devId}&lang=${config.lang}`, { method: 'POST', mode: 'cors' }).catch(() => { });
    logAction('scan_qr', null, `Tour Scan: ${tourId}`);
}

async function sendTrace() {
    const devId = getDeviceId();
    const payload = { DeviceId: devId, Lat: state.userPos[0], Lng: state.userPos[1], Timestamp: new Date().toISOString() };
    fetch(`${API}/api/trace`, { method: 'POST', mode: 'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }).catch(() => { });
}

// ── LOAD POI DATA ──────────────────────────────────────────
async function loadData() {
    try {
        const res = await fetch(`${API}/api/pois`, { mode: 'cors' });
        if (res.ok) {
            const data = await res.json();
            if (data && data.length > 0) {
                state.allPoi = data.map(p => normalizePoi(p));
                renderMarkers();
                renderPoiList();
                console.log(`✅ Loaded ${state.allPoi.length} POIs from API`);
            }
        }
    } catch (e) { console.warn("API fail, dùng offline POIs", e); }
}

function normalizePoi(p) {
    const poi = {
        Id: p.Id || p.id,
        Name: p.Name || p.name,
        Latitude: parseFloat(p.Latitude || p.latitude),
        Longitude: parseFloat(p.Longitude || p.longitude),
        RadiusMeters: parseInt(p.RadiusMeters || p.radiusMeters || p.Radius || 30),
        Category: p.Category || p.category,
        Content: p.Content || p.content || {},
        AudioUrls: p.AudioUrls || p.audioUrls || {}
    };
    poi.Description = poi.Content[config.lang] || poi.Content['vi'] || poi.Content['en'] || Object.values(poi.Content)[0] || "";
    poi.AudioUrl = (poi.AudioUrls || {})[config.lang] || "";
    return poi;
}

function reloadPoiLanguage() {
    state.allPoi = state.allPoi.map(poi => {
        poi.Description = poi.Content[config.lang] || poi.Content['vi'] || poi.Content['en'] || Object.values(poi.Content)[0] || "";
        poi.AudioUrl = (poi.AudioUrls || {})[config.lang] || "";
        return poi;
    });
    renderPoiList();
}

// ── COOLDOWN (PERSIST QUA LOCALSTORAGE) ─────────────────────
function loadCooldownState() {
    try {
        const saved = localStorage.getItem('vk_cooldown');
        return saved ? JSON.parse(saved) : {};
    } catch { return {}; }
}

function saveCooldownState() {
    try { localStorage.setItem('vk_cooldown', JSON.stringify(state.poiLastPlayed)); } catch {}
}

function canPlay(poiId) {
    const lastPlayed = state.poiLastPlayed[poiId] || 0;
    const cooldownMs = config.cooldown * 1000; // GIÂY (UI: 3s, 10s, 30s, 60s)
    const elapsed = Date.now() - lastPlayed;
    if (elapsed >= cooldownMs) return true;
    const waitSec = Math.ceil((cooldownMs - elapsed) / 1000);
    console.log(`⏳ Cooldown: "${poiId}" còn ${waitSec}s`);
    return false;
}

// ── PROXIMITY CHECK ────────────────────────────────────────
function checkProximity() {
    const inRange = [];
    state.allPoi.forEach(poi => {
        if (!poi) return;
        const dist = getDistance(state.userPos[0], state.userPos[1], poi.Latitude, poi.Longitude);
        if (dist <= poi.RadiusMeters) inRange.push({ poi, dist });
    });

    if (state.currentPoi && !inRange.find(r => r.poi.Id === state.currentPoi.Id)) {
        stopAllAudio();
        hideBottomSheet();
        state.currentPoi = null;
    }

    if (inRange.length === 0) return;

    const nearest = inRange.sort((a, b) => a.dist - b.dist)[0].poi;
    if (!state.currentPoi || state.currentPoi.Id !== nearest.Id) {
        state.currentPoi = nearest;
        state.entryTime = Date.now();
        highlightMarker(nearest.Id); // Nổi bật POI gần nhất
        showBottomSheet(nearest);
        if (canPlay(nearest.Id)) {
            playMedia(nearest);
            logAction('play_audio', nearest);
        }
    }
}

// ── AUDIO ENGINE ───────────────────────────────────────────
let _repeatCount = 0;
let _playTimeout = null;

function playMedia(poi, immediate = false) {
    stopAllAudio();
    _repeatCount = 0;
    highlightMarker(poi.Id);
    sendTrace();

    const playNow = () => {
        state.poiLastPlayed[poi.Id] = Date.now();
        saveCooldownState();
        const audioUrls = poi.AudioUrls || {};
        const audioUrl = audioUrls[config.lang] || '';
        
        updateStatus(`🔊 Đang phát: ${poi.Name}`);

        if (audioUrl && audioUrl.startsWith('http')) {
            _playAudioFile(audioUrl, poi);
        } else {
            const desc = poi.Description || poi.Name;
            updateStatus(`🗣️ Đang đọc (TTS): ${poi.Name}`);
            _playTtsWithRepeat(desc);
        }
    };

    if (immediate) {
        playNow();
    } else {
        const delayMs = config.cooldown * 1000;
        console.log(`⏳ Chờ ${config.cooldown}s...`);
        _playTimeout = setTimeout(playNow, delayMs);
    }
}

function _playAudioFile(url, poi) {
    updateStatus(`🔊 Đang nạp nhạc: ${poi.Name}...`);
    audioPlayer.src = url;
    audioPlayer.playbackRate = config.speed;
    audioPlayer.onended = () => {
        _repeatCount++;
        if (_repeatCount < config.repeat) {
            updateStatus(`🔁 Lặp lại lần ${_repeatCount + 1}...`);
            _playTimeout = setTimeout(() => {
                audioPlayer.currentTime = 0;
                audioPlayer.play().catch(() => {});
            }, config.cooldown * 1000);
        } else {
            updateStatus(`✅ Đã phát xong: ${poi.Name}`);
            audioPlayer.onended = null;
        }
    };
    audioPlayer.play().then(() => { audioPlayer.volume = 1.0;
        updateStatus(`▶️ Đang phát: ${poi.Name}`);
    }).catch(e => {
        console.warn('⚠️ Audio blocked → TTS');
        updateStatus(`⚠️ Bị chặn → Đang dùng giọng nói...`);
        _playTtsWithRepeat(poi.Description);
    });
}

function _playTtsWithRepeat(text) {
    if (!text) {
        updateStatus("❌ Không có nội dung để đọc");
        return;
    }
    window.speechSynthesis.cancel(); // Dừng các giọng cũ
    let played = 0;
    function speakOnce() {
        if (played >= config.repeat) {
            updateStatus("✅ Đọc xong");
            return;
        }
        const ut = new SpeechSynthesisUtterance(text);
        ut.lang = config.lang === 'vi' ? 'vi-VN' : 'en-US';
        ut.rate = config.speed;
        ut.onstart = () => updateStatus("🗣️ Đang đọc thuyết minh...");
        ut.onend = () => { 
            played++; 
            if (played < config.repeat) {
                _playTimeout = setTimeout(speakOnce, config.cooldown * 1000);
            }
        };
        ut.onerror = (e) => updateStatus(`❌ Lỗi đọc: ${e.error}`);
        window.speechSynthesis.speak(ut);
    }
    speakOnce();
}

function updateStatus(msg) {
    const el = document.querySelector('.status');
    if (el) el.innerText = msg;
    console.log(`[STATUS] ${msg}`);
}

function highlightMarker(poiId) {
    // 1. Highlight Pin Marker
    Object.keys(poiMarkers).forEach(id => {
        const marker = poiMarkers[id];
        if (marker && marker.getElement()) {
            marker.getElement().classList.remove('active-poi-marker');
            if (id === poiId) marker.getElement().classList.add('active-poi-marker');
        }
    });
    // 2. Highlight Vòng tròn (Circle)
    Object.keys(poiCircles).forEach(id => {
        const circle = poiCircles[id];
        if (circle) {
            if (id === poiId) {
                circle.setStyle({ color: '#2196F3', fillColor: '#2196F3', fillOpacity: 0.3, weight: 3 });
                circle.bringToFront();
            } else {
                circle.setStyle({ color: '#FF5252', fillColor: '#FF5252', fillOpacity: 0.1, weight: 1 });
            }
        }
    });
}

function stopAllAudio() {
    highlightMarker(null); // Bỏ highlight khi dừng
    if (_playTimeout) {
        clearTimeout(_playTimeout);
        _playTimeout = null;
    }
    audioPlayer.pause();
    audioPlayer.currentTime = 0;
    audioPlayer.src = '';
    audioPlayer.onended = null;
    window.speechSynthesis.cancel();
}

// Nút "Nghe thuyết minh" trong trang chi tiết — có cooldown
function playPoiAudio(poi) {
    if (!poi) return;
    unlockAudio();
    playMedia(poi, true); // Manual click -> Phát ngay
    logAction('play_audio', poi);
}

function playTts(text) {
    if (!text) return;
    const ut = new SpeechSynthesisUtterance(text);
    ut.lang = config.lang === 'vi' ? 'vi-VN' : config.lang === 'en' ? 'en-US' : config.lang === 'zh' ? 'zh-CN' : config.lang === 'ja' ? 'ja-JP' : config.lang === 'ko' ? 'ko-KR' : 'vi-VN';
    ut.rate = config.speed;
    window.speechSynthesis.speak(ut);
}

// ── POSITION & MAP ─────────────────────────────────────────
function updateUserPos(lat, lng) {
    state.userPos = [parseFloat(lat), parseFloat(lng)];
    userMarker.setLatLng(state.userPos);
    userCircle.setLatLng(state.userPos);
    if (state.isTracking) checkProximity();
}

function toggleTracking() {
    state.isTracking = !state.isTracking;
    if (state.isTracking) unlockAudio();
    updateTexts();
}

function setSettings(key, val) {
    config[key] = val;
    if (key === 'lang') {
        localStorage.setItem('vk_lang', val);
        stopAllAudio();
        reloadPoiLanguage();
        if (state.currentPoi) playMedia(state.currentPoi);
        updateTexts();
    }
    document.querySelectorAll(`.seg-btn[data-key="${key}"]`).forEach(b => b.classList.toggle('active', b.dataset.val == val));
}

// ── UI ─────────────────────────────────────────────────────
function initUI() {
    document.querySelectorAll('.nav-item').forEach(btn => {
        btn.onclick = () => {
            const page = btn.dataset.page;
            document.querySelectorAll('.view').forEach(v => v.style.display = 'none');
            if (page !== 'map') document.getElementById(`${page}-view`).style.display = 'block';
            document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
            btn.classList.add('active');
            if (page === 'map') hideBottomSheet();
        };
    });
    updateTexts();
}

function updateTexts() {
    const t = LOCALES[config.lang] || LOCALES.vi;
    document.querySelector('.status').innerText = state.isTracking ? t.tracking_on : t.tracking_off;
    document.getElementById('btn-lang').innerText = `${config.lang.toUpperCase()}`;
    document.getElementById('btn-start').innerText = state.isTracking ? t.stop : t.start;
}

function initMap() {
    map = L.map('map', { zoomControl: false, attributionControl: false }).setView([START_LAT, START_LNG], 17);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
    userMarker = L.marker([START_LAT, START_LNG], {
        icon: L.divIcon({ className: 'custom-pin', html: '<div class="pin-body" style="background:#1A237E;z-index:9999"><div class="pin-inner">👤</div></div>', iconSize: [36, 36], iconAnchor: [18, 36] })
    }).addTo(map);
    userCircle = L.circle([START_LAT, START_LNG], { radius: config.radius, color: '#1A237E', fillOpacity: 0.1 }).addTo(map);
}

function renderMarkers() {
    Object.values(poiMarkers).forEach(m => map.removeLayer(m));
    Object.values(poiCircles).forEach(c => map.removeLayer(c));
    poiMarkers = {};
    poiCircles = {};
    state.allPoi.forEach(poi => {
        if (!poi || isNaN(poi.Latitude) || isNaN(poi.Longitude)) return;
        const icon = L.divIcon({
            className: 'poi-pin',
            html: `<div class="pin-body"><div class="pin-inner">${poi.Category === 'food' ? '🍲' : poi.Category === 'drink' ? '🧋' : '🏛️'}</div></div>`,
            iconSize: [30, 30], iconAnchor: [15, 30]
        });
        // Click marker: CHỈ XEM, không tự đọc
        poiMarkers[poi.Id] = L.marker([poi.Latitude, poi.Longitude], { icon }).addTo(map).on('click', () => {
            sendTrace(); 
            highlightMarker(poi.Id); // Highlight để biết đang xem cái nào
            showBottomSheet(poi);
        });
        poiCircles[poi.Id] = L.circle([poi.Latitude, poi.Longitude], {
            radius: poi.RadiusMeters, color: '#FF5252', weight: 1, fillOpacity: 0.1
        }).addTo(map);
    });
}

function renderPoiList() {
    const listEl = document.getElementById('list-view');
    if (!listEl) return;
    const items = state.allPoi.map(poi => `
        <div class="poi-card" onclick="flyToPoi('${poi.Id}')">
            <div class="poi-card-icon">${poi.Category === 'food' ? '🍲' : poi.Category === 'drink' ? '🧋' : '🏛️'}</div>
            <div class="poi-card-info">
                <div class="poi-card-name">${poi.Name}</div>
                <div class="poi-card-desc">${(poi.Description || '').substring(0, 60)}...</div>
            </div>
            <div class="poi-card-audio">${poi.AudioUrl ? '🔊' : '📢'}</div>
        </div>
    `).join('');
    listEl.innerHTML = `<div class="poi-list">${items}</div>`;
}

function flyToPoi(poiId) {
    const poi = state.allPoi.find(p => p && p.Id === poiId);
    if (poi) {
        map.setView([poi.Latitude, poi.Longitude], 18);
        showBottomSheet(poi);
        document.querySelectorAll('.view').forEach(v => v.style.display = 'none');
        document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
        document.querySelector('[data-page="map"]').classList.add('active');
    }
}

function showBottomSheet(poi) {
    state.selectedPoi = poi;
    document.querySelector('.sheet-title').innerText = poi.Name;
    document.querySelector('.bottom-sheet').style.display = 'block';
}
function hideBottomSheet() { document.querySelector('.bottom-sheet').style.display = 'none'; }

// ── DEEP LINK (QR Scan) ─────────────────────────────────────
function handleDeepLink() {
    const params = new URLSearchParams(location.search);
    const poiId = params.get('poiId');
    const tourId = params.get('tourId') || params.get('t'); 
    
    if (tourId) {
        if (sessionStorage.getItem('last_scan') === tourId) return;
        sessionStorage.setItem('last_scan', tourId);
        console.log('?? TOUR SCAN: tourId=' + tourId);
        logTourScan(tourId);
    }

    if (poiId) {
        if (sessionStorage.getItem('last_scan') === poiId) return;
        sessionStorage.setItem('last_scan', poiId);
        
        const devId = getDeviceId();
        const poi = state.allPoi.find(p => p && p.Id === poiId);
        const poiName = poi ? poi.Name : poiId;
        console.log(`📱 POI SCAN: poiId=${poiId}, name=${poiName}`);

        // Ghi log scan vào history (vẫn ghi nhận lượt quét)
        fetch(`${API}/api/history`, {
            method: 'POST', mode: 'cors',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                Action: 'scan_qr', PoiId: poiId, PoiName: poiName,
                Device: devId, Language: config.lang, Duration: 0,
                Lat: state.userPos[0], Lng: state.userPos[1],
                Timestamp: new Date().toISOString()
            })
        }).then(() => console.log('✅ scan_qr logged!')).catch(e => console.warn('❌ scan_qr fail:', e));

        // CHỈ phát audio nếu đạt cooldown
        if (poi) {
            map.setView([poi.Latitude, poi.Longitude], 18);
            showBottomSheet(poi);
            // Quét QR thì phát NGAY (immediate = true)
            playMedia(poi, true);
        } else {
            console.log('⏳ POI chưa load, retry sau 3s...');
            setTimeout(() => {
                const retryPoi = state.allPoi.find(p => p && p.Id === poiId);
                if (retryPoi) {
                    map.setView([retryPoi.Latitude, retryPoi.Longitude], 18);
                    showBottomSheet(retryPoi);
                    if (canPlay(retryPoi.Id)) {
                        playMedia(retryPoi);
                    }
                }
            }, 3000);
        }
    }
}

// ── GPS ẢO (D-pad) ────────────────────────────────────────
function move(dlat, dlng) {
    updateUserPos(state.userPos[0] + dlat, state.userPos[1] + dlng);
    map.panTo(state.userPos);
}

// ── HELPERS ────────────────────────────────────────────────
function getDistance(la1, lo1, la2, lo2) {
    const R = 6371e3;
    const dLat = (la2 - la1) * Math.PI / 180, dLon = (lo2 - lo1) * Math.PI / 180;
    const a = Math.sin(dLat / 2) ** 2 + Math.cos(la1 * Math.PI / 180) * Math.cos(la2 * Math.PI / 180) * Math.sin(dLon / 2) ** 2;
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function getOfflinePois() {
    return [{
        Id: "poi_01", Name: "Cổng Phố Vĩnh Khánh", Latitude: 10.7595, Longitude: 106.7048,
        RadiusMeters: 25, Category: 'landmark', Content: { vi: "Chào mừng đến Phố Vĩnh Khánh!" }, AudioUrls: {}
    }].map(normalizePoi);
}

function testSound() {
    updateStatus('?? �ang ki?m tra �m thanh...');
    unlockAudio();
    const testPoi = { Name: 'Ki?m tra', Description: '�m thanh ho?t �?ng t?t. Ch�c b?n c� m?t chuy?n tham quan vui v?!', AudioUrls: {} };
    playMedia(testPoi, true);
}



