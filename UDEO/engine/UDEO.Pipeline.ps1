# UDEO.Pipeline.psm1 — v3.0
# Workflow pipeline orchestrator. Runs expert pipelines with full traceability.

class UDEOPipeline {
    [string]$Id
    [string]$Name
    [UDEOPipelineStatus]$Status
    [UDEOContext]$Context
    [System.Collections.Generic.List[hashtable]]$Steps
    [datetime]$StartedAt
    [datetime]$CompletedAt
    [hashtable]$Result

    UDEOPipeline([string]$name) {
        $this.Id = "pipeline_$([Guid]::NewGuid().ToString().Substring(0,8))"
        $this.Name = $name
        $this.Status = [UDEOPipelineStatus]::Pending
        $this.Context = [UDEOContext]::new($this.Id)
        $this.Steps = [System.Collections.Generic.List[hashtable]]::new()
        $this.Result = @{}
    }

    [void] AddStep([string]$expertId, [hashtable]$parameters) {
        $this.Steps.Add(@{
            ExpertId   = $expertId
            Parameters = $parameters
            Condition  = $parameters.Condition
            OnFailure  = $parameters.OnFailure  # 'stop', 'continue', 'skip'
        })
    }

    [void] AddConditionalStep([string]$expertId, [hashtable]$parameters, [scriptblock]$condition) {
        $params = $parameters.Clone()
        $params['Condition'] = $condition
        $this.Steps.Add(@{
            ExpertId   = $expertId
            Parameters = $params
            Condition  = $condition
            OnFailure  = $parameters.OnFailure
        })
    }

    [hashtable] Run() {
        $this.Status = [UDEOPipelineStatus]::Running
        $this.StartedAt = [datetime]::UtcNow
        [UDEOLogger]::Info("Pipeline started: $($this.Name) [$($this.Id)]", @{ steps = $this.Steps.Count })

        for ($i = 0; $i -lt $this.Steps.Count; $i++) {
            $step = $this.Steps[$i]
            [UDEOLogger]::Info("  Step $($i+1)/$($this.Steps.Count): $($step.ExpertId)")
            $this.Context.Data['_current_step'] = $i + 1
            $this.Context.Data['_total_steps'] = $this.Steps.Count

            # Check condition
            if ($step.Condition -and (& $step.Condition -Context $this.Context)) {
                [UDEOLogger]::Debug("  Skipping step (condition not met)")
                continue
            }

            # Execute expert
            $stepResult = Invoke-UDEOExpert -ExpertId $step.ExpertId -Context $this.Context -Parameters $step.Parameters

            if (-not $stepResult.Success) {
                $onFailure = if ($step.Parameters.OnFailure) { $step.Parameters.OnFailure } else { 'stop' }
                [UDEOLogger]::Warn("  Step $($i+1) failed. OnFailure=$onFailure")
                switch ($onFailure) {
                    'stop' {
                        $this.Status = [UDEOPipelineStatus]::Failed
                        $this.CompletedAt = [datetime]::UtcNow
                        $this.Result = @{
                            Success     = $false
                            Decision    = 'ERROR'
                            Error       = $stepResult.Error
                            FailedStep  = $step.ExpertId
                            FailedIndex = $i + 1
                            Trace       = $this.Context.GetTrace()
                        }
                        return $this.Result
                    }
                    'skip'    { continue }
                    'continue' { }
                }
            }

            # Check for terminal decisions
            $terminalDecisions = @('APPROVED', 'REJECTED', 'FLAGGED')
            if ($stepResult.DecisionCode -in $terminalDecisions) {
                [UDEOLogger]::Info("  Terminal decision reached: $($stepResult.DecisionCode)")
                $this.Status = [UDEOPipelineStatus]::Completed
                $this.CompletedAt = [datetime]::UtcNow
                $this.Result = @{
                    Success    = $true
                    Decision   = $stepResult.DecisionCode
                    Reason     = $stepResult.RuleFired
                    Trace      = $this.Context.GetTrace()
                    Context    = $this.Context
                }
                return $this.Result
            }
        }

        # All steps completed without terminal
        $this.Status = [UDEOPipelineStatus]::Completed
        $this.CompletedAt = [datetime]::UtcNow
        $this.Result = @{
            Success  = $true
            Decision = if ($this.Context.Data['_final_decision']) { $this.Context.Data['_final_decision'] } else { 'PENDING' }
            Trace    = $this.Context.GetTrace()
            Context  = $this.Context
        }

        [UDEOLogger]::Info("Pipeline completed: $($this.Name)", @{
            decision = $this.Result.Decision
            steps    = $this.Steps.Count
            duration = [math]::Round(($this.CompletedAt - $this.StartedAt).TotalMilliseconds, 2)
        })
        return $this.Result
    }
}

