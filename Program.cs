using BlazorApp1.Components;
using BlazorApp1.Data;
using BlazorApp1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === EF Core + SQLite ===
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=CareHomeDB.db"));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AanmeldenService>();

// === Cookie Authentication ===
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "NaastElkaarAdmin";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
// Required for [Authorize] / AuthorizeView to work in Blazor Server interactive components
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Run migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// === Login endpoint (plain minimal API so SignInAsync works outside Blazor's SignalR context) ===
app.MapPost("/login-submit", async (HttpContext ctx, IConfiguration config, IHostEnvironment env) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString().Trim();
    var password = form["password"].ToString();

    var storedUsername = config["AdminCredentials:Username"] ?? "";
    var storedHash    = config["AdminCredentials:PasswordHash"] ?? "";

    bool valid = false;

    if (env.IsDevelopment() && string.IsNullOrEmpty(storedHash))
    {
        // DEV-ONLY fallback: active only in Development when PasswordHash is not yet configured.
        // Remove this block (or just set a real PasswordHash secret) before going to production.
        valid = username == "admin" && password == "admin123";
    }
    else if (!string.IsNullOrEmpty(storedHash) && !string.IsNullOrEmpty(storedUsername))
    {
        valid = username == storedUsername && VerifyPassword(password, storedHash);
    }

    if (valid)
    {
        var claims   = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Redirect("/admin/aanmeldingen");
    }

    return Results.Redirect("/login?error=1");
}).DisableAntiforgery();

// === Logout endpoint ===
app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
}).DisableAntiforgery();

// === Dev-only: generate a PBKDF2 hash for a given password (visit once to get your PasswordHash) ===
// Example: GET /dev/hash-password?password=MijnWachtwoord123
// Copy the output into dotnet user-secrets or Render env var AdminCredentials__PasswordHash
if (app.Environment.IsDevelopment())
{
    app.MapGet("/dev/hash-password", (string password) => Results.Text(HashPassword(password)));
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// ── PBKDF2 helpers (SHA-256, 100 000 iterations, no extra NuGet package needed) ──────────────
static string HashPassword(string password)
{
    byte[] salt = RandomNumberGenerator.GetBytes(16);
    byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
}

static bool VerifyPassword(string password, string storedHash)
{
    var parts = storedHash.Split(':');
    if (parts.Length != 2) return false;
    try
    {
        byte[] salt         = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] hash         = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }
    catch { return false; }
}
