using Core.IRepo;
using Data;
using Data.Entities;
using Data.Repo;
using Data.Services.Implementation;
using Data.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AdtDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("ATDConnectionStrings")));
builder.Services.AddHttpClient();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient("Dojah", cfg =>
    {
        cfg.DefaultRequestHeaders.Add("Authorization", $"{Environment.GetEnvironmentVariable("DOJAH_API_KEY")}");
        cfg.DefaultRequestHeaders.Add("AppId", $"{Environment.GetEnvironmentVariable("DOJAH_APP_ID")}");
        cfg.BaseAddress = new Uri("https://api.dojah.io");
    });
}
else
{
    builder.Services.AddHttpClient("Dojah", cfg =>
    {
        cfg.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("DOJAH_API_KEY")}");
        cfg.DefaultRequestHeaders.Add("AppId", $"{Environment.GetEnvironmentVariable("DOJAH_APP_ID")}");
        cfg.BaseAddress = new Uri("https://api.dojah.io");
    });
}
builder.Services.AddIdentity<Persona, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = false;

}).AddRoles<Role>()
  .AddEntityFrameworkStores<AdtDbContext>()
  .AddDefaultTokenProviders();
builder.Services.AddTransient<IAdminRepo, AdminRepo>();
builder.Services.AddTransient<IMembersRepo, MembersRepo>();
builder.Services.AddTransient<ILocationService, LocationService>();
builder.Services.AddTransient<IRoleService, RoleService>();
builder.Services.AddTransient<IDirectorRepo, DirectorRepo>();
builder.Services.AddTransient<IPaymentInfo, PaymentRepo>();
builder.Services.AddTransient<ISMSService, SMSSevice>();
builder.Services.AddTransient<IPdfService, PdfService>();





builder.Services.AddHostedService<DbSeed>();

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
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
