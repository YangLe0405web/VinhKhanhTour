/* ══════════════════════════════════════════════════════════════
   VinhKhanhTour Web App — JS (Bản V10.2: FIX QR & GHOST DEVICE)
   ══════════════════════════════════════════════════════════════ */

const API = 'https://vinhkhanh-api.onrender.com';
const CENTER_LAT = 10.75836, CENTER_LNG = 106.70512;

const LOCALES = {
    vi: { map: "Bản đồ", list: "Danh sách", settings: "Cài đặt", start: "Bắt đầu", stop: "Dừng", tracking_on: "📍 Đang định vị...", tracking_off: "⏸ Tạm dừng" },
    en: { map: "Map", list: "List", settings: "Settings", start: "Start", stop: "Stop", tracking_on: "📍 Tracking...", tracking_off: "⏸ Paused" }
};

let config = { speed: 1.0, repeat: 1, cooldown: 3, radius: 25, lang: localStorage.getItem('vk_lang') || 'vi' };
let state = { 
    deviceId: getDeviceId(), isTracking: false, userPos: [CENTER_LAT, CENTER_LNG], 
    allPoi: [], currentPoi: null, entryTime: null, selectedPoi: null 
};
let map, userMarker, userCircle, poiMarkers = {}, poiCircles = {}, audioPlayer = new Audio();

function getDeviceId() {
    let id = localStorage.getItem('vk_device_id');
    if (!id || id === 'undefined' || id === 'Web Browser') { 
        id = 'Web_' + Math.random().toString(36).substr(2, 9); 
        localStorage.setItem('vk_device_id', id); 
    }
    return id;
}

function unlockAudio() {
    audioPlayer.play().then(() => { audioPlayer.pause(); audioPlayer.currentTime = 0; console.log("🔊 Audio Unlocked"); }).catch(e => console.warn("Audio Context blocked", e));
}

window.onload = async () => {
    initMap();
    initUI();
    state.allPoi = getOfflinePois();
    renderMarkers();
    await loadData();
    handleDeepLink();
    startGps();
    logAction('log_visit', null, 'Web Visitor Online');
    setInterval(() => { if(state.isTracking) sendTrace(); }, 3000);
};

// ── SYNC FIREBASE CMS (FIX QR & GHOST DEVICE) ─────────────
async function logAction(type, poi, note = '', duration = 0) {
    const devId = getDeviceId(); // Đảm bảo lấy ID mới nhất
    const payload = {
        DeviceId: devId, EventType: type, PoiId: poi ? poi.Id : '',
        Language: config.lang, Lat: parseFloat(state.userPos[0]), Lng: parseFloat(state.userPos[1]),
        Duration: parseInt(duration), Timestamp: new Date().toISOString()
    };
    const opt = { method: 'POST', mode: 'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) };
    
    fetch(`${API}/api/analytics`, opt).catch(() => {});
    fetch(`${API}/api/history`, { 
        ...opt, 
        body: JSON.stringify({
            Action: type, PoiId: payload.PoiId, PoiName: poi ? poi.Name : note, 
            Device: devId, // KHÔNG DÙNG "Web Browser"
            Language: config.lang, Duration: parseInt(duration), Timestamp: payload.Timestamp
        }) 
    }).catch(() => {});
}

async function logTourScan(tourId) {
    if(!tourId) return;
    // Tăng số lượt quét cho Tour
    fetch(`${API}/api/tours/${tourId}/scan`, { method: 'POST', mode: 'cors' }).catch(() => {});
    // Ghi nhận vào báo cáo tổng quát
    logAction('scan_qr', null, `Tour Scan: ${tourId}`);
}

async function sendTrace() {
    const devId = getDeviceId();
    const payload = { DeviceId: devId, Lat: state.userPos[0], Lng: state.userPos[1], Timestamp: new Date().toISOString() };
    fetch(`${API}/api/trace`, { method: 'POST', mode:'cors', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) }).catch(() => {});
}

async function loadData() {
    try {
        const res = await fetch(`${API}/api/pois`, { mode:'cors' });
        if (res.ok) {
            const data = await res.json();
            if (data && data.length > 0) {
                state.allPoi = data.map(normalizePoi);
                renderMarkers();
            }
        }
    } catch (e) { console.warn("API fail", e); }
}

function normalizePoi(p) {
    const poi = {
        Id: p.Id || p.id, Name: p.Name || p.name, Latitude: parseFloat(p.Latitude || p.latitude), Longitude: parseFloat(p.Longitude || p.longitude),
        RadiusMeters: parseInt(p.RadiusMeters || p.radiusMeters || p.Radius || 30), Category: p.Category || p.category,
        Content: p.Content || p.content || {}, AudioUrls: p.AudioUrls || p.audioUrls || {}
    };
    poi.Description = poi.Content[config.lang] || poi.Content['vi'] || "";
    poi.AudioUrl = poi.AudioUrls[config.lang] || poi.AudioUrls['vi'] || "";
    return poi;
}

function checkProximity() {
    const inRange = [];
    state.allPoi.forEach(poi => {
        if (!poi) return;
        const dist = getDistance(state.userPos[0], state.userPos[1], poi.Latitude, poi.Longitude);
        if (dist <= poi.RadiusMeters) inRange.push({ poi, dist });
    });

    if (state.currentPoi && !inRange.find(r => r.poi.Id === state.currentPoi.Id)) {
        const dur = Math.round((Date.now() - state.entryTime)/1000);
        logAction('play_audio_end', state.currentPoi, '', dur);
        stopAllAudio(); hideBottomSheet(); state.currentPoi = null;
    }

    if (inRange.length === 0) return;
    const nearest = inRange.sort((a,b) => a.dist - b.dist)[0].poi;
    if (!state.currentPoi || state.currentPoi.Id !== nearest.Id) {
        state.currentPoi = nearest; state.entryTime = Date.now();
        showBottomSheet(nearest); playMedia(nearest); logAction('play_audio', nearest);
    }
}

