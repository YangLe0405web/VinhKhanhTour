using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace VinhKhanhTour.CMS.Security;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient http, 
                       AuthenticationStateProvider authStateProvider, 
                       ILocalStorageService localStorage)
    {
        _http = http;
        _authStateProvider = authStateProvider;
        _localStorage = localStorage;
    }

    public async Task<string?> Login(string username, string password)
    {
        var loginResponse = await _http.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

        if (!loginResponse.IsSuccessStatusCode)
        {
            return await loginResponse.Content.ReadAsStringAsync();
        }

        var result = await loginResponse.Content.ReadFromJsonAsync<LoginResult>();
        
        await _localStorage.SetItemAsync("authToken", result!.Token);
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated(result.Token);
        
        _http.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.Token);

        return null; // success
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        _http.DefaultRequestHeaders.Authorization = null;
    }
}

public class LoginResult
{
    public string Token { get; set; } = "";
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}
