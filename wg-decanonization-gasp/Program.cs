using Microsoft.AspNetCore.Mvc.Razor;
using GaspApp;
using GaspApp.Data;
using Microsoft.EntityFrameworkCore;
using GaspApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using GaspApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Localization
builder.Services.AddLocalization();
builder.Services.AddSingleton<SharedViewLocalizer>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    /* TODO: Validate these cultures with the team. UN website lists the current official languages as:
     * - Arabic
     * - Chinese
     * - English
     * - French
     * - Russian
     * - Spanish
     * Are the full locales below correct to match this list? (e.g. Chinese simplified vs traditional, Arabic SA or other...)
     */
    var supportedCultures = LocaleConstants.SUPPORTED_LOCALES;
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();
builder.Services.AddDbContext<GaspDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/SignOut";
    });
builder.Services.AddSingleton<IPasswordHasher<Account>>(new PasswordHasher<Account>());
builder.Services.AddScoped<AccountService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}");
});

app.Run();