function playMedia(poi) {
    stopAllAudio();
    if (poi.AudioUrl && poi.AudioUrl.startsWith('http')) {
        audioPlayer.src = poi.AudioUrl;
        audioPlayer.play().catch(() => playTts(poi.Description));
    } else {
        playTts(poi.Description);
    }
}

function stopAllAudio() { audioPlayer.pause(); window.speechSynthesis.cancel(); }
function playTts(text) { if(!text) return; const ut = new SpeechSynthesisUtterance(text); ut.lang = config.lang==='vi'?'vi-VN':'en-US'; ut.rate = config.speed; window.speechSynthesis.speak(ut); }
function updateUserPos(lat, lng) { state.userPos = [parseFloat(lat), parseFloat(lng)]; userMarker.setLatLng(state.userPos); userCircle.setLatLng(state.userPos); updateTexts(); }

let proxInterval = setInterval(() => { if(state.isTracking) checkProximity(); }, config.cooldown * 1000);

function toggleTracking() { 
    state.isTracking = !state.isTracking; 
    if(state.isTracking) unlockAudio(); 
    updateTexts(); 
}

function setSettings(key, val) {
    config[key] = val;
    if(key==='lang') { localStorage.setItem('vk_lang', val); updateTexts(); }
    if(key==='cooldown') { clearInterval(proxInterval); proxInterval = setInterval(() => { if(state.isTracking) checkProximity(); }, val * 1000); }
    document.querySelectorAll(`.seg-btn[data-key="${key}"]`).forEach(b => b.classList.toggle('active', b.dataset.val == val));
}

function initUI() { document.querySelectorAll('.nav-item').forEach(btn => { btn.onclick = () => { const page = btn.dataset.page; document.querySelectorAll('.view').forEach(v => v.style.display = 'none'); if(page !== 'map') document.getElementById(`${page}-view`).style.display = 'block'; document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active')); btn.classList.add('active'); if(page === 'map') hideBottomSheet(); }; }); updateTexts(); }
function updateTexts() { const t = LOCALES[config.lang] || LOCALES.vi; document.querySelector('.status').innerText = state.isTracking ? t.tracking_on : t.tracking_off; document.getElementById('btn-lang').innerText = `${config.lang.toUpperCase()}`; document.getElementById('btn-start').innerText = state.isTracking ? t.stop : t.start; }
function initMap() { map = L.map('map', { zoomControl: false, attributionControl: false }).setView([CENTER_LAT, CENTER_LNG], 17); L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map); userMarker = L.marker([CENTER_LAT, CENTER_LNG], { icon: L.divIcon({ className:'custom-pin', html:'<div class="pin-body" style="background:#1A237E;z-index:9999"><div class="pin-inner">👤</div></div>', iconSize:[36,36], iconAnchor:[18,36] }) }).addTo(map); userCircle = L.circle([CENTER_LAT, CENTER_LNG], { radius:config.radius, color:'#1A237E', fillOpacity:0.1 }).addTo(map); }
function renderMarkers() { Object.values(poiMarkers).forEach(m => map.removeLayer(m)); Object.values(poiCircles).forEach(c => map.removeLayer(c)); state.allPoi.forEach(poi => { if(!poi) return; const icon = L.divIcon({ className:'poi-pin', html:`<div class="pin-body"><div class="pin-inner">${poi.Category === 'food' ? '🍲' : '🏛️'}</div></div>`, iconSize:[30,30], iconAnchor:[15,30] }); poiMarkers[poi.Id] = L.marker([poi.Latitude, poi.Longitude], { icon }).addTo(map).on('click', () => showBottomSheet(poi)); poiCircles[poi.Id] = L.circle([poi.Latitude, poi.Longitude], { radius: poi.RadiusMeters, color: '#FF5252', weight: 1, fillOpacity: 0.1 }).addTo(map); }); }
function showBottomSheet(poi) { state.selectedPoi = poi; document.querySelector('.sheet-title').innerText = poi.Name; document.querySelector('.bottom-sheet').style.display = 'block'; }
function hideBottomSheet() { document.querySelector('.bottom-sheet').style.display = 'none'; }
function startGps() { if ("geolocation" in navigator) navigator.geolocation.watchPosition(pos => { if (state.isTracking) updateUserPos(pos.coords.latitude, pos.coords.longitude); }, null, { enableHighAccuracy: true }); }
function getDistance(la1, lo1, la2, lo2) { const R = 6371e3; const dLat = (la2-la1)*Math.PI/180, dLon = (lo2-lo1)*Math.PI/180; const a = Math.sin(dLat/2)**2 + Math.cos(la1*Math.PI/180)*Math.cos(la2*Math.PI/180)*Math.sin(dLon/2)**2; return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a)); }
function getOfflinePois() { return [{ Id:"poi_01", Name:"Cổng Phố Vĩnh Khánh", Latitude:10.7595, Longitude:106.7048, RadiusMeters:30, Content:{vi:"Chào mừng đến Phố Vĩnh Khánh!"} }].map(normalizePoi); }
function handleDeepLink() { const params = new URLSearchParams(location.search); const poiId = params.get('poiId'); const tourId = params.get('tourId'); if (poiId) { const poi = state.allPoi.find(p => p && p.Id === poiId); if (poi) { map.setView([poi.Latitude, poi.Longitude], 18); showBottomSheet(poi); logAction('scan_qr', poi); } } if (tourId) { logTourScan(tourId); } }
function move(dl, dg) { updateUserPos(state.userPos[0]+dl, state.userPos[1]+dg); map.panTo(state.userPos); }
