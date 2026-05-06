/**
 * UDEO Pipeline — v4.0
 * Workflow pipeline orchestrator. Runs sequences of expert invocations with
 * conditional steps, failure policies, and terminal decision detection.
 */

'use strict';

const { shortUuid } = require('./engine');
const { Context, Logger, PipelineStatus } = require('./engine');
const { invokeExpert } = require('./experts');

// ============================================================
// PIPELINE STEP
// ============================================================

class PipelineStep {
  /**
   * A single step in the pipeline — expert invocation with optional condition.
   * @param {string} expertId
   * @param {object} parameters
   * @param {function(Context): boolean} [condition] - if returns true, step is SKIPPED
   * @param {string} [onFailure]
   */
  constructor(expertId, parameters, condition = null, onFailure = 'stop') {
    this.expertId = expertId;
    this.parameters = parameters;
    this.condition = condition;
    this.onFailure = onFailure;
  }
}

// ============================================================
// PIPELINE
// ============================================================

const TERMINAL_DECISIONS = new Set(['APPROVED', 'REJECTED', 'FLAGGED']);

class Pipeline {
  /**
   * Orchestrates a sequence of expert invocations with full traceability.
   * @param {string} name
   */
  constructor(name) {
    this.id = `pipeline_${shortUuid()}`;
    this.name = name;
    this.status = PipelineStatus.PENDING;
    this.context = new Context(this.id);
    this.steps = [];
    this.startedAt = null;
    this.completedAt = null;
    this.result = {};
  }

  /**
   * Add a step that always executes.
   * @param {string} expertId
   * @param {object} [parameters]
   * @param {string} [onFailure]
   */
  addStep(expertId, parameters = {}, onFailure = 'stop') {
    const params = { ...parameters, on_failure: parameters.on_failure || onFailure };
    this.steps.push(new PipelineStep(expertId, params, null, params.on_failure));
  }

  /**
   * Add a step that only executes when condition returns false (returns true = skip).
   * @param {string} expertId
   * @param {object} [parameters]
   * @param {function(Context): boolean} [condition] - if returns true, step is skipped
   * @param {string} [onFailure]
   */
  addConditionalStep(expertId, parameters = {}, condition = null, onFailure = 'stop') {
    const params = { ...parameters, on_failure: parameters.on_failure || onFailure };
    this.steps.push(new PipelineStep(expertId, params, condition, params.on_failure));
  }

  /**
   * Execute the pipeline.
   * @returns {object} { success, decision, reason, trace, context, [error], [failed_step], [failed_index] }
   */
  run() {
    this.status = PipelineStatus.RUNNING;
    this.startedAt = new Date().toISOString();

    Logger.info(`Pipeline started: ${this.name} [${this.id}]`,
                { steps: this.steps.length });

    for (let i = 0; i < this.steps.length; i++) {
      const step = this.steps[i];
      Logger.info(`  Step ${i + 1}/${this.steps.length}: ${step.expertId}`);
      this.context.data._current_step = i + 1;
      this.context.data._total_steps = this.steps.length;

      // Check condition (skip if condition returns true)
      if (step.condition) {
        try {
          const shouldSkip = step.condition(this.context);
          if (shouldSkip) {
            Logger.debug('  Skipping step (condition not met)');
            continue;
          }
        } catch (e) {
          Logger.error(`  Condition evaluation failed: ${e.message}`);
          continue;
        }
      }

      // Execute
      const stepResult = invokeExpert(step.expertId, this.context, step.parameters);

      if (!stepResult.success) {
        const onFailure = step.onFailure || 'stop';
        Logger.warn(`  Step ${i + 1} failed. OnFailure=${onFailure}`);

        if (onFailure === 'stop') {
          this.status = PipelineStatus.FAILED;
          this.completedAt = new Date().toISOString();
          this.result = {
            success: false,
            decision: 'ERROR',
            error: stepResult.error,
            failed_step: step.expertId,
            failed_index: i + 1,
            trace: this.context.getTrace(),
            context: this.context,
          };
          return this.result;
        } else if (onFailure === 'skip') {
          continue;
        }
        // 'continue': fall through
      }

      // Check terminal decision
      const code = stepResult.decision_code;
      if (TERMINAL_DECISIONS.has(code)) {
        Logger.info(`  Terminal decision reached: ${code}`);
        this.status = PipelineStatus.COMPLETED;
        this.completedAt = new Date().toISOString();
        this.result = {
          success: true,
          decision: code,
          reason: stepResult.rule_fired,
          trace: this.context.getTrace(),
          context: this.context,
        };
        return this.result;
      }
    }

    // All steps completed without terminal
    this.status = PipelineStatus.COMPLETED;
    this.completedAt = new Date().toISOString();
    this.result = {
      success: true,
      decision: this.context.data._final_decision || 'PENDING',
      trace: this.context.getTrace(),
      context: this.context,
    };

    let durationMs = 0.0;
    if (this.startedAt && this.completedAt) {
      const t0 = new Date(this.startedAt);
      const t1 = new Date(this.completedAt);
      durationMs = (t1.getTime() - t0.getTime());
    }

    Logger.info(`Pipeline completed: ${this.name}`, {
      decision: this.result.decision,
      steps: this.steps.length,
      duration_ms: Math.round(durationMs * 100) / 100,
    });
    return this.result;
  }
}

