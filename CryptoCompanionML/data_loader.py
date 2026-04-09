import os
import yfinance as yf
import pandas as pd

CACHE_DIR = "data_cache"

def get_crypto_data(symbol: str, period: str = "2y", interval: str = "1d", force_download=False) -> pd.DataFrame:
    """
    Fetches OHLCV data for a cryptocurrency from Yahoo Finance.
    Caches the data locally as CSV to speed up subsequent runs.
    
    Args:
        symbol (str): Ticker symbol (e.g. 'BTC-USD')
        period (str): Amount of historical data to fetch (e.g., '1mo', '1y', 'max')
        interval (str): Granularity of data (e.g., '1d', '1h', '5m')
        force_download (bool): If true, ignores cache and downloads fresh data
        
    Returns:
        pd.DataFrame: Pandas dataframe with OHLCV data.
    """
    stablecoin_keywords = ["USDT", "USDC", "USDE", "FDUSD", "DAI", "BUSD", "TUSD"]
    # Check if the base asset is a stablecoin
    base_asset = symbol.split("-")[0]
    if base_asset in stablecoin_keywords:
        print(f"Skipping stablecoin {symbol}")
        return pd.DataFrame()

    os.makedirs(CACHE_DIR, exist_ok=True)
    cache_file = os.path.join(CACHE_DIR, f"{symbol}_{period}_{interval}.csv")
    
    if not force_download and os.path.exists(cache_file):
        # Read from cache
        print(f"[{symbol}] Loading cached data from {cache_file}")
        df = pd.read_csv(cache_file, index_col=0, parse_dates=True)
        return df

    print(f"[{symbol}] Downloading {period} of data at {interval} intervals...")
    ticker = yf.Ticker(symbol)
    df = ticker.history(period=period, interval=interval)
    
    if df.empty:
        print(f"Warning: No data found for {symbol}")
        return df

    # Remove timezone information for easier manipulation locally if necessary, 
    # but yfinance returns tz-aware. We'll keep it as is, but save it.
    df.to_csv(cache_file)
    print(f"[{symbol}] Saved to cache {cache_file}")
    
    return df

if __name__ == "__main__":
    # Test
    df = get_crypto_data("BTC-USD", period="1mo", interval="1d")
    print(df.head())
