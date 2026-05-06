#!/usr/bin/env node

/**
 * UDEO v4.0 — CLI Entry Point
 * ============================
 * Universal Deterministic Expert Orchestrator
 * Usage: node cli.js <command> [subcommand] [options]
 *        or: udeo <command> [subcommand] [options]
 */

'use strict';

const fs = require('fs');
const path = require('path');

const { Config, Logger } = require('./engine');
const { ExpertRegistry } = require('./experts');
const { createLoanApprovalPipeline } = require('./pipeline');
const { Store } = require('./store');

const UDEO_VERSION = '4.0.0';

// ============================================================
// Helpers
// ============================================================

function resolveWorkspace() {
  const envWs = process.env.UDEO_WORKSPACE;
  if (envWs) {
    return path.resolve(envWs);
  }
  return process.cwd();
}

function bootstrap() {
  const ws = resolveWorkspace();
  Config.load(ws);
  Store.initialize(Config.get('store_path', path.join(ws, '.udeo', 'store')));
  Logger.configure({
    level: Config.get('log_level', 'INFO'),
    logFile: path.join(ws, '.udeo', 'udeo.log'),
    quiet: Config.get('quiet', false),
  });

  let pluginDir = path.join(ws, 'plugins');
  if (!fs.existsSync(pluginDir) || !fs.statSync(pluginDir).isDirectory()) {
    pluginDir = path.join(__dirname, 'plugins');
  }
  ExpertRegistry.discoverPlugins(pluginDir);
  return ws;
}

function parseParams(paramArgs) {
  const params = {};
  for (const p of paramArgs) {
    const eq = p.indexOf('=');
    if (eq === -1) continue;
    const k = p.substring(0, eq);
    let v = p.substring(eq + 1);

    if (v === 'true') { v = true; }
    else if (v === 'false') { v = false; }
    else if (/^-?\d+$/.test(v)) { v = parseInt(v, 10); }
    else if (/^-?\d+\.\d+$/.test(v)) { v = parseFloat(v); }
    params[k] = v;
  }
  return params;
}

// ============================================================
// Commands
// ============================================================

function cmdInit(ws) {
  console.log('Initializing UDEO workspace at:', ws);
  const udeoDir = path.join(ws, '.udeo');
  const storeDir = path.join(udeoDir, 'store');
  fs.mkdirSync(storeDir, { recursive: true });

  const configFile = path.join(udeoDir, 'config.json');
  if (!fs.existsSync(configFile)) {
    const config = {
      log_level: 'Info',
      quiet: false,
      pipeline: { auto_escalate: true, default_timeout: 60 },
    };
    fs.writeFileSync(configFile, JSON.stringify(config, null, 2), 'utf-8');
    console.log(`  Created: ${configFile}`);
  } else {
    console.log(`  Config already exists: ${configFile}`);
  }

  const pluginsDir = path.join(ws, 'plugins');
  fs.mkdirSync(pluginsDir, { recursive: true });
  console.log(`  Created: ${pluginsDir}`);

  console.log('UDEO workspace ready.');
  console.log('Next: udeo run loan-approval');
}

function cmdRun(name, params, ws) {
  let pipeline;
  if (name === 'loan-approval' || name === 'loan') {
    pipeline = createLoanApprovalPipeline(params);
  } else {
    process.stderr.write(`Unknown pipeline: ${name}\n`);
    process.stderr.write('Available: loan-approval\n');
    process.exit(1);
  }

  console.log(`\n=== UDEO Pipeline: ${pipeline.name} ===`);
  console.log(`Pipeline ID: ${pipeline.id}`);

  const result = pipeline.run();

  console.log('\n=== RESULT ===');
  const decision = result.decision || 'UNKNOWN';
  const styles = {
    APPROVED: '\x1b[92m',
    REJECTED: '\x1b[91m',
    FLAGGED: '\x1b[93m',
    ROUTE_TO_HUMAN: '\x1b[95m',
  };
  const color = styles[decision] || '';
  const reset = '\x1b[0m';
  console.log(`  Decision: ${color}${decision}${reset}`);
  if (result.reason) {
    console.log(`  Reason: ${result.reason}`);
  }
  const trace = result.trace || [];
  console.log(`  Steps: ${trace.length}`);

  console.log('\n=== DECISION TRACE ===');
  console.log(`  ${'Expert ID'.padEnd(22)} ${'Decision'.padEnd(18)} ${'Rule Fired'.padEnd(45)} ${'Time(ms)'.padStart(9)}`);
  console.log(`  ${'-'.repeat(22)} ${'-'.repeat(18)} ${'-'.repeat(45)} ${'-'.repeat(9)}`);
  for (const t of trace) {
    console.log(`  ${(t.expertId || '').padEnd(22)} ${(t.decisionCode || '').padEnd(18)} ${(t.ruleFired || '').padEnd(45)} ${String(t.executionTimeMs).padStart(9)}`);
  }

  // Save to store
  if (result.context) {
    Store.save(result.context);
    console.log(`\nSaved run: ${result.context.pipelineId}`);
  }
}

