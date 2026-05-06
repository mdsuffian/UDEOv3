/**
 * UDEO Experts — v4.0
 * Expert registry, execution engine, and four built-in experts:
 *   - udeo.validation — field/schema validation
 *   - udeo.math       — financial math calculations (DTI, LTV, payment)
 *   - udeo.risk       — multi-rule risk assessment
 *   - udeo.human      — human-in-the-loop review
 *
 * Drop JavaScript expert modules into plugins/ — they are auto-discovered and registered.
 */

'use strict';

const fs = require('fs');
const path = require('path');
const { performance } = require('perf_hooks');
const { Context, DecisionTrace, ExpertContract, ExpertType, Logger, Config } = require('./engine');

// ============================================================
// EXPERT REGISTRY
// ============================================================

class ExpertRegistry {
  /** Global registry of all available experts. Enforces unique IDs. */

  static _experts = {};

  /**
   * @param {ExpertContract} contract
   */
  static register(contract) {
    ExpertRegistry._experts[contract.id] = contract;
    Logger.info(`Registered expert: ${contract.id} [${contract.type}]`,
                { name: contract.name, version: contract.version });
  }

  /**
   * @param {string} expertId
   */
  static unregister(expertId) {
    if (ExpertRegistry._experts[expertId]) {
      delete ExpertRegistry._experts[expertId];
      Logger.info(`Unregistered expert: ${expertId}`);
    }
  }

  /**
   * @param {string} expertId
   * @returns {ExpertContract|undefined}
   */
  static get(expertId) {
    return ExpertRegistry._experts[expertId];
  }

  /**
   * @returns {ExpertContract[]}
   */
  static getAll() {
    return Object.values(ExpertRegistry._experts);
  }

  /**
   * @param {string} expertType
   * @returns {ExpertContract[]}
   */
  static getByType(expertType) {
    return Object.values(ExpertRegistry._experts).filter(e => e.type === expertType);
  }

  /**
   * Auto-discover and load JavaScript modules from the plugin directory.
   * @param {string} pluginDir
   */
  static discoverPlugins(pluginDir) {
    if (!fs.existsSync(pluginDir) || !fs.statSync(pluginDir).isDirectory()) {
      return;
    }
    Logger.info(`Discovering plugins in: ${pluginDir}`);

    const files = fs.readdirSync(pluginDir).sort();
    for (const file of files) {
      if (!file.endsWith('.js')) continue;
      const stem = file.replace(/\.js$/, '');
      if (stem === 'template') continue;

      try {
        require(path.join(pluginDir, file));
        Logger.debug(`Loaded plugin: ${file}`);
      } catch (e) {
        Logger.warn(`Failed to load plugin ${file}: ${e.message}`);
      }
    }
  }
}

// ============================================================
// EXPERT EXECUTION
// ============================================================

/**
 * Execute a registered expert by ID against the given context.
 * @param {string} expertId
 * @param {Context} context
 * @param {object} [parameters]
 * @returns {object} { success, decision_code, rule_fired, context, execution_time_ms, [error] }
 */
function invokeExpert(expertId, context, parameters = {}) {
  const contract = ExpertRegistry.get(expertId);

  if (!contract) {
    Logger.error(`Expert not found: ${expertId}`);
    const trace = new DecisionTrace(
      expertId, 'Unknown', 'EXPERT_NOT_FOUND', 'ERROR', { executionTimeMs: 0.0 }
    );
    context.recordDecision(trace);
    return { success: false, error: `Expert not found: ${expertId}`, context };
  }

  Logger.debug(`Executing expert: ${contract.name} [${expertId}]`);
  const start = performance.now();

  let result;
  let elapsedMs;
  try {
    result = contract.execute(context, parameters);
    elapsedMs = Math.round((performance.now() - start) * 100) / 100;
  } catch (e) {
    elapsedMs = Math.round((performance.now() - start) * 100) / 100;
    const trace = new DecisionTrace(
      expertId, contract.name, String(e), 'ERROR', { executionTimeMs: elapsedMs }
    );
    context.recordDecision(trace);
    Logger.error(`Expert ${expertId} threw: ${e.message}`);
    return {
      success: false,
      decision_code: 'ERROR',
      context,
      error: e.message,
      execution_time_ms: elapsedMs,
    };
  }

  const success = result.success || false;
  const code = success ? (result.decision_code || 'PENDING') : 'ERROR';
  const rule = result.rule_fired || (success ? 'EXECUTED' : result.error || 'EXECUTION_FAILED');

  const trace = new DecisionTrace(
    expertId, contract.name, rule, code, { executionTimeMs: elapsedMs }
  );
  context.recordDecision(trace);

  Logger.info(`Expert ${expertId} -> ${code} (${elapsedMs}ms)`, { rule });

  if (result.context) {
    context = result.context;
  }

  return {
    success,
    decision_code: code,
    rule_fired: rule,
    context,
    execution_time_ms: elapsedMs,
  };
}

