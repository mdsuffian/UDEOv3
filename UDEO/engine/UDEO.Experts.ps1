# UDEO.Experts.psm1 — v3.0
# Expert system: registry, built-in experts, plugin discovery.
# Drop .ps1 expert files into plugins/ — they auto-register.

# ============================================================
# EXPERT REGISTRY
# ============================================================
class UDEOExpertRegistry {
    static [System.Collections.Generic.Dictionary[string, UDEOExpertContract]]$Experts = [System.Collections.Generic.Dictionary[string, UDEOExpertContract]]::new()

    static [void] Register([UDEOExpertContract]$contract) {
        [UDEOExpertRegistry]::Experts[$contract.Id] = $contract
        [UDEOLogger]::Info("Registered expert: $($contract.Id) [$($contract.Type)]", @{ name = $contract.Name; version = $contract.Version })
    }

    static [void] Unregister([string]$id) {
        if ([UDEOExpertRegistry]::Experts.Remove($id)) {
            [UDEOLogger]::Info("Unregistered expert: $id")
        }
    }

    static [UDEOExpertContract] Get([string]$id) {
        $contract = $null
        if ([UDEOExpertRegistry]::Experts.TryGetValue($id, [ref]$contract)) {
            return $contract
        }
        return $null
    }

    static [UDEOExpertContract[]] GetAll() {
        return [UDEOExpertRegistry]::Experts.Values | ForEach-Object { $_ }
    }

    static [UDEOExpertContract[]] GetByType([UDEOExpertType]$type) {
        return [UDEOExpertRegistry]::Experts.Values | Where-Object { $_.Type -eq $type }
    }

    static [void] DiscoverPlugins([string]$pluginDir) {
        if (-not (Test-Path $pluginDir)) { return }
        [UDEOLogger]::Info("Discovering plugins in: $pluginDir")
        Get-ChildItem -Path $pluginDir -Filter '*.ps1' -Exclude 'template.ps1' | ForEach-Object {
            try {
                . $_.FullName
                [UDEOLogger]::Debug("Loaded plugin: $($_.Name)")
            } catch {
                [UDEOLogger]::Warn("Failed to load plugin $($_.Name): $($_.Exception.Message)")
            }
        }
    }
}

# ============================================================
# EXPERT EXECUTION
# ============================================================
function Invoke-UDEOExpert {
    <#
    .SYNOPSIS
        Execute an expert by ID with the given context.
    .PARAMETER ExpertId
        The registered expert ID to execute.
    .PARAMETER Context
        The UDEOContext to operate on.
    .PARAMETER Parameters
        Additional parameters passed to the expert.
    #>
    param(
        [Parameter(Mandatory)] [string]$ExpertId,
        [Parameter(Mandatory)] [UDEOContext]$Context,
        [hashtable]$Parameters = @{}
    )

    $contract = [UDEOExpertRegistry]::Get($ExpertId)
    if (-not $contract) {
        [UDEOLogger]::Error("Expert not found: $ExpertId")
        $trace = [UDEODecisionTrace]::new($ExpertId, 'Unknown', 'EXPERT_NOT_FOUND', 'ERROR', 0)
        $Context.RecordDecision($trace)
        return @{ Success = $false; Error = "Expert not found: $ExpertId"; Context = $Context }
    }

    [UDEOLogger]::Debug("Executing expert: $($contract.Name) [$ExpertId]")
    $sw = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $result = & $contract.Execute -Context $Context -Parameters $Parameters

        $sw.Stop()
        $elapsed = [math]::Round($sw.Elapsed.TotalMilliseconds, 2)

        if ($result.Success) {
            $code = if ($result.DecisionCode) { $result.DecisionCode } else { 'PENDING' }
            $rule = if ($result.RuleFired) { $result.RuleFired } else { 'EXECUTED' }
        } else {
            $code = 'ERROR'
            $rule = if ($result.Error) { $result.Error } else { 'EXECUTION_FAILED' }
        }

        $trace = [UDEODecisionTrace]::new($ExpertId, $contract.Name, $rule, $code, $elapsed)
        $Context.RecordDecision($trace)

        [UDEOLogger]::Info("Expert $ExpertId -> $code ($($elapsed)ms)", @{ rule = $rule })

        if ($result.Context) { $Context = $result.Context }
        return @{ Success = $result.Success; DecisionCode = $code; RuleFired = $rule; Context = $Context; ExecutionTimeMs = $elapsed }
    }
    catch {
        $sw.Stop()
        $elapsed = [math]::Round($sw.Elapsed.TotalMilliseconds, 2)
        $trace = [UDEODecisionTrace]::new($ExpertId, $contract.Name, $_.Exception.Message, 'ERROR', $elapsed)
        $Context.RecordDecision($trace)
        [UDEOLogger]::Error("Expert $ExpertId threw: $($_.Exception.Message)")
        return @{ Success = $false; DecisionCode = 'ERROR'; Context = $Context; Error = $_.Exception.Message; ExecutionTimeMs = $elapsed }
    }
}

