# Example 02: Loan Approval — Full deterministic pipeline
# Demonstrates UDEO's built-in experts: validation, math, risk, human review.

. $PSScriptRoot/../udeo.ps1

Write-Host "=== UDEO Loan Approval Pipeline ===" -ForegroundColor Cyan
Write-Host ""

# Create pipeline with specific applicant data
$pipeline = New-UDEOLoanApprovalPipeline `
    -Income 120000 `
    -Debt 30000 `
    -CreditScore 750 `
    -LoanAmount 450000 `
    -InterestRate 0.0625 `
    -TermMonths 360 `
    -PropertyValue 550000

# Show initial context
Write-Host "Applicant Data:" -ForegroundColor Cyan
$pipeline.Context.Data.applicant | Format-Table -AutoSize
Write-Host ""

# Run the pipeline
$result = $pipeline.Run()

# Display detailed result
Write-Host ""
Write-Host "=== Pipeline Result ===" -ForegroundColor Green
Write-Host "Decision: $($result.Decision)" -ForegroundColor White
if ($result.Reason) {
    Write-Host "Reason: $($result.Reason)" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Calculations ===" -ForegroundColor Cyan
if ($result.Context.Data.calculations) {
    $result.Context.Data.calculations | Format-Table -AutoSize
}

Write-Host ""
Write-Host "=== Decision Trace ===" -ForegroundColor Cyan
$result.Trace | Format-Table Step, ExpertId, DecisionCode, RuleFired, ExecutionTimeMs -AutoSize

# Save to store
[UDEOStore]::Save($result.Context)
Write-Host ""
Write-Host "Run saved as: $($result.Context.PipelineId)" -ForegroundColor DarkGray

# Test different scenarios
Write-Host ""
Write-Host "=== Additional Scenarios ===" -ForegroundColor Cyan

# Scenario: High DTI
Write-Host ""
Write-Host "--- High DTI (should flag) ---" -ForegroundColor Yellow
$pipeline2 = New-UDEOLoanApprovalPipeline -Income 50000 -Debt 35000 -CreditScore 700 -LoanAmount 200000 -PropertyValue 250000
$r2 = $pipeline2.Run()
Write-Host "Decision: $($r2.Decision) - $($r2.Reason)" -ForegroundColor White

# Scenario: Bad credit
Write-Host ""
Write-Host "--- Bad Credit (should reject) ---" -ForegroundColor Yellow
$pipeline3 = New-UDEOLoanApprovalPipeline -Income 80000 -Debt 10000 -CreditScore 550 -LoanAmount 150000 -PropertyValue 200000
$r3 = $pipeline3.Run()
Write-Host "Decision: $($r3.Decision) — $($r3.Reason)" -ForegroundColor White

# Scenario: Perfect applicant
Write-Host ""
Write-Host "--- Perfect Applicant (should approve) ---" -ForegroundColor Yellow
$pipeline4 = New-UDEOLoanApprovalPipeline -Income 200000 -Debt 5000 -CreditScore 820 -LoanAmount 300000 -PropertyValue 500000
$r4 = $pipeline4.Run()
Write-Host "Decision: $($r4.Decision) — $($r4.Reason)" -ForegroundColor White

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green
