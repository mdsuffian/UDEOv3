/**
 * UDEO Telemetry — v4.0
 * Observability: distributed tracing spans, counters, histograms, audit log.
 * In-memory with optional JSON audit file persistence.
 */

'use strict';

const fs = require('fs');
const path = require('path');
const { performance } = require('perf_hooks');
const { shortUuid } = require('./engine');

class Telemetry {
  /** In-process observability collector for UDEO pipeline runs. */

  static _spans = {};
  static _counters = {};
  static _histograms = {};
  static _auditPath = null;

  // --- Tracing ---

  /**
   * @param {string} name
   * @returns {string} span ID
   */
  static startSpan(name) {
    const spanId = shortUuid();
    const span = {
      id: spanId,
      name,
      started_at: performance.now(),
      ended_at: null,
      duration_ms: null,
      metadata: {},
    };
    if (!Telemetry._spans[name]) {
      Telemetry._spans[name] = [];
    }
    Telemetry._spans[name].push(span);
    return spanId;
  }

  /**
   * @param {string} name
   * @param {string} spanId
   */
  static endSpan(name, spanId) {
    const spans = Telemetry._spans[name];
    if (!spans) return;
    for (const span of spans) {
      if (span.id === spanId && span.ended_at === null) {
        span.ended_at = performance.now();
        span.duration_ms = Math.round((span.ended_at - span.started_at) * 100) / 100;
        break;
      }
    }
  }

  // --- Metrics ---

  /**
   * @param {string} name
   * @param {number} [delta]
   */
  static inc(name, delta = 1.0) {
    Telemetry._counters[name] = (Telemetry._counters[name] || 0) + delta;
  }

  /**
   * @param {string} name
   * @param {number} value
   */
  static record(name, value) {
    if (!Telemetry._histograms[name]) {
      Telemetry._histograms[name] = [];
    }
    Telemetry._histograms[name].push(value);
  }

  // --- Audit ---

  /**
   * @param {string} auditPath
   */
  static initializeAudit(auditPath) {
    Telemetry._auditPath = path.resolve(auditPath);
    const dir = path.dirname(Telemetry._auditPath);
    fs.mkdirSync(dir, { recursive: true });
  }

  /**
   * @param {string} event
   * @param {object} [details]
   */
  static audit(event, details = {}) {
    if (!Telemetry._auditPath) return;
    const entry = {
      timestamp: new Date().toISOString(),
      event,
      details: details || {},
    };
    fs.appendFileSync(Telemetry._auditPath, JSON.stringify(entry) + '\n', 'utf-8');
  }

  // --- Summary ---

  /**
   * @returns {object}
   */
  static summary() {
    const spansSummary = {};
    for (const [name, spanList] of Object.entries(Telemetry._spans)) {
      const completed = spanList.filter(s => s.duration_ms !== null);
      const totalMs = completed.reduce((sum, s) => sum + s.duration_ms, 0);
      spansSummary[name] = {
        count: spanList.length,
        total_ms: Math.round(totalMs * 100) / 100,
        avg_ms: completed.length > 0 ? Math.round((totalMs / completed.length) * 100) / 100 : 0,
      };
    }

    const histogramsSummary = [];
    for (const [name, values] of Object.entries(Telemetry._histograms)) {
      const avg = values.length > 0 ? values.reduce((a, b) => a + b, 0) / values.length : 0;
      histogramsSummary.push({
        name,
        count: values.length,
        avg: Math.round(avg * 100) / 100,
      });
    }

    return {
      spans: spansSummary,
      counters: { ...Telemetry._counters },
      histograms: histogramsSummary,
    };
  }

  /**
   * Reset all telemetry data.
   */
  static reset() {
    Telemetry._spans = {};
    Telemetry._counters = {};
    Telemetry._histograms = {};
  }
}

module.exports = { Telemetry };
