// global-teardown.ts
import { readFileSync, unlinkSync, existsSync } from 'fs';

async function globalTeardown() {
  const pidFile = '.maildev-pid';

  console.log('🟢 Global teardown initiated.');

  if (existsSync(pidFile)) {
    const pidContent = readFileSync(pidFile, 'utf-8').trim();
    const pid = parseInt(pidContent, 10);
    console.log(`🔍 PID read from file: ${pid}`);

    try {
      console.log(`⚙️ Attempting to stop MailDev process group (PGID ${pid})...`);
      process.kill(-pid, 'SIGTERM'); // Negative PID means kill the entire group
      console.log('✅ MailDev process group stopped successfully.');
    } catch (error) {
      console.error(`❌ Error stopping MailDev process group (PGID ${pid}):`, error);
    }

    try {
      unlinkSync(pidFile);
      console.log(`✅ PID file "${pidFile}" deleted successfully.`);
    } catch (error) {
      console.error(`❌ Error deleting PID file "${pidFile}":`, error);
    }
  } else {
    console.warn(`⚠️ PID file "${pidFile}" not found. MailDev may not have been running.`);
  }

  console.log('🟢 Global teardown completed.');
}

export default globalTeardown;

