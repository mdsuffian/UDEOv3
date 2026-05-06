/**
 * UDEO Store — v4.0
 * JSON file-based state persistence. Human-readable, zero-dependency.
 * Stores pipeline context + decision traces to .udeo/store/*.json
 */

'use strict';

const fs = require('fs');
const path = require('path');
const { Logger } = require('./engine');

class Store {
  /** File-system-backed persistence for pipeline run contexts. */

  static _rootPath = null;
  static _isReady = false;

  /**
   * @param {string} rootPath
   */
  static initialize(rootPath) {
    Store._rootPath = path.resolve(rootPath);
    fs.mkdirSync(Store._rootPath, { recursive: true });
    Store._isReady = true;
    Logger.debug(`Store initialized: ${rootPath}`);
  }

  static _requireReady() {
    if (!Store._isReady) {
      Logger.warn('Store not initialized');
      return false;
    }
    return true;
  }

  /**
   * @param {import('./engine').Context} context
   */
  static save(context) {
    if (!Store._requireReady()) return;
    const filePath = path.join(Store._rootPath, `${context.pipelineId}.json`);
    const data = context.toDict();
    fs.writeFileSync(filePath, JSON.stringify(data, null, 2), 'utf-8');
    Logger.debug(`Saved: ${filePath}`);
  }

  /**
   * @param {string} pipelineId
   * @returns {object|null}
   */
  static load(pipelineId) {
    if (!Store._requireReady()) return null;
    const filePath = path.join(Store._rootPath, `${pipelineId}.json`);
    if (!fs.existsSync(filePath)) {
      Logger.debug(`Context not found: ${pipelineId}`);
      return null;
    }
    try {
      return JSON.parse(fs.readFileSync(filePath, 'utf-8'));
    } catch (e) {
      Logger.error(`Failed to load context: ${e.message}`);
      return null;
    }
  }

  /**
   * @returns {string[]}
   */
  static listAll() {
    if (!Store._requireReady()) return [];
    const files = fs.readdirSync(Store._rootPath)
      .filter(f => f.endsWith('.json'))
      .map(f => f.replace(/\.json$/, ''));
    return files.sort();
  }

  /**
   * @param {string} pipelineId
   */
  static delete(pipelineId) {
    if (!Store._requireReady()) return;
    const filePath = path.join(Store._rootPath, `${pipelineId}.json`);
    if (fs.existsSync(filePath)) {
      fs.unlinkSync(filePath);
      Logger.debug(`Deleted: ${pipelineId}`);
    }
  }

  /**
   * Remove all stored pipeline runs.
   */
  static purge() {
    if (!Store._requireReady()) return;
    const dir = Store._rootPath;
    const files = fs.readdirSync(dir).filter(f => f.endsWith('.json'));
    for (const f of files) {
      fs.unlinkSync(path.join(dir, f));
    }
    Logger.info(`Store purged: ${files.length} files removed`);
  }
}

module.exports = { Store };
