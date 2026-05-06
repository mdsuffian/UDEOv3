/**
 * UDEO Plugin Template — v4.0
 * ================================
 * Drop JavaScript modules into the plugins/ directory.
 * They are auto-discovered on framework bootstrap.
 *
 * A plugin module must:
 *   1. Define an execute(context, parameters) -> object function
 *   2. Call registerExpert() to register itself
 */

'use strict';

const { registerExpert, ExpertType } = require('../index');

/**
 * Custom expert logic.
 *
 * @param {import('../engine').Context} context - The pipeline's execution context (read/write context.data).
 * @param {object} parameters - Step-level parameters from the pipeline definition.
 * @returns {object} A result with:
 *   success (boolean): Whether the expert completed successfully.
 *   decision_code (string): One of APPROVED, REJECTED, FLAGGED, ROUTE_TO_HUMAN,
 *                            VALID, INVALID, PENDING, ERROR.
 *   rule_fired (string): Human-readable reason for the decision.
 */
function execute(context, parameters) {
  const name = parameters.name || 'World';
  context.data.custom_data = `Processed by ${name}`;
  return {
    success: true,
    decision_code: 'VALID',
    rule_fired: 'CUSTOM_EXPERT_EXECUTED',
  };
}

// Register this expert so it's discovered on startup.
registerExpert(
  'plugin.template',
  'Template Plugin Expert',
  ExpertType.CUSTOM,
  execute,
  { description: 'Replace this with your own expert logic.' }
);
