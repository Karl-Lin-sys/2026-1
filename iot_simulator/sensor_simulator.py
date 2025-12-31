"""
==========================================================================
IoTセンサーシミュレーター
本物のセンサーがなくても、センサーのふりをしてデータを送る

子供向け説明：
本物の温度計がなくても、「今は25度です！」と教えてくれる
お芝居のセンサーです。テストに便利！

==========================================================================
"""

import random
import time
import json
from datetime import datetime
from typing import Dict, List, Optional
import threading

# requestsがない場合のフォールバック
try:
    import requests
    REQUESTS_AVAILABLE = True
except ImportError:
    REQUESTS_AVAILABLE = False
    print("⚠️ requestsがインストールされていません。ローカルモードで動作します。")


class SensorSimulator:
    """
    IoTセンサーシミュレーター
    
    子供向け説明：
    本物の温度計や電気メーターのふりをして、
    「今は○○度です」「今は○○kWh使いました」と報告するロボット
    """
    
    def __init__(self, api_url: str = "http://localhost:5000/api"):
        self.api_url = api_url
        self.sensors: List[Dict] = []
        self.is_running = False
        self.thread: Optional[threading.Thread] = None
        
        # デフォルトのセンサーを登録
        self._setup_default_sensors()
    
    def _setup_default_sensors(self):
        """デフォルトのセンサーを設定"""
        self.sensors = [
            {
                'id': 1,
                'name': '1階ロビー電力計',
                'type': 'energy',
                'location': '1階ロビー',
                'base_value': 20,  # 基準値 (kWh)
                'variance': 5      # 変動幅
            },
            {
                'id': 2,
                'name': '2階オフィス電力計',
                'type': 'energy',
                'location': '2階オフィス',
                'base_value': 35,
                'variance': 10
            },
            {
                'id': 3,
                'name': '屋上太陽光パネル',
                'type': 'energy',
                'location': '屋上',
                'base_value': -15,  # 発電（マイナス）
                'variance': 10
            },
            {
                'id': 4,
                'name': '1階温度センサー',
                'type': 'temperature',
                'location': '1階ロビー',
                'base_value': 23,
                'variance': 3
            },
            {
                'id': 5,
                'name': 'サーバールーム温度',
                'type': 'temperature',
                'location': '地下1階サーバールーム',
                'base_value': 18,
                'variance': 2
            }
        ]
    
    def _get_time_factor(self) -> float:
        """
        時間帯による係数を取得
        
        子供向け説明：
        昼間は電気をたくさん使うけど、夜は少ない。
        この関数はその違いを表す数字を返す。
        """
        hour = datetime.now().hour
        
        # 9時〜18時は多く使う
        if 9 <= hour <= 18:
            return 1.5
        # 6時〜9時、18時〜22時は普通
        elif 6 <= hour <= 9 or 18 <= hour <= 22:
            return 1.0
        # 深夜は少ない
        else:
            return 0.5
    
    def generate_reading(self, sensor: Dict) -> Dict:
        """
        センサーの読み取り値を生成
        
        子供向け説明：
        センサーが「今の値は○○です」と報告する値を作る
        """
        base = sensor['base_value']
        variance = sensor['variance']
        
        # 時間帯による調整
        if sensor['type'] == 'energy':
            base *= self._get_time_factor()
        
        # ランダムな変動を追加
        value = base + random.uniform(-variance, variance)
        
        # 電力データ用の追加フィールド
        if sensor['type'] == 'energy':
            return {
                'sensorId': sensor['id'],
                'timestamp': datetime.now().isoformat(),
                'consumption': max(0, value),
                'power': max(0, value * 1000 / 60),  # kWh to W/min
                'voltage': 220 + random.uniform(-5, 5),
                'current': max(0, value * 1000 / 220 / 60)
            }
        else:
            return {
                'sensorId': sensor['id'],
                'timestamp': datetime.now().isoformat(),
                'value': value,
                'unit': '°C' if sensor['type'] == 'temperature' else ''
            }
    
    def send_reading(self, reading: Dict) -> bool:
        """
        読み取り値をAPIに送信
        
        子供向け説明：
        「今、計測したデータをサーバーに教えるよ！」という作業
        """
        if not REQUESTS_AVAILABLE:
            print(f"📡 [ローカル] {reading}")
            return True
        
        try:
            response = requests.post(
                f"{self.api_url}/energy",
                json=reading,
                headers={'Content-Type': 'application/json'},
                timeout=5
            )
            return response.status_code == 201
        except requests.RequestException as e:
            print(f"❌ 送信エラー: {e}")
            return False
    
    def run_once(self):
        """すべてのセンサーから1回データを生成・送信"""
        print(f"\n⚡ {datetime.now().strftime('%H:%M:%S')} - データを生成中...")
        
        for sensor in self.sensors:
            if sensor['type'] != 'energy':
                continue  # 電力センサーのみ送信
            
            reading = self.generate_reading(sensor)
            success = self.send_reading(reading)
            
            status = "✅" if success else "❌"
            print(f"  {status} {sensor['name']}: {reading['consumption']:.2f} kWh")
    
    def start(self, interval: int = 60):
        """
        シミュレーションを開始
        
        子供向け説明：
        「よーいドン！」で、1分ごとにデータを送り続ける
        """
        if self.is_running:
            print("⚠️ すでに実行中です")
            return
        
        self.is_running = True
        
        def loop():
            print(f"🚀 センサーシミュレーター起動！")
            print(f"   {len(self.sensors)}個のセンサーをシミュレート")
            print(f"   送信間隔: {interval}秒")
            print(f"   API: {self.api_url}")
            print("-" * 50)
            
            while self.is_running:
                self.run_once()
                time.sleep(interval)
        
        self.thread = threading.Thread(target=loop, daemon=True)
        self.thread.start()
    
    def stop(self):
        """シミュレーションを停止"""
        self.is_running = False
        if self.thread:
            self.thread.join(timeout=2)
        print("🛑 シミュレーター停止")
    
    def print_status(self):
        """現在の状態を表示"""
        print("\n" + "=" * 50)
        print("📡 センサーシミュレーター状態")
        print("=" * 50)
        print(f"稼働状態: {'🟢 実行中' if self.is_running else '🔴 停止'}")
        print(f"登録センサー: {len(self.sensors)}個")
        print("\nセンサー一覧:")
        for s in self.sensors:
            print(f"  - [{s['id']}] {s['name']} ({s['type']}) @ {s['location']}")


def main():
    """メイン実行"""
    print("=" * 60)
    print("📡 IoTセンサーシミュレーター")
    print("=" * 60)
    print("\n子供向け説明：")
    print("  このプログラムは、本物のセンサーのふりをして")
    print("  「今は電気をこれくらい使ってます」というデータを")
    print("  サーバーに送り続けます。")
    print()
    
    simulator = SensorSimulator()
    simulator.print_status()
    
    print("\n📍 デモモード: 3回データを生成して終了します")
    print("-" * 50)
    
    for i in range(3):
        simulator.run_once()
        if i < 2:
            time.sleep(2)
    
    print("\n" + "=" * 60)
    print("📊 生成されたサンプルデータ:")
    print("=" * 60)
    
    for sensor in simulator.sensors[:2]:
        reading = simulator.generate_reading(sensor)
        print(f"\n{sensor['name']}:")
        print(json.dumps(reading, indent=2, ensure_ascii=False))
    
    print("\n✅ デモ完了！")
    print("\n💡 継続実行するには:")
    print("   simulator = SensorSimulator()")
    print("   simulator.start(interval=60)  # 60秒間隔で送信")


if __name__ == "__main__":
    main()
