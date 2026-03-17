using Google.Cloud.Firestore;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class LocationTrace
{
    [FirestoreProperty] public string SessionId { get; set; } = "";
    // Làm tròn 3 chữ số → ẩn danh ~111m
    [FirestoreProperty] public double Lat { get; set; }
    [FirestoreProperty] public double Lng { get; set; }
    [FirestoreProperty] public string Language { get; set; } = "vi";
    [FirestoreProperty] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}