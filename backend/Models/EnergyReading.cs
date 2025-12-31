// =============================================================================
// データモデル - EnergyReading.cs
// 電力使用量データを表す（電気メーターの読み取り値）
// =============================================================================

namespace SmartBuildingDashboard.Models;

/// <summary>
/// 電力データ（Energy Reading）
/// 例え話：電気メーターに表示される数字を記録したもの
/// </summary>
public class EnergyReading
{
    /// <summary>ID（識別番号）- 本につける背番号みたいなもの</summary>
    public int Id { get; set; }
    
    /// <summary>センサーID - どの電気メーターからのデータか</summary>
    public int SensorId { get; set; }
    
    /// <summary>計測日時 - いつ測ったか</summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>電力消費量（kWh）- どれだけ電気を使ったか</summary>
    public double Consumption { get; set; }
    
    /// <summary>電力（W）- 今どれくらいの勢いで電気を使っているか</summary>
    public double Power { get; set; }
    
    /// <summary>電圧（V）- 電気の圧力</summary>
    public double Voltage { get; set; }
    
    /// <summary>電流（A）- 電気の流れる量</summary>
    public double Current { get; set; }
    
    /// <summary>関連センサー</summary>
    public virtual Sensor? Sensor { get; set; }
}
