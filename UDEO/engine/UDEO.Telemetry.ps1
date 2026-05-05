# UDEO.Telemetry.psm1 — v3.0
# Observability: tracing spans, counters, histograms, audit log.

class UDEOTelemetry {
    static [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[hashtable]]]$Spans = [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[hashtable]]]::new()
    static [System.Collections.Generic.Dictionary[string, double]]$Counters = [System.Collections.Generic.Dictionary[string, double]]::new()
    static [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[double]]]$Histograms = [System.Collections.Generic.Dictionary[string, System.Collections.Generic.List[double]]]::new()
    static [string]$AuditPath

    # --- Tracing ---
    static [string] StartSpan([string]$name) {
        $id = [Guid]::NewGuid().ToString().Substring(0, 8)
        $span = @{
            Id        = $id
            Name      = $name
            StartedAt = [datetime]::UtcNow
            EndedAt   = $null
            DurationMs = $null
            Metadata  = @{}
        }
        if (-not [UDEOTelemetry]::Spans.ContainsKey($name)) {
            [UDEOTelemetry]::Spans[$name] = [System.Collections.Generic.List[hashtable]]::new()
        }
        [UDEOTelemetry]::Spans[$name].Add($span)
        return $id
    }

    static [void] EndSpan([string]$name, [string]$id) {
        $spanList = $null
        if ([UDEOTelemetry]::Spans.TryGetValue($name, [ref]$spanList)) {
            foreach ($s in $spanList) {
                if ($s.Id -eq $id -and -not $s.EndedAt) {
                    $s.EndedAt = [datetime]::UtcNow
                    $s.DurationMs = [math]::Round(($s.EndedAt - $s.StartedAt).TotalMilliseconds, 2)
                    break
                }
            }
        }
    }

    # --- Metrics ---
    static [void] Inc([string]$name, [double]$delta = 1) {
        $val = 0
        if ([UDEOTelemetry]::Counters.TryGetValue($name, [ref]$val)) {
            [UDEOTelemetry]::Counters[$name] = $val + $delta
        } else {
            [UDEOTelemetry]::Counters[$name] = $delta
        }
    }

    static [void] Record([string]$name, [double]$value) {
        if (-not [UDEOTelemetry]::Histograms.ContainsKey($name)) {
            [UDEOTelemetry]::Histograms[$name] = [System.Collections.Generic.List[double]]::new()
        }
        [UDEOTelemetry]::Histograms[$name].Add($value)
    }

    # --- Audit ---
    static [void] InitializeAudit([string]$path) {
        [UDEOTelemetry]::AuditPath = $path
        $dir = Split-Path $path -Parent
        if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    }

    static [void] Audit([string]$event, [hashtable]$details) {
        if (-not [UDEOTelemetry]::AuditPath) { return }
        $entry = @{
            Timestamp = [datetime]::UtcNow.ToString('O')
            Event     = $event
            Details   = $details
        }
        ($entry | ConvertTo-Json -Compress -Depth 3) | Out-File -FilePath [UDEOTelemetry]::AuditPath -Append -Encoding utf8
    }

    # --- Summary ---
    static [hashtable] Summary() {
        $spansSummary = @{}
        foreach ($k in [UDEOTelemetry]::Spans.Keys) {
            $spanArr = [UDEOTelemetry]::Spans[$k]
            $spansSummary[$k] = @{
                Count       = $spanArr.Count
                TotalMs     = [math]::Round(($spanArr | Where-Object { $_.DurationMs } | Measure-Object -Property DurationMs -Sum).Sum, 2)
                AvgMs       = [math]::Round(($spanArr | Where-Object { $_.DurationMs } | Measure-Object -Property DurationMs -Average).Average, 2)
            }
        }
        return @{
            Spans     = $spansSummary
            Counters  = [UDEOTelemetry]::Counters | ConvertTo-Json -Compress
            Histograms = [UDEOTelemetry]::Histograms.Keys | ForEach-Object {
                $vals = [UDEOTelemetry]::Histograms[$_]
                @{ Name = $_ ; Count = $vals.Count ; Avg = [math]::Round(($vals | Measure-Object -Average).Average, 2) }
            }
        }
    }

    static [void] Reset() {
        [UDEOTelemetry]::Spans.Clear()
        [UDEOTelemetry]::Counters.Clear()
        [UDEOTelemetry]::Histograms.Clear()
    }
}

# ============================================================
# PUBLIC FUNCTIONS
# ============================================================
function Start-UDEOSpan {
    param([Parameter(Mandatory)] [string]$Name)
    return [UDEOTelemetry]::StartSpan($Name)
}
function Stop-UDEOSpan {
    param([Parameter(Mandatory)] [string]$Name, [Parameter(Mandatory)] [string]$Id)
    [UDEOTelemetry]::EndSpan($Name, $Id)
}
function Add-UDEOCounter {
    param([Parameter(Mandatory)] [string]$Name, [double]$Value = 1)
    [UDEOTelemetry]::Inc($Name, $Value)
}
function Add-UDEOMetric {
    param([Parameter(Mandatory)] [string]$Name, [double]$Value)
    [UDEOTelemetry]::Record($Name, $Value)
}
function Write-UDEOAudit {
    param([Parameter(Mandatory)] [string]$Event, [hashtable]$Details = @{})
    [UDEOTelemetry]::Audit($Event, $Details)
}
function Get-UDEOTelemetry {
    return [UDEOTelemetry]::Summary()
}

# Functions exported: Start-UDEOSpan, Stop-UDEOSpan, Add-UDEOCounter, Add-UDEOMetric, Write-UDEOAudit, Get-UDEOTelemetry
