// =============================================================================
// データモデル - Sensor.cs
// IoTセンサーを表す（温度計や電気メーターなど）
// =============================================================================

namespace SmartBuildingDashboard.Models;

/// <summary>
/// センサー（Sensor）
/// 例え話：ビルに設置された「温度計」「電気メーター」「湿度計」など
/// </summary>
public class Sensor
{
    /// <summary>ID（識別番号）</summary>
    public int Id { get; set; }
    
    /// <summary>センサー名 - 「1階ロビー温度計」など</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>センサータイプ - Temperature, Energy, Humidity など</summary>
    public SensorType Type { get; set; }
    
    /// <summary>設置場所 - 「1階ロビー」「屋上」など</summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>フロア番号</summary>
    public int Floor { get; set; }
    
    /// <summary>部屋番号または区画</summary>
    public string? Zone { get; set; }
    
    /// <summary>稼働中かどうか</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>最終データ受信日時</summary>
    public DateTime LastReading { get; set; }
    
    /// <summary>設置日</summary>
    public DateTime InstalledDate { get; set; }
    
    /// <summary>このセンサーからの計測データ</summary>
    public virtual ICollection<EnergyReading> Readings { get; set; } = new List<EnergyReading>();
}

/// <summary>
/// センサータイプ（どんな種類のセンサーか）
/// </summary>
public enum SensorType
{
    /// <summary>温度センサー - 暑い・寒いを測る</summary>
    Temperature,
    
    /// <summary>電力センサー - 電気の使用量を測る</summary>
    Energy,
    
    /// <summary>湿度センサー - じめじめ度を測る</summary>
    Humidity,
    
    /// <summary>照明センサー - 明るさを測る</summary>
    Light,
    
    /// <summary>人感センサー - 人がいるかを検知</summary>
    Occupancy,
    
    /// <summary>空気品質センサー - CO2などを測る</summary>
    AirQuality
}
