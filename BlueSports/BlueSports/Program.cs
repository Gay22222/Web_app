﻿using AspNetCoreHero.ToastNotification;
using BlueSports.Data;
using BlueSports.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(p =>
                {
                    p.Cookie.Name = "UserLoginCookie";
                    p.ExpireTimeSpan = TimeSpan.FromDays(1);
                    p.LoginPath = "/Log-in";
                    //p.LogoutPath = "/dang-xuat/html";
                    p.AccessDeniedPath = "/not-found.html";
                });

// KHOITD-EV SECTION
// ** BEGIN **
// Register MoMoPaymentService
builder.Services.AddHttpClient<MoMoPaymentService>();
// ** END **

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
/*builder.Services.AddSignalR();*/
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseCookiePolicy();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();