// =============================================================================
// PredictionController.cs - AI予測API
// AIを使って将来の電力消費量を予測する（水晶玉みたいな機能）
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using SmartBuildingDashboard.Models;
using SmartBuildingDashboard.Services;

namespace SmartBuildingDashboard.Controllers;

/// <summary>
/// 予測コントローラー
/// 例え話：未来を予測する魔法使い。「明日は電気いっぱい使いそう！」と教えてくれる
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly EnergyPredictionService _predictionService;
    private readonly ILogger<PredictionController> _logger;
    
    public PredictionController(
        EnergyPredictionService predictionService,
        ILogger<PredictionController> logger)
    {
        _predictionService = predictionService;
        _logger = logger;
    }
    
    /// <summary>
    /// 電力消費量予測を取得
    /// GET /api/prediction
    /// 子供向け説明：「明日は電気どれくらい使うかな？」に答える
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PredictionResponse>> GetPrediction(
        [FromQuery] int daysAhead = 1,
        [FromQuery] int? sensorId = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var request = new PredictionRequest
            {
                DaysAhead = daysAhead,
                SensorId = sensorId
            };
            
            var predictions = await _predictionService.PredictAsync(request);
            
            stopwatch.Stop();
            
            _logger.LogInformation($"🔮 {daysAhead}日先の予測を実行: {predictions.Count}件");
            
            return Ok(new PredictionResponse
            {
                Success = true,
                Predictions = predictions,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予測処理でエラーが発生");
            
            return Ok(new PredictionResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            });
        }
    }
    
    /// <summary>
    /// 週間予測を取得
    /// GET /api/prediction/weekly
    /// 子供向け説明：「来週は毎日どれくらい電気使う？」
    /// </summary>
    [HttpGet("weekly")]
    public async Task<ActionResult<object>> GetWeeklyPrediction()
    {
        var request = new PredictionRequest { DaysAhead = 7 };
        var predictions = await _predictionService.PredictAsync(request);
        
        return Ok(new
        {
            Summary = new
            {
                TotalPredictedConsumption = predictions.Sum(p => p.PredictedConsumption),
                AverageDailyConsumption = predictions.Average(p => p.PredictedConsumption),
                AverageConfidence = predictions.Average(p => p.Confidence)
            },
            DailyPredictions = predictions.Select(p => new
            {
                Date = p.TargetDate.ToString("yyyy-MM-dd"),
                DayOfWeek = p.TargetDate.DayOfWeek.ToString(),
                PredictedConsumption = Math.Round(p.PredictedConsumption, 2),
                Confidence = Math.Round(p.Confidence, 1),
                Range = $"{Math.Round(p.LowerBound, 2)} - {Math.Round(p.UpperBound, 2)} kWh"
            })
        });
    }
    
    /// <summary>
    /// 予測精度情報を取得
    /// GET /api/prediction/accuracy
    /// 子供向け説明：「この予測、どれくらい当たる？」
    /// </summary>
    [HttpGet("accuracy")]
    public async Task<ActionResult<object>> GetAccuracyMetrics()
    {
        var metrics = await _predictionService.CalculateAccuracyAsync();
        
        return Ok(new
        {
            ModelVersion = "v1.0",
            LastTrainedAt = DateTime.Now.AddDays(-1),
            Metrics = new
            {
                MAPE = Math.Round(metrics["mape"], 2), // Mean Absolute Percentage Error
                RMSE = Math.Round(metrics["rmse"], 2), // Root Mean Square Error
                Accuracy = Math.Round(100 - metrics["mape"], 1)
            },
            Description = new
            {
                MAPE = "平均絶対パーセント誤差 - 予測がどれくらいずれるか（%）",
                RMSE = "二乗平均平方根誤差 - 大きなずれをより重視した誤差",
                Accuracy = "予測精度 - 100に近いほど良い"
            }
        });
    }
    
    /// <summary>
    /// 省エネ推奨アクションを取得
    /// GET /api/prediction/recommendations
    /// 子供向け説明：「どうすれば電気を節約できる？」
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<object>> GetRecommendations()
    {
        var recommendations = await _predictionService.GetRecommendationsAsync();
        
        return Ok(new
        {
            GeneratedAt = DateTime.Now,
            Recommendations = recommendations.Select(r => new
            {
                r.Priority,
                r.Category,
                r.Title,
                r.Description,
                PotentialSavings = $"{r.PotentialSavingsKwh} kWh ({r.PotentialSavingsPercent}%)"
            })
        });
    }
}
