# udeo.ps1 — UDEO v3.0 CLI Entry Point
# Usage: . .\udeo.ps1          # dot-source to load
#        udeo init              # initialize workspace
#        udeo run <pipeline>    # run a pipeline
#        udeo expert list       # list experts
#        udeo history           # view past runs

param(
    [Parameter(Position = 0)]
    [ValidateSet('init', 'run', 'expert', 'workflow', 'history', 'config', 'version', 'help')]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$SubCommand,

    [Parameter(ValueFromRemainingArguments)]
    [string[]]$Remaining
)

# ============================================================
# BOOTSTRAP: Load all engine modules
# ============================================================
$UDEORoot = Split-Path $PSCommandPath -Parent
$enginePath = Join-Path $UDEORoot 'engine'

$modules = @(
    'UDEO.Engine.ps1'
    'UDEO.Experts.ps1'
    'UDEO.Pipeline.ps1'
    'UDEO.Store.ps1'
    'UDEO.Telemetry.ps1'
)

foreach ($m in $modules) {
    $path = Join-Path $enginePath $m
    if (Test-Path $path) {
        . $path
    } else {
        Write-Error "Engine file not found: $path"
        return
    }
}

# ============================================================
# CONFIGURE
# ============================================================
$workspace = if ($env:UDEO_WORKSPACE) { $env:UDEO_WORKSPACE } else { $PWD.Path }
[UDEOConfig]::Load($workspace)
[UDEOStore]::Initialize((Join-Path (Join-Path $workspace '.udeo') 'store'))
[UDEOLogger]::Configure([UDEOConfig]::Get('logLevel'), (Join-Path (Join-Path $workspace '.udeo') 'udeo.log'), [UDEOConfig]::Get('quiet'))

# Discover plugins
$pluginDir = Join-Path $workspace 'plugins'
if (-not (Test-Path $pluginDir)) { $pluginDir = Join-Path $UDEORoot 'plugins' }
[UDEOExpertRegistry]::DiscoverPlugins($pluginDir)

# ============================================================
# COMMANDS
# ============================================================

