using MCBA.Data;
using MCBA.Services;
using MCBA.Filters;
using Microsoft.EntityFrameworkCore;
using SimpleHashing.Net;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(DatabaseContext)));

    // Enable lazy loading.
    options.UseLazyLoadingProxies();
});

// For password hashing
builder.Services.AddSingleton<ISimpleHash, SimpleHash>();
// for DI password
builder.Services.AddScoped<PasswordService>();



// Store session into Web-Server memory.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Make the session cookie essential.
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
// Global authorisation check
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AuthorizeCustomerAttribute()));

var app = builder.Build();

// Seed data.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    SeedData.Initialize(services);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapDefaultControllerRoute().WithStaticAssets();

app.MapControllerRoute(
        "default",
        "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();