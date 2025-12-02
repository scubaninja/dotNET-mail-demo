/**
 * Accessibility Utilities for CLI
 * 
 * Provides utilities for screen reader-friendly CLI output.
 * These helpers ensure consistent, clear output that works well
 * with terminal screen readers like NVDA, JAWS, VoiceOver, and Orca.
 */

const consola = require("consola");

/**
 * Announces a status message clearly for screen readers.
 * Uses a consistent format: [STATUS] message
 * 
 * @param {string} status - Status type (e.g., "SUCCESS", "ERROR", "INFO")
 * @param {string} message - The message to announce
 */
const announceStatus = (status, message) => {
  // Add a blank line before for better screen reader parsing
  console.log("");
  console.log(`[${status.toUpperCase()}] ${message}`);
};

/**
 * Announces the start of an operation.
 * Screen readers will clearly hear the operation starting.
 * 
 * @param {string} operation - Description of the operation
 */
const announceStart = (operation) => {
  announceStatus("START", `Beginning: ${operation}`);
};

/**
 * Announces successful completion.
 * 
 * @param {string} message - Success message
 */
const announceSuccess = (message) => {
  announceStatus("SUCCESS", message);
  consola.success(message);
};

/**
 * Announces an error clearly.
 * 
 * @param {string} message - Error message
 */
const announceError = (message) => {
  announceStatus("ERROR", message);
  consola.error(message);
};

/**
 * Announces informational message.
 * 
 * @param {string} message - Info message
 */
const announceInfo = (message) => {
  announceStatus("INFO", message);
  consola.info(message);
};

/**
 * Announces a warning.
 * 
 * @param {string} message - Warning message
 */
const announceWarning = (message) => {
  announceStatus("WARNING", message);
  consola.warn(message);
};

/**
 * Announces a progress update.
 * 
 * @param {number} current - Current item number
 * @param {number} total - Total items
 * @param {string} item - Description of current item (optional)
 */
const announceProgress = (current, total, item = "") => {
  const percentage = Math.round((current / total) * 100);
  const progressMessage = item 
    ? `Progress: ${current} of ${total} (${percentage}%) - ${item}`
    : `Progress: ${current} of ${total} (${percentage}%)`;
  console.log(progressMessage);
};

/**
 * Announces a list of items in a screen reader-friendly format.
 * 
 * @param {string} title - List title
 * @param {Array<string>} items - Array of items
 */
const announceList = (title, items) => {
  console.log("");
  console.log(`[LIST] ${title} (${items.length} items):`);
  items.forEach((item, index) => {
    console.log(`  ${index + 1}. ${item}`);
  });
  console.log(`[END LIST] ${title}`);
};

/**
 * Announces a prompt is waiting for user input.
 * This helps screen reader users know when to respond.
 * 
 * @param {string} promptDescription - Description of what input is expected
 */
const announcePrompt = (promptDescription) => {
  console.log("");
  console.log(`[INPUT REQUIRED] ${promptDescription}`);
};

/**
 * Announces the completion of a long-running operation.
 * 
 * @param {string} operation - Description of the operation
 * @param {number} durationMs - Duration in milliseconds (optional)
 */
const announceComplete = (operation, durationMs = null) => {
  const timeInfo = durationMs 
    ? ` Completed in ${(durationMs / 1000).toFixed(1)} seconds.`
    : "";
  announceStatus("COMPLETE", `Finished: ${operation}.${timeInfo}`);
};

module.exports = {
  announceStatus,
  announceStart,
  announceSuccess,
  announceError,
  announceInfo,
  announceWarning,
  announceProgress,
  announceList,
  announcePrompt,
  announceComplete
};
