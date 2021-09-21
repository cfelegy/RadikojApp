using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

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
    var supportedCultures = new[] { "en-US", "ar-sa", "zh-Hans", "fr", "ru", "es" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

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

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}");
});

app.Run();
