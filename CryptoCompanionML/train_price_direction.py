import os
import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestRegressor
from sklearn.metrics import mean_squared_error, r2_score
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

from data_loader import get_crypto_data
from feature_engineering import prepare_features

def train_and_export():
    print("Training Price Direction Model (Random Forest)...")
    
    symbols = ["BTC-USD", "ETH-USD", "SOL-USD"]
    all_data = []
    
    for symbol in symbols:
        df = get_crypto_data(symbol, period="2y", interval="1d")
        if df.empty:
            continue
            
        # 2. Engineer features
        df = prepare_features(df)
        
        # 3. Create target: Percentage difference relative to current day
        df["Target"] = (df["Close"].shift(-1) - df["Close"]) / df["Close"]
        
        df = df.dropna()
        all_data.append(df)
        
    df = pd.concat(all_data)
    
    # Define features to use
    features = [
        "Return_1", "MA_10", "MA_20", "MA_50", 
        "RSI_14", "Volatility_20", "Volume_Change_1"
    ]
    
    X = df[features].values
    y = df["Target"].values
    
    # 4. Evaluation: Chronological Train/Test Split (NO shuffling!)
    # We use the first 80% of time for training, last 20% for testing
    split_index = int(len(X) * 0.8)
    X_train, X_test = X[:split_index], X[split_index:]
    y_train, y_test = y[:split_index], y[split_index:]
    
    print(f"Training on {len(X_train)} samples, testing on {len(X_test)} samples.")
    
    # 5. Train Model
    model = RandomForestRegressor(n_estimators=100, max_depth=7, random_state=42)
    model.fit(X_train, y_train)
    
    # 6. Evaluation metrics
    y_pred = model.predict(X_test)
    
    print("\n--- Model Evaluation (Test Set) ---")
    print(f"MSE: {mean_squared_error(y_test, y_pred):.6f}")
    print(f"R^2: {r2_score(y_test, y_pred):.4f}")
    
    # 7. Simple Directional Backtesting Logic
    # Simulate buying if positive prediction
    test_df = df.iloc[split_index:].copy()
    test_df["Predicted_Signal"] = (y_pred > 0).astype(int)
    
    test_df["Strategy_Return"] = test_df["Return_1"].shift(-1) * test_df["Predicted_Signal"]
    
    total_market_return = (test_df["Return_1"].shift(-1) + 1).prod() - 1
    total_strategy_return = (test_df["Strategy_Return"] + 1).prod() - 1
    
    print("\n--- Simple Backtest ---")
    print(f"Market Return (Buy & Hold): {total_market_return * 100:.2f}%")
    print(f"Strategy Return:            {total_strategy_return * 100:.2f}%")
    
    # 8. Export to ONNX
    # We have 7 features inputted
    initial_type = [('float_input', FloatTensorType([None, len(features)]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/price_direction.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("\nExported to models/price_direction.onnx")

if __name__ == "__main__":
    train_and_export()
