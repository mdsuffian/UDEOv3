# Example 04: Compliance Check — Demonstrates UDEO in a regulatory context
# Shows how to use UDEO for automated regulatory compliance checking.

. $PSScriptRoot/../udeo.ps1

Write-Host "=== UDEO Compliance Check Example ===" -ForegroundColor Cyan
Write-Host ""

# Register domain-specific compliance rules
Register-UDEOExpert -Id 'kyc_check' `
    -Name 'KYC Verification Expert' `
    -Type Validation `
    -Execute {
        param($Context, $Parameters)
        $applicant = $Context.Data['applicant']
        $issues = @()

        if (-not $applicant['name'] -or $applicant['name'].Length -lt 2) {
            $issues += 'Invalid name'
        }
        if (-not $applicant['id_number']) {
            $issues += 'Missing ID number'
        }
        if ($applicant['age'] -lt 18) {
            $issues += 'Under 18'
        }
        if ($applicant['country'] -in @('sanctioned_country_A', 'sanctioned_country_B')) {
            $issues += 'Sanctioned country'
        }

        $Context.Data['kyc_result'] = @{
            passed = ($issues.Count -eq 0)
            issues = $issues
        }

        if ($issues.Count -gt 0) {
            return @{ Success = $true; DecisionCode = 'REJECTED'; RuleFired = "KYC_FAILED:$($issues -join ', ')" }
        }
        return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = 'KYC_PASSED' }
    }

Register-UDEOExpert -Id 'aml_screening' `
    -Name 'AML Screening Expert' `
    -Type Risk `
    -Execute {
        param($Context, $Parameters)
        $applicant = $Context.Data['applicant']

        # Simulate AML screening
        $watchlistMatch = $applicant['name'] -like '*sanctioned*'
        $highRiskCountry = $applicant['country'] -eq 'high_risk_country'
        $largeTransaction = [double]$Context.Data['transaction_amount'] -gt 100000

        if ($watchlistMatch) {
            return @{ Success = $true; DecisionCode = 'REJECTED'; RuleFired = 'WATCHLIST_MATCH' }
        }
        if ($highRiskCountry -and $largeTransaction) {
            return @{ Success = $true; DecisionCode = 'FLAGGED'; RuleFired = 'HIGH_RISK_COUNTRY_LARGE_TX' }
        }
        if ($largeTransaction) {
            return @{ Success = $true; DecisionCode = 'FLAGGED'; RuleFired = 'LARGE_TRANSACTION_REVIEW' }
        }
        return @{ Success = $true; DecisionCode = 'APPROVED'; RuleFired = 'AML_CLEAR' }
    }

# Build the compliance pipeline
$pipeline = [UDEOPipeline]::new("Customer-Onboarding")
$pipeline.Context.Data = @{
    applicant = @{
        name    = 'Alice Smithson'
        age     = 34
        id_number = 'AB123456'
        country = 'United Kingdom'
    }
    transaction_amount = 50000
    account_type = 'personal_savings'
}

# Step 1: KYC Check
$pipeline.AddStep('kyc_check', @{})

# Step 2: AML Screening
$pipeline.AddStep('aml_screening', @{})

# Step 3: Conditional human review
$pipeline.AddStep('udeo.human', @{ Reason = 'Compliance flagged for review'; OnFailure = 'continue' })

# Run
$result = $pipeline.Run()

Write-Host ""
Write-Host "=== Onboarding Result ===" -ForegroundColor Green
Write-Host "Decision: $($result.Decision)" -ForegroundColor White
Write-Host "KYC: $(if($result.Context.Data.kyc_result.passed){'PASSED'}else{'FAILED'})" -ForegroundColor White
Write-Host ""
Write-Host "=== Full Trace ===" -ForegroundColor Cyan
$result.Trace | Select-Object @{N='#';E={$_.Timestamp.ToString('HH:mm:ss')}}, ExpertId, DecisionCode, RuleFired, ExecutionTimeMs | Format-Table -AutoSize
