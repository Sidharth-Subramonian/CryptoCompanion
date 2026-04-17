# Review III: Presentation Demo Script (5 Minutes)

This script is your step-by-step guide for the Final Review. Follow it to ensure you hit all the requirements.

---

### PRE-DEMO SETUP:
1.  Open your API project in a terminal and run: `dotnet run`.
2.  Open **GitHub** in your browser to the "Actions" tab (or have the screenshot ready).
3.  Open `security_demo_attack.ps1` in PowerShell but **don't run it yet**.

---

### Part 1: Security & DevSecOps (2 Minutes)

**[Action: Show Slide 2 or CodeQL Workflow on GitHub]**
*   **Say**: "For Review III, our primary focus was production hardening. We integrated **GitHub CodeQL** directly into our CI/CD pipeline. This means every push to our main branch undergoes a complete Static Analysis (SAST) to identify vulnerabilities like credential leaks or SQL injection before they reach production."

**[Action: Open security_demo_attack.ps1 and run it]**
*   **Say**: "To demonstrate the impact, we identified a critical SQL Injection vulnerability in our legacy search controller. Using this attack script, we can see that a standard malicious payload—`BTC' OR '1'='1`—successfully bypasses the logic and exposes the full table on our **Vulnerable** endpoint."
*   **Say**: "However, as seen in the final step, our **Secure** endpoint uses Parameterized Queries, which treats that same input as a literal string, effectively neutralizing the attack. This is our standard across the entire platform."

---

### Part 2: Monitoring & AI Observability (2 Minutes)

**[Action: Show Slide 4 or Application Map screenshot]**
*   **Say**: "Beyond security, we implemented full production observability using **Azure Application Insights**. This gives us real-time visibility into the health of our AI features."

**[Action: Briefly show AzureOpenAiAdvisorService.cs line 55-60]**
*   **Say**: "We've instrumented our AI Advisor service to track the **4 Golden Signals**. Specifically, we monitor **Latency**—to ensure our market summaries are delivered in under 2 seconds—and **Error Rates**, so we know immediately if the OpenAI service goes down."

---

### Part 3: Conclusion (1 Minute)

**[Action: Show Slide 6]**
*   **Say**: "In conclusion, we have transformed CryptoCompanion from a simple feature-set into a secure, monitored, and production-ready cloud platform. With automated security scanning and real-time performance tracking, we can confidently deploy updates to our users while maintaining high availability and security."

---

**[Stop Screen Share]**
*   **Say**: "Thank you. Are there any questions regarding our security architecture or monitoring metrics?"
