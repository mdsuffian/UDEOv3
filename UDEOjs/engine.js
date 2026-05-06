/**
 * UDEO Engine — v4.0
 * Core engine types: enums, DecisionTrace, Context, ExpertContract, Logger, Config.
 * Zero external dependencies. Pure Node.js 18+.
 */

'use strict';

const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

// ============================================================
// UUID UTILITY (zero-dependency, crypto-backed)
// ============================================================

function shortUuid() {
  return crypto.randomBytes(8).toString('hex');
}

function fullUuid() {
  return crypto.randomUUID();
}

// ============================================================
// ENUMS (frozen objects)
// ============================================================

const ExpertType = Object.freeze({
  RULE: 'Rule',
  MATH: 'Math',
  VALIDATION: 'Validation',
  RISK: 'Risk',
  HUMAN_REVIEW: 'HumanReview',
  CUSTOM: 'Custom',
});

const DecisionCode = Object.freeze({
  APPROVED: 'APPROVED',
  REJECTED: 'REJECTED',
  FLAGGED: 'FLAGGED',
  ROUTE_TO_HUMAN: 'ROUTE_TO_HUMAN',
  VALID: 'VALID',
  INVALID: 'INVALID',
  PENDING: 'PENDING',
  ERROR: 'ERROR',
});

const PipelineStatus = Object.freeze({
  PENDING: 'Pending',
  RUNNING: 'Running',
  COMPLETED: 'Completed',
  FAILED: 'Failed',
  CANCELLED: 'Cancelled',
});

// ============================================================
// DECISION TRACE
// ============================================================

class DecisionTrace {
  /**
   * A single immutable trace entry recording an expert's decision.
   * @param {string} expertId
   * @param {string} expertName
   * @param {string} ruleFired
   * @param {string} decisionCode
   * @param {string} [timestamp]
   * @param {number} [executionTimeMs]
   * @param {object} [metadata]
   */
  constructor(expertId, expertName, ruleFired, decisionCode,
              { timestamp, executionTimeMs = 0.0, metadata = {} } = {}) {
    this.expertId = expertId;
    this.expertName = expertName;
    this.ruleFired = ruleFired;
    this.decisionCode = decisionCode;
    this.timestamp = timestamp || new Date().toISOString();
    this.executionTimeMs = executionTimeMs;
    this.metadata = { ...metadata };
  }

  toDict() {
    return {
      expert_id: this.expertId,
      expert_name: this.expertName,
      rule_fired: this.ruleFired,
      decision_code: this.decisionCode,
      timestamp: this.timestamp,
      execution_time_ms: this.executionTimeMs,
    };
  }
}

// ============================================================
// EXECUTION CONTEXT
// ============================================================

class Context {
  /**
   * Mutable context flowing through the pipeline. Accumulates decision traces.
   * @param {string} pipelineId
   */
  constructor(pipelineId) {
    this.pipelineId = pipelineId;
    this.correlationId = fullUuid().substring(0, 16);
    this.step = 0;
    this.data = {};
    this._trace = [];
    this.createdAt = new Date().toISOString();
    this.updatedAt = this.createdAt;
  }

  recordDecision(trace) {
    this.step += 1;
    this._trace.push(trace);
    this.updatedAt = new Date().toISOString();
  }

  getTrace() {
    return [...this._trace];
  }

  toDict() {
    return {
      pipeline_id: this.pipelineId,
      correlation_id: this.correlationId,
      step: this.step,
      data: this.data,
      decision_trace: this._trace.map(t => t.toDict()),
      created_at: this.createdAt,
      updated_at: this.updatedAt,
    };
  }
}

// ============================================================
// EXPERT CONTRACT
// ============================================================

class ExpertContract {
  /**
   * Defines the interface a registered expert must fulfil.
   * @param {object} opts
   * @param {string} opts.id
   * @param {string} opts.name
   * @param {string} opts.type - value from ExpertType
   * @param {function(Context, object): object} opts.execute
   * @param {string} [opts.version]
   * @param {string} [opts.description]
   * @param {number} [opts.timeoutSeconds]
   * @param {function(): boolean} [opts.healthCheck]
   */
  constructor({ id, name, type, execute, version = '4.0.0',
                description = '', timeoutSeconds = 30,
                healthCheck = () => true }) {
    this.id = id;
    this.name = name;
    this.type = type;
    this.execute = execute;
    this.version = version;
    this.description = description;
    this.timeoutSeconds = timeoutSeconds;
    this.healthCheck = healthCheck;
  }
}

// ============================================================
// LOGGER
// ============================================================

class Logger {
  /** Thread-safe structured logger with configurable levels and optional file output. */

  static LEVELS = { TRACE: 0, DEBUG: 1, INFO: 2, WARN: 3, ERROR: 4 };

