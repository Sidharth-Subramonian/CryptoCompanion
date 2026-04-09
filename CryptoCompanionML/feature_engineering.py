import numpy as np
import pandas as pd

def add_returns(df: pd.DataFrame, column: str = "Close", periods: int = 1) -> pd.DataFrame:
    """Calculates percentage return over 'periods' intervals."""
    df[f"Return_{periods}"] = df[column].pct_change(periods=periods)
    return df

def add_moving_averages(df: pd.DataFrame, column: str = "Close", windows=[10, 20, 50]) -> pd.DataFrame:
    """Calculates Simple Moving Averages relative to price."""
    for w in windows:
        ma = df[column].rolling(window=w).mean()
        df[f"MA_{w}"] = (df[column] - ma) / ma
    return df

def add_rsi(df: pd.DataFrame, column: str = "Close", window: int = 14) -> pd.DataFrame:
    """Calculates Relative Strength Index (RSI)."""
    delta = df[column].diff()
    gain = (delta.where(delta > 0, 0)).rolling(window=window).mean()
    loss = (-delta.where(delta < 0, 0)).rolling(window=window).mean()
    
    rs = gain / loss
    # Handling division by zero
    rsi = np.where(loss == 0, 100, 100 - (100 / (1 + rs)))
    df[f"RSI_{window}"] = rsi
    return df

def add_volatility(df: pd.DataFrame, column: str = "Close", window: int = 20) -> pd.DataFrame:
    """Calculates Volatility (normalized continuous standard deviation of prices)."""
    df[f"Volatility_{window}"] = df[column].rolling(window=window).std() / df[column]
    return df

def add_volume_change(df: pd.DataFrame, column: str = "Volume", periods: int = 1) -> pd.DataFrame:
    """Calculates percentage change in volume."""
    df[f"Volume_Change_{periods}"] = df[column].pct_change(periods=periods)
    return df

def prepare_features(df: pd.DataFrame) -> pd.DataFrame:
    """
    Applies all standard feature engineering functions and drops NaN rows
    resulting from rolling calculations.
    """
    df = df.copy()
    df = add_returns(df)
    df = add_moving_averages(df, windows=[10, 20, 50])
    df = add_rsi(df)
    df = add_volatility(df)
    df = add_volume_change(df)
    
    # Drop rows with NaN values (which exist at the start of rolling windows)
    df = df.dropna()
    return df

if __name__ == "__main__":
    # Small test
    from data_loader import get_crypto_data
    df = get_crypto_data("ETH-USD", period="1y", interval="1d")
    df_features = prepare_features(df)
    print("Features generated:")
    print(df_features.columns)
    print(df_features[["Close", "Return_1", "MA_20", "RSI_14", "Volatility_20"]].tail())
