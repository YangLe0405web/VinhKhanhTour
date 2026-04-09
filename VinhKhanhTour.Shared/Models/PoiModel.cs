using Google.Cloud.Firestore;
using static Google.Api.Distribution.Types;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;

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
    [FirestoreProperty] public Dictionary<string, string> AudioUrls { get; set; } = new();

    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [FirestoreProperty] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string GetContent(string lang)
        => Content.TryGetValue(lang, out var v) ? v
         : Content.TryGetValue("vi", out var vv) ? vv : "";
}
