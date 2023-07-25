using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AWSGymWebsite.Data;
using AWSGymWebsite.Areas.Identity.Data;
using AWSGymWebsite.Models;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Core;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AWSGymWebsiteContextConnection") ?? throw new InvalidOperationException("Connection string 'AWSGymWebsiteContextConnection' not found.");

builder.Services.AddDbContext<AWSGymWebsiteContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<AWSGymWebsiteUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AWSGymWebsiteContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
//
app.UseXRay("XRay-GymWebsite");
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();// Check customer permission
app.UseAuthorization();// Direct Login page


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
