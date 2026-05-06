#!/usr/bin/env node

/**
 * Example 04: KYC/AML Compliance Check
 * Demonstrates a multi-stage compliance pipeline with conditional steps.
 *
 * Usage: node examples/04_compliance_check.js
 */

'use strict';

const { Pipeline, ExpertType, registerExpert } = require('../index');

// Custom KYC check expert
function kycCheckExecute(context, parameters) {
  const customer = context.data.customer || {};
  const checks = {
    identity_verified: 'Identity not verified',
    address_verified: 'Address not verified',
    sanctions_screened: 'Not screened against sanctions list',
    pep_screened: 'Not screened as Politically Exposed Person',
  };

  const failures = [];
  for (const [check, msg] of Object.entries(checks)) {
    if (!customer[check]) {
      failures.push(msg);
    }
  }

  if (failures.length > 0) {
    return {
      success: false,
      decision_code: 'FLAGGED',
      rule_fired: `KYC_FAILED:${failures.join('; ')}`,
    };
  }
  return { success: true, decision_code: 'VALID', rule_fired: 'KYC_PASSED' };
}

// Custom AML transaction check
function amlCheckExecute(context, parameters) {
  const transaction = context.data.transaction || {};
  const amount = parseFloat(transaction.amount || 0);
  const threshold = parseFloat(parameters.threshold || 10000);

  if (amount >= threshold) {
    context.data.aml_flagged = true;
    return {
      success: true,
      decision_code: 'FLAGGED',
      rule_fired: `AML_THRESHOLD_EXCEEDED:${amount}>=${threshold}`,
    };
  }
  return { success: true, decision_code: 'VALID', rule_fired: 'AML_CLEAR' };
}

registerExpert('kyc.check', 'KYC Verification', ExpertType.VALIDATION, kycCheckExecute);
registerExpert('aml.check', 'AML Screening', ExpertType.RISK, amlCheckExecute);

// Build the compliance pipeline
const pipeline = new Pipeline('KYC_AML_Onboarding');

pipeline.context.data = {
  onboarding_id: 'ONB-2026-042',
  customer: {
    name: 'Jane Smith',
    identity_verified: true,
    address_verified: true,
    sanctions_screened: true,
    pep_screened: true,
  },
  transaction: {
    amount: 5000,
    currency: 'USD',
    source_country: 'US',
    destination_country: 'US',
  },
};

// Step 1: Validate customer name
pipeline.addStep('udeo.validation', {
  field: 'customer.name',
  schema: 'non_empty_string',
  required: true,
});

// Step 2: KYC verification
pipeline.addStep('kyc.check', {});

// Step 3: AML screening (conditional — only for transactions above $1,000)
pipeline.addConditionalStep('aml.check', { threshold: 10000 }, (ctx) => {
  return (ctx.data.transaction && ctx.data.transaction.amount || 0) < 1000;
});

// Step 4: Risk check on the combined outcome
pipeline.addStep('udeo.risk', {
  rules: [
    {
      field: 'aml_flagged',
      op: 'eq',
      value: 'True',
      action: 'ROUTE_TO_HUMAN',
      reason: 'AML threshold exceeded — manual review required',
    },
  ],
});

// Step 5: Human review
pipeline.addStep('udeo.human', {
  reason: 'Compliance onboarding final review',
  on_failure: 'continue',
});

const result = pipeline.run();

console.log('\n=== KYC/AML Result ===');
console.log(`Decision: ${result.decision}`);
console.log(`Reason: ${result.reason || 'N/A'}`);

console.log('\nDecision Trace:');
for (const t of result.trace) {
  console.log(`  [${t.expertId}] ${t.decisionCode} — ${t.ruleFired} (${t.executionTimeMs}ms)`);
}
