# Example 01: Quickstart — Minimal UDEO Pipeline
# Demonstrates the simplest possible usage.

. $PSScriptRoot/../udeo.ps1

Write-Host "=== UDEO Quickstart Example ===" -ForegroundColor Cyan
Write-Host ""

# Register a minimal custom expert inline
Register-UDEOExpert -Id 'greeter' -Name 'Greeting Expert' -Type Custom -Execute {
    param($Context, $Parameters)
    [UDEOLogger]::Info("  Greeter says: Hello, $($Parameters.Name)!")
    $Context.Data['greeting'] = "Hello, $($Parameters.Name)!"
    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = 'GREETING_DELIVERED' }
}

# Create and run a pipeline
$pipeline = [UDEOPipeline]::new("Quickstart")
$pipeline.Context.Data['message'] = 'UDEO works!'
$pipeline.AddStep('greeter', @{ Name = 'World' })
$pipeline.AddStep('udeo.validation', @{ Field = 'message'; Schema = 'non_empty_string'; Required = $true })

$result = $pipeline.Run()

Write-Host ""
Write-Host "=== Result ===" -ForegroundColor Green
Write-Host "Decision: $($result.Decision)" -ForegroundColor White
Write-Host "Trace entries: $($result.Trace.Count)" -ForegroundColor White
Write-Host "Context greeting: $($result.Context.Data['greeting'])" -ForegroundColor White

# Show telemetry
$tele = Get-UDEOTelemetry
Write-Host ""
Write-Host "=== Telemetry ===" -ForegroundColor Cyan
$tele | ConvertTo-Json -Depth 3
