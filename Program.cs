/*
    NoBright - Simple brightness toggle for Windows
    Copyright (C) 2025  peyoker

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Management;
using System.Threading.Tasks;

namespace NoBright
{
    // Ventana overlay oscura con opacidad ajustable
    public class DarkOverlay : Form
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_SHOWWINDOW = 0x0040;

        public DarkOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.Opacity = 0.0;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            
            Rectangle bounds = Screen.AllScreens[0].Bounds;
            foreach (Screen screen in Screen.AllScreens)
            {
                bounds = Rectangle.Union(bounds, screen.Bounds);
            }
            this.Bounds = bounds;
        }

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public void SetOpacitySafe(double opacity)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Opacity = opacity));
            }
            else
            {
                this.Opacity = opacity;
            }
        }

        public void ShowSafe()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Show()));
            }
            else
            {
                this.Show();
            }
        }

        public void HideSafe()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Hide()));
            }
            else
            {
                this.Hide();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }
    }

    public static class SingleInstance
    {
        private static Mutex mutex = null;
        
        public static bool IsFirstInstance()
        {
            bool isNew;
            mutex = new Mutex(true, "NoBright_SingleInstance_Mutex", out isNew);
            return isNew;
        }
    }

    public class ConfigForm : Form
    {
        private ComboBox cmbKey;
        private ComboBox cmbLanguage;
        private NumericUpDown nudSeconds;
        private CheckBox chkStartup;
        private CheckBox chkDarkMode;
        private Button btnSave;
        private Button btnTest;
        private Label lblKey;
        private Label lblSeconds;
        private Label lblLanguage;
        private Label lblStatus;
        private Label lblVersion;
        private Label lblInfo;
        private TextBox txtLog;
        private TrackBar trackBrightness;
        private Label lblBrightness;
        private Label lblTheme;

        private const string VERSION = "1.1.0";

        private string[] texts_en = new string[] {
            "NoBright - Settings",
            "Activation key:",
            "Hold duration (0 = instant):",
            "Language:",
            "Theme:",
            "Dark mode",
            "Manual brightness control:",
            "Start with Windows",
            "Test Toggle",
            "Save",
            "Event log:",
            "Application started successfully",
            "Current brightness:",
            "Testing brightness toggle...",
            "Brightness state:",
            "DARK",
            "NORMAL",
            "Settings saved:",
            "✓ Saved",
            "Window minimized to tray",
            "Screen darkened",
            "Screen restored to normal",
            "Error adjusting brightness:",
            "Brightness manually adjusted:",
            "Automatic startup enabled",
            "Automatic startup disabled",
            "Error configuring startup:",
            "WARNING: Cannot control brightness on this device",
            "Auto mode: Uses overlay on lock screen, brightness control when unlocked",
            "Lock screen detected - switching to overlay mode",
            "Session unlocked - switching to brightness mode",
            "Smooth transition completed"
        };

        private string[] texts_es = new string[] {
            "NoBright - Configuración",
            "Tecla de activación:",
            "Duración de pulsación (0 = instantáneo):",
            "Idioma:",
            "Tema:",
            "Modo oscuro",
            "Control manual de brillo:",
            "Iniciar con Windows",
            "Probar Toggle",
            "Guardar",
            "Registro de eventos:",
            "Aplicación iniciada correctamente",
            "Brillo actual:",
            "Probando toggle de brillo...",
            "Estado del brillo:",
            "OSCURO",
            "NORMAL",
            "Configuración guardada:",
            "✓ Guardado",
            "Ventana minimizada a la bandeja",
            "Pantalla oscurecida",
            "Pantalla restaurada a normal",
            "Error ajustando brillo:",
            "Brillo ajustado manualmente:",
            "Inicio automático activado",
            "Inicio automático desactivado",
            "Error configurando inicio:",
            "ADVERTENCIA: No se puede controlar el brillo en este equipo",
            "Modo auto: Usa overlay en pantalla bloqueada, control de brillo al desbloquear",
            "Pantalla bloqueada detectada - cambiando a modo overlay",
            "Sesión desbloqueada - cambiando a modo brillo",
            "Transición suave completada"
        };

        public string[] currentTexts;

        public ConfigForm()
        {
            int lang = Properties.Settings.Default.Language;
            currentTexts = lang == 0 ? texts_en : texts_es;
            
            InitializeComponent();
            LoadSettings();
            ApplyTheme(Properties.Settings.Default.DarkMode);
            
            LogMessage(currentTexts[11]);
            
            if (Program.CanControlBrightness())
            {
                int current = Program.GetCurrentBrightness();
                LogMessage($"{currentTexts[12]} {current}%");
                trackBrightness.Value = current;
            }
            else
            {
                LogMessage(currentTexts[27]);
            }
        }

        private void InitializeComponent()
        {
            this.Text = currentTexts[0];
            this.Size = new Size(450, 580);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 20;

            // Info
            lblInfo = new Label();
            lblInfo.Text = currentTexts[28];
            lblInfo.Location = new Point(20, yPos);
            lblInfo.Size = new Size(400, 40);
            lblInfo.ForeColor = Color.DarkBlue;
            this.Controls.Add(lblInfo);

            yPos += 50;

            // Idioma
            lblLanguage = new Label();
            lblLanguage.Text = currentTexts[3];
            lblLanguage.Location = new Point(20, yPos);
            lblLanguage.Size = new Size(150, 20);
            this.Controls.Add(lblLanguage);

            cmbLanguage = new ComboBox();
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.Location = new Point(20, yPos + 25);
            cmbLanguage.Size = new Size(180, 25);
            cmbLanguage.Items.AddRange(new object[] { "English", "Español" });
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            this.Controls.Add(cmbLanguage);

            yPos += 60;

            // Label tecla
            lblKey = new Label();
            lblKey.Text = currentTexts[1];
            lblKey.Location = new Point(20, yPos);
            lblKey.Size = new Size(400, 20);
            this.Controls.Add(lblKey);

            cmbKey = new ComboBox();
            cmbKey.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKey.Location = new Point(20, yPos + 25);
            cmbKey.Size = new Size(400, 25);
            cmbKey.Items.AddRange(new object[] {
                "Left Control", "Right Control",
                "Left Alt", "Right Alt",
                "Left Shift", "Right Shift",
                "F1", "F2", "F3", "F4", "F5", "F6",
                "F7", "F8", "F9", "F10", "F11", "F12"
            });
            this.Controls.Add(cmbKey);

            yPos += 60;

            lblSeconds = new Label();
            lblSeconds.Text = currentTexts[2];
            lblSeconds.Location = new Point(20, yPos);
            lblSeconds.Size = new Size(350, 20);
            this.Controls.Add(lblSeconds);

            nudSeconds = new NumericUpDown();
            nudSeconds.Location = new Point(20, yPos + 25);
            nudSeconds.Size = new Size(100, 25);
            nudSeconds.Minimum = 0;
            nudSeconds.Maximum = 10;
            nudSeconds.DecimalPlaces = 1;
            nudSeconds.Increment = 0.5m;
            this.Controls.Add(nudSeconds);

            yPos += 60;

            lblTheme = new Label();
            lblTheme.Text = currentTexts[4];
            lblTheme.Location = new Point(20, yPos);
            lblTheme.Size = new Size(150, 20);
            this.Controls.Add(lblTheme);

            chkDarkMode = new CheckBox();
            chkDarkMode.Text = currentTexts[5];
            chkDarkMode.Location = new Point(20, yPos + 25);
            chkDarkMode.Size = new Size(200, 20);
            chkDarkMode.CheckedChanged += ChkDarkMode_CheckedChanged;
            this.Controls.Add(chkDarkMode);

            yPos += 55;

            Label lblManual = new Label();
            lblManual.Text = currentTexts[6];
            lblManual.Location = new Point(20, yPos);
            lblManual.Size = new Size(250, 20);
            this.Controls.Add(lblManual);

            lblBrightness = new Label();
            lblBrightness.Text = "50%";
            lblBrightness.Location = new Point(370, yPos);
            lblBrightness.Size = new Size(50, 20);
            lblBrightness.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblBrightness);

            trackBrightness = new TrackBar();
            trackBrightness.Location = new Point(20, yPos + 25);
            trackBrightness.Size = new Size(400, 45);
            trackBrightness.Minimum = 0;
            trackBrightness.Maximum = 100;
            trackBrightness.TickFrequency = 10;
            trackBrightness.Value = 50;
            trackBrightness.ValueChanged += TrackBrightness_ValueChanged;
            this.Controls.Add(trackBrightness);

            yPos += 75;

            chkStartup = new CheckBox();
            chkStartup.Text = currentTexts[7];
            chkStartup.Location = new Point(20, yPos);
            chkStartup.Size = new Size(200, 20);
            this.Controls.Add(chkStartup);

            yPos += 35;

            btnTest = new Button();
            btnTest.Text = currentTexts[8];
            btnTest.Location = new Point(20, yPos);
            btnTest.Size = new Size(125, 30);
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            btnSave = new Button();
            btnSave.Text = currentTexts[9];
            btnSave.Location = new Point(160, yPos);
            btnSave.Size = new Size(125, 30);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(295, yPos + 5);
            lblStatus.Size = new Size(125, 20);
            lblStatus.ForeColor = Color.Green;
            this.Controls.Add(lblStatus);

            yPos += 40;

            lblVersion = new Label();
            lblVersion.Text = $"v{VERSION}";
            lblVersion.Location = new Point(370, yPos);
            lblVersion.Size = new Size(50, 15);
            lblVersion.Font = new Font(lblVersion.Font.FontFamily, 7);
            lblVersion.ForeColor = Color.Gray;
            lblVersion.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblVersion);

            yPos += 10;

            Label lblLog = new Label();
            lblLog.Text = currentTexts[10];
            lblLog.Location = new Point(20, yPos);
            lblLog.Size = new Size(150, 20);
            this.Controls.Add(lblLog);

            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Location = new Point(20, yPos + 25);
            txtLog.Size = new Size(400, 100);
            txtLog.ReadOnly = true;
            this.Controls.Add(txtLog);

            this.FormClosing += ConfigForm_FormClosing;
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Language = cmbLanguage.SelectedIndex;
            Properties.Settings.Default.Save();
            
            MessageBox.Show(
                cmbLanguage.SelectedIndex == 0 
                    ? "Language changed. Please restart the application." 
                    : "Idioma cambiado. Por favor reinicia la aplicación.",
                "NoBright",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ChkDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            ApplyTheme(chkDarkMode.Checked);
        }

        private void ApplyTheme(bool darkMode)
        {
            if (darkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.White;
                txtLog.BackColor = Color.FromArgb(45, 45, 45);
                txtLog.ForeColor = Color.White;
                lblInfo.ForeColor = Color.LightBlue;
            }
            else
            {
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                txtLog.BackColor = Color.White;
                txtLog.ForeColor = Color.Black;
                lblInfo.ForeColor = Color.DarkBlue;
            }
        }

        private void TrackBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = $"{trackBrightness.Value}%";
            Program.SetBrightness(trackBrightness.Value);
            LogMessage($"{currentTexts[23]} {trackBrightness.Value}%");
        }

        public void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\r\n");
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                LogMessage(currentTexts[19]);
            }
        }

        private void LoadSettings()
        {
            cmbLanguage.SelectedIndex = Properties.Settings.Default.Language;
            cmbKey.SelectedIndex = Math.Max(0, Math.Min(Properties.Settings.Default.KeyIndex, cmbKey.Items.Count - 1));
            nudSeconds.Value = (decimal)Properties.Settings.Default.HoldSeconds;
            chkStartup.Checked = IsInStartup();
            chkDarkMode.Checked = Properties.Settings.Default.DarkMode;
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            LogMessage(currentTexts[13]);
            Program.ToggleBrightness();
            string state = Program.brightnessIsLow ? currentTexts[15] : currentTexts[16];
            LogMessage($"{currentTexts[14]} {state}");
            
            int current = Program.GetCurrentBrightness();
            if (trackBrightness.Value != current)
            {
                trackBrightness.Value = current;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.KeyIndex = cmbKey.SelectedIndex;
            Properties.Settings.Default.HoldSeconds = (double)nudSeconds.Value;
            Properties.Settings.Default.DarkMode = chkDarkMode.Checked;
            Properties.Settings.Default.Save();

            SetStartup(chkStartup.Checked);

            lblStatus.Text = currentTexts[18];
            LogMessage($"{currentTexts[17]} {cmbKey.Text}, {nudSeconds.Value}s");
            
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000;
            timer.Tick += (s, ev) => { lblStatus.Text = ""; timer.Stop(); };
            timer.Start();
        }

        private bool IsInStartup()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                return key?.GetValue("NoBright") != null;
            }
            catch { return false; }
        }

        private void SetStartup(bool enable)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (enable)
                {
                    key.SetValue("NoBright", "\"" + Application.ExecutablePath + "\"");
                    LogMessage(currentTexts[24]);
                }
                else
                {
                    key.DeleteValue("NoBright", false);
                    LogMessage(currentTexts[25]);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"{currentTexts[26]} {ex.Message}");
            }
        }
    }

    static class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll")]
        static extern bool CloseDesktop(IntPtr hDesktop);

        public static bool brightnessIsLow = false;
        static int savedBrightness = 50;
        static NotifyIcon trayIcon;
        static ConfigForm configForm;
        static DateTime keyPressStart = DateTime.MinValue;
        static bool wasPressed = false;
        static DarkOverlay overlay = null;
        static bool wasLocked = false;
        static bool isTransitioning = false;

        static int[] keyCodes = { 0xA2, 0xA3, 0xA4, 0xA5, 0xA0, 0xA1, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B };

        public static bool IsWorkstationLocked()
        {
            IntPtr hDesktop = OpenDesktop("Default", 0, false, 0x0100);
            if (hDesktop == IntPtr.Zero)
            {
                return true; // Si no puede abrir el escritorio, probablemente está bloqueado
            }
            CloseDesktop(hDesktop);
            return false;
        }

        public static bool CanControlBrightness()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");
                ManagementObjectCollection collection = searcher.Get();
                return collection.Count > 0;
            }
            catch { return false; }
        }

        public static int GetCurrentBrightness()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    return Convert.ToInt32(obj["CurrentBrightness"]);
                }
            }
            catch { }
            return 50;
        }

        public static void SetBrightness(int brightness)
        {
            try
            {
                brightness = Math.Max(0, Math.Min(100, brightness));
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    obj.InvokeMethod("WmiSetBrightness", new object[] { 1, brightness });
                    break;
                }
            }
            catch { }
        }

        // Transición suave entre overlay y brillo
        static async Task SmoothTransition(bool toOverlay)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            const int steps = 20;
            const int delayMs = 50; // Total: 1 segundo

            if (toOverlay)
            {
                // Transición: Brillo → Overlay
                int currentBrightness = GetCurrentBrightness();
                
                for (int i = 0; i <= steps; i++)
                {
                    double progress = (double)i / steps;
                    
                    // Reducir brillo gradualmente
                    int newBrightness = (int)(currentBrightness * (1 - progress));
                    SetBrightness(newBrightness);
                    
                    // Aumentar overlay gradualmente
                    if (overlay != null && overlay.Visible)
                    {
                        overlay.SetOpacitySafe(0.95 * progress);
                    }
                    
                    await Task.Delay(delayMs);
                }
            }
            else
            {
                // Transición: Overlay → Brillo
                for (int i = 0; i <= steps; i++)
                {
                    double progress = (double)i / steps;
                    
                    // Reducir overlay gradualmente
                    if (overlay != null && overlay.Visible)
                    {
                        overlay.SetOpacitySafe(0.95 * (1 - progress));
                    }
                    
                    // Aumentar brillo gradualmente
                    int newBrightness = (int)(savedBrightness * progress);
                    SetBrightness(newBrightness);
                    
                    await Task.Delay(delayMs);
                }
                
                // Ocultar overlay al final
                if (overlay != null)
                {
                    overlay.HideSafe();
                }
            }

            isTransitioning = false;
            configForm?.LogMessage(configForm.currentTexts[31]); // "Smooth transition completed"
        }

        public static async void ToggleBrightness()
        {
            bool isLocked = IsWorkstationLocked();

            if (!brightnessIsLow)
            {
                // Activar modo oscuro
                savedBrightness = GetCurrentBrightness();
                
                if (isLocked)
                {
                    // Pantalla bloqueada: usar overlay
                    if (overlay == null)
                    {
                        overlay = new DarkOverlay();
                    }
                    overlay.SetOpacitySafe(0.0);
                    overlay.ShowSafe();
                    await SmoothTransition(true);
                }
                else
                {
                    // Pantalla desbloqueada: usar brillo WMI
                    SetBrightness(0);
                }
                
                brightnessIsLow = true;
                configForm?.LogMessage($"✓ {configForm.currentTexts[20]}");
            }
            else
            {
                // Restaurar modo normal
                if (isLocked && overlay != null && overlay.Visible)
                {
                    // Estaba en overlay, restaurar con transición
                    await SmoothTransition(false);
                }
                else
                {
                    // Estaba en modo brillo, restaurar directamente
                    SetBrightness(savedBrightness);
                }
                
                brightnessIsLow = false;
                configForm?.LogMessage($"✓ {configForm.currentTexts[21]}");
            }
        }

        // Monitor de cambio de estado de bloqueo
        static async void MonitorLockState()
        {
            while (true)
            {
                try
                {
                    bool isLocked = IsWorkstationLocked();
                    
                    // Detectar cambio de estado
                    if (isLocked != wasLocked)
                    {
                        wasLocked = isLocked;
                        
                        if (brightnessIsLow)
                        {
                            if (isLocked)
                            {
                                // Acaba de bloquear la pantalla
                                configForm?.LogMessage(configForm.currentTexts[29]); // "Lock screen detected..."
                                
                                // Cambiar de brillo a overlay
                                if (overlay == null)
                                {
                                    overlay = new DarkOverlay();
                                }
                                overlay.SetOpacitySafe(0.0);
                                overlay.ShowSafe();
                                await SmoothTransition(true);
                            }
                            else
                            {
                                // Acaba de desbloquear la pantalla
                                configForm?.LogMessage(configForm.currentTexts[30]); // "Session unlocked..."
                                
                                // Cambiar de overlay a brillo
                                await SmoothTransition(false);
                            }
                        }
                    }
                    
                    await Task.Delay(500); // Revisar cada medio segundo
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }

        [STAThread]
        static void Main()
        {
            if (!SingleInstance.IsFirstInstance())
            {
                MessageBox.Show(
                    "NoBright is already running.\nCheck the system tray.",
                    "NoBright",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            configForm = new ConfigForm();

            trayIcon = new NotifyIcon();
            try
            {
                trayIcon.Icon = new Icon("icon.ico");
            }
            catch
            {
                try
                {
                    trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                catch
                {
                    trayIcon.Icon = SystemIcons.Application;
                }
            }
            trayIcon.Text = "NoBright - Right click for options";
            trayIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("⚙️ Settings", null, (s, e) => {
                configForm.Show();
                configForm.WindowState = FormWindowState.Normal;
                configForm.BringToFront();
            });
            menu.Items.Add("-");
            menu.Items.Add("❌ Exit", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });
            trayIcon.ContextMenuStrip = menu;

            trayIcon.DoubleClick += (s, e) => {
                configForm.Show();
                configForm.WindowState = FormWindowState.Normal;
                configForm.BringToFront();
            };

            if (CanControlBrightness())
            {
                trayIcon.ShowBalloonTip(2000, "NoBright", 
                    "Application started.\nAuto mode: overlay on lock screen, brightness when unlocked.", 
                    ToolTipIcon.Info);
            }

            configForm.Show();

            // Iniciar monitor de estado de bloqueo
            Task.Run(() => MonitorLockState());

            // Iniciar monitoreo de teclas
            Thread monitorThread = new Thread(MonitorKeyPress);
            monitorThread.IsBackground = true;
            monitorThread.Start();

            Application.Run();
        }

        static void MonitorKeyPress()
        {
            while (true)
            {
                try
                {
                    int selectedKey = keyCodes[Math.Max(0, Math.Min(Properties.Settings.Default.KeyIndex, keyCodes.Length - 1))];
                    double holdSeconds = Properties.Settings.Default.HoldSeconds;
                    bool keyPressed = (GetAsyncKeyState(selectedKey) & 0x8000) != 0;

                    if (keyPressed && !wasPressed)
                    {
                        wasPressed = true;
                        keyPressStart = DateTime.Now;
                        if (holdSeconds == 0)
                        {
                            ToggleBrightness();
                        }
                    }
                    else if (keyPressed && wasPressed && holdSeconds > 0)
                    {
                        TimeSpan elapsed = DateTime.Now - keyPressStart;
                        if (elapsed.TotalSeconds >= holdSeconds && keyPressStart != DateTime.MaxValue)
                        {
                            ToggleBrightness();
                            keyPressStart = DateTime.MaxValue;
                        }
                    }
                    else if (!keyPressed && wasPressed)
                    {
                        wasPressed = false;
                        keyPressStart = DateTime.MinValue;
                    }
                    Thread.Sleep(50);
                }
                catch { Thread.Sleep(1000); }
            }
        }
    }
}

namespace NoBright.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        public static Settings Default { get { return defaultInstance; } }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int KeyIndex
        {
            get { return ((int)(this["KeyIndex"])); }
            set { this["KeyIndex"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public double HoldSeconds
        {
            get { return ((double)(this["HoldSeconds"])); }
            set { this["HoldSeconds"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int Language
        {
            get { return ((int)(this["Language"])); }
            set { this["Language"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("false")]
        public bool DarkMode
        {
            get { return ((bool)(this["DarkMode"])); }
            set { this["DarkMode"] = value; }
        }
    }
}

