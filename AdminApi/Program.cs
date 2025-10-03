var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register DatabaseContext (same as MCBA)
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(DatabaseContext)));

    // Enable lazy loading (same as MCBA)
    options.UseLazyLoadingProxies();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
