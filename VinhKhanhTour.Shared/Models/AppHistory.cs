using Google.Cloud.Firestore;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class AppHistory
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string Action { get; set; } = "";
    // open_app | play_poi | scan_qr | route_start | language_change | pay_audio
    [FirestoreProperty] public string PoiId { get; set; } = "";
    [FirestoreProperty] public string PoiName { get; set; } = "";
    [FirestoreProperty] public string Language { get; set; } = "vi";
    [FirestoreProperty] public string Device { get; set; } = "";
    [FirestoreProperty] public int Duration { get; set; } = 0;
    [FirestoreProperty] public double Lat { get; set; } = 0;
    [FirestoreProperty] public double Lng { get; set; } = 0;
    [FirestoreProperty] public string Currency { get; set; } = "VND";
    [FirestoreProperty] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}