function udeo {
    param([string]$Cmd, [string]$Sub, [string[]]$Args)

    switch ($Cmd) {
        'init' {
            Write-Host "Initializing UDEO workspace at: $workspace" -ForegroundColor Cyan
            $udeoDir = Join-Path $workspace '.udeo'
            New-Item -ItemType Directory -Force -Path (Join-Path $udeoDir 'store') | Out-Null

            $configFile = Join-Path $udeoDir 'config.json'
            if (-not (Test-Path $configFile)) {
                @{
                    logLevel = 'Info'
                    quiet    = $false
                    pipeline = @{ autoEscalate = $true; defaultTimeout = 60 }
                } | ConvertTo-Json -Depth 3 | Set-Content $configFile -Encoding utf8
                Write-Host "  Created: $configFile" -ForegroundColor Green
            } else {
                Write-Host "  Config already exists: $configFile" -ForegroundColor Yellow
            }

            $pluginsDir = Join-Path $workspace 'plugins'
            if (-not (Test-Path $pluginsDir)) {
                New-Item -ItemType Directory -Path $pluginsDir -Force | Out-Null
                Copy-Item (Join-Path (Join-Path $UDEORoot 'plugins') 'template.ps1') $pluginsDir -ErrorAction SilentlyContinue
                Write-Host "  Created: $pluginsDir" -ForegroundColor Green
            }

            Write-Host "UDEO workspace ready." -ForegroundColor Green
            Write-Host "Next: udeo run loan-approval" -ForegroundColor Cyan
        }

        'run' {
            $name = if ($Sub) { $Sub } else { 'custom' }
            $params = @{}
            foreach ($a in $Args) {
                if ($a -match '^--?([^=]+)=?(.*)$') {
                    $key = $Matches[1] -replace '-(\w)', { $_.Groups[1].Value.ToUpper() }
                    $val = if ($Matches[2]) { $Matches[2] } else { $true }
                    $params[$key] = $val
                }
            }

            switch ($name) {
                'loan-approval' {
                    $pipeline = New-UDEOLoanApprovalPipeline @params
                }
                'loan' {
                    $pipeline = New-UDEOLoanApprovalPipeline @params
                }
                default {
                    Write-Host "Unknown pipeline: $name" -ForegroundColor Red
                    Write-Host "Available: loan-approval" -ForegroundColor Yellow
                    return
                }
            }

            Write-Host "`n=== UDEO Pipeline: $($pipeline.Name) ===" -ForegroundColor Cyan
            Write-Host "Pipeline ID: $($pipeline.Id)" -ForegroundColor DarkGray

            $result = $pipeline.Run()

            Write-Host "`n=== RESULT ===" -ForegroundColor Cyan
            switch ($result.Decision) {
                'APPROVED' {
                    Write-Host "  Decision: APPROVED" -ForegroundColor Green
                }
                'REJECTED' {
                    Write-Host "  Decision: REJECTED" -ForegroundColor Red
                }
                'FLAGGED' {
                    Write-Host "  Decision: FLAGGED" -ForegroundColor Yellow
                }
                'ROUTE_TO_HUMAN' {
                    Write-Host "  Decision: ROUTE TO HUMAN" -ForegroundColor Magenta
                }
                default {
                    Write-Host "  Decision: $($result.Decision)" -ForegroundColor White
                }
            }
            if ($result.Reason) {
                Write-Host "  Reason: $($result.Reason)" -ForegroundColor White
            }
            Write-Host "  Steps: $($result.Trace.Count)" -ForegroundColor White

            Write-Host "`n=== DECISION TRACE ===" -ForegroundColor Cyan
            $result.Trace | Format-Table ExpertId, DecisionCode, RuleFired, ExecutionTimeMs -AutoSize

            # Save to store
            if ($result.Context) {
                [UDEOStore]::Save($result.Context)
                Write-Host "Saved run: $($result.Context.PipelineId)" -ForegroundColor DarkGray
            }
        }

        'expert' {
            switch ($Sub) {
                'list' {
                    $experts = [UDEOExpertRegistry]::GetAll()
                    Write-Host "`nRegistered Experts:" -ForegroundColor Cyan
                    $experts | Format-Table Id, Name, Type, Version -AutoSize
                }
                'register' {
                    Write-Host "To register a custom expert, drop a .ps1 file in: plugins/" -ForegroundColor Yellow
                    Write-Host "See: plugins/template.ps1 for the format." -ForegroundColor Yellow
                }
                default {
                    Write-Host "Usage: udeo expert <list|register>" -ForegroundColor Yellow
                }
            }
        }

        'workflow' {
            Write-Host "Workflow commands:" -ForegroundColor Cyan
            Write-Host "  udeo run loan-approval    Run the loan approval pipeline" -ForegroundColor White
            Write-Host "  udeo workflow create <name>   Create a custom workflow (TODO)" -ForegroundColor White
        }

        'history' {
            $runs = [UDEOStore]::List()
            Write-Host "`nPast Runs:" -ForegroundColor Cyan
            if ($runs.Count -eq 0) {
                Write-Host "  No runs found." -ForegroundColor DarkGray
                return
            }
            foreach ($id in $runs) {
                $data = [UDEOStore]::Load($id)
                $decision = if ($data.DecisionTrace) {
                    $data.DecisionTrace[-1].DecisionCode
                } else { 'UNKNOWN' }
                $ts = if ($data.UpdatedAt) { $data.UpdatedAt.Substring(0, 19) } else { 'unknown' }
                Write-Host "  $id  [$decision]  $ts" -ForegroundColor White
            }
            Write-Host "`nView a specific run: Get-UDEORun -PipelineId '<id>'" -ForegroundColor DarkGray
        }

        'config' {
            Write-Host "`nUDEO Configuration:" -ForegroundColor Cyan
            Write-Host "  Workspace: $workspace" -ForegroundColor White
            Write-Host "  Store:     $([UDEOConfig]::Get('storePath'))" -ForegroundColor White
            Write-Host "  LogLevel:  $([UDEOConfig]::Get('logLevel'))" -ForegroundColor White
            Write-Host "  Plugins:   $([UDEOConfig]::Get('experts.pluginDirectory'))" -ForegroundColor White
            Write-Host "`nFull config: .udeo/config.json" -ForegroundColor DarkGray
        }

        'version' {
            Write-Host "UDEO v3.0.0 - Universal Deterministic Expert Orchestrator" -ForegroundColor Cyan
            Write-Host "Zero-dependency, pluggable expert pipeline framework." -ForegroundColor White
        }

        'help' {
            Write-Host @"

UDEO v3.0 - Universal Deterministic Expert Orchestrator
========================================================

COMMANDS:
  udeo init                  Initialize workspace (creates .udeo/ + plugins/)
  udeo run loan-approval     Run the loan approval pipeline
  udeo expert list           List all registered experts
  udeo history               View past pipeline runs
  udeo config                Show current configuration
  udeo version               Show version info
  udeo help                  Show this help

PIPELINE PARAMETERS:
  udeo run loan-approval --Income=100000 --CreditScore=680 --LoanAmount=250000

PROGRAMMATIC USAGE:
  . .\udeo.ps1
  `$p = New-UDEOLoanApprovalPipeline -Income 100000 -CreditScore 720
  `$result = `$p.Run()

ADD CUSTOM EXPERTS:
  Drop a .ps1 file in plugins/ using the format from plugins/template.ps1

"@
        }

        default {
            Write-Host "UDEO v3.0 - Use 'udeo help' for commands" -ForegroundColor Cyan
        }
    }
}

# Run the command
if ($Command) {
    udeo $Command $SubCommand $Remaining
} else {
    Write-Host "UDEO v3.0 - Universal Deterministic Expert Orchestrator" -ForegroundColor Cyan
    Write-Host "Loaded $($modules.Count) engine modules, $([UDEOExpertRegistry]::GetAll().Count) experts." -ForegroundColor DarkGray
    Write-Host "Use 'udeo help' for commands, 'udeo run loan-approval' to try a pipeline." -ForegroundColor DarkGray
}