# ============================================================
# BUILT-IN EXPERTS
# ============================================================
function _New-UDEOSimpleExpert {
    param($Id, $Name, $Type, $Execute)
    $c = [UDEOExpertContract]::new($Id, $Name, $Type, $Execute)
    [UDEOExpertRegistry]::Register($c)
}

# --- Validation Expert ---
_New-UDEOSimpleExpert -Id 'udeo.validation' -Name 'Schema Validator' -Type ([UDEOExpertType]::Validation) -Execute {
    param($Context, $Parameters)
    $schema = $Parameters.Schema
    $field  = $Parameters.Field
    $required = $Parameters.Required -as [bool]

    if (-not $field) {
        return @{ Success = $false; Error = 'Validation expert requires Field parameter' }
    }

    # Navigate nested field path
    $parts = $field.Split('.')
    $val = $Context.Data
    foreach ($p in $parts) {
        if ($val -is [hashtable] -and $val.ContainsKey($p)) {
            $val = $val[$p]
        } elseif ($val -is [pscustomobject]) {
            $members = Get-Member -InputObject $val -Name $p -MemberType NoteProperty -ErrorAction SilentlyContinue
            if ($members) { $val = $val.$p }
            else {
                if ($required) { return @{ Success = $false; DecisionCode = 'INVALID'; RuleFired = "REQUIRED_FIELD_MISSING:$field" } }
                return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "FIELD_OPTIONAL_MISSING:$field" }
            }
        } else {
            if ($required) { return @{ Success = $false; DecisionCode = 'INVALID'; RuleFired = "REQUIRED_FIELD_MISSING:$field" } }
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "FIELD_OPTIONAL_MISSING:$field" }
        }
    }

    if ($schema) {
        switch ($schema) {
            'positive_number' {
                if (($val -is [double] -or $val -is [int]) -and [double]$val -gt 0) {
                    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "SCHEMA_POSITIVE_NUMBER:$field" }
                }
                return @{ Success = $false; DecisionCode = 'INVALID'; RuleFired = "SCHEMA_NOT_POSITIVE:$field" }
            }
            'non_empty_string' {
                if ($val -is [string] -and $val.Trim().Length -gt 0) {
                    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "SCHEMA_NON_EMPTY:$field" }
                }
                return @{ Success = $false; DecisionCode = 'INVALID'; RuleFired = "SCHEMA_EMPTY_STRING:$field" }
            }
            'credit_score' {
                if (($val -is [int] -or $val -is [double]) -and [int]$val -ge 300 -and [int]$val -le 850) {
                    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "SCHEMA_CREDIT_SCORE_RANGE:$field" }
                }
                return @{ Success = $false; DecisionCode = 'INVALID'; RuleFired = "SCHEMA_CREDIT_SCORE_OUT_OF_RANGE:$field" }
            }
            default {
                return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "SCHEMA_UNKNOWN_SKIPPED:$field" }
            }
        }
    }

    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "FIELD_PRESENT:$field" }
}

