using Microsoft.EntityFrameworkCore;
using EvacuationApi.Data;
using EvacuationApi.Repositories;
using EvacuationApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enum в JSON — строками ("Bus", "Own", "Main"), а не числами.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Эвакуация — расчёты и планирование API",
        Version = "v1",
        Description = "Сотрудники, транспортная ведомость, маршруты (с картами), пункты временного размещения, " +
                      "расчётные таблицы и автораспределение (Пункт №7: «Расчёты и планирование эвакуации»)."
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=evacuation.db"));

// Репозитории
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IReceptionPointRepository, ReceptionPointRepository>();

// Сервисы
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IReceptionPointService, ReceptionPointService>();
builder.Services.AddScoped<IEvacuationPlanningService, EvacuationPlanningService>();

// Файловое хранилище карт маршрутов
builder.Services.AddSingleton<IMapFileStorage, MapFileStorage>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (app.Configuration.GetValue<bool>("SeedDemoData"))
        DbSeeder.SeedDemoData(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Эвакуация API v1");
    c.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
