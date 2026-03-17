import numpy as np
from sklearn.cluster import KMeans
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import os

def train_and_export():
    print("Training Portfolio Risk Clustering (K-Means)...")
    
    # Features: Volatility (30d), Avg Daily Return %, Market Cap (Normalized)
    np.random.seed(42)
    n_samples = 300
    
    # Create 3 distinct clusters: Low Risk, Medium Risk, High Risk (Memecoins)
    cluster_1 = np.random.randn(100, 3) + [1.0, 0.5, 10.0] # Low vol, steady return, high MC
    cluster_2 = np.random.randn(100, 3) + [5.0, 2.0, 5.0]  # Med vol, med return, med MC
    cluster_3 = np.random.randn(100, 3) + [15.0, 10.0, 1.0] # High vol, high return, low MC
    
    X = np.vstack((cluster_1, cluster_2, cluster_3))
    
    model = KMeans(n_clusters=3, random_state=42, n_init=10)
    model.fit(X)
    
    print(f"Fitted Clusters: {model.cluster_centers_}")
    
    # Export to ONNX
    initial_type = [('float_input', FloatTensorType([None, 3]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/portfolio_clustering.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("Exported to models/portfolio_clustering.onnx")

if __name__ == "__main__":
    train_and_export()
