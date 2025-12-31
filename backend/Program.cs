// =============================================================================
// Smart Building Energy Dashboard - Main Entry Point
// Program.cs - ASP.NET Core Web API
// =============================================================================

using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Data;
using SmartBuildingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// サービス登録（Services - お店の従業員を雇う）
// =============================================================================

// データベース接続（SQLite使用 - 簡単なデータベース）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=smartbuilding.db"));

// コントローラー（APIの入り口）
builder.Services.AddControllers();

// CORS設定（外部からのアクセス許可）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// カスタムサービス（自分で作った機能）
builder.Services.AddScoped<EnergyPredictionService>();
builder.Services.AddScoped<SensorDataService>();

// Swagger（API説明書の自動生成）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Smart Building Energy API",
        Version = "v1",
        Description = "智慧建築能源管理システムのAPI - 五百戶科技応募用ポートフォリオ"
    });
});

var app = builder.Build();

// =============================================================================
// データベース初期化（お店を開ける準備）
// =============================================================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    
    // シードデータがなければ追加
    if (!context.Sensors.Any())
    {
        SeedData.Initialize(context);
    }
}

// =============================================================================
// ミドルウェア設定（お店のルール）
// =============================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// =============================================================================
// 起動！🚀
// =============================================================================

Console.WriteLine("===========================================");
Console.WriteLine("🏢 Smart Building Energy Dashboard API");
Console.WriteLine("===========================================");
Console.WriteLine("📍 API running at: http://localhost:5000");
Console.WriteLine("📚 Swagger UI: http://localhost:5000/swagger");
Console.WriteLine("===========================================");

app.Run();
