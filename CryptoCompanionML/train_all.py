import subprocess
import sys

def main():
    print("========================================")
    print("CryptoCompanion ML Pipeline Orchestrator")
    print("========================================\n")
    
    scripts = [
        "train_price_direction.py",
        "train_crypto_ranking.py",
        "train_anomaly_detector.py",
        "train_portfolio_clustering.py",
        "train_sentiment_classifier.py"
    ]
    
    for script in scripts:
        print(f"[*] Running {script}...")
        try:
            # Run the script and stream output
            result = subprocess.run([sys.executable, script], check=True)
            print("-" * 40)
        except subprocess.CalledProcessError as e:
            print(f"[!] Error running {script}. Pipeline halted.")
            sys.exit(1)
            
    print("\n✅ All models trained and exported to ONNX successfully!")

if __name__ == "__main__":
    main()
