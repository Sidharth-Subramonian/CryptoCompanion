import numpy as np
import pandas as pd
from sklearn.linear_model import LinearRegression
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import os

def train_and_export():
    print("Training Price Forecast Model (Linear Regression)...")
    
    # Generate dummy historical price data (features: moving avg, RSI, volume)
    # Target: Price in 24h
    np.random.seed(42)
    n_samples = 1000
    
    ma_50 = np.random.uniform(30000, 70000, n_samples)
    rsi = np.random.uniform(20, 80, n_samples)
    volume = np.random.uniform(1e9, 5e10, n_samples)
    
    # Simple linear relationship for demonstration
    target_price = ma_50 * 1.05 + (rsi - 50) * 100 + np.random.normal(0, 500, n_samples)
    
    X = np.column_stack((ma_50, rsi, volume))
    y = target_price
    
    model = LinearRegression()
    model.fit(X, y)
    
    print(f"Model R^2 Score: {model.score(X, y):.4f}")
    
    # Export to ONNX
    initial_type = [('float_input', FloatTensorType([None, 3]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/price_forecast.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("Exported to models/price_forecast.onnx")

if __name__ == "__main__":
    train_and_export()
