# UDEO.Engine.psm1 — v3.0
# Core engine: Context, DecisionTrace, Logger, Configuration
# Zero dependencies. Pure PowerShell 7+.

# ============================================================
# ENUMS
# ============================================================
enum UDEOExpertType {
    Rule
    Math
    Validation
    Risk
    HumanReview
    Custom
}

enum UDEODecisionCode {
    APPROVED
    REJECTED
    FLAGGED
    ROUTE_TO_HUMAN
    VALID
    INVALID
    PENDING
    ERROR
}

enum UDEOPipelineStatus {
    Pending
    Running
    Completed
    Failed
    Cancelled
}

# ============================================================
# CLASSES
# ============================================================

class UDEODecisionTrace {
    [string]$ExpertId
    [string]$ExpertName
    [string]$RuleFired
    [string]$DecisionCode
    [datetime]$Timestamp
    [double]$ExecutionTimeMs
    [hashtable]$Metadata

    UDEODecisionTrace([string]$expertId, [string]$expertName, [string]$ruleFired, [string]$decisionCode, [double]$executionTimeMs) {
        $this.ExpertId = $expertId
        $this.ExpertName = $expertName
        $this.RuleFired = $ruleFired
        $this.DecisionCode = $decisionCode
        $this.Timestamp = [datetime]::UtcNow
        $this.ExecutionTimeMs = $executionTimeMs
        $this.Metadata = @{}
    }
}

class UDEOContext {
    [string]$PipelineId
    [string]$CorrelationId
    [int]$Step
    [hashtable]$Data
    [System.Collections.Generic.List[UDEODecisionTrace]]$DecisionTrace
    [datetime]$CreatedAt
    [datetime]$UpdatedAt

    UDEOContext([string]$pipelineId) {
        $this.PipelineId = $pipelineId
        $this.CorrelationId = [Guid]::NewGuid().ToString()
        $this.Step = 0
        $this.Data = @{}
        $this.DecisionTrace = [System.Collections.Generic.List[UDEODecisionTrace]]::new()
        $this.CreatedAt = [datetime]::UtcNow
        $this.UpdatedAt = [datetime]::UtcNow
    }

    [void] RecordDecision([UDEODecisionTrace]$trace) {
        $this.Step++
        $this.DecisionTrace.Add($trace)
        $this.UpdatedAt = [datetime]::UtcNow
    }

    [UDEODecisionTrace[]] GetTrace() {
        return $this.DecisionTrace.ToArray()
    }

    [hashtable] ToHashtable() {
        $trace = @()
        foreach ($t in $this.DecisionTrace) {
            $trace += @{
                ExpertId       = $t.ExpertId
                ExpertName     = $t.ExpertName
                RuleFired      = $t.RuleFired
                DecisionCode   = $t.DecisionCode
                Timestamp      = $t.Timestamp.ToString('O')
                ExecutionTimeMs = $t.ExecutionTimeMs
            }
        }
        return @{
            PipelineId     = $this.PipelineId
            CorrelationId  = $this.CorrelationId
            Step           = $this.Step
            Data           = $this.Data
            DecisionTrace  = $trace
            CreatedAt      = $this.CreatedAt.ToString('O')
            UpdatedAt      = $this.UpdatedAt.ToString('O')
        }
    }
}

class UDEOExpertContract {
    [string]$Id
    [string]$Name
    [string]$Version
    [UDEOExpertType]$Type
    [string]$Description
    [int]$TimeoutSeconds
    [scriptblock]$Execute
    [scriptblock]$HealthCheck

    UDEOExpertContract([string]$id, [string]$name, [UDEOExpertType]$type, [scriptblock]$execute) {
        $this.Id = $id
        $this.Name = $name
        $this.Version = '3.0.0'
        $this.Type = $type
        $this.Description = ''
        $this.TimeoutSeconds = 30
        $this.Execute = $execute
        $this.HealthCheck = { return $true }
    }
}

# ============================================================
# LOGGER
# ============================================================
class UDEOLogger {
    static [string]$LogLevel = 'Info'
    static [string]$LogFile = $null
    static [bool]$Quiet = $false
    static [bool]$UseColors = $true

    static [void] Configure([string]$level, [string]$logFile, [bool]$quiet) {
        [UDEOLogger]::LogLevel = $level
        [UDEOLogger]::LogFile = $logFile
        [UDEOLogger]::Quiet = $quiet
    }

