using Google.Cloud.Firestore;
using static Google.Api.Distribution.Types;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class AnalyticsEvent
{
    [FirestoreProperty] public string EventType { get; set; } = "";
    [FirestoreProperty] public string PoiId { get; set; } = "";
    [FirestoreProperty] public string Language { get; set; } = "vi";
    [FirestoreProperty] public int Duration { get; set; } = 0;
    [FirestoreProperty] public double Lat { get; set; }
    [FirestoreProperty] public double Lng { get; set; }
    [FirestoreProperty] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
