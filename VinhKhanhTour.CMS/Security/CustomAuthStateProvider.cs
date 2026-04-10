using System.Net.Http.Json;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace VinhKhanhTour.CMS.Security;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;

    public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _http.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        
        var claims = new List<Claim>();
        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                var key = kvp.Key;
                if (key == "role" || key == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") key = ClaimTypes.Role;
                if (key == "unique_name" || key == "name" || key == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name") key = ClaimTypes.Name;

                if (kvp.Value is System.Text.Json.JsonElement element)
                {
                    if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            claims.Add(new Claim(key, item.ToString()!));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(key, element.ToString()!));
                    }
                }
                else
                {
                    claims.Add(new Claim(key, kvp.Value.ToString()!));
                }
            }
        }
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
