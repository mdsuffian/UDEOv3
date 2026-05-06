/**
 * UDEO — Universal Deterministic Expert Orchestrator
 * ===================================================
 * A zero-dependency, pluggable, deterministic expert pipeline framework.
 * Built for regulated domains: loan underwriting, compliance checks, risk assessment.
 *
 * Version 4.0.0 — Pure Node.js, zero dependencies beyond the standard library.
 */

'use strict';

const engine = require('./engine');
const experts = require('./experts');
const pipeline = require('./pipeline');
const { Store } = require('./store');
const { Telemetry } = require('./telemetry');

// Re-export everything from engine
const {
  ExpertType,
  DecisionCode,
  PipelineStatus,
  DecisionTrace,
  Context,
  ExpertContract,
  Logger,
  Config,
} = engine;

// Re-export from experts
const {
  ExpertRegistry,
  invokeExpert,
  registerExpert,
  getExpert,
} = experts;

// Re-export from pipeline
const {
  Pipeline,
  PipelineStep,
  createLoanApprovalPipeline,
} = pipeline;

const __version__ = '4.0.0';

module.exports = {
  // Version
  __version__,

  // Engine
  ExpertType,
  DecisionCode,
  PipelineStatus,
  DecisionTrace,
  Context,
  ExpertContract,
  Logger,
  Config,

  // Experts
  ExpertRegistry,
  invokeExpert,
  registerExpert,
  getExpert,

  // Pipeline
  Pipeline,
  PipelineStep,
  createLoanApprovalPipeline,

  // Store
  Store,

  // Telemetry
  Telemetry,
};
