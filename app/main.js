const { app, BrowserWindow, dialog } = require('electron');
const path = require('path');
const { spawn } = require('child_process');

let backendProcess = null;
let win = null;
let splash = null;
const BACKEND_PORT = 5000;
const BACKEND_URL = `http://localhost:${BACKEND_PORT}`;
const HEALTH_URL = `${BACKEND_URL}/health`;

// ─── Determinar si estamos en producción ──────────────────────────────────────
const isDev = !app.isPackaged;

// ─── Ventana de Carga (Splash) ────────────────────────────────────────────────
function createSplashWindow() {
  splash = new BrowserWindow({
    width: 450,
    height: 350,
    transparent: true,
    frame: false,
    alwaysOnTop: true,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true
    }
  });

  splash.loadFile(path.join(__dirname, 'splash.html'));

  splash.on('closed', () => {
    splash = null;
  });
}

// ─── Esperar a que el backend responda ────────────────────────────────────────
async function waitForBackend(maxAttempts = 40, intervalMs = 500) {
  for (let i = 0; i < maxAttempts; i++) {
    try {
      const res = await fetch(HEALTH_URL);
      if (res.ok) return true;
    } catch {
      // Backend aún no arrancó
    }
    await new Promise(resolve => setTimeout(resolve, intervalMs));
  }
  return false;
}

// ─── Iniciar el backend ───────────────────────────────────────────────────────
function startBackend() {
  let backendExecutable;
  let backendArgs = [];
  let backendCwd;

  if (isDev) {
    // En desarrollo: usar `dotnet run`
    backendExecutable = 'dotnet';
    backendArgs = ['run'];
    backendCwd = path.join(__dirname, '..', 'backend');
  } else {
    // En producción: usar el binario publicado incluido en resources
    const exeName = process.platform === 'win32' ? 'Backend.exe' : 'Backend';
    backendExecutable = path.join(process.resourcesPath, 'backend', exeName);
    backendCwd = path.join(process.resourcesPath, 'backend');
  }

  backendProcess = spawn(backendExecutable, backendArgs, {
    cwd: backendCwd,
    stdio: ['ignore', 'pipe', 'pipe'],
    shell: isDev, // Solo usar shell en dev (para `dotnet run`)
    windowsHide: true
  });

  // Redirigir stdout/stderr del backend a la consola de Electron (visible en logs)
  backendProcess.stdout?.on('data', (data) => {
    process.stdout.write(`[Backend] ${data}`);
  });
  backendProcess.stderr?.on('data', (data) => {
    process.stderr.write(`[Backend:ERR] ${data}`);
  });

  backendProcess.on('error', (err) => {
    console.error('No se pudo iniciar el backend:', err);
    if (splash) splash.close();
    dialog.showErrorBox(
      'Error al iniciar la aplicación',
      `No se pudo iniciar el servidor servidor interno.\n\nDetalle: ${err.message}`
    );
    app.quit();
  });

  backendProcess.on('exit', (code, signal) => {
    // Si el backend muere inesperadamente (no por nosotros), mostrar error
    if (code !== 0 && code !== null) {
      console.error(`Backend terminó con código ${code}`);
      if (splash) splash.close();
      if (win && !win.isDestroyed()) {
        dialog.showErrorBox(
          'Error del servidor',
          `El servidor interno se detuvo inesperadamente (código ${code}).\n\nLa aplicación se cerrará.`
        );
        app.quit();
      }
    }
  });
}

// ─── Crear la ventana principal ───────────────────────────────────────────────
function createWindow() {
  win = new BrowserWindow({
    width: 1280,
    height: 800,
    minWidth: 1024,
    minHeight: 600,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: true
    },
    show: false // No mostrar hasta que esté listo
  });

  // Cargar la UI servida por el backend (no como file://)
  win.loadURL(BACKEND_URL);

  win.once('ready-to-show', () => {
    if (splash) {
      splash.close();
    }
    win.show();
  });

  win.on('closed', () => {
    win = null;
  });
}

// ─── Ciclo de vida ────────────────────────────────────────────────────────────
app.whenReady().then(async () => {
  createSplashWindow();
  startBackend();

  // Esperar a que el backend esté disponible antes de crear la ventana
  const backendReady = await waitForBackend();

  if (!backendReady) {
    if (splash) splash.close();
    dialog.showErrorBox(
      'Error al iniciar la aplicación',
      `El servidor interno no respondió en el tiempo esperado.\n\nAsegurate de que el puerto ${BACKEND_PORT} no esté en uso por otra aplicación.`
    );
    app.quit();
    return;
  }

  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// ─── Graceful shutdown ────────────────────────────────────────────────────────
app.on('before-quit', () => {
  if (backendProcess) {
    try {
      // En Windows, taskkill garantiza que el proceso hijo y sus hijos mueran
      if (process.platform === 'win32') {
        spawn('taskkill', ['/pid', backendProcess.pid.toString(), '/f', '/t'], {
          stdio: 'ignore'
        });
      } else {
        backendProcess.kill('SIGTERM');
      }
    } catch (err) {
      console.error('Error al terminar el backend:', err);
    }
    backendProcess = null;
  }
});
