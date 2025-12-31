"""
==========================================================================
AI予測モジュール - train_model.py
電力消費量を予測するAIモデルを訓練する

子供向け説明：
過去のデータを見て、「明日はこれくらい電気使うよ！」と
予測できるようにAIを教育するプログラム

==========================================================================
"""

import numpy as np
import json
from datetime import datetime, timedelta
from pathlib import Path
import pickle

# sklearnがインストールされていない場合に備えたフォールバック
try:
    from sklearn.linear_model import Ridge
    from sklearn.preprocessing import StandardScaler
    from sklearn.metrics import mean_absolute_error, mean_squared_error
    SKLEARN_AVAILABLE = True
except ImportError:
    SKLEARN_AVAILABLE = False
    print("⚠️ scikit-learn がインストールされていません。シンプルモデルを使用します。")


class SimplePredictor:
    """
    シンプルな予測モデル（sklearnがない場合のフォールバック）
    
    子供向け説明：
    「過去4週間の同じ曜日の平均」で予測する、シンプルな方法
    """
    def __init__(self):
        self.daily_averages = {}  # 曜日ごとの平均
    
    def fit(self, X, y, day_of_weeks):
        """モデルを訓練"""
        for i, dow in enumerate(day_of_weeks):
            if dow not in self.daily_averages:
                self.daily_averages[dow] = []
            self.daily_averages[dow].append(y[i])
        
        # 平均を計算
        for dow in self.daily_averages:
            self.daily_averages[dow] = np.mean(self.daily_averages[dow])
        
        return self
    
    def predict(self, X, day_of_weeks):
        """予測を実行"""
        predictions = []
        for dow in day_of_weeks:
            if dow in self.daily_averages:
                predictions.append(self.daily_averages[dow])
            else:
                predictions.append(np.mean(list(self.daily_averages.values())))
        return np.array(predictions)


