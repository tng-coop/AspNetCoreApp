// global-setup.ts
import { spawn } from 'child_process';
import { writeFileSync } from 'fs';

async function globalSetup() {
  console.log('🔵 Global setup started.');

  // Start process in detached mode to form its own process group
  const maildevProcess = spawn('npx', ['maildev', '--smtp', '1025', '--web', '1080'], {
    detached: true,
    stdio: ['ignore', 'pipe', 'pipe'],
  });

  maildevProcess.unref(); // Allow process to run independently

  await new Promise<void>((resolve, reject) => {
    maildevProcess.stdout.on('data', (data) => {
      const output = data.toString();
      console.log(`MailDev output: ${output}`);

      if (output.includes('MailDev webapp running at')) {
        console.log('✅ MailDev started successfully.');
        resolve();
      }
    });

    maildevProcess.stderr.on('data', (data) => {
      const errOutput = data.toString();
      console.error(`❌ MailDev stderr: ${errOutput}`);
      reject(errOutput);
    });

    maildevProcess.on('error', (err) => {
      console.error(`❌ Failed to start MailDev: ${err}`);
      reject(err);
    });
  });

  const pid = maildevProcess.pid;
  console.log(`🟢 MailDev parent PID (process group): ${pid}`);

  writeFileSync('.maildev-pid', pid.toString(), 'utf-8');
  console.log('✅ PID file created.');
}

export default globalSetup;

