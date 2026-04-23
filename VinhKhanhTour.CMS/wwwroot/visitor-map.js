/* ══════════════════════════════════════════════════════════════
   VinhKhanhTour Web App — JS (V10.3: Audio per-lang, Virtual GPS, QR Fix)
   ══════════════════════════════════════════════════════════════ */

const API = 'https://vinhkhanh-api.onrender.com';
// Cổng Vào Phố Vĩnh Khánh (poi_01) — luôn bắt đầu tại đây
const START_LAT = 10.7595, START_LNG = 106.7048;

const LOCALES = {
    vi: { map: "Bản đồ", list: "Danh sách", settings: "Cài đặt", start: "Bắt đầu", stop: "Dừng", tracking_on: "📍 Đang định vị...", tracking_off: "⏸ Tạm dừng" },
    en: { map: "Map", list: "List", settings: "Settings", start: "Start", stop: "Stop", tracking_on: "📍 Tracking...", tracking_off: "⏸ Paused" }
};

let config = { speed: 1.0, repeat: 1, cooldown: 3, radius: 25, lang: localStorage.getItem('vk_lang') || 'vi' };
let state = {
    deviceId: getDeviceId(), isTracking: false, userPos: [START_LAT, START_LNG],
    allPoi: [], currentPoi: null, entryTime: null, selectedPoi: null
};
let map, userMarker, userCircle, poiMarkers = {}, poiCircles = {}, audioPlayer = new Audio();

// ── Device ID (KHÔNG BAO GIỜ dùng "Web Browser") ──────────
function getDeviceId() {
    let id = localStorage.getItem('vk_device_id');
    if (!id || id === 'undefined' || id === 'Web Browser') {
        id = 'Web_' + Math.random().toString(36).substr(2, 9);
        localStorage.setItem('vk_device_id', id);
    }
    return id;
}

function unlockAudio() {
    audioPlayer.play().then(() => { audioPlayer.pause(); audioPlayer.currentTime = 0; console.log("🔊 Audio Unlocked"); }).catch(e => console.warn("Audio unlock blocked", e));
}

// ── KHỞI ĐỘNG ──────────────────────────────────────────────
window.onload = async () => {
    initMap();
    initUI();
    state.allPoi = getOfflinePois();
    renderMarkers();
    await loadData();
    handleDeepLink();
    // Chỉ log 1 lần duy nhất khi mở trang (tiết kiệm writes)
    if (!sessionStorage.getItem('vk_visited')) {
        logAction('log_visit', null, 'Web Visitor Online');
        sessionStorage.setItem('vk_visited', '1');
    }
    // GPS trace mỗi 60 giây thay vì 3 giây (giảm 20x writes)
    setInterval(() => { if (state.isTracking) sendTrace(); }, 60000);
};

// ── SYNC FIREBASE CMS ──────────────────────────────────────
async function logAction(type, poi, note = '', duration = 0) {
    const devId = getDeviceId();
    // CHỈ gửi 1 endpoint (history) thay vì 2 — tiết kiệm 50% writes
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
    }).catch(() => {});
}

async function logTourScan(tourId) {
    if (!tourId) return;
    const devId = getDeviceId();
    fetch(`${API}/api/tours/${tourId}/scan?deviceId=${devId}&lang=${config.lang}`, { method: 'POST', mode: 'cors' }).catch(() => {});
    logAction('scan_qr', null, `Tour Scan: ${tourId}`);
}