class EnergyPredictor:
    """
    電力消費量予測モデル
    
    子供向け説明：
    このクラスは「電気使用量を予測するロボット」です。
    1. 過去のデータを見て学習する（fit）
    2. 未来を予測する（predict）
    という2つの仕事ができます。
    """
    
    def __init__(self, model_dir: str = "model"):
        self.model_dir = Path(model_dir)
        self.model_dir.mkdir(exist_ok=True)
        
        if SKLEARN_AVAILABLE:
            self.model = Ridge(alpha=1.0)  # リッジ回帰
            self.scaler = StandardScaler()
        else:
            self.model = SimplePredictor()
            self.scaler = None
        
        self.is_trained = False
        self.metrics = {}
    
    def generate_demo_data(self, days: int = 30):
        """
        デモ用のサンプルデータを生成
        
        子供向け説明：
        本物のデータがないときに、練習用の偽データを作る
        """
        np.random.seed(42)
        
        data = []
        now = datetime.now()
        
        for day in range(days, 0, -1):
            date = now - timedelta(days=day)
            day_of_week = date.weekday()  # 0=月曜, 6=日曜
            
            # 曜日によって基本消費量を変える
            # 平日は多い、週末は少ない
            if day_of_week < 5:  # 平日
                base_consumption = 250 + np.random.normal(0, 20)
            else:  # 週末
                base_consumption = 180 + np.random.normal(0, 15)
            
            # 季節の影響（夏と冬は電気をよく使う）
            month = date.month
            if month in [7, 8, 1, 2]:
                base_consumption *= 1.2
            
            # ランダムな変動を追加
            consumption = base_consumption + np.random.normal(0, 10)
            
            data.append({
                'date': date.strftime('%Y-%m-%d'),
                'day_of_week': day_of_week,
                'month': month,
                'consumption': max(0, consumption)
            })
        
        return data
    
    def prepare_features(self, data: list):
        """
        特徴量を準備
        
        子供向け説明：
        「何曜日か」「何月か」などの情報を、
        AIが理解できる数字の形に変える
        """
        X = []
        y = []
        day_of_weeks = []
        
        for record in data:
            # 特徴量: [曜日(0-6), 月(1-12), 週末フラグ]
            dow = record['day_of_week']
            month = record['month']
            is_weekend = 1 if dow >= 5 else 0
            
            X.append([dow, month, is_weekend])
            y.append(record['consumption'])
            day_of_weeks.append(dow)
        
        return np.array(X), np.array(y), day_of_weeks
    
    def train(self, data: list = None):
        """
        モデルを訓練
        
        子供向け説明：
        過去のデータを見せて、AIに「こういうパターンがあるよ」と教える
        """
        if data is None:
            print("📊 デモデータを生成中...")
            data = self.generate_demo_data(60)  # 60日分
        
        X, y, day_of_weeks = self.prepare_features(data)
        
        print(f"📚 訓練データ: {len(X)}件")
        
        # データを分割（80%訓練、20%テスト）
        split_idx = int(len(X) * 0.8)
        X_train, X_test = X[:split_idx], X[split_idx:]
        y_train, y_test = y[:split_idx], y[split_idx:]
        dow_train = day_of_weeks[:split_idx]
        dow_test = day_of_weeks[split_idx:]
        
        if SKLEARN_AVAILABLE:
            # 特徴量を標準化
            X_train_scaled = self.scaler.fit_transform(X_train)
            X_test_scaled = self.scaler.transform(X_test)
            
            # モデルを訓練
            self.model.fit(X_train_scaled, y_train)
            
            # 予測と評価
            y_pred = self.model.predict(X_test_scaled)
        else:
            self.model.fit(X_train, y_train, dow_train)
            y_pred = self.model.predict(X_test, dow_test)
        
        # 評価指標を計算
        mae = np.mean(np.abs(y_test - y_pred))
        mse = np.mean((y_test - y_pred) ** 2)
        rmse = np.sqrt(mse)
        mape = np.mean(np.abs((y_test - y_pred) / y_test)) * 100
        
        self.metrics = {
            'mae': float(mae),
            'rmse': float(rmse),
            'mape': float(mape),
            'accuracy': float(100 - mape)
        }
        
        self.is_trained = True
        
        print("✅ 訓練完了！")
        print(f"   MAE: {mae:.2f} kWh")
        print(f"   RMSE: {rmse:.2f} kWh")
        print(f"   MAPE: {mape:.2f}%")
        print(f"   精度: {100-mape:.1f}%")
        
        return self.metrics
    
    def predict(self, target_date: datetime = None):
        """
        電力消費量を予測
        
        子供向け説明：
        「明日は電気をどれくらい使う？」と聞くと、答えてくれる
        """
        if not self.is_trained:
            print("⚠️ モデルが訓練されていません。先にtrain()を実行してください。")
            return None
        
        if target_date is None:
            target_date = datetime.now() + timedelta(days=1)
        
        dow = target_date.weekday()
        month = target_date.month
        is_weekend = 1 if dow >= 5 else 0
        
        X = np.array([[dow, month, is_weekend]])
        
        if SKLEARN_AVAILABLE:
            X_scaled = self.scaler.transform(X)
            prediction = self.model.predict(X_scaled)[0]
        else:
            prediction = self.model.predict(X, [dow])[0]
        
        # 信頼区間を計算（単純な方法）
        confidence = 85 - self.metrics.get('mape', 15)
        margin = prediction * (100 - confidence) / 100
        
        result = {
            'target_date': target_date.strftime('%Y-%m-%d'),
            'day_of_week': ['月', '火', '水', '木', '金', '土', '日'][dow],
            'predicted_consumption': round(prediction, 2),
            'confidence': round(confidence, 1),
            'lower_bound': round(prediction - margin, 2),
            'upper_bound': round(prediction + margin, 2),
            'unit': 'kWh'
        }
        
        return result
    
    def predict_week(self):
        """1週間分の予測を生成"""
        predictions = []
        for day in range(1, 8):
            target = datetime.now() + timedelta(days=day)
            pred = self.predict(target)
            if pred:
                predictions.append(pred)
        return predictions
    
    def save_model(self, filename: str = "energy_predictor.pkl"):
        """モデルを保存"""
        path = self.model_dir / filename
        with open(path, 'wb') as f:
            pickle.dump({
                'model': self.model,
                'scaler': self.scaler,
                'metrics': self.metrics,
                'is_trained': self.is_trained
            }, f)
        print(f"💾 モデルを保存しました: {path}")
    
    def load_model(self, filename: str = "energy_predictor.pkl"):
        """モデルを読み込み"""
        path = self.model_dir / filename
        if path.exists():
            with open(path, 'rb') as f:
                data = pickle.load(f)
                self.model = data['model']
                self.scaler = data['scaler']
                self.metrics = data['metrics']
                self.is_trained = data['is_trained']
            print(f"📂 モデルを読み込みました: {path}")
            return True
        return False


def main():
    """メイン実行"""
    print("=" * 60)
    print("🤖 電力消費量予測AI - 訓練スクリプト")
    print("=" * 60)
    
    # 予測モデルを作成
    predictor = EnergyPredictor()
    
    # 訓練
    metrics = predictor.train()
    
    # モデルを保存
    predictor.save_model()
    
    # 予測テスト
    print("\n" + "=" * 60)
    print("🔮 予測テスト")
    print("=" * 60)
    
    # 明日の予測
    tomorrow = predictor.predict()
    print(f"\n明日 ({tomorrow['target_date']}, {tomorrow['day_of_week']}曜日) の予測:")
    print(f"  予測消費量: {tomorrow['predicted_consumption']} kWh")
    print(f"  信頼度: {tomorrow['confidence']}%")
    print(f"  予測範囲: {tomorrow['lower_bound']} - {tomorrow['upper_bound']} kWh")
    
    # 週間予測
    print("\n📅 週間予測:")
    week_predictions = predictor.predict_week()
    for pred in week_predictions:
        print(f"  {pred['target_date']} ({pred['day_of_week']}): "
              f"{pred['predicted_consumption']} kWh "
              f"(信頼度: {pred['confidence']}%)")
    
    print("\n✅ 完了！")


if __name__ == "__main__":
    main()
