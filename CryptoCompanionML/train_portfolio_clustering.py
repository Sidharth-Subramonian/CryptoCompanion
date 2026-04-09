import os
import pandas as pd
from sklearn.cluster import KMeans
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

from data_loader import get_crypto_data
from feature_engineering import prepare_features

def train_and_export():
    print("Training Portfolio Risk Clustering (K-Means)...")
    
    symbols = ["BTC-USD", "ETH-USD", "SOL-USD", "BNB-USD", "ADA-USD", "DOGE-USD", "SHIB-USD", "LTC-USD"]
    
    coin_metrics = []
    valid_symbols = []
    
    # 1. Fetch real historical data to calculate average risk profiles
    for symbol in symbols:
        df = get_crypto_data(symbol, period="1y", interval="1d")
        if df.empty:
            continue
            
        df = prepare_features(df)
        if df.empty:
            continue
            
        # We define a coin's "profile" over the last year
        avg_volatility = df["Volatility_20"].mean()
        avg_daily_return = df["Return_1"].mean()
        
        # Approximate "Market Size" proxy for demo (yfinance doesn't cleanly give exact current MarketCap in historical)
        # We'll use average price as a pseudo-metric for size distinction just to have 3 features like before
        avg_price = df["Close"].mean()
        
        # We need to scale these metrics reasonably
        avg_volatility_scaled = avg_volatility * 100
        avg_return_scaled = avg_daily_return * 100
        avg_price_scaled = min(avg_price / 1000, 100) # Cap the upper limit
        
        coin_metrics.append([avg_volatility_scaled, avg_return_scaled, avg_price_scaled])
        valid_symbols.append(symbol)

    if not coin_metrics:
        print("Error: No data available for clustering.")
        return

    X = pd.DataFrame(coin_metrics, columns=["Avg_Volatility", "Avg_Daily_Return", "Size_Proxy"])
    
    print(f"Clustering {len(X)} cryptocurrencies based on historical risk metrics.")
    
    # 2. Train KMeans Model
    model = KMeans(n_clusters=3, random_state=42, n_init=10)
    clusters = model.fit_predict(X)
    
    X["Symbol"] = valid_symbols
    X["Risk_Cluster"] = clusters
    
    print("\n--- Asset Risk Clusters ---")
    for cluster_id in range(3):
        cluster_coins = X[X["Risk_Cluster"] == cluster_id]["Symbol"].tolist()
        print(f"Cluster {cluster_id}: {', '.join(cluster_coins)}")
        
    print("\nCluster Centers (Volatility, Return, Size_Proxy):")
    for i, center in enumerate(model.cluster_centers_):
        print(f"Cluster {i}: {center.round(4)}")
    
    # 3. Export to ONNX
    # Takes 3 float inputs natively matching the old file
    initial_type = [('float_input', FloatTensorType([None, 3]))]
    onx = convert_sklearn(model, initial_types=initial_type)
    
    os.makedirs('models', exist_ok=True)
    with open("models/portfolio_clustering.onnx", "wb") as f:
        f.write(onx.SerializeToString())
        
    print("\nExported to models/portfolio_clustering.onnx")

if __name__ == "__main__":
    train_and_export()
