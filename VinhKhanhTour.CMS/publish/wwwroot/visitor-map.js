/* ══════════════════════════════════════════════════════════════
   VinhKhanhTour Web App — JS (Bản V9.5: Final Sync & UI Fix)
   ══════════════════════════════════════════════════════════════ */

const API = 'https://vinhkhanh-api.onrender.com';
const CENTER_LAT = 10.75836, CENTER_LNG = 106.70512;

const LOCALES = {
    vi: { map: "Bản đồ", list: "Danh sách", settings: "Cài đặt", start: "Bắt đầu", stop: "Dừng", search: "Tìm kiếm...", language: "Ngôn ngữ thuyết minh", radius: "Bán kính quét (m)", cooldown: "Thời gian chờ (giây)", tracking_on: "📍 Đang định vị...", tracking_off: "⏸ Tạm dừng", arrived: "Bạn đã đến nơi!", stop_route: "Tắt chỉ đường" },
    en: { map: "Map", list: "List", settings: "Settings", start: "Start", stop: "Stop", search: "Search...", language: "Audio Language", radius: "Scan Radius (m)", cooldown: "Cooldown (sec)", tracking_on: "📍 Tracking...", tracking_off: "⏸ Paused", arrived: "You arrived!", stop_route: "Stop Route" }
};

let config = { speed: 1.0, repeat: 1, cooldown: 3, radius: 25, lang: localStorage.getItem('vk_lang') || 'vi' };
let state = { 
    deviceId: getDeviceId(), isTracking: false, userPos: [CENTER_LAT, CENTER_LNG], 
    allPoi: [], currentPoi: null, entryTime: null, visited: {}, 
    audioQueue: [], isPlaying: false, selectedPoi: null, destinationPoi: null 
};
let map, userMarker, userCircle, poiMarkers = {}, poiCircles = {}, routingControl, audioPlayer = new Audio();

window.onload = async () => {
    initMap();
    initUI();
    await loadData();
    handleDeepLink();
    startGps();
    logAction('log_visit', null, 'Web Visitor Online');
    setInterval(() => { if(state.isTracking) sendTrace(); }, 3000);
};

function getDeviceId() {
    let id = localStorage.getItem('vk_device_id');
    if (!id) { id = 'Web_' + Math.random().toString(36).substr(2, 9); localStorage.setItem('vk_device_id', id); }
    return id;
}

