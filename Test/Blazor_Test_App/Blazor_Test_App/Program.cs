using Blazor_Test_App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Để sử dụng IHttpContextAccessor

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình dịch vụ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian chờ session hết hạn
    options.Cookie.HttpOnly = true; // Đảm bảo cookie chỉ được sử dụng qua HTTP
    options.Cookie.IsEssential = true; // Cookie quan trọng
});

// Thêm IHttpContextAccessor để truy cập HttpContext trong Views
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Cấu hình các dịch vụ MVC và Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession();
var app = builder.Build();

// Cấu hình các middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}





app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Sử dụng session trong middleware pipeline
app.UseSession();



app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
