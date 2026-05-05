# UDEO.Store.psm1 — v3.0
# JSON file-based state persistence. Human-readable, zero setup.

class UDEOStore {
    static [string]$RootPath
    static [bool]$IsReady = $false

    static [void] Initialize([string]$rootPath) {
        [UDEOStore]::RootPath = $rootPath
        if (-not (Test-Path $rootPath)) {
            New-Item -ItemType Directory -Path $rootPath -Force | Out-Null
        }
        [UDEOStore]::IsReady = $true
        [UDEOLogger]::Debug("Store initialized: $rootPath")
    }

    static [void] Save([UDEOContext]$context) {
        if (-not [UDEOStore]::IsReady) {
            [UDEOLogger]::Warn("Store not initialized, cannot save context $($context.PipelineId)")
            return
        }
        $root = [UDEOStore]::RootPath
        $file = Join-Path $root "$($context.PipelineId).json"
        $data = $context.ToHashtable()
        $json = $data | ConvertTo-Json -Depth 6 -Compress
        Set-Content -Path $file -Value $json -Encoding utf8
        [UDEOLogger]::Debug("Saved: $file")
    }

    static [hashtable] Load([string]$pipelineId) {
        if (-not [UDEOStore]::IsReady) {
            [UDEOLogger]::Warn("Store not initialized, cannot load context $pipelineId")
            return $null
        }
        $root = [UDEOStore]::RootPath
        $file = Join-Path $root "$pipelineId.json"
        if (-not (Test-Path $file)) {
            [UDEOLogger]::Debug("Context not found: $pipelineId")
            return $null
        }
        try {
            $json = Get-Content $file -Raw
            return $json | ConvertFrom-Json -AsHashtable
        } catch {
            [UDEOLogger]::Error("Failed to load context: $($_.Exception.Message)")
            return $null
        }
    }

    static [string[]] List() {
        if (-not [UDEOStore]::IsReady) { return @() }
        $files = Get-ChildItem -Path [UDEOStore]::RootPath -Filter '*.json' | ForEach-Object { $_.BaseName }
        return @($files)
    }

    static [void] Delete([string]$pipelineId) {
        if (-not [UDEOStore]::IsReady) { return }
        $root = [UDEOStore]::RootPath
        $file = Join-Path $root "$pipelineId.json"
        if (Test-Path $file) {
            Remove-Item $file -Force
            [UDEOLogger]::Debug("Deleted: $pipelineId")
        }
    }

    static [void] Purge() {
        if (-not [UDEOStore]::IsReady) { return }
        $files = Get-ChildItem -Path [UDEOStore]::RootPath -Filter '*.json'
        foreach ($f in $files) { Remove-Item $f.FullName -Force }
        [UDEOLogger]::Info("Store purged: $($files.Count) files removed")
    }
}

# ============================================================
# PUBLIC FUNCTIONS
# ============================================================
function Initialize-UDEOStore {
    param([string]$Path = (Join-Path (Join-Path $PWD '.udeo') 'store'))
    [UDEOStore]::Initialize($Path)
}

function Save-UDEORun {
    param([Parameter(Mandatory)] [UDEOContext]$Context)
    [UDEOStore]::Save($Context)
}

function Get-UDEORun {
    param([string]$PipelineId)
    [UDEOStore]::Load($PipelineId)
}

function Get-UDEORuns {
    [UDEOStore]::List()
}

# Functions exported: Initialize-UDEOStore, Save-UDEORun, Get-UDEORun, Get-UDEORuns
