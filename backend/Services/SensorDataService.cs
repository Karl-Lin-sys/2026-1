// =============================================================================
// SensorDataService.cs - センサーデータサービス
// センサーからのデータを処理・集計する
// =============================================================================

using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Data;
using SmartBuildingDashboard.Models;

namespace SmartBuildingDashboard.Services;

/// <summary>
/// センサーデータサービス
/// 例え話：温度計や電気メーターからのデータを整理してくれる係
/// </summary>
public class SensorDataService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SensorDataService> _logger;
    
    public SensorDataService(AppDbContext context, ILogger<SensorDataService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// センサーからデータを受信して保存
    /// 子供向け説明：「温度計から数字が届いたから、ノートに書いておくね！」
    /// </summary>
    public async Task<bool> ProcessSensorDataAsync(SensorDataPayload payload)
    {
        try
        {
            var sensor = await _context.Sensors.FindAsync(payload.SensorId);
            
            if (sensor == null)
            {
                _logger.LogWarning($"未登録のセンサーからデータ受信: {payload.SensorId}");
                return false;
            }
            
            // データを保存
            if (sensor.Type == SensorType.Energy)
            {
                var reading = new EnergyReading
                {
                    SensorId = payload.SensorId,
                    Timestamp = payload.Timestamp,
                    Consumption = payload.Value,
                    Power = payload.Value * 1000 / 60, // kWh to W/min
                    Voltage = payload.Metadata.GetValueOrDefault("voltage", 220),
                    Current = payload.Metadata.GetValueOrDefault("current", 0)
                };
                
                _context.EnergyReadings.Add(reading);
            }
            
            // センサーの最終読み取り時刻を更新
            sensor.LastReading = payload.Timestamp;
            
            await _context.SaveChangesAsync();
            
            // アラートチェック
            await CheckAlertsAsync(sensor, payload.Value);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "センサーデータ処理でエラー");
            return false;
        }
    }
    
    /// <summary>
    /// センサー異常値をチェック
    /// 子供向け説明：「温度が高すぎたら教えてくれる番犬」
    /// </summary>
    private async Task CheckAlertsAsync(Sensor sensor, double value)
    {
        var thresholds = GetThresholdsForSensor(sensor.Type);
        
        if (value > thresholds.High)
        {
            _logger.LogWarning($"⚠️ 高値アラート: {sensor.Name} = {value} (閾値: {thresholds.High})");
            // 実際はここでメール通知やWebSocket通知を行う
        }
        else if (value < thresholds.Low)
        {
            _logger.LogWarning($"⚠️ 低値アラート: {sensor.Name} = {value} (閾値: {thresholds.Low})");
        }
    }
    
    /// <summary>
    /// センサータイプ別の閾値を取得
    /// </summary>
    private (double Low, double High) GetThresholdsForSensor(SensorType type)
    {
        return type switch
        {
            SensorType.Temperature => (15, 35),    // 摂氏
            SensorType.Energy => (0, 100),         // kWh
            SensorType.Humidity => (30, 70),       // パーセント
            SensorType.Light => (100, 10000),      // ルクス
            SensorType.AirQuality => (0, 1000),    // ppm
            _ => (0, double.MaxValue)
        };
    }
    
    /// <summary>
    /// センサー別のサマリー統計を計算
    /// </summary>
    public async Task<List<SensorSummary>> GetSensorSummariesAsync()
    {
        var sensors = await _context.Sensors
            .Include(s => s.Readings)
            .ToListAsync();
        
        var summaries = new List<SensorSummary>();
        var now = DateTime.Now;
        var last24h = now.AddHours(-24);
        
        foreach (var sensor in sensors)
        {
            var recentReadings = sensor.Readings
                .Where(r => r.Timestamp >= last24h)
                .ToList();
            
            if (!recentReadings.Any())
            {
                summaries.Add(new SensorSummary
                {
                    SensorId = sensor.Id,
                    SensorName = sensor.Name,
                    HasData = false
                });
                continue;
            }
            
            summaries.Add(new SensorSummary
            {
                SensorId = sensor.Id,
                SensorName = sensor.Name,
                HasData = true,
                Min = recentReadings.Min(r => r.Consumption),
                Max = recentReadings.Max(r => r.Consumption),
                Average = recentReadings.Average(r => r.Consumption),
                Total = recentReadings.Sum(r => r.Consumption),
                ReadingCount = recentReadings.Count,
                LastReading = recentReadings.Max(r => r.Timestamp)
            });
        }
        
        return summaries;
    }
}

/// <summary>
/// センサーからのデータペイロード
/// </summary>
public class SensorDataPayload
{
    public int SensorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public double Value { get; set; }
    public Dictionary<string, double> Metadata { get; set; } = new();
}

/// <summary>
/// センサーサマリー
/// </summary>
public class SensorSummary
{
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Average { get; set; }
    public double Total { get; set; }
    public int ReadingCount { get; set; }
    public DateTime LastReading { get; set; }
}
