using Google.Cloud.Firestore;

namespace VinhKhanhTour.Shared.Models;

[FirestoreData]
public class AppUser
{
    [FirestoreProperty] public string Id { get; set; } = Guid.NewGuid().ToString();
    [FirestoreProperty] public string Username { get; set; } = "";
    [FirestoreProperty] public string PasswordHash { get; set; } = "";
    [FirestoreProperty] public string FullName { get; set; } = "";
    [FirestoreProperty] public string Role { get; set; } = "owner"; // admin or owner
    
    [FirestoreProperty] public string PhoneNumber { get; set; } = "";
    [FirestoreProperty] public string Email { get; set; } = "";
    [FirestoreProperty] public string Address { get; set; } = "";
    
    // For owner: list of POIs they are allowed to manage
    [FirestoreProperty] public List<string> ManagedPoiIds { get; set; } = new();
    
    [FirestoreProperty] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
