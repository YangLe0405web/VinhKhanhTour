// Service worker tối giản để hỗ trợ PWA install
self.addEventListener('install', (e) => {
  console.log('[PWA] Service Worker installed');
});

self.addEventListener('fetch', (e) => {
  // Không làm gì, để trình duyệt tự xử lý mạng
  return;
});
