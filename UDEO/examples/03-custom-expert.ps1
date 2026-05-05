# Example 03: Custom Expert — Build and register your own domain expert
# Demonstrates the plugin system and custom pipeline composition.

. $PSScriptRoot/../udeo.ps1

Write-Host "=== Custom Expert Example ===" -ForegroundColor Cyan
Write-Host ""

# Register a compliance-checking expert
Register-UDEOExpert -Id 'compliance_checker' `
    -Name 'GDPR Compliance Checker' `
    -Type Validation `
    -Description 'Checks data processing for GDPR compliance requirements.' `
    -Execute {
        param($Context, $Parameters)

        $data = $Context.Data
        $violations = @()

        # Rule 1: Must have consent for processing
        if (-not $data.ContainsKey('consent_given') -or -not $data['consent_given']) {
            $violations += 'Missing explicit consent (GDPR Art. 6)'
        }

        # Rule 2: Must have data retention policy
        if (-not $data.ContainsKey('retention_days') -or [int]$data['retention_days'] -le 0) {
            $violations += 'No data retention policy (GDPR Art. 5(e))'
        }

        # Rule 3: Must not process sensitive data without explicit consent
        if ($data.ContainsKey('sensitive_data') -and $data['sensitive_data'] -and
            (-not $data.ContainsKey('explicit_consent') -or -not $data['explicit_consent'])) {
            $violations += 'Processing sensitive data without explicit consent (GDPR Art. 9)'
        }

        $Context.Data['compliance_check'] = @{
            passed     = ($violations.Count -eq 0)
            violations = $violations
            checked_at = [datetime]::UtcNow.ToString('O')
        }

        if ($violations.Count -eq 0) {
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = 'GDPR_COMPLIANT' }
        } else {
            $ruleName = "GDPR_VIOLATIONS:" + ($violations -join '; ')
            return @{ Success = $true; DecisionCode = 'INVALID'; RuleFired = $ruleName }
        }
    }

# Register a scoring expert
Register-UDEOExpert -Id 'compliance_scorer' `
    -Name 'Compliance Score Calculator' `
    -Type Math `
    -Description 'Calculates an overall compliance score from check results.' `
    -Execute {
        param($Context, $Parameters)

        $checks = $Context.Data['compliance_check']
        if (-not $checks) {
            return @{ Success = $false; Error = 'No compliance check data found' }
        }

        $totalViolations = $checks.violations.Count
        $maxViolations = $Parameters.MaxViolations -as [int]
        if (-not $maxViolations) { $maxViolations = 10 }

        $score = [math]::Round((1 - ($totalViolations / $maxViolations)) * 100, 0)
        if ($score -lt 0) { $score = 0 }

        if (-not $Context.Data.ContainsKey('calculations')) { $Context.Data['calculations'] = @{} }
        $Context.Data['calculations']['compliance_score'] = $score

        if ($score -ge 80) {
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "COMPLIANCE_HIGH:${score}%" }
        } elseif ($score -ge 50) {
            return @{ Success = $true; DecisionCode = 'FLAGGED'; RuleFired = "COMPLIANCE_MEDIUM:${score}%" }
        } else {
            return @{ Success = $true; DecisionCode = 'INVALID'; RuleFired = "COMPLIANCE_LOW:${score}%" }
        }
    }

Write-Host "Custom experts registered: compliance_checker, compliance_scorer" -ForegroundColor Green
Write-Host ""

# Build a custom pipeline
$pipeline = [UDEOPipeline]::new("GDPR-Compliance-Audit")

# Seed context with data processing info
$pipeline.Context.Data = @{
    processor_name  = 'Acme Analytics Ltd'
    data_type       = 'customer_purchase_history'
    consent_given   = $true
    retention_days  = 365
    sensitive_data  = $false
    explicit_consent = $false
    data_subjects   = 15000
    third_party_share = $false
}

# Step 1: Run compliance check
$pipeline.AddStep('compliance_checker', @{})

# Step 2: Calculate compliance score
$pipeline.AddStep('compliance_scorer', @{ MaxViolations = 10 })

# Step 3: If flagged, route to human
$pipeline.AddConditionalStep('udeo.human', @{
    Reason = 'Compliance score below threshold'
    OnFailure = 'continue'
}, {
    param($Context)
    $score = $Context.Data['calculations']?.compliance_score
    return $score -ge 80  # Only route to human if score < 80
})

# Run
$result = $pipeline.Run()

Write-Host ""
Write-Host "=== Compliance Audit Result ===" -ForegroundColor Green
Write-Host "Decision: $($result.Decision)" -ForegroundColor White
Write-Host "Compliance Score: $($result.Context.Data.calculations.compliance_score)%" -ForegroundColor White
if ($result.Context.Data.compliance_check.violations.Count -gt 0) {
    Write-Host "Violations:" -ForegroundColor Red
    $result.Context.Data.compliance_check.violations | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
} else {
    Write-Host "No violations found." -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Trace ===" -ForegroundColor Cyan
$result.Trace | Format-Table ExpertId, DecisionCode, ExecutionTimeMs -AutoSize