// ── SYNC FIREBASE CMS ────────────────────────────────────
async function logAction(type, poi, note = '', duration = 0) {
    const payload = {
        DeviceId: state.deviceId, EventType: type, PoiId: poi ? poi.Id : '',
        Language: config.lang, Lat: parseFloat(state.userPos[0]), Lng: parseFloat(state.userPos[1]),
        Duration: parseInt(duration), Timestamp: new Date().toISOString()
    };
    const opt = { method: 'POST', mode: 'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) };
    
    // Ghi nhận vào Analytics & History
    fetch(`${API}/api/analytics`, opt).catch(() => {});
    fetch(`${API}/api/history`, { 
        ...opt, 
        body: JSON.stringify({
            Action: type, PoiId: payload.PoiId, PoiName: poi ? poi.Name : note, 
            Device: 'Web Browser', Language: config.lang, Duration: parseInt(duration), Timestamp: payload.Timestamp
        }) 
    }).catch(() => {});
}

async function sendTrace() {
    const payload = { DeviceId: state.deviceId, Lat: state.userPos[0], Lng: state.userPos[1], Timestamp: new Date().toISOString() };
    fetch(`${API}/api/trace`, { method: 'POST', mode:'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }).catch(() => {});
}

async function loadData() {
    try {
        const res = await fetch(`${API}/api/pois`, { mode:'cors', signal: AbortSignal.timeout(5000) });
        const data = res.ok ? await res.json() : [];
        state.allPoi = (data && data.length > 0) ? data.map(normalizePoi) : getOfflinePois();
    } catch { state.allPoi = getOfflinePois(); }
    renderMarkers();
    updatePoiList();
}

function normalizePoi(p) {
    const poi = {
        Id: p.Id || p.id, Name: p.Name || p.name, Latitude: p.Latitude || p.latitude, Longitude: p.Longitude || p.longitude,
        RadiusMeters: p.RadiusMeters || p.radiusMeters || p.Radius || 30, Category: p.Category || p.category || 'food',
        Content: p.Content || p.content || {}, AudioUrls: p.AudioUrls || p.audioUrls || {}
    };
    poi.Description = poi.Content[config.lang] || poi.Content['vi'] || "Vinh Khanh Tour!";
    poi.TtsScript = poi.Description;
    poi.AudioUrl = poi.AudioUrls[config.lang] || poi.AudioUrls['vi'] || "";
    return poi;
}

function checkProximity() {
    const inRange = [];
    state.allPoi.forEach(poi => {
        const dist = getDistance(state.userPos[0], state.userPos[1], poi.Latitude, poi.Longitude);
        if (dist <= poi.RadiusMeters) inRange.push({ poi, dist });
        if (poiCircles[poi.Id]) poiCircles[poi.Id].setStyle({ color: '#E65100', weight: 1 });
    });

    if (state.currentPoi) {
        if (!inRange.find(r => r.poi.Id === state.currentPoi.Id)) {
            const dur = Math.round((Date.now() - state.entryTime)/1000);
            logAction('play_audio_end', state.currentPoi, '', dur);
            stopAllAudio(); hideBottomSheet(); state.currentPoi = null;
        }
    }

    if (inRange.length === 0) return;
    inRange.sort((a,b) => a.dist - b.dist);
    const nearest = inRange[0].poi;
    if (poiCircles[nearest.Id]) poiCircles[nearest.Id].setStyle({ color: '#2ECC71', weight: 3 });

    if (!state.currentPoi || state.currentPoi.Id !== nearest.Id) {
        state.currentPoi = nearest; state.entryTime = Date.now();
        showBottomSheet(nearest); playMedia(nearest); logAction('play_audio', nearest);
    }
}

function playMedia(poi) {
    stopAllAudio();
    if (poi.AudioUrl) { audioPlayer.src = poi.AudioUrl; audioPlayer.play().catch(() => playTts(poi.TtsScript)); }
    else playTts(poi.TtsScript);
}

function stopAllAudio() { audioPlayer.pause(); window.speechSynthesis.cancel(); }
function playTts(text) { 
    const ut = new SpeechSynthesisUtterance(text); 
    ut.lang = config.lang==='vi'?'vi-VN':'en-US'; 
    ut.rate = config.speed; 
    window.speechSynthesis.speak(ut); 
}

function updateUserPos(lat, lng) {
    state.userPos = [lat, lng];
    userMarker.setLatLng(state.userPos); userCircle.setLatLng(state.userPos);
    updateTexts();
}

// ỨNG DỤNG COOLDOWN DYNAMIC
let proxInterval = setInterval(() => { if(state.isTracking) checkProximity(); }, config.cooldown * 1000);

function setSettings(key, val) {
    config[key] = val;
    if(key==='lang') { localStorage.setItem('vk_lang', val); updateTexts(); updatePoiList(); }
    if(key==='cooldown') {
        clearInterval(proxInterval);
        proxInterval = setInterval(() => { if(state.isTracking) checkProximity(); }, val * 1000);
    }
    document.querySelectorAll(`.seg-btn[data-key="${key}"]`).forEach(b => b.classList.toggle('active', b.dataset.val == val));
}

function initUI() { 
    document.querySelectorAll('.nav-item').forEach(btn => { 
        btn.onclick = () => { 
            const page = btn.dataset.page; 
            document.querySelectorAll('.view').forEach(v => v.style.display = 'none'); 
            if(page !== 'map') document.getElementById(`${page}-view`).style.display = 'block'; 
            document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active')); 
            btn.classList.add('active'); if(page === 'map') hideBottomSheet(); 
        }; 
    }); 
    updateTexts(); 
}

function updateTexts() { 
    const t = LOCALES[config.lang] || LOCALES.vi; 
    document.querySelector('.status').innerText = state.isTracking ? t.tracking_on : t.tracking_off;
    document.getElementById('btn-lang').innerText = `${getFlag(config.lang)} ${getLangName(config.lang)}`;
    document.getElementById('btn-start').innerText = state.isTracking ? t.stop : t.start;
}

function getFlag(l){ return l==='vi'?'🇻🇳':l==='en'?'🇬🇧':l==='zh'?'🇨🇳':l==='ja'?'🇯🇵':'🇰🇷'; }
function getLangName(l){ return l==='vi'?'Việt':l==='en'?'English':l==='zh'?'中文':l==='ja'?'日本語':'한국어'; }
function initMap() { map = L.map('map', { zoomControl: false, attributionControl: false }).setView([CENTER_LAT, CENTER_LNG], 17); L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map); userMarker = L.marker([CENTER_LAT, CENTER_LNG], { icon: L.divIcon({ className:'custom-pin', html:'<div class="pin-body" style="background:#1A237E;z-index:9999"><div class="pin-inner">👤</div></div>', iconSize:[36,36], iconAnchor:[18,36] }) }).addTo(map); userCircle = L.circle([CENTER_LAT, CENTER_LNG], { radius:config.radius, color:'#1A237E', fillOpacity:0.1 }).addTo(map); }
function showBottomSheet(poi) { state.selectedPoi = poi; document.querySelector('.sheet-title').innerText = poi.Name; document.querySelector('.bottom-sheet').style.display = 'block'; }
function hideBottomSheet() { document.querySelector('.bottom-sheet').style.display = 'none'; }
function move(dl, dg) { updateUserPos(state.userPos[0]+dl, state.userPos[1]+dg); map.panTo(state.userPos); }
function toggleTracking() { state.isTracking = !state.isTracking; updateTexts(); }
function startGps() { if ("geolocation" in navigator) navigator.geolocation.watchPosition(pos => { if (state.isTracking) updateUserPos(pos.coords.latitude, pos.coords.longitude); }, null, { enableHighAccuracy: true }); }
function getDistance(la1, lo1, la2, lo2) { const R = 6371e3; const dLat = (la2-la1)*Math.PI/180, dLon = (lo2-lo1)*Math.PI/180; const a = Math.sin(dLat/2)**2 + Math.cos(la1*Math.PI/180)*Math.cos(la2*Math.PI/180)*Math.sin(dLon/2)**2; return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a)); }
function updatePoiList() { const list = document.getElementById('poi-list'); list.innerHTML = ''; state.allPoi.forEach(p => { const div = document.createElement('div'); div.className = 'poi-card'; div.innerHTML = `<div class="name">${p.Name}</div>`; div.onclick = () => { showBottomSheet(p); document.querySelector('.nav-item[data-page="map"]').click(); map.setView([p.Latitude, p.Longitude], 18); }; list.appendChild(div); }); }
function handleDeepLink() { const poiId = new URLSearchParams(location.search).get('poiId'); if (poiId) { const poi = state.allPoi.find(p => p.Id === poiId); if (poi) { map.setView([poi.Latitude, poi.Longitude], 18); showBottomSheet(poi); logAction('scan_qr', poi); } } }
function getOfflinePois() { return [{ Id:"poi_01", Name:"Cổng Phố Vĩnh Khánh", Latitude:10.7595, Longitude:106.7048, RadiusMeters:30, Content:{vi:"Chào mừng đến Phố Vĩnh Khánh!"} }].map(normalizePoi); }
