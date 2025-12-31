// =============================================================================
// EnergyController.cs - 電力データAPI
// 電力消費量のデータを提供する（電気メーターの読み取り値を教える）
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Data;
using SmartBuildingDashboard.Models;

namespace SmartBuildingDashboard.Controllers;

/// <summary>
/// 電力データコントローラー
/// 例え話：電気メーターのデータを見せてくれる「窓口係」
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EnergyController> _logger;
    
    public EnergyController(AppDbContext context, ILogger<EnergyController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// 最新の電力データを取得
    /// GET /api/energy
    /// 子供向け説明：「今、電気をどれくらい使ってるの？」に答える
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnergyReading>>> GetLatestReadings(
        [FromQuery] int limit = 100)
    {
        var readings = await _context.EnergyReadings
            .Include(e => e.Sensor)
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToListAsync();
        
        _logger.LogInformation($"📊 最新{limit}件の電力データを返却");
        return Ok(readings);
    }
    
    /// <summary>
    /// 日別の電力消費量サマリーを取得
    /// GET /api/energy/daily
    /// 子供向け説明：「今日は全部でどれくらい電気使った？」
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<object>> GetDailySummary(
        [FromQuery] int days = 7)
    {
        var startDate = DateTime.Now.AddDays(-days);
        
        var summary = await _context.EnergyReadings
            .Where(e => e.Timestamp >= startDate)
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalConsumption = g.Sum(e => e.Consumption),
                AveragePower = g.Average(e => e.Power),
                ReadingCount = g.Count()
            })
            .OrderByDescending(s => s.Date)
            .ToListAsync();
        
        _logger.LogInformation($"📅 過去{days}日分の日別サマリーを返却");
        return Ok(summary);
    }
    
    /// <summary>
    /// 時間別の電力消費量を取得
    /// GET /api/energy/hourly
    /// 子供向け説明：「何時が一番電気使うの？」
    /// </summary>
    [HttpGet("hourly")]
    public async Task<ActionResult<object>> GetHourlySummary(
        [FromQuery] string? date = null)
    {
        var targetDate = string.IsNullOrEmpty(date) 
            ? DateTime.Today 
            : DateTime.Parse(date);
        
        var summary = await _context.EnergyReadings
            .Where(e => e.Timestamp.Date == targetDate)
            .GroupBy(e => e.Timestamp.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                TotalConsumption = g.Sum(e => e.Consumption),
                AveragePower = g.Average(e => e.Power)
            })
            .OrderBy(s => s.Hour)
            .ToListAsync();
        
        return Ok(new
        {
            Date = targetDate.ToString("yyyy-MM-dd"),
            Data = summary
        });
    }
    
    /// <summary>
    /// センサー別の電力データを取得
    /// GET /api/energy/sensor/{sensorId}
    /// </summary>
    [HttpGet("sensor/{sensorId}")]
    public async Task<ActionResult<IEnumerable<EnergyReading>>> GetBySensor(
        int sensorId,
        [FromQuery] int hours = 24)
    {
        var startTime = DateTime.Now.AddHours(-hours);
        
        var readings = await _context.EnergyReadings
            .Where(e => e.SensorId == sensorId && e.Timestamp >= startTime)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
        
        if (!readings.Any())
        {
            return NotFound($"センサーID {sensorId} のデータが見つかりません");
        }
        
        return Ok(readings);
    }
    
    /// <summary>
    /// リアルタイム電力データを送信（IoTデバイスから）
    /// POST /api/energy
    /// 子供向け説明：「電気メーターが新しい数字を教えてくれた！」
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<EnergyReading>> PostReading(
        [FromBody] EnergyReadingDto dto)
    {
        var sensor = await _context.Sensors.FindAsync(dto.SensorId);
        if (sensor == null)
        {
            return NotFound($"センサーID {dto.SensorId} が見つかりません");
        }
        
        var reading = new EnergyReading
        {
            SensorId = dto.SensorId,
            Timestamp = DateTime.Now,
            Consumption = dto.Consumption,
            Power = dto.Power,
            Voltage = dto.Voltage,
            Current = dto.Current
        };
        
        _context.EnergyReadings.Add(reading);
        
        // センサーの最終読み取り時刻を更新
        sensor.LastReading = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"⚡ 新しい電力データを受信: センサー{dto.SensorId}, {dto.Consumption}kWh");
        
        return CreatedAtAction(nameof(GetLatestReadings), new { id = reading.Id }, reading);
    }
    
    /// <summary>
    /// 統計サマリーを取得
    /// GET /api/energy/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var weekAgo = today.AddDays(-7);
        
        var todayConsumption = await _context.EnergyReadings
            .Where(e => e.Timestamp.Date == today)
            .SumAsync(e => e.Consumption);
        
        var yesterdayConsumption = await _context.EnergyReadings
            .Where(e => e.Timestamp.Date == yesterday)
            .SumAsync(e => e.Consumption);
        
        var weeklyAverage = await _context.EnergyReadings
            .Where(e => e.Timestamp >= weekAgo)
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => g.Sum(e => e.Consumption))
            .AverageAsync();
        
        var changePercent = yesterdayConsumption > 0
            ? ((todayConsumption - yesterdayConsumption) / yesterdayConsumption) * 100
            : 0;
        
        return Ok(new
        {
            TodayConsumption = Math.Round(todayConsumption, 2),
            YesterdayConsumption = Math.Round(yesterdayConsumption, 2),
            WeeklyAverageDaily = Math.Round(weeklyAverage, 2),
            ChangeFromYesterday = Math.Round(changePercent, 1),
            Unit = "kWh",
            GeneratedAt = DateTime.Now
        });
    }
}

/// <summary>
/// 電力データ送信用DTO
/// </summary>
public class EnergyReadingDto
{
    public int SensorId { get; set; }
    public double Consumption { get; set; }
    public double Power { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
}