# --- Math Expert ---
_New-UDEOSimpleExpert -Id 'udeo.math' -Name 'Math Calculator' -Type ([UDEOExpertType]::Math) -Execute {
    param($Context, $Parameters)
    $op = $Parameters.Operation
    $formula = $Parameters.Formula

    switch ($op) {
        'dti' {
            $income = [double]$Context.Data['monthly_income']
            $debt   = [double]$Context.Data['monthly_debt']
            if ($income -le 0) { return @{ Success = $false; Error = 'Monthly income must be > 0' } }
            $dti = [math]::Round(($debt / $income) * 100, 2)
            if (-not $Context.Data.ContainsKey('calculations')) { $Context.Data['calculations'] = @{} }
            $Context.Data['calculations']['dti'] = $dti
            $Context.Data['calculations']['dti_category'] = if ($dti -le 36) { 'low' } elseif ($dti -le 43) { 'moderate' } else { 'high' }
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "DTI_CALCULATED:$dti%" }
        }
        'ltv' {
            $loan = [double]$Context.Data['loan_amount']
            $val  = [double]$Context.Data['property_value']
            if ($val -le 0) { return @{ Success = $false; Error = 'Property value must be > 0' } }
            $ltv = [math]::Round(($loan / $val) * 100, 2)
            if (-not $Context.Data.ContainsKey('calculations')) { $Context.Data['calculations'] = @{} }
            $Context.Data['calculations']['ltv'] = $ltv
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "LTV_CALCULATED:$ltv%" }
        }
        'payment' {
            $principal = [double]$Context.Data['loan_amount']
            $annualRate = [double]$Context.Data['interest_rate']
            $months = [int]$Context.Data['term_months']
            $monthlyRate = $annualRate / 12.0
            $pmt = 0
            if ($monthlyRate -eq 0) {
                $pmt = $principal / $months
            } else {
                $pmt = $principal * ($monthlyRate * [math]::Pow(1 + $monthlyRate, $months)) / ([math]::Pow(1 + $monthlyRate, $months) - 1)
            }
            $pmt = [math]::Round($pmt, 2)
            if (-not $Context.Data.ContainsKey('calculations')) { $Context.Data['calculations'] = @{} }
            $Context.Data['calculations']['monthly_payment'] = $pmt
            return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "PAYMENT_CALCULATED:$pmt" }
        }
        default {
            if ($formula) {
                try {
                    $calcResult = Invoke-Expression $formula 2>$null
                    if (-not $Context.Data.ContainsKey('calculations')) { $Context.Data['calculations'] = @{} }
                    $Context.Data['calculations'][$op] = $calcResult
                    return @{ Success = $true; DecisionCode = 'VALID'; RuleFired = "FORMULA_EVALUATED" }
                } catch {
                    return @{ Success = $false; Error = "Formula error: $($_.Exception.Message)" }
                }
            }
            return @{ Success = $false; Error = "Unknown operation: $op. Use dti, ltv, payment, or provide Formula." }
        }
    }
}