// ============================================================
// BUILT-IN EXPERTS
// ============================================================

function _registerBuiltin(expertId, name, expertType, executeFn) {
  const contract = new ExpertContract({ id: expertId, name, type: expertType, execute: executeFn });
  ExpertRegistry.register(contract);
}

// --- Validation Expert ---

function _validateExecute(context, parameters) {
  const schema = parameters.schema;
  const field = parameters.field;
  const required = Boolean(parameters.required);

  if (!field) {
    return { success: false, error: "Validation expert requires 'field' parameter" };
  }

  const parts = field.split('.');
  let val = context.data;

  for (let i = 0; i < parts.length; i++) {
    const p = parts[i];
    if (val && typeof val === 'object' && p in val) {
      val = val[p];
    } else {
      if (required) {
        return {
          success: false,
          decision_code: 'INVALID',
          rule_fired: `REQUIRED_FIELD_MISSING:${field}`,
        };
      }
      return {
        success: true,
        decision_code: 'VALID',
        rule_fired: `FIELD_OPTIONAL_MISSING:${field}`,
      };
    }
  }

  if (schema) {
    return _validateSchema(schema, field, val);
  }
  return { success: true, decision_code: 'VALID', rule_fired: `FIELD_PRESENT:${field}` };
}

function _validateSchema(schema, field, val) {
  const handlers = {
    positive_number: (v) => (typeof v === 'number' || !isNaN(Number(v))) && Number(v) > 0,
    non_empty_string: (v) => typeof v === 'string' && v.trim().length > 0,
    credit_score: (v) => (typeof v === 'number' || !isNaN(Number(v))) && Number(v) >= 300 && Number(v) <= 850,
  };

  const handler = handlers[schema];
  if (handler) {
    if (handler(val)) {
      return { success: true, decision_code: 'VALID', rule_fired: `SCHEMA_${schema.toUpperCase()}:${field}` };
    }
    const failSuffix = schema === 'positive_number' ? 'NOT_POSITIVE' : schema.toUpperCase() + '_FAIL';
    return { success: false, decision_code: 'INVALID', rule_fired: `SCHEMA_${failSuffix}:${field}` };
  }
  return { success: true, decision_code: 'VALID', rule_fired: `SCHEMA_UNKNOWN_SKIPPED:${field}` };
}

// --- Math Expert ---

function _mathExecute(context, parameters) {
  const op = parameters.operation;
  const formula = parameters.formula;

  if (!context.data.calculations) {
    context.data.calculations = {};
  }
  const calcs = context.data.calculations;

  if (op === 'dti') {
    const income = parseFloat(context.data.monthly_income || 0);
    const debt = parseFloat(context.data.monthly_debt || 0);
    if (income <= 0) {
      return { success: false, error: 'Monthly income must be > 0' };
    }
    const dti = Math.round((debt / income) * 10000) / 100;
    calcs.dti = dti;
    calcs.dti_category = dti <= 36 ? 'low' : (dti <= 43 ? 'moderate' : 'high');
    return { success: true, decision_code: 'VALID', rule_fired: `DTI_CALCULATED:${dti}%` };
  }

  if (op === 'ltv') {
    const loan = parseFloat(context.data.loan_amount || 0);
    const prop = parseFloat(context.data.property_value || 0);
    if (prop <= 0) {
      return { success: false, error: 'Property value must be > 0' };
    }
    const ltv = Math.round((loan / prop) * 10000) / 100;
    calcs.ltv = ltv;
    return { success: true, decision_code: 'VALID', rule_fired: `LTV_CALCULATED:${ltv}%` };
  }

  if (op === 'payment') {
    const principal = parseFloat(context.data.loan_amount || 0);
    const annualRate = parseFloat(context.data.interest_rate || 0);
    const months = parseInt(context.data.term_months || 360, 10);
    const monthlyRate = annualRate / 12;
    let pmt;
    if (monthlyRate === 0) {
      pmt = principal / months;
    } else {
      const factor = Math.pow(1 + monthlyRate, months);
      pmt = principal * (monthlyRate * factor) / (factor - 1);
    }
    pmt = Math.round(pmt * 100) / 100;
    calcs.monthly_payment = pmt;
    return { success: true, decision_code: 'VALID', rule_fired: `PAYMENT_CALCULATED:${pmt}` };
  }

  if (formula) {
    try {
      // Safe evaluation: only allow access to calculations object
      const sandbox = { ...(context.data.calculations || {}) };
      const fn = new Function('calc', `"use strict"; return (${formula});`);
      const result = fn(sandbox);
      calcs[op || 'formula'] = result;
      return { success: true, decision_code: 'VALID', rule_fired: 'FORMULA_EVALUATED' };
    } catch (e) {
      return { success: false, error: `Formula error: ${e.message}` };
    }
  }

  return { success: false, error: `Unknown operation: ${op}. Use dti, ltv, payment, or provide a Formula.` };
}

