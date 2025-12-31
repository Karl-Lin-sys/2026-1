/**
 * ==========================================================================
 * メインロジック
 * ダッシュボード全体を管理する（指揮者）
 * ==========================================================================
 */

/**
 * アプリケーションメインクラス
 * 子供向け説明：オーケストラの指揮者みたいに、全員をまとめる人
 */
class Dashboard {
    constructor() {
        this.refreshInterval = 30000; // 30秒ごとに更新
        this.intervalId = null;
    }

    /**
     * ダッシュボードを初期化
     */
    async init() {
        console.log('🚀 ダッシュボードを初期化中...');

        // 時計を開始
        this.startClock();

        // 初回データ読み込み
        await this.loadAllData();

        // 定期更新を開始
        this.startAutoRefresh();

        console.log('✅ ダッシュボード初期化完了！');
    }

    /**
     * 全データを読み込み
     */
    async loadAllData() {
        try {
            await Promise.all([
                this.loadEnergyStats(),
                this.loadHourlyChart(),
                this.loadDailyChart(),
                this.loadSensorStatus(),
                this.loadPrediction(),
                this.loadWeeklyPrediction(),
                this.loadRecommendations()
            ]);
        } catch (error) {
            console.error('❌ データ読み込みエラー:', error);
        }
    }

    /**
     * 電力統計を読み込み
     */
    async loadEnergyStats() {
        const stats = await API.getEnergyStats();

        // 今日の消費量
        document.getElementById('todayConsumption').textContent =
            `${stats.todayConsumption.toFixed(1)} kWh`;

        // 昨日との比較
        const changeEl = document.getElementById('todayChange');
        const changeValue = stats.changeFromYesterday;
        if (changeValue > 0) {
            changeEl.textContent = `↑ +${changeValue.toFixed(1)}% vs 昨日`;
            changeEl.className = 'stat-change negative';
        } else {
            changeEl.textContent = `↓ ${changeValue.toFixed(1)}% vs 昨日`;
            changeEl.className = 'stat-change positive';
        }

        // 週間平均
        document.getElementById('weeklyAverage').textContent =
            `${stats.weeklyAverageDaily.toFixed(1)} kWh`;
    }

    /**
     * 時間別グラフを読み込み
     */
    async loadHourlyChart() {
        const data = await API.getHourlyEnergy();
        Charts.drawHourlyChart('hourlyChart', data);
    }

    /**
     * 日別グラフを読み込み
     */
    async loadDailyChart() {
        const data = await API.getDailyEnergy(7);
        Charts.drawDailyChart('dailyChart', data);
    }

    /**
     * センサー状態を読み込み
     */
    async loadSensorStatus() {
        const status = await API.getSensorStatus();

        // 統計カード更新
        document.getElementById('activeSensors').textContent =
            `${status.activeSensors}台`;
        document.getElementById('sensorStatus').textContent =
            `オンライン: ${status.onlineSensors} / オフライン: ${status.offlineSensors}`;

        // センサーグリッド更新
        const grid = document.getElementById('sensorGrid');
        grid.innerHTML = '';

        const sensorTypes = ['Energy', 'Temperature', 'Humidity', 'Light', 'Occupancy', 'AirQuality'];

        status.sensors.forEach(sensor => {
            const card = document.createElement('div');
            card.className = `sensor-card ${sensor.isOnline ? '' : 'offline'}`;
            card.innerHTML = `
                <h4>${sensor.name}</h4>
                <div class="sensor-type">${sensorTypes[sensor.type] || 'Unknown'}</div>
                <div class="sensor-location">📍 ${sensor.location}</div>
                <span class="sensor-status ${sensor.isOnline ? 'online' : 'offline'}">
                    ${sensor.isOnline ? '🟢 オンライン' : '🔴 オフライン'}
                </span>
            `;
            grid.appendChild(card);
        });
    }

    /**
     * AI予測を読み込み
     */
    async loadPrediction() {
        const prediction = await API.getPrediction(1);

        if (prediction.success && prediction.predictions.length > 0) {
            const p = prediction.predictions[0];
            document.getElementById('prediction').textContent =
                `${p.predictedConsumption.toFixed(1)} kWh`;
            document.getElementById('predictionConfidence').textContent =
                `信頼度: ${p.confidence.toFixed(0)}%`;
        }
    }

    /**
     * 週間予測グラフを読み込み
     */
    async loadWeeklyPrediction() {
        const data = await API.getWeeklyPrediction();
        Charts.drawWeeklyPredictionChart('weeklyPredictionChart', data);
    }

    /**
     * 省エネ推奨を読み込み
     */
    async loadRecommendations() {
        const data = await API.getRecommendations();

        const list = document.getElementById('recommendationsList');
        list.innerHTML = '';

        data.recommendations.forEach(rec => {
            const item = document.createElement('div');
            item.className = 'recommendation-item';
            item.innerHTML = `
                <div class="priority">${rec.priority}</div>
                <div class="content">
                    <div class="title">${rec.title}</div>
                    <div class="description">${rec.description}</div>
                </div>
                <div class="savings">${rec.potentialSavings}</div>
            `;
            list.appendChild(item);
        });
    }

    /**
     * 時計を開始
     */
    startClock() {
        const updateClock = () => {
            const now = new Date();
            const timeStr = now.toLocaleTimeString('ja-JP', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit'
            });
            const dateStr = now.toLocaleDateString('ja-JP', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                weekday: 'short'
            });
            document.getElementById('currentTime').textContent = `${dateStr} ${timeStr}`;
        };

        updateClock();
        setInterval(updateClock, 1000);
    }

    /**
     * 自動更新を開始
     */
    startAutoRefresh() {
        this.intervalId = setInterval(() => {
            console.log('🔄 データを自動更新中...');
            this.loadAllData();
        }, this.refreshInterval);
    }

    /**
     * 自動更新を停止
     */
    stopAutoRefresh() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
    }
}

// ============ アラートチェッカー ============

/**
 * アラートをチェックして表示
 * 子供向け説明：「危ない！」「注意！」を教えてくれる番犬
 */
function checkAlerts(sensorData) {
    // 温度が高すぎる
    if (sensorData.temperature > 30) {
        showAlert('⚠️ 温度が高すぎます: ' + sensorData.location);
    }

    // 電力使用量が閾値を超えた
    if (sensorData.energyUsage > 80) {
        showAlert('⚡ 電力使用量が高いです: ' + sensorData.location);
    }
}

/**
 * アラートを表示
 */
function showAlert(message) {
    // 実際のアプリではトースト通知やモーダルを使用
    console.warn('🚨 ALERT:', message);
}

// ============ 初期化 ============

// DOMが読み込まれたらダッシュボードを開始
document.addEventListener('DOMContentLoaded', () => {
    const dashboard = new Dashboard();
    dashboard.init();

    // グローバルに公開（デバッグ用）
    window.dashboard = dashboard;
});
