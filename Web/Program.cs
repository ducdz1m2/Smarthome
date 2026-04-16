using MudBlazor.Services;
using Web.Components;
using Application;
using Infrastructure;
using Web.Services;
using Web.Hubs;
using Infrastructure.Data;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable detailed errors for debugging
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => options.DetailedErrors = true);
builder.Services.AddMudServices();

// Add session support for JWT token storage
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add authentication & authorization
var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!))
    };
    // Support SignalR: token via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TechnicianOnly", policy =>
        policy.RequireRole("Technician"));
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Register custom services
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddHttpContextAccessor();

// Register SignalR services
builder.Services.AddSignalR();
builder.Services.AddScoped<Web.Services.SignalRService>();

// Add HttpClient for API calls with JWT token handler
builder.Services.AddScoped<JwtTokenHandler>();
builder.Services.AddScoped(sp => 
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    var tokenHandler = sp.GetRequiredService<JwtTokenHandler>();
    var httpClient = new HttpClient(new JwtTokenMessageHandler(tokenHandler)) { BaseAddress = new Uri(navigationManager.BaseUri) };
    return httpClient;
});

// Add authentication state provider
builder.Services.AddScoped<AuthenticationStateProvider, LocalAuthStateProvider>();
builder.Services.AddScoped<LocalAuthStateProvider>(sp => (LocalAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddScoped<CircuitHandler, CircuitAccessor>();

var app = builder.Build();

// Seed data
await DataSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseSession();
app.UseHttpsRedirection();

// Serve static files from wwwroot/uploads
app.UseStaticFiles();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<InstallationHub>("/hubs/installation");

app.Run();
