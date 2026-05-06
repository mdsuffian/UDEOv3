#!/usr/bin/env node

/**
 * Example 02: Loan Approval — Full Deterministic Pipeline
 * Demonstrates UDEO's built-in experts: validation, math, risk, human review.
 * Runs multiple scenarios to show different risk outcomes.
 *
 * Usage: node examples/02_loan_approval.js
 */

'use strict';

const path = require('path');
const { createLoanApprovalPipeline, Store, Config, Logger } = require('../index');

// Bootstrap config with workspace
const workspace = process.env.UDEO_WORKSPACE || process.cwd();
Config.load(workspace);
Store.initialize(path.join(workspace, '.udeo', 'store'));

console.log('=== UDEO Loan Approval Pipeline ===\n');

// Scenario 1: Strong applicant
console.log('--- Strong Applicant ---');
const pipeline = createLoanApprovalPipeline({
  income: 120000,
  debt: 30000,
  creditScore: 750,
  loanAmount: 450000,
  interestRate: 0.0625,
  termMonths: 360,
  propertyValue: 550000,
});
const result = pipeline.run();

console.log(`Decision: ${result.decision}`);
console.log(`Reason: ${result.reason || 'N/A'}`);

if (result.context && result.context.data.calculations) {
  const calcs = result.context.data.calculations;
  console.log(`DTI: ${calcs.dti}%`);
  console.log(`LTV: ${calcs.ltv}%`);
  console.log(`Monthly Payment: $${calcs.monthly_payment}`);
}

console.log('\nDecision Trace:');
for (const t of result.trace) {
  console.log(`  ${t.expertId} -> ${t.decisionCode} (${t.ruleFired}) [${t.executionTimeMs}ms]`);
}

// Save
Store.save(result.context);
console.log(`\nRun saved as: ${result.context.pipelineId}`);

// Scenario 2: High DTI (should flag)
console.log('\n--- High DTI (should flag) ---');
const p2 = createLoanApprovalPipeline({
  income: 50000,
  debt: 35000,
  creditScore: 700,
  loanAmount: 200000,
  propertyValue: 250000,
});
const r2 = p2.run();
console.log(`Decision: ${r2.decision} — ${r2.reason}`);

// Scenario 3: Bad credit (should reject)
console.log('\n--- Bad Credit (should reject) ---');
const p3 = createLoanApprovalPipeline({
  income: 80000,
  debt: 10000,
  creditScore: 550,
  loanAmount: 150000,
  propertyValue: 200000,
});
const r3 = p3.run();
console.log(`Decision: ${r3.decision} — ${r3.reason}`);

// Scenario 4: Perfect applicant (should approve)
console.log('\n--- Perfect Applicant (should approve) ---');
const p4 = createLoanApprovalPipeline({
  income: 200000,
  debt: 5000,
  creditScore: 820,
  loanAmount: 300000,
  propertyValue: 500000,
});
const r4 = p4.run();
console.log(`Decision: ${r4.decision} — ${r4.reason}`);

console.log('\n=== Done ===');
