using Microsoft.EntityFrameworkCore;
using SonodaSoftware.Data;
using SonodaSoftware.Services;
using SonodaSoftware.Services.JobServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
Appsetting.ConnectionStrings = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SND_DBContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString(Appsetting.ConnectionStrings)));
builder.Services.AddScoped<IPartService,PartService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.Cookie.Name = "minnion";
    option.IdleTimeout = TimeSpan.FromDays(1);
    option.Cookie.IsEssential = true;
    option.Cookie.HttpOnly = true;
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

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