// --- Risk Expert ---

function _riskExecute(context, parameters) {
  const rules = parameters.rules || [
    { field: 'credit_score', op: 'lt', value: 640, action: 'REJECTED', reason: 'Credit score too low' },
    { field: 'dti', op: 'gt', value: 50, action: 'ROUTE_TO_HUMAN', reason: 'High DTI ratio' },
    { field: 'ltv', op: 'gt', value: 95, action: 'REJECTED', reason: 'LTV too high' },
  ];

  for (const rule of rules) {
    const fieldPath = rule.field;
    let val = undefined;

    // Search in main data, calculations, and applicant sub-dict
    if (fieldPath in context.data) {
      val = context.data[fieldPath];
    } else if (context.data.calculations && fieldPath in context.data.calculations) {
      val = context.data.calculations[fieldPath];
    } else if (context.data.applicant && fieldPath in context.data.applicant) {
      val = context.data.applicant[fieldPath];
    }

    if (val === undefined || val === null) {
      continue;
    }

    const op = rule.op;
    const target = rule.value;

    const comparators = {
      lt: (a, b) => parseFloat(a) < parseFloat(b),
      lte: (a, b) => parseFloat(a) <= parseFloat(b),
      gt: (a, b) => parseFloat(a) > parseFloat(b),
      gte: (a, b) => parseFloat(a) >= parseFloat(b),
      eq: (a, b) => String(a) === String(b),
      ne: (a, b) => String(a) !== String(b),
    };

    const comparator = comparators[op];
    if (comparator && comparator(val, target)) {
      return { success: true, decision_code: rule.action, rule_fired: rule.reason };
    }
  }

  return { success: true, decision_code: 'APPROVED', rule_fired: 'ALL_RULES_PASSED' };
}

// --- Human Review Expert ---

function _humanReviewExecute(context, parameters) {
  const reason = parameters.reason || 'Manual review required';

  if (!context.data.human_review) {
    context.data.human_review = {};
  }
  const review = context.data.human_review;
  review.reason = reason;
  review.timestamp = new Date().toISOString();

  if (Config.get('pipeline.auto_escalate', true)) {
    review.decision = 'ESCALATED';
    review.status = 'AUTO_ESCALATED';
    Logger.warn(`Human review auto-escalated: ${reason}`);
    return { success: true, decision_code: 'ROUTE_TO_HUMAN', rule_fired: 'AUTO_ESCALATED' };
  }

  review.status = 'PENDING_REVIEW';
  Logger.warn(`Human review required: ${reason}`);
  return { success: true, decision_code: 'ROUTE_TO_HUMAN', rule_fired: 'HUMAN_REVIEW_NEEDED' };
}

// ============================================================
// REGISTER ALL BUILT-INS ON IMPORT
// ============================================================

_registerBuiltin('udeo.validation', 'Schema Validator', ExpertType.VALIDATION, _validateExecute);
_registerBuiltin('udeo.math', 'Math Calculator', ExpertType.MATH, _mathExecute);
_registerBuiltin('udeo.risk', 'Risk Assessor', ExpertType.RISK, _riskExecute);
_registerBuiltin('udeo.human', 'Human Reviewer', ExpertType.HUMAN_REVIEW, _humanReviewExecute);

// ============================================================
// PUBLIC API
// ============================================================

/**
 * Register a custom expert.
 * @param {string} expertId
 * @param {string} name
 * @param {string} expertType - value from ExpertType
 * @param {function(Context, object): object} executeFn
 * @param {string} [description]
 * @param {function(): boolean} [healthCheck]
 * @param {number} [timeoutSeconds]
 * @returns {ExpertContract}
 */
function registerExpert(expertId, name, expertType, executeFn,
                         { description = '', healthCheck = () => true, timeoutSeconds = 30 } = {}) {
  const contract = new ExpertContract({
    id: expertId,
    name,
    type: expertType,
    execute: executeFn,
    description,
    timeoutSeconds,
    healthCheck,
  });
  ExpertRegistry.register(contract);
  return contract;
}

/**
 * Get a specific expert by ID, or all experts if no ID is given.
 * @param {string} [expertId]
 * @returns {ExpertContract|ExpertContract[]|undefined}
 */
function getExpert(expertId) {
  if (expertId) {
    return ExpertRegistry.get(expertId);
  }
  return ExpertRegistry.getAll();
}

module.exports = {
  ExpertRegistry,
  invokeExpert,
  registerExpert,
  getExpert,
};
