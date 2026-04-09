import onnxruntime as ort
import sys

def inspect(model_path):
    print(f"--- Inspecting {model_path} ---")
    try:
        session = ort.InferenceSession(model_path)
        print("Inputs:")
        for i in session.get_inputs():
            print(f"  Name: {i.name}, Shape: {i.shape}, Type: {i.type}")
        print("Outputs:")
        for o in session.get_outputs():
            print(f"  Name: {o.name}, Shape: {o.shape}, Type: {o.type}")
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    inspect("models/price_direction.onnx")
    inspect("models/crypto_ranking.onnx")
    inspect("models/portfolio_clustering.onnx")
    inspect("models/anomaly_detector.onnx")
    inspect("models/sentiment_classifier.onnx")
