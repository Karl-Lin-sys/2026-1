# 🏢 智慧建築能源管理ダッシュボード
# Smart Building Energy Management Dashboard

![Build](https://img.shields.io/badge/Build-Passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/License-MIT-blue)

---

## 🎯 これは何？（What is this?）

**お家やビルの電気を賢く管理するシステムです！**

想像してみてください：
- 🌡️ **センサー** = お家に付いている「温度計」や「電気メーター」
- 📊 **ダッシュボード** = すべてのデータが見える「魔法の画面」
- 🤖 **AI** = 「明日は電気をたくさん使いそう！」と教えてくれる賢いロボット

このシステムは、ビルのセンサーからデータを集めて、画面に見やすく表示し、AIで将来の電気使用量を予測します。

---

## 🧩 システム構成（System Architecture）

```
┌─────────────────────────────────────────────────────────────┐
│                    👨‍💼 ユーザー (User)                        │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  🖥️ Webダッシュボード                        │
│              (HTML + CSS + JavaScript)                      │
│    📈 リアルタイムグラフ  |  🔔 アラート  |  🎯 予測表示      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  ⚙️ バックエンドAPI                          │
│                (ASP.NET Core Web API)                       │
│   /api/energy   |   /api/sensors   |   /api/predict        │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  🗄️ データベース │ │  🤖 AI予測モデル │ │  📡 IoTセンサー  │
│    (SQL Server) │ │   (ML.NET/Py)   │ │   (シミュレータ) │
└─────────────────┘ └─────────────────┘ └─────────────────┘
```

---

## 🛠️ 使用技術（Technologies Used）

| カテゴリ | 技術 | なぜ使うの？ |
|---------|------|-------------|
| **バックエンド** | C# / ASP.NET Core | APIを作る「お店の裏側」 |
| **フロントエンド** | JavaScript / HTML / CSS | 画面を作る「お店の表側」 |
| **データベース** | SQL Server / SQLite | データを保存する「本棚」 |
| **AI/ML** | Python / scikit-learn | 将来を予測する「水晶玉」 |
| **IoT** | センサーシミュレータ | データを送る「温度計」 |

---

## 📁 プロジェクト構成（Project Structure）

```
smart_building_dashboard/
│
├── 📂 backend/                    # バックエンド（お店の裏側）
│   ├── Controllers/               # APIの入り口
│   │   ├── EnergyController.cs    # 電力データAPI
│   │   ├── SensorController.cs    # センサーAPI
│   │   └── PredictionController.cs# AI予測API
│   ├── Models/                    # データの形
│   │   ├── EnergyReading.cs       # 電力データ
│   │   ├── Sensor.cs              # センサー情報
│   │   └── Prediction.cs          # 予測結果
│   ├── Services/                  # 仕事をする部分
│   │   └── EnergyPredictionService.cs
│   └── Program.cs                 # プログラムの開始点
│
├── 📂 frontend/                   # フロントエンド（お店の表側）
│   ├── index.html                 # メインページ
│   ├── css/
│   │   └── style.css              # 見た目を決める
│   └── js/
│       ├── main.js                # メインロジック
│       ├── charts.js              # グラフ描画
│       └── api.js                 # APIとお話する
│
├── 📂 ai_module/                  # AI（賢いロボット）
│   ├── train_model.py             # AIを教育する
│   ├── predict.py                 # 予測を実行する
│   └── model/                     # 学習済みモデル
│
├── 📂 iot_simulator/              # IoTシミュレータ
│   └── sensor_simulator.py        # センサーのふりをする
│
└── README.md                      # この説明書
```

---

## 🚀 始め方（Quick Start）

### 1️⃣ ダウンロード
```bash
git clone https://github.com/I-PingLin/2026-1.git
cd smart_building_dashboard
```

### 2️⃣ バックエンドを起動
```bash
cd backend
dotnet restore
dotnet run
# → http://localhost:5000 でAPI起動！
```

### 3️⃣ フロントエンドを開く
```bash
# 別のターミナルで
cd frontend
# ブラウザで index.html を開く
# または: python -m http.server 8080
# → http://localhost:8080 でダッシュボード表示！
```

### 4️⃣ IoTシミュレータを起動（オプション）
```bash
cd iot_simulator
python sensor_simulator.py
# → センサーデータが自動送信される！
```

---

## 📊 API一覧（API Endpoints）

| メソッド | パス | 説明 | 子供向け説明 |
|---------|------|------|-------------|
| GET | `/api/energy` | 電力データ取得 | 「今どれくらい電気使ってる？」 |
| GET | `/api/energy/{date}` | 日別データ | 「昨日はどうだった？」 |
| GET | `/api/sensors` | センサー一覧 | 「温度計はどこにある？」 |
| GET | `/api/sensors/{id}/readings` | センサーデータ | 「この温度計は何度？」 |
| GET | `/api/predict` | AI予測 | 「明日は電気いっぱい使う？」 |
| POST | `/api/sensors/data` | データ送信 | 「新しいデータを教えて！」 |

---

## 🧪 機能デモ（Feature Demo）

### 1. リアルタイムエネルギー監視
```javascript
// frontend/js/charts.js より
// 電気の使用量をリアルタイムでグラフに表示

fetch('/api/energy')
  .then(response => response.json())
  .then(data => {
    // グラフを更新！
    updateChart(data);
  });
```

**子供向け説明**: 電気メーターの数字を、きれいなグラフにして見せてくれるよ！

### 2. AI予測機能
```csharp
// backend/Services/EnergyPredictionService.cs より
public class EnergyPredictionService
{
    public PredictionResult PredictNextDayConsumption(
        List<EnergyReading> historicalData)
    {
        // 過去のデータから明日の電気使用量を予測
        var prediction = _mlModel.Predict(historicalData);
        return prediction;
    }
}
```

**子供向け説明**: 過去のデータを見て、「明日はきっとこれくらい電気使うよ！」と教えてくれるロボット！

### 3. センサーアラート
```javascript
// frontend/js/main.js より
function checkAlerts(sensorData) {
    if (sensorData.temperature > 30) {
        showAlert('⚠️ 温度が高すぎます！');
    }
    if (sensorData.energyUsage > threshold) {
        showAlert('⚡ 電気使いすぎ注意！');
    }
}
```

**子供向け説明**: 「暑すぎるよ！」「電気使いすぎだよ！」と教えてくれる番犬みたいな機能！

---

## 💡 このプロジェクトで示すスキル

| 求人要件 | 本プロジェクトでの実装 |
|---------|----------------------|
| **C# / ASP.NET** | バックエンドAPI全体 |
| **JavaScript** | フロントエンドダッシュボード |
| **SQL** | Entity FrameworkでDBアクセス |
| **AI** | エネルギー予測モデル |
| **IoT** | センサーシミュレータ・データ統合 |
| **スマートビル** | 全体コンセプト |

---

## 🏆 五百戶科技（500House）との関連性

このプロジェクトは、五百戶科技の事業領域に直接関連しています：

| 会社の事業 | 本プロジェクト |
|-----------|---------------|
| AI | ✅ エネルギー予測AI |
| IoT | ✅ センサー統合 |
| スマートビル | ✅ ビル管理ダッシュボード |
| エネルギー管理 | ✅ 電力監視・最適化 |
| ESG | ✅ 省エネ支援機能 |

---

## 📜 ライセンス

MIT License - 自由に使ってOK！

---

## 👤 作者

**I-Ping Lin**
- 応募職位: RD/AI エンジニア
- 作成日: 2025年12月

---

*このプロジェクトは五百戶科技株式会社のRD/AIエンジニア職への応募用ポートフォリオです*
