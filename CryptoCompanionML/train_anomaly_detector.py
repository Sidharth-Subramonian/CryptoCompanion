import os
import pandas as pd
from sklearn.ensemble import IsolationForest
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

from data_loader import get_crypto_data
from feature_engineering import add_returns, add_volume_change

def train_and_export():
    print("Training Anomaly Detector (Isolation Forest)...")
    
    # Fast-moving pairs often show anomalies
    symbols = ["BTC-USD", "ETH-USD"]
    all_data = []
    
    # 1. Fetch real historical data
    for symbol in symbols:
        df = get_crypto_data(symbol, period="1y", interval="1h")
        if df.empty:
            continue
            
        # 2. Engineer specific anomaly features
        # We look for sudden price jumps (returns) and volume spikes
        df = add_returns(df, periods=1)
        df = add_volume_change(df, periods=1)
        df = df.dropna()
        all_data.append(df)
        
    if not all_data:
        print("Error: No data fetched for anomaly model.")
        return
        
    combined_df = pd.concat(all_data)
    
    features = ["Return_1", "Volume_Change_1"]
    
    # Need to handle infinite values that can happen with volume percentage change
    # if trading volume goes from 0 to something
    combined_df.replace([float('inf'), float('-inf')], 0, inplace=True)
    
    X = combined_df[features].values
    
    print(f"Training on {len(X)} hourly samples.")
    
    # 3. Train Model
    # Expect roughly 1% of hourly movements to be "anomalous" flashes
    model = IsolationForest(contamination=0.01, random_state=42)
    model.fit(X)
    
    # 4. Briefly evaluate
    predictions = model.predict(X)
    anomalies = combined_df[predictions == -1]
    
    print(f"\nFound {len(anomalies)} anomalies out of {len(X)} samples.")
    if len(anomalies) > 0:
        print("\nTop 3 Anomalies (Largest Vol Spikes):")
        top_anomalies = anomalies.sort_values(by="Volume_Change_1", ascending=False).head(3)
        print(top_anomalies[["Close", "Return_1", "Volume_Change_1"]])
        
    # 5. Export to ONNX
    initial_type = [('float_input', FloatTensorType([None, 2]))]
    
    try:
        onx = convert_sklearn(model, initial_types=initial_type, target_opset={'': 15, 'ai.onnx.ml': 3})
        os.makedirs('models', exist_ok=True)
        with open("models/anomaly_detector.onnx", "wb") as f:
            f.write(onx.SerializeToString())
        print("\nExported to models/anomaly_detector.onnx")
    except Exception as e:
        print(f"Error converting Isolation Forest to ONNX: {e}")

if __name__ == "__main__":
    train_and_export()
