using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Shofy.Services.MoMo;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env
Env.Load();

// Get environment variables
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "ShofyDb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "yourStrong(!)Password";

var emailHost = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? "smtp.gmail.com";
var emailPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out int port) ? port : 587;
var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "";
var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "";
var emailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "no-reply@example.com";

// Dynamic connection string
var connectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";

// Register DbContext
builder.Services.AddDbContext<ShofyContext>(options =>
    options.UseSqlServer(connectionString));

// MoMo configuration
builder.Services.Configure<MoMoConfig>(builder.Configuration.GetSection("MoMo"));
builder.Services.AddHttpClient<MoMoService>();
builder.Services.AddScoped<MoMoService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// SMTP client
builder.Services.AddSingleton(new SmtpClient
{
    Host = emailHost,
    Port = emailPort,
    EnableSsl = true,
    Credentials = new NetworkCredential(emailUsername, emailPassword)
});
builder.Services.AddSingleton(new MailAddress(emailFrom, "Shofy Support"));

// Authentication
builder.Services.AddAuthentication(options =>
{
 options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddFacebook(facebookOptions =>
{
 facebookOptions.AppId = Environment.GetEnvironmentVariable("FACEBOOK_APP_ID") ?? "FACEBOOK_APP_ID";
 facebookOptions.AppSecret = Environment.GetEnvironmentVariable("FACEBOOK_APP_SECRET") ?? "FACEBOOK_APP_SECRET";
 facebookOptions.CallbackPath = "/signin-facebook";
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "GOOGLE_CLIENT_ID";
    googleOptions.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "GOOGLE_CLIENT_SECRET";
    googleOptions.CallbackPath = "/signin-google"; // phải trùng URI trong Google Console
});


// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();