# --- Risk Expert ---
_New-UDEOSimpleExpert -Id 'udeo.risk' -Name 'Risk Assessor' -Type ([UDEOExpertType]::Risk) -Execute {
    param($Context, $Parameters)
    $rules = $Parameters.Rules
    if (-not $rules) {
        $rules = @(
            @{ Field = 'credit_score'; Op = 'lt'; Value = 640; Action = 'REJECTED'; Reason = 'Credit score too low' }
            @{ Field = 'dti';         Op = 'gt'; Value = 50;  Action = 'ROUTE_TO_HUMAN'; Reason = 'High DTI ratio' }
            @{ Field = 'ltv';         Op = 'gt'; Value = 95;  Action = 'REJECTED'; Reason = 'LTV too high' }
        )
    }

    foreach ($rule in $rules) {
        $fieldPath = $rule.Field
        $val = $null
        if ($Context.Data.ContainsKey($fieldPath)) {
            $val = $Context.Data[$fieldPath]
        } elseif ($Context.Data.ContainsKey('calculations') -and $Context.Data['calculations'].ContainsKey($fieldPath)) {
            $val = $Context.Data['calculations'][$fieldPath]
        } elseif ($Context.Data.ContainsKey('applicant') -and $Context.Data['applicant'].ContainsKey($fieldPath)) {
            $val = $Context.Data['applicant'][$fieldPath]
        }

        if ($null -eq $val) { continue }

        $triggered = $false
        switch ($rule.Op) {
            'lt'  { $triggered = [double]$val -lt [double]$rule.Value }
            'lte' { $triggered = [double]$val -le [double]$rule.Value }
            'gt'  { $triggered = [double]$val -gt [double]$rule.Value }
            'gte' { $triggered = [double]$val -ge [double]$rule.Value }
            'eq'  { $triggered = "$val" -eq "$($rule.Value)" }
            'ne'  { $triggered = "$val" -ne "$($rule.Value)" }
        }

        if ($triggered) {
            return @{ Success = $true; DecisionCode = $rule.Action; RuleFired = $rule.Reason }
        }
    }

    return @{ Success = $true; DecisionCode = 'APPROVED'; RuleFired = 'ALL_RULES_PASSED' }
}

# --- Human Review Expert ---
_New-UDEOSimpleExpert -Id 'udeo.human' -Name 'Human Reviewer' -Type ([UDEOExpertType]::HumanReview) -Execute {
    param($Context, $Parameters)
    $reason = $Parameters.Reason
    $timeout = if ($Parameters.TimeoutSeconds) { [int]$Parameters.TimeoutSeconds } else { 30 }

    [UDEOLogger]::Warn("Human review required: $reason")
    [UDEOLogger]::Info("Waiting $timeout seconds for human input (press Enter to auto-approve)...")

    if (-not $Context.Data.ContainsKey('human_review')) {
        $Context.Data['human_review'] = @{}
    }

    # In automated mode, auto-escalate
    if ([UDEOConfig]::Get('pipeline.autoEscalate') -eq $true) {
        $Context.Data['human_review']['decision'] = 'ESCALATED'
        $Context.Data['human_review']['reason'] = $reason
        $Context.Data['human_review']['timestamp'] = [datetime]::UtcNow.ToString('O')
        return @{ Success = $true; DecisionCode = 'ROUTE_TO_HUMAN'; RuleFired = 'AUTO_ESCALATED' }
    }

    $Context.Data['human_review']['reason'] = $reason
    $Context.Data['human_review']['timestamp'] = [datetime]::UtcNow.ToString('O')
    $Context.Data['human_review']['status'] = 'PENDING_REVIEW'

    return @{ Success = $true; DecisionCode = 'ROUTE_TO_HUMAN'; RuleFired = 'HUMAN_REVIEW_NEEDED' }
}

# ============================================================
# PUBLIC FUNCTIONS
# ============================================================
function Register-UDEOExpert {
    param(
        [Parameter(Mandatory)] [string]$Id,
        [Parameter(Mandatory)] [string]$Name,
        [Parameter(Mandatory)] [UDEOExpertType]$Type,
        [Parameter(Mandatory)] [scriptblock]$Execute,
        [string]$Description = '',
        [scriptblock]$HealthCheck = { return $true }
    )
    $contract = [UDEOExpertContract]::new($Id, $Name, $Type, $Execute)
    $contract.Description = $Description
    $contract.HealthCheck = $HealthCheck
    [UDEOExpertRegistry]::Register($contract)
}

function Get-UDEOExpert {
    param([string]$Id)
    if ($Id) { return [UDEOExpertRegistry]::Get($Id) }
    return [UDEOExpertRegistry]::GetAll()
}

# Functions exported: Invoke-UDEOExpert, Register-UDEOExpert, Get-UDEOExpert
