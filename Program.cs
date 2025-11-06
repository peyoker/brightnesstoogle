using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Management;
using System.Diagnostics;
using System.Globalization;
using System.Resources;

namespace NoBright
{
    // Gestor de instancia única
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

    // Formulario principal de configuración
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
        private TextBox txtLog;
        private TrackBar trackBrightness;
        private Label lblBrightness;
        private Label lblTheme;

        private const string VERSION = "1.0.0";

        // Textos en diferentes idiomas
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
            "MINIMUM",
            "NORMAL",
            "Settings saved:",
            "✓ Saved",
            "Window minimized to tray",
            "Brightness reduced to minimum (saved:",
            "Brightness restored to",
            "Error adjusting brightness:",
            "Brightness manually adjusted:",
            "Automatic startup enabled",
            "Automatic startup disabled",
            "Error configuring startup:",
            "WARNING: Cannot control brightness on this device"
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
            "MÍNIMO",
            "NORMAL",
            "Configuración guardada:",
            "✓ Guardado",
            "Ventana minimizada a la bandeja",
            "Brillo reducido al mínimo (guardado:",
            "Brillo restaurado a",
            "Error ajustando brillo:",
            "Brillo ajustado manualmente:",
            "Inicio automático activado",
            "Inicio automático desactivado",
            "Error configurando inicio:",
            "ADVERTENCIA: No se puede controlar el brillo en este equipo"
        };

        private string[] currentTexts;

        public ConfigForm()
        {
            // Cargar idioma
            int lang = Properties.Settings.Default.Language;
            currentTexts = lang == 0 ? texts_en : texts_es;
            
            InitializeComponent();
            LoadSettings();
            ApplyTheme(Properties.Settings.Default.DarkMode);
            
            LogMessage(currentTexts[11]); // "Application started successfully"
            
            if (Program.CanControlBrightness())
            {
                int current = Program.GetCurrentBrightness();
                LogMessage($"{currentTexts[12]} {current}%"); // "Current brightness: X%"
                trackBrightness.Value = current;
            }
            else
            {
                LogMessage(currentTexts[27]); // "WARNING: Cannot control..."
                btnTest.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = currentTexts[0]; // "NoBright - Settings"
            this.Size = new Size(450, 550);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 20;

            // Idioma
            lblLanguage = new Label();
            lblLanguage.Text = currentTexts[3]; // "Language:"
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
            lblKey.Text = currentTexts[1]; // "Activation key:"
            lblKey.Location = new Point(20, yPos);
            lblKey.Size = new Size(400, 20);
            this.Controls.Add(lblKey);

            // ComboBox teclas
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

            // Label segundos
            lblSeconds = new Label();
            lblSeconds.Text = currentTexts[2]; // "Hold duration..."
            lblSeconds.Location = new Point(20, yPos);
            lblSeconds.Size = new Size(350, 20);
            this.Controls.Add(lblSeconds);

            // NumericUpDown segundos
            nudSeconds = new NumericUpDown();
            nudSeconds.Location = new Point(20, yPos + 25);
            nudSeconds.Size = new Size(100, 25);
            nudSeconds.Minimum = 0;
            nudSeconds.Maximum = 10;
            nudSeconds.DecimalPlaces = 1;
            nudSeconds.Increment = 0.5m;
            this.Controls.Add(nudSeconds);

            yPos += 60;

            // Tema
            lblTheme = new Label();
            lblTheme.Text = currentTexts[4]; // "Theme:"
            lblTheme.Location = new Point(20, yPos);
            lblTheme.Size = new Size(150, 20);
            this.Controls.Add(lblTheme);

            chkDarkMode = new CheckBox();
            chkDarkMode.Text = currentTexts[5]; // "Dark mode"
            chkDarkMode.Location = new Point(20, yPos + 25);
            chkDarkMode.Size = new Size(200, 20);
            chkDarkMode.CheckedChanged += ChkDarkMode_CheckedChanged;
            this.Controls.Add(chkDarkMode);

            yPos += 55;

            // Control manual de brillo
            Label lblManual = new Label();
            lblManual.Text = currentTexts[6]; // "Manual brightness control:"
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

            // CheckBox inicio con Windows
            chkStartup = new CheckBox();
            chkStartup.Text = currentTexts[7]; // "Start with Windows"
            chkStartup.Location = new Point(20, yPos);
            chkStartup.Size = new Size(200, 20);
            this.Controls.Add(chkStartup);

            yPos += 35;

            // Botones
            btnTest = new Button();
            btnTest.Text = currentTexts[8]; // "Test Toggle"
            btnTest.Location = new Point(20, yPos);
            btnTest.Size = new Size(125, 30);
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            btnSave = new Button();
            btnSave.Text = currentTexts[9]; // "Save"
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

            // Versión
            lblVersion = new Label();
            lblVersion.Text = $"v{VERSION}";
            lblVersion.Location = new Point(370, yPos);
            lblVersion.Size = new Size(50, 15);
            lblVersion.Font = new Font(lblVersion.Font.FontFamily, 7);
            lblVersion.ForeColor = Color.Gray;
            lblVersion.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblVersion);

            yPos += 10;

            // TextBox log
            Label lblLog = new Label();
            lblLog.Text = currentTexts[10]; // "Event log:"
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
                // Tema oscuro
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.White;
                txtLog.BackColor = Color.FromArgb(45, 45, 45);
                txtLog.ForeColor = Color.White;
            }
            else
            {
                // Tema claro
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                txtLog.BackColor = Color.White;
                txtLog.ForeColor = Color.Black;
            }
        }

        private void TrackBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = $"{trackBrightness.Value}%";
            Program.SetBrightness(trackBrightness.Value);
            LogMessage($"{currentTexts[23]} {trackBrightness.Value}%"); // "Brightness manually adjusted: X%"
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
                LogMessage(currentTexts[19]); // "Window minimized to tray"
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
            LogMessage(currentTexts[13]); // "Testing brightness toggle..."
            Program.ToggleBrightness();
            string state = Program.brightnessIsLow ? currentTexts[15] : currentTexts[16]; // "MINIMUM" : "NORMAL"
            LogMessage($"{currentTexts[14]} {state}"); // "Brightness state: X"
            
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

            lblStatus.Text = currentTexts[18]; // "✓ Saved"
            LogMessage($"{currentTexts[17]} {cmbKey.Text}, {nudSeconds.Value}s"); // "Settings saved: X, Ys"
            
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
                    LogMessage(currentTexts[24]); // "Automatic startup enabled"
                }
                else
                {
                    key.DeleteValue("NoBright", false);
                    LogMessage(currentTexts[25]); // "Automatic startup disabled"
                }
            }
            catch (Exception ex)
            {
                LogMessage($"{currentTexts[26]} {ex.Message}"); // "Error configuring startup: X"
            }
        }
    }

    static class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public static bool brightnessIsLow = false;
        static int savedBrightness = 50;
        static NotifyIcon trayIcon;
        static ConfigForm configForm;
        static DateTime keyPressStart = DateTime.MinValue;
        static bool wasPressed = false;

        static int[] keyCodes = { 0xA2, 0xA3, 0xA4, 0xA5, 0xA0, 0xA1, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B };

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
            catch (Exception ex)
            {
                configForm?.LogMessage($"Error: {ex.Message}");
            }
        }

        public static void ToggleBrightness()
        {
            if (!brightnessIsLow)
            {
                savedBrightness = GetCurrentBrightness();
                SetBrightness(0);
                brightnessIsLow = true;
                string msg = configForm?.currentTexts?[20] ?? "Brightness reduced to minimum (saved:";
                configForm?.LogMessage($"✓ {msg} {savedBrightness}%)");
            }
            else
            {
                SetBrightness(savedBrightness);
                brightnessIsLow = false;
                string msg = configForm?.currentTexts?[21] ?? "Brightness restored to";
                configForm?.LogMessage($"✓ {msg} {savedBrightness}%");
            }
        }

        [STAThread]
        static void Main()
        {
            // Verificar instancia única
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
            // Intentar cargar icono personalizado, si no existe usar el por defecto
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
                    "Application started.\nRight click icon to configure.", 
                    ToolTipIcon.Info);
            }

            configForm.Show();

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
