using Google.Cloud.Firestore;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class PoiModel
{
    [FirestoreProperty] public string Id { get; set; } = "";
    [FirestoreProperty] public string Name { get; set; } = "";
    [FirestoreProperty] public string Category { get; set; } = "food";
    [FirestoreProperty] public double Latitude { get; set; }
    [FirestoreProperty] public double Longitude { get; set; }
    [FirestoreProperty] public double Radius { get; set; } = 30;
    [FirestoreProperty] public int Priority { get; set; } = 1;
    [FirestoreProperty] public bool IsActive { get; set; } = true;

    [FirestoreProperty] public double Price { get; set; } = 25000;
    [FirestoreProperty] public bool RequirePayment { get; set; } = true;

    [FirestoreProperty] public Dictionary<string, string> Content { get; set; } = new();
    [FirestoreProperty] public Dictionary<string, string> AudioUrls { get; set; } = new();

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [FirestoreProperty] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string GetContent(string lang)
        => Content.TryGetValue(lang, out var v) ? v
         : Content.TryGetValue("vi", out var vv) ? vv : "";
}
