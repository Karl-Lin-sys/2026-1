// =============================================================================
// SensorController.cs - センサーAPI
// IoTセンサーの管理と状態確認（温度計や電気メーターの管理）
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Data;
using SmartBuildingDashboard.Models;

namespace SmartBuildingDashboard.Controllers;

/// <summary>
/// センサーコントローラー
/// 例え話：ビルに設置された全ての計器の管理人
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SensorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SensorController> _logger;
    
    public SensorController(AppDbContext context, ILogger<SensorController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// 全センサー一覧を取得
    /// GET /api/sensor
    /// 子供向け説明：「ビルにどんな計器があるの？」に答える
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetAllSensors()
    {
        var sensors = await _context.Sensors.ToListAsync();
        _logger.LogInformation($"📡 {sensors.Count}個のセンサー情報を返却");
        return Ok(sensors);
    }
    
    /// <summary>
    /// 特定のセンサー情報を取得
    /// GET /api/sensor/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Sensor>> GetSensor(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        
        if (sensor == null)
        {
            return NotFound($"センサーID {id} が見つかりません");
        }
        
        return Ok(sensor);
    }
    
    /// <summary>
    /// タイプ別センサー一覧
    /// GET /api/sensor/type/{type}
    /// 子供向け説明：「温度計だけ見せて！」みたいなリクエスト
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetByType(SensorType type)
    {
        var sensors = await _context.Sensors
            .Where(s => s.Type == type)
            .ToListAsync();
        
        return Ok(sensors);
    }
    
    /// <summary>
    /// フロア別センサー一覧
    /// GET /api/sensor/floor/{floor}
    /// </summary>
    [HttpGet("floor/{floor}")]
    public async Task<ActionResult<IEnumerable<Sensor>>> GetByFloor(int floor)
    {
        var sensors = await _context.Sensors
            .Where(s => s.Floor == floor)
            .ToListAsync();
        
        return Ok(sensors);
    }
    
    /// <summary>
    /// センサーステータスダッシュボード
    /// GET /api/sensor/status
    /// 子供向け説明：「全部の計器、ちゃんと動いてる？」
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<object>> GetSensorStatus()
    {
        var sensors = await _context.Sensors.ToListAsync();
        var now = DateTime.Now;
        
        var status = sensors.Select(s => new
        {
            s.Id,
            s.Name,
            s.Type,
            s.Location,
            s.IsActive,
            LastReading = s.LastReading,
            // 5分以上データがなければ「オフライン」
            IsOnline = s.IsActive && (now - s.LastReading).TotalMinutes < 5,
            MinutesSinceLastReading = (int)(now - s.LastReading).TotalMinutes
        });
        
        var summary = new
        {
            TotalSensors = sensors.Count,
            ActiveSensors = sensors.Count(s => s.IsActive),
            OnlineSensors = status.Count(s => s.IsOnline),
            OfflineSensors = status.Count(s => !s.IsOnline),
            ByType = sensors.GroupBy(s => s.Type).Select(g => new
            {
                Type = g.Key.ToString(),
                Count = g.Count()
            }),
            Sensors = status
        };
        
        return Ok(summary);
    }
    
    /// <summary>
    /// 新しいセンサーを登録
    /// POST /api/sensor
    /// 子供向け説明：「新しい温度計を付けたから登録して！」
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Sensor>> CreateSensor([FromBody] CreateSensorDto dto)
    {
        var sensor = new Sensor
        {
            Name = dto.Name,
            Type = dto.Type,
            Location = dto.Location,
            Floor = dto.Floor,
            Zone = dto.Zone,
            IsActive = true,
            InstalledDate = DateTime.Now,
            LastReading = DateTime.Now
        };
        
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"🆕 新しいセンサーを登録: {sensor.Name}");
        
        return CreatedAtAction(nameof(GetSensor), new { id = sensor.Id }, sensor);
    }
    
    /// <summary>
    /// センサーを更新
    /// PUT /api/sensor/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Sensor>> UpdateSensor(int id, [FromBody] UpdateSensorDto dto)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        
        if (sensor == null)
        {
            return NotFound($"センサーID {id} が見つかりません");
        }
        
        if (!string.IsNullOrEmpty(dto.Name))
            sensor.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Location))
            sensor.Location = dto.Location;
        if (dto.IsActive.HasValue)
            sensor.IsActive = dto.IsActive.Value;
        
        await _context.SaveChangesAsync();
        
        return Ok(sensor);
    }
    
    /// <summary>
    /// センサーを削除
    /// DELETE /api/sensor/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSensor(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        
        if (sensor == null)
        {
            return NotFound($"センサーID {id} が見つかりません");
        }
        
        _context.Sensors.Remove(sensor);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"🗑️ センサーを削除: {sensor.Name}");
        
        return NoContent();
    }
}

/// <summary>
/// センサー作成用DTO
/// </summary>
public class CreateSensorDto
{
    public string Name { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string? Zone { get; set; }
}

/// <summary>
/// センサー更新用DTO
/// </summary>
public class UpdateSensorDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public bool? IsActive { get; set; }
}
