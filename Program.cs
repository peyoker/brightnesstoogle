using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;
using System.Management;

namespace NoBright
{
    // Formulario principal de configuración
    public class ConfigForm : Form
    {
        private ComboBox cmbKey;
        private NumericUpDown nudSeconds;
        private CheckBox chkStartup;
        private Button btnSave;
        private Button btnTest;
        private Label lblKey;
        private Label lblSeconds;
        private Label lblStatus;
        private TextBox txtLog;
        private TrackBar trackBrightness;
        private Label lblBrightness;

        public ConfigForm()
        {
            InitializeComponent();
            LoadSettings();
            LogMessage("Aplicación iniciada correctamente");
            
            // Detectar método de control
            if (Program.CanControlBrightness())
            {
                int current = Program.GetCurrentBrightness();
                LogMessage($"Brillo actual: {current}%");
                trackBrightness.Value = current;
            }
            else
            {
                LogMessage("ADVERTENCIA: No se puede controlar el brillo en este equipo");
                btnTest.Enabled = false;
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Control de Brillo - Configuración";
            this.Size = new Size(450, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Label tecla
            lblKey = new Label();
            lblKey.Text = "Tecla de activación:";
            lblKey.Location = new Point(20, 20);
            lblKey.Size = new Size(150, 20);
            this.Controls.Add(lblKey);

            // ComboBox teclas
            cmbKey = new ComboBox();
            cmbKey.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKey.Location = new Point(20, 45);
            cmbKey.Size = new Size(400, 25);
            cmbKey.Items.AddRange(new object[] {
                "Control Izquierdo",
                "Control Derecho",
                "Alt Izquierdo",
                "Alt Derecho",
                "Shift Izquierdo",
                "Shift Derecho",
                "F1", "F2", "F3", "F4", "F5", "F6",
                "F7", "F8", "F9", "F10", "F11", "F12"
            });
            this.Controls.Add(cmbKey);

            // Label segundos
            lblSeconds = new Label();
            lblSeconds.Text = "Segundos mantener pulsada (0 = instantáneo):";
            lblSeconds.Location = new Point(20, 80);
            lblSeconds.Size = new Size(300, 20);
            this.Controls.Add(lblSeconds);

            // NumericUpDown segundos
            nudSeconds = new NumericUpDown();
            nudSeconds.Location = new Point(20, 105);
            nudSeconds.Size = new Size(100, 25);
            nudSeconds.Minimum = 0;
            nudSeconds.Maximum = 10;
            nudSeconds.DecimalPlaces = 1;
            nudSeconds.Increment = 0.5m;
            this.Controls.Add(nudSeconds);

            // Control manual de brillo
            Label lblManual = new Label();
            lblManual.Text = "Control manual de brillo:";
            lblManual.Location = new Point(20, 140);
            lblManual.Size = new Size(200, 20);
            this.Controls.Add(lblManual);

            lblBrightness = new Label();
            lblBrightness.Text = "50%";
            lblBrightness.Location = new Point(370, 140);
            lblBrightness.Size = new Size(50, 20);
            lblBrightness.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblBrightness);

            trackBrightness = new TrackBar();
            trackBrightness.Location = new Point(20, 165);
            trackBrightness.Size = new Size(400, 45);
            trackBrightness.Minimum = 0;
            trackBrightness.Maximum = 100;
            trackBrightness.TickFrequency = 10;
            trackBrightness.Value = 50;
            trackBrightness.ValueChanged += TrackBrightness_ValueChanged;
            this.Controls.Add(trackBrightness);

            // CheckBox inicio con Windows
            chkStartup = new CheckBox();
            chkStartup.Text = "Iniciar con Windows";
            chkStartup.Location = new Point(20, 220);
            chkStartup.Size = new Size(200, 20);
            this.Controls.Add(chkStartup);

            // Botón probar
            btnTest = new Button();
            btnTest.Text = "Probar Toggle";
            btnTest.Location = new Point(20, 250);
            btnTest.Size = new Size(125, 30);
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            // Botón guardar
            btnSave = new Button();
            btnSave.Text = "Guardar";
            btnSave.Location = new Point(160, 250);
            btnSave.Size = new Size(125, 30);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Label estado
            lblStatus = new Label();
            lblStatus.Text = "";
            lblStatus.Location = new Point(295, 255);
            lblStatus.Size = new Size(125, 20);
            lblStatus.ForeColor = Color.Green;
            this.Controls.Add(lblStatus);

            // TextBox log
            Label lblLog = new Label();
            lblLog.Text = "Registro de eventos:";
            lblLog.Location = new Point(20, 290);
            lblLog.Size = new Size(150, 20);
            this.Controls.Add(lblLog);

            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Location = new Point(20, 315);
            txtLog.Size = new Size(400, 110);
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.White;
            this.Controls.Add(txtLog);

            this.FormClosing += ConfigForm_FormClosing;
        }

        private void TrackBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = $"{trackBrightness.Value}%";
            Program.SetBrightness(trackBrightness.Value);
            LogMessage($"Brillo ajustado manualmente: {trackBrightness.Value}%");
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
                LogMessage("Ventana minimizada a la bandeja");
            }
        }

