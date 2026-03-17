import numpy as np
from sklearn.ensemble import RandomForestRegressor
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import os

def train_and_export():
    print("Training Crypto Ranking Model (Random Forest)...")
    
    # Features: RSI, MA50, MA200, Volume Change %, Price Change %
    np.random.seed(42)
    n_samples = 500
    
    X = np.random.randn(n_samples, 5)
    # Target: A "strength" score from 1-100 indicating ranking potential
    y = np.sum(X * np.array([10, 5, 2, 15, 20]), axis=1) + 50
    y = np.clip(y, 1, 100)
    
    model = RandomForestRegressor(n_estimators=50, max_depth=5, random_state=42)
    model.fit(X, y)
    
    print(f"Model R^2 Score: {model.score(X, y):.4f}")
    
    # Export to ONNX
    initial_type = [('float_input', FloatTensorType([None, 5]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/crypto_ranking.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("Exported to models/crypto_ranking.onnx")

if __name__ == "__main__":
    train_and_export()
