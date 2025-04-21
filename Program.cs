using Shofy.Data;
using Shofy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;
using System.Net;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables từ .env
Env.Load();

// Lấy biến từ môi trường
var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "ShofyDb";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "sa";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "yourStrong(!)Password";

var emailHost = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? "smtp.example.com";
var emailPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out int port) ? port : 587;
var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? "";
var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? "";
var emailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? "no-reply@example.com";

// Connection string động
var connectionString = $"Server={dbServer};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";

// Đăng ký DbContext
builder.Services.AddDbContext<ShofyContext>(options =>
    options.UseSqlServer(connectionString));

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

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Dịch vụ Razor
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
