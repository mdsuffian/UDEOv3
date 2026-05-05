# UDEO v3.0 — Universal Deterministic Expert Orchestrator

A zero-dependency, pluggable, pure-PowerShell framework for building deterministic decision pipelines with full audit trails.

**Drop it into any workspace. Run it immediately. No setup required.**

## What UDEO Does

UDEO executes **expert pipelines** — sequences of specialized decision-making components that process data, apply rules, calculate results, and produce traceable decisions. Every step is logged. Every decision is auditable. Zero hallucinations.

Built for: regulatory compliance, loan underwriting, risk assessment, data validation, automated approvals, and any domain where **deterministic, auditable decisions** matter.

## Quick Start

```powershell
# 1. Dot-source the entry point
. .\udeo.ps1

# 2. Initialize the workspace
udeo init

# 3. Run a loan approval pipeline
udeo run loan-approval --Income=120000 --CreditScore=750 --LoanAmount=450000
```

Output:
```
=== UDEO Pipeline: LoanApproval ===
[12:34:01.234] [Info] Pipeline started: LoanApproval [pipeline_a1b2c3d4]
[12:34:01.250] [Info]   Step 1/6: udeo.validation
[12:34:01.270] [Info] Expert udeo.validation -> VALID (12ms)
[12:34:01.300] [Info]   Step 4/6: udeo.math
[12:34:01.320] [Info] Expert udeo.math -> VALID (8ms) | DTI: 25%
[12:34:01.340] [Info]   Step 5/6: udeo.risk
[12:34:01.360] [Info] Expert udeo.risk -> APPROVED (5ms) | ALL_RULES_PASSED

=== RESULT ===
  Decision: APPROVED
  Steps: 4

=== DECISION TRACE ===
ExpertId          DecisionCode  RuleFired                  ExecutionTimeMs
--------          ------------  ---------                  ---------------
udeo.validation   VALID         SCHEMA_CREDIT_SCORE_RANGE  12.00
udeo.validation   VALID         SCHEMA_POSITIVE_NUMBER      3.00
udeo.math         VALID         DTI_CALCULATED:25%          8.00
udeo.risk         APPROVED      ALL_RULES_PASSED            5.00
```

## Commands

| Command | Description |
|---|---|
| `udeo init` | Initialize workspace (creates `.udeo/` + `plugins/`) |
| `udeo run loan-approval` | Run the built-in loan approval pipeline |
| `udeo expert list` | List all registered experts |
| `udeo history` | View past pipeline runs |
| `udeo config` | Show current configuration |
| `udeo version` | Show version info |

## Programmatic Usage

```powershell
. .\udeo.ps1

# Use the built-in pipeline
$pipeline = New-UDEOLoanApprovalPipeline -Income 100000 -CreditScore 720
$result = $pipeline.Run()

# Or compose your own
$pipeline = New-UDEOPipeline "MyWorkflow"
$pipeline.Context.Data = @{ field = 'value' }
Add-UDEOPipelineStep $pipeline 'udeo.validation' @{ Field = 'field'; Required = $true }
Add-UDEOPipelineStep $pipeline 'udeo.math' @{ Operation = 'dti' }
$result = $pipeline.Run()
```

## Built-in Experts

| Expert ID | Type | Description |
|---|---|---|
| `udeo.validation` | Validation | Schema-based field validation (credit_score, positive_number, non_empty_string) |
| `udeo.math` | Math | Calculations: DTI, LTV, monthly payment, custom formulas |
| `udeo.risk` | Risk | Configurable risk rules with comparison operators (lt, gt, eq, etc.) |
| `udeo.human` | HumanReview | Escalates to human review with configurable timeout |

## Custom Experts

Drop a `.ps1` file in the `plugins/` directory. It auto-registers on startup.

```powershell
# plugins/my-expert.ps1
Register-UDEOExpert -Id 'my.expert' `
    -Name 'My Expert' `
    -Type Custom `
    -Execute {
        param($Context, $Parameters)
        # Your logic here
        return @{
            Success      = $true
            DecisionCode = 'APPROVED'
            RuleFired    = 'CUSTOM_RULE_FIRED'
        }
    }
```

See `plugins/template.ps1` for the full template.

## Architecture

```
udeo.ps1                  ← Entry point (dot-source this)
engine/
  UDEO.Engine.psm1        ← Core: Context, DecisionTrace, Logger, Config
  UDEO.Experts.psm1       ← Expert registry + 4 built-in experts
  UDEO.Pipeline.psm1      ← Pipeline orchestrator + loan-approval template
  UDEO.Store.psm1         ← JSON file persistence (.udeo/store/)
  UDEO.Telemetry.psm1     ← Spans, counters, histograms, audit log
plugins/                  ← User expert plugins (auto-discovered)
.udeo/                    ← Workspace state (auto-created by `udeo init`)
  store/                  ← Pipeline run JSON files
  config.json             ← Workspace configuration overrides
  udeo.log                ← Execution log
```

## Decision Trace

Every pipeline run produces a full decision trace — who decided what, when, based on what rule:

```json
[
  {
    "ExpertId": "udeo.validation",
    "ExpertName": "Schema Validator",
    "RuleFired": "SCHEMA_CREDIT_SCORE_RANGE:applicant.credit_score",
    "DecisionCode": "VALID",
    "Timestamp": "2026-05-04T16:00:00.000Z",
    "ExecutionTimeMs": 12.0
  },
  ...
]
```

## Examples

| File | Description |
|---|---|
| `examples/01-quickstart.ps1` | Minimal pipeline with custom expert |
| `examples/02-loan-approval.ps1` | Full loan underwriting with multiple scenarios |
| `examples/03-custom-expert.ps1` | GDPR compliance audit pipeline |
| `examples/04-compliance-check.ps1` | KYC/AML onboarding pipeline |

## Requirements

- **PowerShell 7+** (Windows, Linux, or macOS)
- Nothing else. No .NET SDK, no NuGet, no Docker, no database.

## Configuration

Workspace config: `.udeo/config.json`

```json
{
  "logLevel": "Info",
  "quiet": false,
  "pipeline": {
    "autoEscalate": true,
    "defaultTimeout": 60
  }
}
```

Environment variable: `UDEO_WORKSPACE` — override the workspace directory.

## Telemetry

```powershell
# Track spans
$spanId = Start-UDEOSpan "my_operation"
# ... do work ...
Stop-UDEOSpan "my_operation" $spanId

# Counters
Add-UDEOCounter "requests_processed" 1

# Get summary
Get-UDEOTelemetry
```

## Why Not AI/LLMs?

- **Deterministic** — same input always produces same output
- **Auditable** — every decision is logged with the rule that fired
- **Cheap** — zero API costs, runs locally in milliseconds
- **Compliant** — meets regulatory requirements (GDPR, SOX, PCI-DSS, HIPAA)
- **No hallucinations** — experts only apply defined rules

## Migration from v2.x

UDEO v3.0 is a complete rewrite. The new framework shares the same concepts (experts, pipelines, decision traces) but uses a clean PowerShell-only architecture. All v2.x C# and PowerShell code in `src/`, `tools/`, `scripts/`, `tests/` has been replaced by the `engine/` modules and `udeo.ps1` entry point.
