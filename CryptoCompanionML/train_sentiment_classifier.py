import os
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline
from sklearn.metrics import accuracy_score, classification_report
from sklearn.model_selection import train_test_split
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import StringTensorType

def train_and_export():
    print("Training Sentiment Classifier (TF-IDF + Logistic Regression)...")
    
    # 1. Provide a semi-realistic bundled text dataset for demonstration
    # 1 = Bullish (Positive), 0 = Bearish (Negative)
    data = [
        ("Bitcoin hits new all-time high amidst institutional buying", 1),
        ("Crypto markets plummet as regulatory fears mount", 0),
        ("Ethereum core developers successfully deploy new upgrade", 1),
        ("Major exchange hacked, millions stolen in latest crypto heist", 0),
        ("Solana network experiences another outage, users frustrated", 0),
        ("Federal Reserve hints at interest rate cuts, crypto rallies", 1),
        ("Over 10,000 retail locations now accept cryptocurrency payments", 1),
        ("SEC denies latest spot ETF applications, citing manipulation concerns", 0),
        ("Mining difficulty increases, signaling strong network security", 1),
        ("Whales move massive amounts of BTC to exchanges, sell-off feared", 0),
        ("Venture capital firm announces new $500M crypto fund", 1),
        ("Leading stablecoin loses peg slightly, causing market panic", 0),
        ("Analysts predict a strong altcoin season approaching", 1),
        ("Crypto founder arrested on fraud charges", 0),
        ("New layer-2 scaling solution drastically reduces gas fees", 1),
        ("NFT trading volume drops 90% from peak, bubble bursts", 0),
        ("Global bank integrates crypto trading for high-net-worth clients", 1),
        ("Government proposes strict ban on proof-of-work mining", 0),
        ("Cardano launches smart contracts, ecosystem expands", 1),
        ("Scam project rug-pulls investors, founders disappear", 0)
    ]
    
    corpus, labels = zip(*data)
    X = np.array(corpus)
    y = np.array(labels)
    
    # 2. Evaluation: Train/Test split
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.25, random_state=42)
    
    print(f"Training on {len(X_train)} samples, testing on {len(X_test)} samples.")
    
    # 3. Pipeline: TF-IDF -> Logistic Regression
    model = Pipeline([
        ('tfidf', TfidfVectorizer(max_features=1000)),
        ('clf', LogisticRegression(random_state=42))
    ])
    
    model.fit(X_train, y_train)
    
    # 4. Evaluation
    y_pred = model.predict(X_test)
    print("\n--- Model Evaluation (Test Set) ---")
    print(f"Accuracy: {accuracy_score(y_test, y_pred):.2f}")
    print("Classification Report:")
    print(classification_report(y_test, y_pred, target_names=["Bearish", "Bullish"]))
    
    # 5. Export to ONNX
    initial_type = [('text_input', StringTensorType([None, 1]))]
    
    try:
        onx = convert_sklearn(model, initial_types=initial_type, options={'tfidf': {'tokenexp': r'[a-zA-Z0-9_]+'}, 'clf': {'zipmap': False}})
        os.makedirs('models', exist_ok=True)
        with open("models/sentiment_classifier.onnx", "wb") as f:
            f.write(onx.SerializeToString())
        print("Exported to models/sentiment_classifier.onnx")
    except Exception as e:
        print(f"Warning: String/TFIDF to ONNX conversion error: {e}")
        print("Fallback: Ensure your skl2onnx and onnxruntime versions support string tokenization.")

if __name__ == "__main__":
    train_and_export()
