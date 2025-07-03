using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using carp_tm_2025.Data;
using Microsoft.AspNetCore.Identity;
using carp_tm_delegati.Areas.Identity.Data;
using carp_tm_delegati.Area.Identity.data;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Negotiate;





var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<carp_tm_2025Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("carp_tm_2025Context") ?? throw new InvalidOperationException("Connection string 'carp_tm_delegatiContext' not found.")));



//builder.Services.AddDbContext<carp_tm_delegati_UsersContext>(options =>
  //options.UseSqlServer(builder.Configuration.GetConnectionString("carp_tm_delegatiUsersContext") ?? throw new InvalidOperationException("Connection string 'carp_tm_delegatiUsersContext' not found.")));
//builder.Services.AddDefaultIdentity<carp_tm_delegatiUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<carp_tm_delegati_UsersContext>();




// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var supportedCultures = new[]
{
 new CultureInfo("en-US"),
 new CultureInfo("fr"),
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,
    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});



app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.MapRazorPages();

app.Run();
