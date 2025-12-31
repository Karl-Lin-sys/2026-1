// =============================================================================
// EnergyPredictionService.cs - AI予測サービス
// 電力消費量を予測する賢いロボット
// =============================================================================

using Microsoft.EntityFrameworkCore;
using SmartBuildingDashboard.Data;
using SmartBuildingDashboard.Models;

namespace SmartBuildingDashboard.Services;

/// <summary>
/// エネルギー予測サービス
/// 例え話：過去のデータを見て、未来を予測する賢いロボット
/// 
/// このサービスは以下のことができます：
/// 1. 過去のデータから明日の電力使用量を予測
/// 2. 予測の精度を計算
/// 3. 省エネのアドバイスを提案
/// </summary>
public class EnergyPredictionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EnergyPredictionService> _logger;
    
    public EnergyPredictionService(AppDbContext context, ILogger<EnergyPredictionService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// 電力消費量を予測
    /// 
    /// 予測の仕組み（子供向け説明）：
    /// 「先週の月曜日はこれくらい電気使ったから、
    ///   今度の月曜日もたぶんこれくらい使うよね！」
    /// という考え方で予測します。
    /// </summary>
    public async Task<List<Prediction>> PredictAsync(PredictionRequest request)
    {
        var predictions = new List<Prediction>();
        var now = DateTime.Now;
        
        // 過去データを取得（予測の材料）
        var historicalData = await GetHistoricalDataAsync(request.SensorId);
        
        for (int day = 1; day <= request.DaysAhead; day++)
        {
            var targetDate = now.AddDays(day).Date;
            
            // 同じ曜日の過去データから予測
            var predictedConsumption = PredictDailyConsumption(
                historicalData, 
                targetDate.DayOfWeek
            );
            
            // 予測の信頼度を計算（データが多いほど信頼度が高い）
            var confidence = CalculateConfidence(historicalData, targetDate.DayOfWeek);
            
            // 予測の上限・下限を計算
            var (lower, upper) = CalculateBounds(predictedConsumption, confidence);
            
            predictions.Add(new Prediction
            {
                TargetDate = targetDate,
                PredictedAt = now,
                PredictedConsumption = predictedConsumption,
                Confidence = confidence,
                LowerBound = lower,
                UpperBound = upper,
                ModelVersion = "v1.0-SimpleAverage"
            });
        }
        
        return predictions;
    }
    
    /// <summary>
    /// 過去の電力データを取得
    /// </summary>
    private async Task<List<DailyAggregate>> GetHistoricalDataAsync(int? sensorId)
    {
        var query = _context.EnergyReadings.AsQueryable();
        
        if (sensorId.HasValue)
        {
            query = query.Where(e => e.SensorId == sensorId.Value);
        }
        
        var fourWeeksAgo = DateTime.Now.AddDays(-28);
        
        var dailyData = await query
            .Where(e => e.Timestamp >= fourWeeksAgo)
            .GroupBy(e => e.Timestamp.Date)
            .Select(g => new DailyAggregate
            {
                Date = g.Key,
                DayOfWeek = g.Key.DayOfWeek,
                TotalConsumption = g.Sum(e => e.Consumption),
                ReadingCount = g.Count()
            })
            .ToListAsync();
        
        return dailyData;
    }
    
    /// <summary>
    /// 日別消費量を予測
    /// 
    /// アルゴリズム（子供向け説明）：
    /// 「過去4週間の同じ曜日の平均を計算するよ！
    ///   例：月曜日の予測 = 過去4回の月曜日の平均」
    /// </summary>
    private double PredictDailyConsumption(
        List<DailyAggregate> historicalData, 
        DayOfWeek targetDay)
    {
        var sameDayData = historicalData
            .Where(d => d.DayOfWeek == targetDay)
            .ToList();
        
        if (!sameDayData.Any())
        {
            // データがなければ全体平均を返す
            return historicalData.Any() 
                ? historicalData.Average(d => d.TotalConsumption)
                : 100; // デフォルト値
        }
        
        // 加重平均（最近のデータを重視）
        var weightedSum = 0.0;
        var weightSum = 0.0;
        var orderedData = sameDayData.OrderByDescending(d => d.Date).ToList();
        
        for (int i = 0; i < orderedData.Count; i++)
        {
            var weight = 1.0 / (i + 1); // 最新が一番重い
            weightedSum += orderedData[i].TotalConsumption * weight;
            weightSum += weight;
        }
        
        return weightedSum / weightSum;
    }
    
    /// <summary>
    /// 予測の信頼度を計算
    /// </summary>
    private double CalculateConfidence(
        List<DailyAggregate> historicalData, 
        DayOfWeek targetDay)
    {
        var sameDayData = historicalData
            .Where(d => d.DayOfWeek == targetDay)
            .ToList();
        
        if (sameDayData.Count < 2) return 50; // データ不足
        
        // 標準偏差が小さい = データの一貫性が高い = 信頼度が高い
        var mean = sameDayData.Average(d => d.TotalConsumption);
        var variance = sameDayData.Average(d => Math.Pow(d.TotalConsumption - mean, 2));
        var stdDev = Math.Sqrt(variance);
        
        // 変動係数から信頼度を計算（変動が小さいほど信頼度が高い）
        var cv = stdDev / mean;
        var confidence = Math.Max(50, 100 - cv * 100);
        
        return Math.Min(95, confidence);
    }
    
    /// <summary>
    /// 予測の上限・下限を計算
    /// </summary>
    private (double lower, double upper) CalculateBounds(
        double predicted, 
        double confidence)
    {
        // 信頼度に応じて範囲を調整
        var margin = predicted * (100 - confidence) / 100;
        return (predicted - margin, predicted + margin);
    }
    
    /// <summary>
    /// 予測精度を計算
    /// </summary>
    public async Task<Dictionary<string, double>> CalculateAccuracyAsync()
    {
        // 実際はここで過去の予測と実績を比較します
        // デモ用にシミュレート値を返却
        return new Dictionary<string, double>
        {
            ["mape"] = 8.5,  // Mean Absolute Percentage Error
            ["rmse"] = 12.3, // Root Mean Square Error
            ["mae"] = 10.1   // Mean Absolute Error
        };
    }
    
    /// <summary>
    /// 省エネ推奨アクションを取得
    /// 
    /// 子供向け説明：
    /// 「こうすれば電気を節約できるよ！」というアドバイス
    /// </summary>
    public async Task<List<Recommendation>> GetRecommendationsAsync()
    {
        // 実際は使用パターン分析から推奨を生成
        // デモ用にサンプル推奨を返却
        return new List<Recommendation>
        {
            new Recommendation
            {
                Priority = 1,
                Category = "lighting",
                Title = "照明の自動消灯",
                Description = "22時以降、人がいないエリアの照明を自動消灯することで、約15%の電力削減が期待できます。",
                PotentialSavingsKwh = 45.5,
                PotentialSavingsPercent = 15
            },
            new Recommendation
            {
                Priority = 2,
                Category = "hvac",
                Title = "空調の最適化",
                Description = "オフィスアワー外の空調設定温度を2度調整することで、約10%の電力削減が期待できます。",
                PotentialSavingsKwh = 30.2,
                PotentialSavingsPercent = 10
            },
            new Recommendation
            {
                Priority = 3,
                Category = "equipment",
                Title = "スタンバイ電力の削減",
                Description = "未使用機器の電源を完全にオフにすることで、約5%の電力削減が期待できます。",
                PotentialSavingsKwh = 15.8,
                PotentialSavingsPercent = 5
            }
        };
    }
}

/// <summary>
/// 日別集計データ
/// </summary>
public class DailyAggregate
{
    public DateTime Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public double TotalConsumption { get; set; }
    public int ReadingCount { get; set; }
}

/// <summary>
/// 省エネ推奨アクション
/// </summary>
public class Recommendation
{
    public int Priority { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double PotentialSavingsKwh { get; set; }
    public double PotentialSavingsPercent { get; set; }
}