        private void LoadSettings()
        {
            cmbKey.SelectedIndex = Math.Max(0, Math.Min(Properties.Settings.Default.KeyIndex, cmbKey.Items.Count - 1));
            nudSeconds.Value = (decimal)Properties.Settings.Default.HoldSeconds;
            chkStartup.Checked = IsInStartup();
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            LogMessage("Probando toggle de brillo...");
            Program.ToggleBrightness();
            LogMessage($"Estado del brillo: {(Program.brightnessIsLow ? "MÍNIMO (0%)" : "NORMAL")}");
            
            // Actualizar trackbar
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
            Properties.Settings.Default.Save();

            SetStartup(chkStartup.Checked);

            lblStatus.Text = "✓ Guardado";
            LogMessage($"Configuración guardada: {cmbKey.Text}, {nudSeconds.Value}s");
            
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
                return key?.GetValue("BrightnessToggle") != null;
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
                    key.SetValue("BrightnessToggle", "\"" + Application.ExecutablePath + "\"");
                    LogMessage("Inicio automático activado");
                }
                else
                {
                    key.DeleteValue("BrightnessToggle", false);
                    LogMessage("Inicio automático desactivado");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error configurando inicio: {ex.Message}");
            }
        }
    }

    // Clase principal del programa
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
            catch
            {
                return false;
            }
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
                configForm?.LogMessage($"Error ajustando brillo: {ex.Message}");
            }
        }

        public static void ToggleBrightness()
        {
            if (!brightnessIsLow)
            {
                savedBrightness = GetCurrentBrightness();
                SetBrightness(0);
                brightnessIsLow = true;
                configForm?.LogMessage($"✓ Brillo reducido al mínimo (guardado: {savedBrightness}%)");
            }
            else
            {
                SetBrightness(savedBrightness);
                brightnessIsLow = false;
                configForm?.LogMessage($"✓ Brillo restaurado a {savedBrightness}%");
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Crear formulario de configuración
            configForm = new ConfigForm();

            // Crear icono de bandeja
            trayIcon = new NotifyIcon();
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Text = "Control de Brillo - Click derecho para opciones";
            trayIcon.Visible = true;

            // Menú contextual
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("📋 Abrir Configuración", null, (s, e) => {
                configForm.Show();
                configForm.WindowState = FormWindowState.Normal;
                configForm.BringToFront();
            });
            menu.Items.Add("-");
            menu.Items.Add("❌ Salir", null, (s, e) => {
                trayIcon.Visible = false;
                Application.Exit();
            });
            trayIcon.ContextMenuStrip = menu;

            trayIcon.DoubleClick += (s, e) => {
                configForm.Show();
                configForm.WindowState = FormWindowState.Normal;
                configForm.BringToFront();
            };

            // Mostrar notificación de inicio
            if (CanControlBrightness())
            {
                trayIcon.ShowBalloonTip(2000, "Control de Brillo", 
                    "Aplicación iniciada correctamente.\nClick derecho en el icono para configurar.", 
                    ToolTipIcon.Info);
            }
            else
            {
                trayIcon.ShowBalloonTip(3000, "Control de Brillo - Advertencia", 
                    "No se puede controlar el brillo en este equipo.\nPuede que necesites habilitar el control de brillo en la BIOS/UEFI.", 
                    ToolTipIcon.Warning);
            }

            // Mostrar ventana al inicio
            configForm.Show();

            // Iniciar monitoreo en segundo plano
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
                catch (Exception ex)
                {
                    configForm?.LogMessage($"Error: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}

namespace BrightnessToggle.Properties
{
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get { return defaultInstance; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int KeyIndex
        {
            get { return ((int)(this["KeyIndex"])); }
            set { this["KeyIndex"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double HoldSeconds
        {
            get { return ((double)(this["HoldSeconds"])); }
            set { this["HoldSeconds"] = value; }
        }
    }

}





