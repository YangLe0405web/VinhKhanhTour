using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using VinhKhanhTour.CMS;
using VinhKhanhTour.CMS.Services;
using VinhKhanhTour.CMS.Security;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URL API backend đang chạy
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://vinhkhanh-api.onrender.com/")
});

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CmsApiService>();
await builder.Build().RunAsync();