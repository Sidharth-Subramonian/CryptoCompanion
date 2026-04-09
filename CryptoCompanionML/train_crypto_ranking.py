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
    print("Training Crypto Ranking Model (Random Forest Regressor)...")
    
    symbols = ["BTC-USD", "ETH-USD", "SOL-USD", "BNB-USD", "ADA-USD"]
    all_data = []
    
    # 1. Fetch real historical data for multiple coins
    for symbol in symbols:
        df = get_crypto_data(symbol, period="2y", interval="1d")
        if df.empty:
            continue
        
        # 2. Engineer features
        df = prepare_features(df)
        
        # 3. Create target: Percentage return over the next 7 days
        # We shift by -7 so today's row has the price 7 days from now
        df["Target_Return_7d"] = df["Close"].shift(-7) / df["Close"] - 1
        
        df = df.dropna()
        all_data.append(df)
        
    if not all_data:
        print("Error: No data fetched for ranking model.")
        return
        
    combined_df = pd.concat(all_data)
    
    # Define features to use
    features = [
        "Return_1", "MA_10", "MA_20", "MA_50", 
        "RSI_14", "Volatility_20", "Volume_Change_1"
    ]
    
    X = combined_df[features].values
    y = combined_df["Target_Return_7d"].values
    
    # 4. Evaluation: Chronological Train/Test Split
    split_index = int(len(X) * 0.8)
    X_train, X_test = X[:split_index], X[split_index:]
    y_train, y_test = y[:split_index], y[split_index:]
    
    print(f"Training on {len(X_train)} samples across multiple coins.")
    
    # 5. Train Model
    model = RandomForestRegressor(n_estimators=100, max_depth=7, random_state=42)
    model.fit(X_train, y_train)
    
    # 6. Evaluation metrics
    y_pred = model.predict(X_test)
    
    print("\n--- Model Evaluation (Test Set) ---")
    print(f"MSE: {mean_squared_error(y_test, y_pred):.4f}")
    # R2 can sometimes be negative for financial forecasting, indicating predicting the mean is better
    print(f"R^2: {r2_score(y_test, y_pred):.4f}") 
    
    # 7. Provide a sample ranking currently
    # Grab the very last day's data for each coin
    print("\n--- Sample Ranking (Latest Available Day) ---")
    latest_features = []
    valid_symbols = []
    for symbol, df in zip(symbols, all_data):
        latest_row = df[features].iloc[-1].values
        latest_features.append(latest_row)
        valid_symbols.append(symbol)
        
    latest_predictions = model.predict(np.array(latest_features))
    
    rankings = sorted(zip(valid_symbols, latest_predictions), key=lambda x: x[1], reverse=True)
    for rank, (sym, pred_return) in enumerate(rankings, 1):
        print(f"{rank}. {sym}: Predicted 7d Return = {pred_return * 100:.2f}%")
    
    # 8. Export to ONNX
    initial_type = [('float_input', FloatTensorType([None, len(features)]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/crypto_ranking.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("\nExported to models/crypto_ranking.onnx")

if __name__ == "__main__":
    train_and_export()
