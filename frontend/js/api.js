/**
 * ==========================================================================
 * API通信モジュール
 * バックエンドAPIとお話する係（データを取りに行く郵便屋さん）
 * ==========================================================================
 */

// APIのベースURL（開発用）
const API_BASE_URL = 'http://localhost:5000/api';

/**
 * APIモジュール
 * 子供向け説明：お店に買い物に行く係。必要なデータを取ってきてくれる
 */
const API = {
    /**
     * 電力統計を取得
     * 子供向け説明：「今日どれくらい電気使った？」を聞きに行く
     */
    async getEnergyStats() {
        try {
            const response = await fetch(`${API_BASE_URL}/energy/stats`);
            if (!response.ok) throw new Error('Failed to fetch energy stats');
            return await response.json();
        } catch (error) {
            console.error('❌ 電力統計の取得に失敗:', error);
            // デモ用のダミーデータを返す
            return this._getDemoEnergyStats();
        }
    },

    /**
     * 時間別電力データを取得
     */
    async getHourlyEnergy(date = null) {
        try {
            const url = date
                ? `${API_BASE_URL}/energy/hourly?date=${date}`
                : `${API_BASE_URL}/energy/hourly`;
            const response = await fetch(url);
            if (!response.ok) throw new Error('Failed to fetch hourly data');
            return await response.json();
        } catch (error) {
            console.error('❌ 時間別データの取得に失敗:', error);
            return this._getDemoHourlyData();
        }
    },

    /**
     * 日別電力データを取得
     */
    async getDailyEnergy(days = 7) {
        try {
            const response = await fetch(`${API_BASE_URL}/energy/daily?days=${days}`);
            if (!response.ok) throw new Error('Failed to fetch daily data');
            return await response.json();
        } catch (error) {
            console.error('❌ 日別データの取得に失敗:', error);
            return this._getDemoDailyData();
        }
    },

    /**
     * センサー状態を取得
     * 子供向け説明：「温度計とか電気メーター、ちゃんと動いてる？」を確認
     */
    async getSensorStatus() {
        try {
            const response = await fetch(`${API_BASE_URL}/sensor/status`);
            if (!response.ok) throw new Error('Failed to fetch sensor status');
            return await response.json();
        } catch (error) {
            console.error('❌ センサー状態の取得に失敗:', error);
            return this._getDemoSensorStatus();
        }
    },

    /**
     * AI予測を取得
     * 子供向け説明：「明日は電気どれくらい使うかな？」とAIに聞く
     */
    async getPrediction(daysAhead = 1) {
        try {
            const response = await fetch(`${API_BASE_URL}/prediction?daysAhead=${daysAhead}`);
            if (!response.ok) throw new Error('Failed to fetch prediction');
            return await response.json();
        } catch (error) {
            console.error('❌ AI予測の取得に失敗:', error);
            return this._getDemoPrediction(daysAhead);
        }
    },

    /**
     * 週間予測を取得
     */
    async getWeeklyPrediction() {
        try {
            const response = await fetch(`${API_BASE_URL}/prediction/weekly`);
            if (!response.ok) throw new Error('Failed to fetch weekly prediction');
            return await response.json();
        } catch (error) {
            console.error('❌ 週間予測の取得に失敗:', error);
            return this._getDemoWeeklyPrediction();
        }
    },

    /**
     * 省エネ推奨を取得
     * 子供向け説明：「どうすれば電気を節約できる？」のアドバイス
     */
    async getRecommendations() {
        try {
            const response = await fetch(`${API_BASE_URL}/prediction/recommendations`);
            if (!response.ok) throw new Error('Failed to fetch recommendations');
            return await response.json();
        } catch (error) {
            console.error('❌ 省エネ推奨の取得に失敗:', error);
            return this._getDemoRecommendations();
        }
    },

    // ============ デモ用ダミーデータ ============
    // バックエンドが動いていなくてもフロントエンドをテストできるようにする

    _getDemoEnergyStats() {
        return {
            todayConsumption: 245.8,
            yesterdayConsumption: 232.5,
            weeklyAverageDaily: 238.4,
            changeFromYesterday: 5.7,
            unit: 'kWh',
            generatedAt: new Date().toISOString()
        };
    },

    _getDemoHourlyData() {
        const data = [];
        for (let hour = 0; hour < 24; hour++) {
            // 昼間は電力使用量が多い
            const baseConsumption = (hour >= 9 && hour <= 18) ? 15 : 5;
            data.push({
                hour: hour,
                totalConsumption: baseConsumption + Math.random() * 10,
                averagePower: (baseConsumption + Math.random() * 10) * 1000 / 60
            });
        }
        return {
            date: new Date().toISOString().split('T')[0],
            data: data
        };
    },

    _getDemoDailyData() {
        const data = [];
        for (let day = 6; day >= 0; day--) {
            const date = new Date();
            date.setDate(date.getDate() - day);
            data.push({
                date: date.toISOString().split('T')[0],
                totalConsumption: 200 + Math.random() * 100,
                averagePower: (200 + Math.random() * 100) * 1000 / 24 / 60,
                readingCount: 24
            });
        }
        return data;
    },

    _getDemoSensorStatus() {
        return {
            totalSensors: 5,
            activeSensors: 5,
            onlineSensors: 4,
            offlineSensors: 1,
            byType: [
                { type: 'Energy', count: 3 },
                { type: 'Temperature', count: 2 }
            ],
            sensors: [
                { id: 1, name: '1階ロビー電力計', type: 0, location: '1階ロビー', isActive: true, isOnline: true },
                { id: 2, name: '2階オフィス電力計', type: 0, location: '2階オフィス', isActive: true, isOnline: true },
                { id: 3, name: '屋上太陽光パネル', type: 0, location: '屋上', isActive: true, isOnline: true },
                { id: 4, name: '1階温度センサー', type: 1, location: '1階ロビー', isActive: true, isOnline: true },
                { id: 5, name: 'サーバールーム温度', type: 1, location: '地下1階', isActive: true, isOnline: false }
            ]
        };
    },

    _getDemoPrediction(daysAhead) {
        const predictions = [];
        for (let day = 1; day <= daysAhead; day++) {
            const date = new Date();
            date.setDate(date.getDate() + day);
            const predicted = 220 + Math.random() * 60;
            const confidence = 75 + Math.random() * 20;
            predictions.push({
                targetDate: date.toISOString(),
                predictedConsumption: predicted,
                confidence: confidence,
                lowerBound: predicted * 0.85,
                upperBound: predicted * 1.15
            });
        }
        return {
            success: true,
            predictions: predictions,
            processingTimeMs: 15
        };
    },

    _getDemoWeeklyPrediction() {
        const predictions = this._getDemoPrediction(7).predictions;
        return {
            summary: {
                totalPredictedConsumption: predictions.reduce((sum, p) => sum + p.predictedConsumption, 0),
                averageDailyConsumption: predictions.reduce((sum, p) => sum + p.predictedConsumption, 0) / 7,
                averageConfidence: predictions.reduce((sum, p) => sum + p.confidence, 0) / 7
            },
            dailyPredictions: predictions.map(p => ({
                date: p.targetDate.split('T')[0],
                dayOfWeek: new Date(p.targetDate).toLocaleDateString('ja-JP', { weekday: 'long' }),
                predictedConsumption: Math.round(p.predictedConsumption * 100) / 100,
                confidence: Math.round(p.confidence * 10) / 10,
                range: `${Math.round(p.lowerBound)} - ${Math.round(p.upperBound)} kWh`
            }))
        };
    },

    _getDemoRecommendations() {
        return {
            generatedAt: new Date().toISOString(),
            recommendations: [
                {
                    priority: 1,
                    category: 'lighting',
                    title: '照明の自動消灯',
                    description: '22時以降、人がいないエリアの照明を自動消灯することで、約15%の電力削減が期待できます。',
                    potentialSavings: '45.5 kWh (15%)'
                },
                {
                    priority: 2,
                    category: 'hvac',
                    title: '空調の最適化',
                    description: 'オフィスアワー外の空調設定温度を2度調整することで、約10%の電力削減が期待できます。',
                    potentialSavings: '30.2 kWh (10%)'
                },
                {
                    priority: 3,
                    category: 'equipment',
                    title: 'スタンバイ電力の削減',
                    description: '未使用機器の電源を完全にオフにすることで、約5%の電力削減が期待できます。',
                    potentialSavings: '15.8 kWh (5%)'
                }
            ]
        };
    }
};

// グローバルに公開
window.API = API;
