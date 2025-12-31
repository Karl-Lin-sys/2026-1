// =============================================================================
// データモデル - Prediction.cs
// AI予測結果を表す
// =============================================================================

namespace SmartBuildingDashboard.Models;

/// <summary>
/// AI予測結果（Prediction）
/// 例え話：天気予報みたいに「明日は電気をこれくらい使いそう」と教えてくれる
/// </summary>
public class Prediction
{
    /// <summary>ID</summary>
    public int Id { get; set; }
    
    /// <summary>予測対象日時</summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>予測日時（いつ予測したか）</summary>
    public DateTime PredictedAt { get; set; }
    
    /// <summary>予測消費量（kWh）</summary>
    public double PredictedConsumption { get; set; }
    
    /// <summary>予測の信頼度（0-100%） - どれくらい自信があるか</summary>
    public double Confidence { get; set; }
    
    /// <summary>予測の下限値</summary>
    public double LowerBound { get; set; }
    
    /// <summary>予測の上限値</summary>
    public double UpperBound { get; set; }
    
    /// <summary>予測モデルのバージョン</summary>
    public string ModelVersion { get; set; } = "v1.0";
}

/// <summary>
/// 予測リクエスト（APIに送るデータ）
/// </summary>
public class PredictionRequest
{
    /// <summary>予測したい日付（何日後まで予測するか）</summary>
    public int DaysAhead { get; set; } = 1;
    
    /// <summary>対象センサーID（指定しない場合は全体）</summary>
    public int? SensorId { get; set; }
}

/// <summary>
/// 予測レスポンス（APIが返すデータ）
/// </summary>
public class PredictionResponse
{
    /// <summary>予測成功かどうか</summary>
    public bool Success { get; set; }
    
    /// <summary>予測結果のリスト</summary>
    public List<Prediction> Predictions { get; set; } = new();
    
    /// <summary>エラーメッセージ（あれば）</summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>処理時間（ミリ秒）</summary>
    public long ProcessingTimeMs { get; set; }
}