function cmdExpert(sub) {
  if (sub === 'list') {
    const experts = ExpertRegistry.getAll();
    console.log('\nRegistered Experts:');
    console.log(`  ${'ID'.padEnd(22)} ${'Name'.padEnd(22)} ${'Type'.padEnd(16)} ${'Version'}`);
    console.log(`  ${'-'.repeat(22)} ${'-'.repeat(22)} ${'-'.repeat(16)} ${'-'.repeat(8)}`);
    for (const e of experts) {
      console.log(`  ${e.id.padEnd(22)} ${e.name.padEnd(22)} ${e.type.padEnd(16)} ${e.version}`);
    }
  } else if (sub === 'register') {
    console.log('To register a custom expert, drop a .js file in: plugins/');
    console.log('See: plugins/template.js for the format.');
  } else {
    process.stderr.write('Usage: udeo expert <list|register>\n');
  }
}

function cmdHistory() {
  const runs = Store.listAll();
  console.log('\nPast Runs:');
  if (runs.length === 0) {
    console.log('  No runs found.');
    return;
  }

  for (const runId of runs) {
    const data = Store.load(runId);
    if (data) {
      const trace = data.decision_trace || [];
      const decision = trace.length > 0 ? (trace[trace.length - 1].decision_code || 'UNKNOWN') : 'UNKNOWN';
      const ts = (data.updated_at || 'unknown').substring(0, 19);
      console.log(`  ${runId}  [${decision}]  ${ts}`);
    }
  }
  console.log("\nView a specific run via the API: Store.load('<id>')");
}

function cmdConfig(ws) {
  console.log('\nUDEO Configuration:');
  console.log(`  Workspace: ${ws}`);
  console.log(`  Store:     ${Config.get('store_path')}`);
  console.log(`  LogLevel:  ${Config.get('log_level')}`);
  console.log(`  Plugins:   ${Config.get('experts.plugin_directory')}`);
  console.log('\nFull config: .udeo/config.json');
}

function cmdVersion() {
  console.log(`UDEO v${UDEO_VERSION} — Universal Deterministic Expert Orchestrator`);
  console.log('Zero-dependency, pluggable expert pipeline framework.');
}

function printUsage() {
  console.log(`UDEO v${UDEO_VERSION} — Universal Deterministic Expert Orchestrator`);
  console.log('');
  console.log('Usage: udeo <command> [options]');
  console.log('');
  console.log('Commands:');
  console.log('  init        Initialize workspace');
  console.log('  run [name]  Run a pipeline (default: loan-approval)');
  console.log('  expert list List registered experts');
  console.log('  history     View past pipeline runs');
  console.log('  config      Show configuration');
  console.log('  version     Show version');
  console.log('');
  console.log('Options:');
  console.log('  Parameters for "run" can be passed as key=value pairs');
  console.log('  Example: udeo run loan-approval income=120000 creditScore=750');
}

// ============================================================
// Main
// ============================================================

function main() {
  const args = process.argv.slice(2);
  const command = args[0];

  if (!command || command === '--help' || command === '-h') {
    printUsage();
    process.exit(0);
  }

  // Bootstrap (load config, store, experts)
  const ws = bootstrap();

  switch (command) {
    case 'init':
      cmdInit(ws);
      break;

    case 'run': {
      const pipelineName = args[1] || 'loan-approval';
      const paramArgs = args.slice(2);
      const params = parseParams(paramArgs);
      cmdRun(pipelineName, params, ws);
      break;
    }

    case 'expert': {
      const sub = args[1] || 'list';
      cmdExpert(sub);
      break;
    }

    case 'history':
      cmdHistory();
      break;

    case 'config':
      cmdConfig(ws);
      break;

    case 'version':
      cmdVersion();
      break;

    default:
      process.stderr.write(`Unknown command: ${command}\n`);
      process.stderr.write('Use "udeo --help" for usage.\n');
      process.exit(1);
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
