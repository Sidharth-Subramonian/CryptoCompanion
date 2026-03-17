import numpy as np
from sklearn.ensemble import IsolationForest
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import os

def train_and_export():
    print("Training Anomaly Detector (Isolation Forest)...")
    
    # Features: Price Change (5m), Volume Spike (5m)
    np.random.seed(42)
    n_samples = 1000
    
    # Normal trading behavior
    X_normal = np.random.normal(0, 1, (n_samples, 2))
    
    # Anomalous behavior (flash crashes / pump & dumps)
    X_outliers = np.random.uniform(low=-10, high=10, size=(50, 2))
    
    X = np.vstack((X_normal, X_outliers))
    
    model = IsolationForest(contamination=0.05, random_state=42)
    model.fit(X)
    
    # Export to ONNX
    # IsolationForest conversion requires target_opset >= 12
    initial_type = [('float_input', FloatTensorType([None, 2]))]
    
    try:
        onx = convert_sklearn(model, initial_types=initial_type, target_opset={'': 15, 'ai.onnx.ml': 3})
        os.makedirs('models', exist_ok=True)
        with open("models/anomaly_detector.onnx", "wb") as f:
            f.write(onx.SerializeToString())
        print("Exported to models/anomaly_detector.onnx")
    except Exception as e:
        print(f"Error converting Isolation Forest to ONNX: {e}")

if __name__ == "__main__":
    train_and_export()