async function sendTrace() {
    const devId = getDeviceId();
    const payload = { DeviceId: devId, Lat: state.userPos[0], Lng: state.userPos[1], Timestamp: new Date().toISOString() };
    fetch(`${API}/api/trace`, { method: 'POST', mode: 'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }).catch(() => {});
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
    // Mô tả: ưu tiên ngôn ngữ hiện tại, fallback vi -> en -> bất kỳ
    poi.Description = poi.Content[config.lang] || poi.Content['vi'] || poi.Content['en'] || Object.values(poi.Content)[0] || "";
    // Audio: CHỈ lấy đúng ngôn ngữ hiện tại — không có thì để trống (sẽ phát TTS)
    poi.AudioUrl = (poi.AudioUrls || {})[config.lang] || "";
    return poi;
}

// Re-normalize khi đổi ngôn ngữ — cập nhật AudioUrl + Description cho tất cả POI
function reloadPoiLanguage() {
    state.allPoi = state.allPoi.map(poi => {
        poi.Description = poi.Content[config.lang] || poi.Content['vi'] || poi.Content['en'] || Object.values(poi.Content)[0] || "";
        poi.AudioUrl = (poi.AudioUrls || {})[config.lang] || "";
        return poi;
    });
    renderPoiList();
}

// ── PROXIMITY CHECK ────────────────────────────────────────
function checkProximity() {
    const inRange = [];
    state.allPoi.forEach(poi => {
        if (!poi) return;
        const dist = getDistance(state.userPos[0], state.userPos[1], poi.Latitude, poi.Longitude);
        if (dist <= poi.RadiusMeters) inRange.push({ poi, dist });
    });

    // Rời khỏi POI
    if (state.currentPoi && !inRange.find(r => r.poi.Id === state.currentPoi.Id)) {
        const dur = Math.round((Date.now() - state.entryTime) / 1000);
        logAction('play_audio_end', state.currentPoi, '', dur);
        stopAllAudio();
        hideBottomSheet();
        state.currentPoi = null;
    }

    if (inRange.length === 0) return;

    const nearest = inRange.sort((a, b) => a.dist - b.dist)[0].poi;
    if (!state.currentPoi || state.currentPoi.Id !== nearest.Id) {
        state.currentPoi = nearest;
        state.entryTime = Date.now();
        showBottomSheet(nearest);
        playMedia(nearest);
        logAction('play_audio', nearest);
    }
}

// ── AUDIO ENGINE ───────────────────────────────────────────
function playMedia(poi) {
    stopAllAudio();
    console.log(`🎵 POI: ${poi.Name} | Lang: ${config.lang} | AudioUrl: ${poi.AudioUrl || 'NONE → TTS'}`);
    if (poi.AudioUrl && poi.AudioUrl.startsWith('http')) {
        audioPlayer.src = poi.AudioUrl;
        audioPlayer.playbackRate = config.speed;
        audioPlayer.play().then(() => {
            console.log('✅ Đang phát Audio file');
        }).catch(e => {
            console.warn('⚠️ Audio bị chặn, chuyển TTS:', e.message);
            playTts(poi.Description);
        });
    } else {
        console.log('📢 Không có audio file cho ngôn ngữ ' + config.lang + ' → Phát TTS');
        playTts(poi.Description);
    }
}

function stopAllAudio() {
    audioPlayer.pause();
    audioPlayer.currentTime = 0;
    audioPlayer.src = '';
    window.speechSynthesis.cancel();
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

let proxInterval = null; // KHÔNG dùng interval riêng nữa — gọi checkProximity trực tiếp trong updateUserPos

function toggleTracking() {
    state.isTracking = !state.isTracking;
    if (state.isTracking) unlockAudio();
    updateTexts();
}

function setSettings(key, val) {
    config[key] = val;
    if (key === 'lang') {
        localStorage.setItem('vk_lang', val);
        // Dừng audio hiện tại và re-normalize tất cả POI
        stopAllAudio();
        reloadPoiLanguage();
        // Nếu đang trong POI, phát lại với ngôn ngữ mới
        if (state.currentPoi) {
            playMedia(state.currentPoi);
        }
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
        poiMarkers[poi.Id] = L.marker([poi.Latitude, poi.Longitude], { icon }).addTo(map).on('click', () => {
            showBottomSheet(poi);
            playMedia(poi);
            logAction('play_audio', poi);
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
        // Chuyển sang tab bản đồ
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

// ── DEEP LINK (QR Scan) ───────────────────────────────────
function handleDeepLink() {
    const params = new URLSearchParams(location.search);
    const poiId = params.get('poiId');
    const tourId = params.get('tourId');
    if (poiId) {
        const poi = state.allPoi.find(p => p && p.Id === poiId);
        if (poi) {
            map.setView([poi.Latitude, poi.Longitude], 18);
            showBottomSheet(poi);
            playMedia(poi);
            logAction('scan_qr', poi);
            // Gọi API tăng counter QR
            const devId = getDeviceId();
            fetch(`${API}/api/tours/${poiId}/scan?deviceId=${devId}&lang=${config.lang}`, { method: 'POST', mode: 'cors' }).catch(() => {});
        }
    }
    if (tourId) { logTourScan(tourId); }
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
