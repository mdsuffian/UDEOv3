#!/usr/bin/env node

/**
 * Example 01: Quickstart — Minimal UDEO Pipeline
 * Demonstrates the simplest possible usage: inline custom expert + built-in validation.
 *
 * Usage: node examples/01_quickstart.js
 */

'use strict';

const { Pipeline, ExpertType, registerExpert, Logger, Telemetry } = require('../index');

// Register a custom expert inline
function greeterExecute(context, parameters) {
  const name = parameters.name || 'World';
  context.data.greeting = `Hello, ${name}!`;
  return { success: true, decision_code: 'VALID', rule_fired: 'GREETING_DELIVERED' };
}

registerExpert('greeter', 'Greeting Expert', ExpertType.CUSTOM, greeterExecute);

// Create and run a minimal pipeline
const pipeline = new Pipeline('Quickstart');
pipeline.context.data.message = 'UDEO works!';
pipeline.addStep('greeter', { name: 'World' });
pipeline.addStep('udeo.validation', { field: 'message', schema: 'non_empty_string', required: true });

const result = pipeline.run();

console.log('\n=== Result ===');
console.log(`Decision: ${result.decision}`);
console.log(`Trace entries: ${result.trace.length}`);
console.log(`Context greeting: ${result.context.data.greeting}`);

// Show telemetry
const tele = Telemetry.summary();
console.log('\n=== Telemetry ===');
for (const [name, data] of Object.entries(tele.spans)) {
  console.log(`  ${name}: count=${data.count}, avg_ms=${data.avg_ms}`);
}
console.log(`  Counters: ${JSON.stringify(tele.counters)}`);