  static COLORS = {
    ERROR: '\x1b[91m',   // red
    WARN: '\x1b[93m',    // yellow
    INFO: '\x1b[96m',    // cyan
    DEBUG: '\x1b[90m',   // dark gray
    TRACE: '\x1b[37m',   // white
    RESET: '\x1b[0m',
  };

  static _logLevel = 'INFO';
  static _logFile = null;
  static _quiet = false;
  static _useColors = true;

  /**
   * @param {object} opts
   * @param {string} [opts.level]
   * @param {string|null} [opts.logFile]
   * @param {boolean} [opts.quiet]
   * @param {boolean} [opts.useColors]
   */
  static configure({ level = 'INFO', logFile = null, quiet = false, useColors = true } = {}) {
    Logger._logLevel = level.toUpperCase();
    Logger._logFile = logFile;
    Logger._quiet = quiet;
    Logger._useColors = useColors;
  }

  static _write(level, message, data = null) {
    const currentWeight = Logger.LEVELS[Logger._logLevel] ?? 2;
    const msgWeight = Logger.LEVELS[level] ?? 2;
    if (msgWeight < currentWeight) {
      return;
    }

    const now = new Date();
    const hh = String(now.getUTCHours()).padStart(2, '0');
    const mm = String(now.getUTCMinutes()).padStart(2, '0');
    const ss = String(now.getUTCSeconds()).padStart(2, '0');
    const ms = String(now.getUTCMilliseconds()).padStart(3, '0');
    const line = `[${hh}:${mm}:${ss}.${ms}] [${level}] ${message}`;

    if (!Logger._quiet) {
      const color = Logger.COLORS[level] || '';
      const reset = Logger._useColors ? Logger.COLORS.RESET : '';
      const stream = level === 'ERROR' ? process.stderr : process.stdout;
      stream.write(`${color}${line}${reset}\n`);
      if (data) {
        stream.write(`  ${JSON.stringify(data)}\n`);
      }
    }

    if (Logger._logFile) {
      const dir = path.dirname(Logger._logFile);
      fs.mkdirSync(dir, { recursive: true });
      let entry = line + '\n';
      if (data) {
        entry += `  ${JSON.stringify(data)}\n`;
      }
      fs.appendFileSync(Logger._logFile, entry, 'utf-8');
    }
  }

  static info(message, data = null) { Logger._write('INFO', message, data); }
  static debug(message, data = null) { Logger._write('DEBUG', message, data); }
  static warn(message, data = null) { Logger._write('WARN', message, data); }
  static error(message, data = null) { Logger._write('ERROR', message, data); }
  static trace(message, data = null) { Logger._write('TRACE', message, data); }
}

// ============================================================
// CONFIGURATION
// ============================================================

class Config {
  /** Workspace configuration loaded from .udeo/config.json with sensible defaults. */

  static _workspaceRoot = null;
  static _data = {};

  /**
   * @param {string} workspaceRoot
   */
  static load(workspaceRoot) {
    Config._workspaceRoot = path.resolve(workspaceRoot);
    const ws = Config._workspaceRoot;

    Config._data = {
      version: '4.0.0',
      store_path: path.join(ws, '.udeo', 'store'),
      log_level: 'INFO',
      quiet: false,
      experts: {
        timeout_seconds: 30,
        plugin_directory: path.join(ws, 'plugins'),
      },
      pipeline: {
        max_retries: 2,
        default_timeout: 60,
        auto_escalate: true,
      },
    };

    const configFile = path.join(Config._workspaceRoot, '.udeo', 'config.json');
    if (fs.existsSync(configFile)) {
      try {
        const override = JSON.parse(fs.readFileSync(configFile, 'utf-8'));
        for (const [k, v] of Object.entries(override)) {
          if (k in Config._data && typeof Config._data[k] === 'object' && !Array.isArray(Config._data[k]) && typeof v === 'object' && !Array.isArray(v)) {
            Object.assign(Config._data[k], v);
          } else {
            Config._data[k] = v;
          }
        }
      } catch (e) {
        Logger.warn(`Failed to parse .udeo/config.json, using defaults: ${e.message}`);
      }
    }

    Logger.debug(`UDEO config loaded: workspace=${workspaceRoot}`);
  }

  /**
   * @param {string} dotPath - e.g. "experts.timeout_seconds"
   * @param {*} [defaultVal]
   * @returns {*}
   */
  static get(dotPath, defaultVal = undefined) {
    const parts = dotPath.split('.');
    let current = Config._data;
    for (const p of parts) {
      if (current && typeof current === 'object' && p in current) {
        current = current[p];
      } else {
        return defaultVal;
      }
    }
    return current;
  }
}

// ============================================================
// EXPORTS
// ============================================================

module.exports = {
  shortUuid,
  fullUuid,
  ExpertType,
  DecisionCode,
  PipelineStatus,
  DecisionTrace,
  Context,
  ExpertContract,
  Logger,
  Config,
};
