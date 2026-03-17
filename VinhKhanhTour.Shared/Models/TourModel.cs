using Google.Cloud.Firestore;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class TourModel
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string Name { get; set; } = "";
    [FirestoreProperty] public string Desc { get; set; } = "";
    [FirestoreProperty] public List<string> PoiIds { get; set; } = new();
    [FirestoreProperty] public bool IsActive { get; set; } = true;
    [FirestoreProperty] public int QrScans { get; set; } = 0;
    [FirestoreProperty] public string QrType { get; set; } = "play";
    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [FirestoreProperty] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}      