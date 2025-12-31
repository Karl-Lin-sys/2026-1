// =============================================================================
// データベースコンテキスト - AppDbContext.cs
// データベースとの接続を管理する（本棚の管理人）
// =============================================================================

using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Models;

namespace SmartBuildingDashboard.Data;

/// <summary>
/// アプリケーションデータベースコンテキスト
/// 例え話：本棚の管理人。どの棚に何の本があるか知っている
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    /// <summary>センサーテーブル（センサー情報の棚）</summary>
    public DbSet<Sensor> Sensors { get; set; }
    
    /// <summary>電力データテーブル（電力データの棚）</summary>
    public DbSet<EnergyReading> EnergyReadings { get; set; }
    
    /// <summary>予測結果テーブル（予測結果の棚）</summary>
    public DbSet<Prediction> Predictions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // センサーと電力データの関係を設定
        modelBuilder.Entity<EnergyReading>()
            .HasOne(e => e.Sensor)
            .WithMany(s => s.Readings)
            .HasForeignKey(e => e.SensorId);
        
        // インデックス設定（検索を速くするための索引）
        modelBuilder.Entity<EnergyReading>()
            .HasIndex(e => e.Timestamp);
        
        modelBuilder.Entity<Sensor>()
            .HasIndex(s => s.Type);
    }
}

/// <summary>
/// シードデータ（最初に入れておくサンプルデータ）
/// </summary>
public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        // サンプルセンサーを追加
        var sensors = new List<Sensor>
        {
            new Sensor
            {
                Name = "1階ロビー電力計",
                Type = SensorType.Energy,
                Location = "1階ロビー",
                Floor = 1,
                Zone = "A",
                IsActive = true,
                InstalledDate = DateTime.Now.AddYears(-1),
                LastReading = DateTime.Now
            },
            new Sensor
            {
                Name = "2階オフィス電力計",
                Type = SensorType.Energy,
                Location = "2階オフィスエリア",
                Floor = 2,
                Zone = "B",
                IsActive = true,
                InstalledDate = DateTime.Now.AddMonths(-6),
                LastReading = DateTime.Now
            },
            new Sensor
            {
                Name = "屋上太陽光パネル",
                Type = SensorType.Energy,
                Location = "屋上",
                Floor = 10,
                Zone = "ROOF",
                IsActive = true,
                InstalledDate = DateTime.Now.AddMonths(-3),
                LastReading = DateTime.Now
            },
            new Sensor
            {
                Name = "1階温度センサー",
                Type = SensorType.Temperature,
                Location = "1階ロビー",
                Floor = 1,
                Zone = "A",
                IsActive = true,
                InstalledDate = DateTime.Now.AddYears(-1),
                LastReading = DateTime.Now
            },
            new Sensor
            {
                Name = "サーバールーム温度",
                Type = SensorType.Temperature,
                Location = "地下1階サーバールーム",
                Floor = -1,
                Zone = "SERVER",
                IsActive = true,
                InstalledDate = DateTime.Now.AddYears(-2),
                LastReading = DateTime.Now
            }
        };
        
        context.Sensors.AddRange(sensors);
        context.SaveChanges();
        
        // サンプル電力データを生成（過去30日分）
        var random = new Random(42);
        var now = DateTime.Now;
        
        foreach (var sensor in sensors.Where(s => s.Type == SensorType.Energy))
        {
            for (int day = 30; day >= 0; day--)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var timestamp = now.AddDays(-day).AddHours(hour);
                    
                    // 時間帯によって消費量を変える（昼間は多い、夜は少ない）
                    var baseConsumption = hour >= 9 && hour <= 18 ? 50 : 20;
                    var consumption = baseConsumption + random.NextDouble() * 30;
                    
                    context.EnergyReadings.Add(new EnergyReading
                    {
                        SensorId = sensor.Id,
                        Timestamp = timestamp,
                        Consumption = consumption,
                        Power = consumption * 1000 / 60, // kWh to W (per minute)
                        Voltage = 220 + random.NextDouble() * 10 - 5,
                        Current = consumption * 1000 / 220 / 60
                    });
                }
            }
        }
        
        context.SaveChanges();
        
        Console.WriteLine($"✅ シードデータ作成完了: {sensors.Count}センサー, 約{30 * 24 * 3}電力データ");
    }
}
