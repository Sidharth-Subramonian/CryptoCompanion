# 📖 Step-by-Step Execution Guide: Review III

Follow these steps exactly as written to perform a flawless demonstration for your final project review.

---

## 🕒 Phase 1: Pre-Review Preparation (10 Minutes Prior)

To ensure the demo works "live," you must have your environment ready.

1.  **Start the API**:
    - Open your terminal.
    - Navigate to the `CryptoCompanionApi` folder.
    - Run: `dotnet run`
    - **Verify**: Open `http://localhost:5000/swagger` in your browser to make sure the API is alive.

2.  **Open the Source Code**:
    - Have **VS Code** or **Visual Studio** open to:
        - `SecurityDemoController.cs` (Line 21 & 37 specifically)
        - `AzureOpenAiAdvisorService.cs` (Line 54-62 for Telemetry)

3.  **Prepare the Attack Script**:
    - Open **PowerShell** as Administrator.
    - Navigate to the project root folder.
    - Type `.\security_demo_attack.ps1` but **do not press Enter yet**.

4.  **Open GitHub**:
    - Go to your repository on GitHub.com.
    - Click the **"Actions"** tab.
    - Find the **"CodeQL"** workflow and have the most recent successful run visible.

---

## 🛡️ Phase 2: The Security Demo (2-3 Minutes)

### Step 1: Prove Automated Security (The "DevSecOps" part)
- **Show your screen**: Display the GitHub Actions page.
- **Action**: Point to the "Analyze (csharp)" job.
- **Say**: *"We've integrated CodeQL for Static Application Security Testing (SAST). Every time I push code, GitHub automatically scans for high-risk vulnerabilities like SQL injection or exposed secrets. This acts as our first line of defense."*

### Step 2: Demo the SQL Injection (The "Attack" part)
- **Show your screen**: Switch to your PowerShell terminal.
- **Action**: Press **Enter** on `.\security_demo_attack.ps1`.
- **Explain as it runs**:
    - **Step 1 (Cyan)**: *"First, we search for 'BTC' normally. It works as expected."*
    - **Step 2 (Red)**: *"Now, we simulate an attack on our legacy 'vulnerable' endpoint. We are passing a payload: `BTC' OR '1'='1`. Because the code just concatenates strings, the database sees this as a command and returns every single asset in the database, bypassing the filter. This is a critical security failure."*

### Step 3: Show the Remediation (The "Remediation" part)
- **Show your screen**: Switch to `SecurityDemoController.cs` in your code editor.
- **Action**: Highlight line 26 (The Bad way) then line 41 (The Good way).
- **Say**: *"To fix this, we transitioned to Parameterized Queries. Instead of building a string, we pass the user input as a safe parameter `{0}`. This ensures the database treats the attack payload as a piece of text, not a command."*
- **Action**: Point back to the PowerShell output (Step 3 - Green).
- **Say**: *"As you can see, the Secure endpoint correctly blocked the attack."*

---

## 📈 Phase 3: Monitoring & Observability (2 Minutes)

### Step 1: Explain "Golden Signals"
- **Show your screen**: Switch to `AzureOpenAiAdvisorService.cs`.
- **Action**: Highlight lines 54-62 (The `_telemetryClient` calls).
- **Say**: *"For observability, we instrumented our AI service with Azure Application Insights. We specifically track the 'Golden Signals'—Latency, Errors, and Success rates."*

### Step 2: Show the Code Implementation
- **Say**: *"We use a Stopwatch to measure exactly how long the OpenAI API takes to respond. We then send this as a custom metric to Azure. This allows us to set alerts—if our AI takes longer than 2 seconds, we get notified immediately so we can investigate performance bottlenecks."*

### Step 3: Deployment Strategy (Optional/Slide)
- **Show your screen**: Show your **Promotion Strategy** slide or document.
- **Say**: *"Finally, our deployment follows a full Dev → Staging → Prod flow using Terraform and Azure Kubernetes Service, ensuring that the same security and monitoring configurations are promoted across all environments."*

---

## ✅ Phase 4: Q&A Wrap-up

- **Closing Statement**: *"By combining automated SAST scanning, parameterized SQL remediation, and real-time Application Insights monitoring, we have built a platform that is secure, transparent, and production-ready. Thank you."*
