import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import StringTensorType
import os

def train_and_export():
    print("Training Sentiment Classifier (Logistic Regression)...")
    
    # Dummy text data for sentiment
    corpus = [
        "Bitcoin is going to the moon! Very bullish.",      # 1 (Bullish)
        "The market looks terrible, I'm selling everything.", # 0 (Bearish)
        "Ethereum ETF approved, huge greens ahead.",        # 1
        "Crypto regulations are destroying the industry.",    # 0
        "I just bought the dip, great support level here.",   # 1
        "Another exchange hacked, losing all my funds.",      # 0
    ]
    labels = np.array([1, 0, 1, 0, 1, 0])
    
    # Pipeline: TF-IDF -> Logistic Regression
    model = Pipeline([
        ('tfidf', TfidfVectorizer()),
        ('clf', LogisticRegression())
    ])
    
    model.fit(corpus, labels)
    print(f"Training Accuracy: {model.score(corpus, labels):.2f}")
    
    # Export to ONNX
    # Because scikit-learn ONNX conversion for strings/TFIDF requires specific handling,
    # we define the initial type as string.
    initial_type = [('text_input', StringTensorType([None, 1]))]
    
    # NOTE: TFIDF conversion in skl2onnx with strings has specific nuances.
    # For a production app, we would use a pre-trained tokenizer step or ensure 
    # the target runtime supports string tokenization ops.
    try:
        onx = convert_sklearn(model, initial_types=initial_type, options={'tfidf': {'tokenexp': r'[a-zA-Z0-9_]+'}})
        os.makedirs('models', exist_ok=True)
        with open("models/sentiment_classifier.onnx", "wb") as f:
            f.write(onx.SerializeToString())
        print("Exported to models/sentiment_classifier.onnx")
    except Exception as e:
        print(f"Warning: String/TFIDF to ONNX conversion error: {e}")
        print("Fallback: In a real .NET MAUI environment, we typically tokenize in C# and pass float arrays to ONNX, or use ML.NET directly for text processing.")

if __name__ == "__main__":
    train_and_export()