# ============================================================
# PUBLIC FUNCTIONS
# ============================================================
function New-UDEOPipeline {
    param([Parameter(Mandatory)] [string]$Name)
    return [UDEOPipeline]::new($Name)
}

function Add-UDEOPipelineStep {
    param(
        [Parameter(Mandatory)] [UDEOPipeline]$Pipeline,
        [Parameter(Mandatory)] [string]$ExpertId,
        [hashtable]$Parameters = @{},
        [scriptblock]$Condition,
        [ValidateSet('stop', 'continue', 'skip')] [string]$OnFailure = 'stop'
    )
    $params = $Parameters.Clone()
    $params['OnFailure'] = $OnFailure
    if ($Condition) {
        $Pipeline.AddConditionalStep($ExpertId, $params, $Condition)
    } else {
        $Pipeline.AddStep($ExpertId, $params)
    }
}

function Invoke-UDEOPipeline {
    param([Parameter(Mandatory)] [UDEOPipeline]$Pipeline)
    return $Pipeline.Run()
}

function New-UDEOLoanApprovalPipeline {
    param(
        [double]$Income = 75000,
        [double]$Debt = 25000,
        [int]$CreditScore = 720,
        [double]$LoanAmount = 300000,
        [double]$InterestRate = 0.065,
        [int]$TermMonths = 360,
        [double]$PropertyValue = 375000
    )

    $pipeline = [UDEOPipeline]::new("LoanApproval")

    # Seed context data
    $pipeline.Context.Data = @{
        applicant       = @{
            name         = 'Applicant'
            income       = $Income
            debt         = $Debt
            credit_score = $CreditScore
            loan_amount  = $LoanAmount
        }
        monthly_income  = [math]::Round($Income / 12, 2)
        monthly_debt    = [math]::Round($Debt / 12, 2)
        loan_amount     = $LoanAmount
        property_value  = $PropertyValue
        interest_rate   = $InterestRate
        term_months     = $TermMonths
    }

    # Step 1: Validate required fields
    $pipeline.AddStep('udeo.validation', @{
        Field    = 'applicant.credit_score'
        Schema   = 'credit_score'
        Required = $true
    })

    # Step 2: Validate loan amount
    $pipeline.AddStep('udeo.validation', @{
        Field    = 'loan_amount'
        Schema   = 'positive_number'
        Required = $true
    })

    # Step 3: Calculate DTI
    $pipeline.AddStep('udeo.math', @{ Operation = 'dti' })

    # Step 4: Calculate LTV (conditional — only if property_value present)
    $pipeline.AddConditionalStep('udeo.math', @{ Operation = 'ltv' }, {
        param($Context)
        return -not ($Context.Data.ContainsKey('property_value') -and [double]$Context.Data['property_value'] -gt 0)
    })

    # Step 5: Risk assessment
    $pipeline.AddStep('udeo.risk', @{
        Rules = @(
            @{ Field = 'credit_score'; Op = 'lt'; Value = 640; Action = 'REJECTED'; Reason = 'Credit score below 640' }
            @{ Field = 'dti';         Op = 'gt'; Value = 50;  Action = 'ROUTE_TO_HUMAN'; Reason = 'DTI exceeds 50%' }
            @{ Field = 'ltv';         Op = 'gt'; Value = 95;  Action = 'FLAGGED'; Reason = 'LTV exceeds 95%' }
        )
    })

    # Step 6: Human review (only if flagged)
    $pipeline.AddStep('udeo.human', @{
        Reason = 'Risk assessment flagged for review'
        OnFailure = 'continue'
    })

    return $pipeline
}

# Functions exported: New-UDEOPipeline, Add-UDEOPipelineStep, Invoke-UDEOPipeline, New-UDEOLoanApprovalPipeline
