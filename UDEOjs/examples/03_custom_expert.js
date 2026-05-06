#!/usr/bin/env node

/**
 * Example 03: Custom Expert — GDPR Compliance Audit
 * Demonstrates building a custom expert and composing a domain-specific pipeline.
 *
 * Usage: node examples/03_custom_expert.js
 */

'use strict';

const { Pipeline, ExpertType, registerExpert, Logger } = require('../index');

// Custom GDPR compliance expert
function gdprCheckExecute(context, parameters) {
  const userData = context.data.user_data || {};
  const issues = [];

  // Check for explicit consent
  if (!userData.consent_given) {
    issues.push('No explicit consent recorded');
  }

  // Check data retention policy
  const retentionDays = userData.retention_days || 0;
  if (retentionDays <= 0 || retentionDays > 365) {
    issues.push(`Invalid data retention period: ${retentionDays} days`);
  }

  // Check right to deletion
  if (!userData.deletion_request_enabled) {
    issues.push('Right to deletion mechanism not implemented');
  }

  if (issues.length > 0) {
    context.data.gdpr_issues = issues;
    return {
      success: false,
      decision_code: 'FLAGGED',
      rule_fired: `GDPR_VIOLATIONS:${issues.join('; ')}`,
    };
  }

  return { success: true, decision_code: 'VALID', rule_fired: 'GDPR_COMPLIANT' };
}

registerExpert('gdpr.check', 'GDPR Compliance Checker', ExpertType.CUSTOM, gdprCheckExecute,
  { description: 'Validates GDPR compliance for user data processing' });

// Build a GDPR audit pipeline
const pipeline = new Pipeline('GDPR_Audit');

// Seed context with user data
pipeline.context.data = {
  audit_id: 'AUD-2026-001',
  company: 'Example Corp',
  user_data: {
    consent_given: true,
    retention_days: 180,
    deletion_request_enabled: true,
    data_encrypted: true,
    cross_border_transfer: false,
  },
};

// Step 1: Validate required fields exist
pipeline.addStep('udeo.validation', {
  field: 'audit_id',
  schema: 'non_empty_string',
  required: true,
});

// Step 2: Validate company name
pipeline.addStep('udeo.validation', {
  field: 'company',
  schema: 'non_empty_string',
  required: true,
});

// Step 3: Run GDPR compliance check
pipeline.addStep('gdpr.check', {});

// Step 4: Audit trail recorded
pipeline.addStep('udeo.human', {
  reason: 'GDPR audit completed — awaiting sign-off',
  on_failure: 'continue',
});

const result = pipeline.run();

console.log('\n=== GDPR Audit Result ===');
console.log(`Decision: ${result.decision}`);
console.log(`Rule: ${result.reason}`);

if (result.context.data.gdpr_issues) {
  console.log('Issues found:');
  for (const issue of result.context.data.gdpr_issues) {
    console.log(`  - ${issue}`);
  }
}

console.log('\nDecision Trace:');
for (const t of result.trace) {
  console.log(`  ${t.expertId} -> ${t.decisionCode}`);
}
