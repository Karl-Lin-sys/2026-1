/**
 * ==========================================================================
 * グラフ描画モジュール
 * Chart.jsを使ってきれいなグラフを描く（画家さん）
 * ==========================================================================
 */

// Chart.jsのデフォルト設定
Chart.defaults.color = '#a0a0a0';
Chart.defaults.borderColor = 'rgba(255, 255, 255, 0.1)';

/**
 * グラフモジュール
 * 子供向け説明：数字をきれいな絵（グラフ）にしてくれるお絵かき係
 */
const Charts = {
    // グラフのインスタンスを保持
    instances: {},

    /**
     * 共通のグラフオプション
     */
    getCommonOptions() {
        return {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    labels: {
                        color: '#eaeaea',
                        font: { size: 12 }
                    }
                }
            },
            scales: {
                x: {
                    grid: { color: 'rgba(255, 255, 255, 0.05)' },
                    ticks: { color: '#a0a0a0' }
                },
                y: {
                    grid: { color: 'rgba(255, 255, 255, 0.05)' },
                    ticks: { color: '#a0a0a0' }
                }
            }
        };
    },

    /**
     * 時間別電力グラフを描画
     * 子供向け説明：「何時にどれくらい電気使ったか」を棒グラフで見せる
     */
    drawHourlyChart(canvasId, data) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        // 既存のグラフがあれば破棄
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        const hours = data.data.map(d => `${d.hour}:00`);
        const consumption = data.data.map(d => d.totalConsumption);

        // グラデーション
        const gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, 'rgba(0, 217, 255, 0.8)');
        gradient.addColorStop(1, 'rgba(0, 217, 255, 0.1)');

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: hours,
                datasets: [{
                    label: '電力消費量 (kWh)',
                    data: consumption,
                    backgroundColor: gradient,
                    borderColor: '#00d9ff',
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                ...this.getCommonOptions(),
                plugins: {
                    ...this.getCommonOptions().plugins,
                    tooltip: {
                        callbacks: {
                            label: (context) => `${context.parsed.y.toFixed(2)} kWh`
                        }
                    }
                }
            }
        });
    },

    /**
     * 日別電力グラフを描画
     * 子供向け説明：「この一週間、毎日どれくらい電気使った？」を線グラフで見せる
     */
    drawDailyChart(canvasId, data) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        const dates = data.map(d => {
            const date = new Date(d.date);
            return `${date.getMonth() + 1}/${date.getDate()}`;
        });
        const consumption = data.map(d => d.totalConsumption);

        // グラデーション（塗りつぶし用）
        const gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, 'rgba(78, 205, 196, 0.5)');
        gradient.addColorStop(1, 'rgba(78, 205, 196, 0.0)');

        this.instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: [{
                    label: '日別消費量 (kWh)',
                    data: consumption,
                    borderColor: '#4ecdc4',
                    backgroundColor: gradient,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#4ecdc4',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5
                }]
            },
            options: {
                ...this.getCommonOptions(),
                plugins: {
                    ...this.getCommonOptions().plugins,
                    tooltip: {
                        callbacks: {
                            label: (context) => `${context.parsed.y.toFixed(2)} kWh`
                        }
                    }
                }
            }
        });
    },

    /**
     * 週間予測グラフを描画
     * 子供向け説明：「来週の電気使用量の予測」を見せる
     */
    drawWeeklyPredictionChart(canvasId, data) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        const dates = data.dailyPredictions.map(d => {
            const date = new Date(d.date);
            return `${date.getMonth() + 1}/${date.getDate()} (${d.dayOfWeek.substring(0, 3)})`;
        });
        const predicted = data.dailyPredictions.map(d => d.predictedConsumption);
        const confidence = data.dailyPredictions.map(d => d.confidence);

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: dates,
                datasets: [
                    {
                        label: '予測消費量 (kWh)',
                        data: predicted,
                        backgroundColor: 'rgba(102, 126, 234, 0.8)',
                        borderColor: '#667eea',
                        borderWidth: 1,
                        borderRadius: 4,
                        yAxisID: 'y'
                    },
                    {
                        label: '信頼度 (%)',
                        data: confidence,
                        type: 'line',
                        borderColor: '#ffd93d',
                        backgroundColor: 'rgba(255, 217, 61, 0.1)',
                        fill: false,
                        tension: 0.4,
                        pointRadius: 4,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                ...this.getCommonOptions(),
                scales: {
                    ...this.getCommonOptions().scales,
                    y: {
                        ...this.getCommonOptions().scales.y,
                        position: 'left',
                        title: {
                            display: true,
                            text: '消費量 (kWh)',
                            color: '#a0a0a0'
                        }
                    },
                    y1: {
                        position: 'right',
                        grid: { drawOnChartArea: false },
                        min: 0,
                        max: 100,
                        title: {
                            display: true,
                            text: '信頼度 (%)',
                            color: '#a0a0a0'
                        },
                        ticks: { color: '#a0a0a0' }
                    }
                }
            }
        });
    },

    /**
     * リアルタイム電力グラフ（ライブ更新用）
     */
    createRealtimeChart(canvasId, initialData = []) {
        const ctx = document.getElementById(canvasId).getContext('2d');

        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
        }

        this.instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: initialData.map((_, i) => i),
                datasets: [{
                    label: 'リアルタイム電力 (W)',
                    data: initialData,
                    borderColor: '#ff6b6b',
                    backgroundColor: 'rgba(255, 107, 107, 0.1)',
                    fill: true,
                    tension: 0.2,
                    pointRadius: 0
                }]
            },
            options: {
                ...this.getCommonOptions(),
                animation: { duration: 0 },
                scales: {
                    x: {
                        display: false
                    },
                    y: {
                        ...this.getCommonOptions().scales.y,
                        beginAtZero: true
                    }
                }
            }
        });

        return this.instances[canvasId];
    },

    /**
     * グラフにデータを追加（リアルタイム更新用）
     */
    addDataPoint(canvasId, label, value, maxPoints = 50) {
        const chart = this.instances[canvasId];
        if (!chart) return;

        chart.data.labels.push(label);
        chart.data.datasets[0].data.push(value);

        // 最大ポイント数を超えたら古いデータを削除
        if (chart.data.labels.length > maxPoints) {
            chart.data.labels.shift();
            chart.data.datasets[0].data.shift();
        }

        chart.update('none'); // アニメーションなしで更新
    }
};

// グローバルに公開
window.Charts = Charts;