    static [void] Write([string]$level, [string]$message, [hashtable]$data) {
        $levels = @{ Trace = 0; Debug = 1; Info = 2; Warn = 3; Error = 4 }
        $currentWeight = $levels[[UDEOLogger]::LogLevel]
        $msgWeight = $levels[$level]
        if ($msgWeight -lt $currentWeight) { return }

        $ts = [datetime]::UtcNow.ToString('HH:mm:ss.fff')
        $line = "[$ts] [$level] $message"

        if (-not [UDEOLogger]::Quiet) {
            if ([UDEOLogger]::UseColors) {
                switch ($level) {
                    'Error' { Write-Host $line -ForegroundColor Red }
                    'Warn'  { Write-Host $line -ForegroundColor Yellow }
                    'Info'  { Write-Host $line -ForegroundColor Cyan }
                    'Debug' { Write-Host $line -ForegroundColor DarkGray }
                    default { Write-Host $line }
                }
            } else {
                Write-Host $line
            }
            if ($data) {
                $json = $data | ConvertTo-Json -Compress -Depth 3
                Write-Host "  $json" -ForegroundColor DarkGray
            }
        }

        if ([UDEOLogger]::LogFile) {
            $logPath = [UDEOLogger]::LogFile
            $dir = Split-Path $logPath -Parent
            if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
            "$line" | Out-File -FilePath $logPath -Append -Encoding utf8
        }
    }

    static [void] Info([string]$msg, $data)  { [UDEOLogger]::Write('Info', $msg, $data) }
    static [void] Info([string]$msg)          { [UDEOLogger]::Write('Info', $msg, $null) }
    static [void] Debug([string]$msg, $data)  { [UDEOLogger]::Write('Debug', $msg, $data) }
    static [void] Debug([string]$msg)         { [UDEOLogger]::Write('Debug', $msg, $null) }
    static [void] Warn([string]$msg, $data)   { [UDEOLogger]::Write('Warn', $msg, $data) }
    static [void] Warn([string]$msg)          { [UDEOLogger]::Write('Warn', $msg, $null) }
    static [void] Error([string]$msg, $data)  { [UDEOLogger]::Write('Error', $msg, $data) }
    static [void] Error([string]$msg)         { [UDEOLogger]::Write('Error', $msg, $null) }
}

# ============================================================
# CONFIGURATION
# ============================================================
class UDEOConfig {
    static [string]$WorkspaceRoot
    static [hashtable]$Data = @{}

    static [void] Load([string]$workspaceRoot) {
        [UDEOConfig]::WorkspaceRoot = $workspaceRoot
        $defaultConfig = @{
            version   = '3.0.0'
            storePath = Join-Path (Join-Path $workspaceRoot '.udeo') 'store'
            logLevel  = 'Info'
            quiet     = $false
            experts   = @{
                timeoutSeconds   = 30
                pluginDirectory  = Join-Path $workspaceRoot 'plugins'
            }
            pipeline  = @{
                maxRetries       = 2
                defaultTimeout   = 60
            }
        }

        # Load workspace overrides if present
        $configFile = Join-Path (Join-Path $workspaceRoot '.udeo') 'config.json'
        if (Test-Path $configFile) {
            try {
                $override = Get-Content $configFile -Raw | ConvertFrom-Json -AsHashtable
                foreach ($k in $override.Keys) {
                    if ($defaultConfig.ContainsKey($k) -and $defaultConfig[$k] -is [hashtable]) {
                        foreach ($sk in $override[$k].Keys) {
                            $defaultConfig[$k][$sk] = $override[$k][$sk]
                        }
                    } else {
                        $defaultConfig[$k] = $override[$k]
                    }
                }
            } catch {
                [UDEOLogger]::Warn("Failed to parse .udeo/config.json, using defaults")
            }
        }

        [UDEOConfig]::Data = $defaultConfig
        [UDEOLogger]::Debug("UDEO config loaded: workspace=$workspaceRoot")
    }

    static [object] Get([string]$path) {
        $parts = $path.Split('.')
        $current = [UDEOConfig]::Data
        foreach ($p in $parts) {
            if ($current -is [hashtable] -and $current.ContainsKey($p)) {
                $current = $current[$p]
            } else {
                return $null
            }
        }
        return $current
    }
}

# Types loaded: UDEOContext, UDEODecisionTrace, UDEOExpertContract, UDEOLogger, UDEOConfig
# Enums loaded: UDEOExpertType, UDEODecisionCode, UDEOPipelineStatus