// ============================================================
// LOAN APPROVAL PIPELINE TEMPLATE
// ============================================================

/**
 * Factory for the loan approval pipeline — the canonical UDEO example.
 * @param {object} opts
 * @returns {Pipeline}
 */
function createLoanApprovalPipeline({
  income = 75000,
  debt = 25000,
  creditScore = 720,
  loanAmount = 300000,
  interestRate = 0.065,
  termMonths = 360,
  propertyValue = 375000,
} = {}) {
  const pipeline = new Pipeline('LoanApproval');

  pipeline.context.data = {
    applicant: {
      name: 'Applicant',
      income: income,
      debt: debt,
      credit_score: creditScore,
      loan_amount: loanAmount,
    },
    monthly_income: Math.round((income / 12) * 100) / 100,
    monthly_debt: Math.round((debt / 12) * 100) / 100,
    loan_amount: loanAmount,
    property_value: propertyValue,
    interest_rate: interestRate,
    term_months: termMonths,
  };

  // Step 1: Validate credit score
  pipeline.addStep('udeo.validation', {
    field: 'applicant.credit_score',
    schema: 'credit_score',
    required: true,
  });

  // Step 2: Validate loan amount is positive
  pipeline.addStep('udeo.validation', {
    field: 'loan_amount',
    schema: 'positive_number',
    required: true,
  });

  // Step 3: Calculate DTI
  pipeline.addStep('udeo.math', { operation: 'dti' });

  // Step 4: Calculate LTV (conditional — skip if no property value)
  pipeline.addConditionalStep('udeo.math', { operation: 'ltv' }, (ctx) => {
    return !('property_value' in ctx.data) || parseFloat(ctx.data.property_value || 0) <= 0;
  });

  // Step 5: Risk assessment
  pipeline.addStep('udeo.risk', {
    rules: [
      { field: 'credit_score', op: 'lt', value: 640, action: 'REJECTED', reason: 'Credit score below 640' },
      { field: 'dti', op: 'gt', value: 50, action: 'ROUTE_TO_HUMAN', reason: 'DTI exceeds 50%' },
      { field: 'ltv', op: 'gt', value: 95, action: 'FLAGGED', reason: 'LTV exceeds 95%' },
    ],
  });

  // Step 6: Human review (only if flagged)
  pipeline.addStep('udeo.human', {
    reason: 'Risk assessment flagged for review',
    on_failure: 'continue',
  });

  return pipeline;
}

module.exports = {
  Pipeline,
  PipelineStep,
  createLoanApprovalPipeline,
};
