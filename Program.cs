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
using System.Diagnostics;
using System.Security.Principal;
using System.Globalization;

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

    // Diálogo About
    public class AboutForm : Form
    {
        public AboutForm(string[] texts, bool darkMode, Icon appIcon)
        {
            this.Text = texts[0]; // "About NoBright"
            this.Size = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = appIcon;

            // Aplicar tema
            if (darkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                this.ForeColor = Color.White;
            }

            int yPos = 20;

            // Icono de la app
            PictureBox icon = new PictureBox();
            icon.Location = new Point(193, yPos);
            icon.Size = new Size(64, 64);
            if (appIcon != null)
            {
                icon.Image = appIcon.ToBitmap();
            }
            else
            {
                icon.Image = SystemIcons.Application.ToBitmap();
            }
            icon.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(icon);

            yPos += 80;

            // Nombre
            Label lblName = new Label();
            lblName.Text = "NoBright";
            lblName.Font = new Font(lblName.Font.FontFamily, 16, FontStyle.Bold);
            lblName.Location = new Point(0, yPos);
            lblName.Size = new Size(450, 30);
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblName);

            yPos += 35;

            // Versión
            Label lblVersion = new Label();
            lblVersion.Text = "Version 1.2.0";
            lblVersion.Location = new Point(0, yPos);
            lblVersion.Size = new Size(450, 20);
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblVersion);

            yPos += 30;

            // Descripción
            Label lblDesc = new Label();
            lblDesc.Text = texts[1]; // Description
            lblDesc.Location = new Point(25, yPos);
            lblDesc.Size = new Size(400, 80);
            lblDesc.TextAlign = ContentAlignment.TopCenter;
            this.Controls.Add(lblDesc);

            yPos += 90;

            // Licencia
            Label lblLicense = new Label();
            lblLicense.Text = texts[2]; // "License: GPL-3.0"
            lblLicense.Location = new Point(0, yPos);
            lblLicense.Size = new Size(450, 20);
            lblLicense.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblLicense);

            yPos += 30;

            // GitHub link
            LinkLabel linkGitHub = new LinkLabel();
            linkGitHub.Text = "https://github.com/peyoker/NoBright";
            linkGitHub.Location = new Point(0, yPos);
            linkGitHub.Size = new Size(450, 20);
            linkGitHub.TextAlign = ContentAlignment.MiddleCenter;
            linkGitHub.LinkColor = darkMode ? Color.LightBlue : Color.Blue;
            linkGitHub.LinkClicked += (s, e) => {
                Process.Start(new ProcessStartInfo("https://github.com/peyoker/NoBright") { UseShellExecute = true });
            };
            this.Controls.Add(linkGitHub);

            yPos += 35;

            // Botón cerrar
            Button btnClose = new Button();
            btnClose.Text = texts[3]; // "Close"
            btnClose.Location = new Point(175, yPos);
            btnClose.Size = new Size(100, 30);
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
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
        private NumericUpDown nudSeconds;
        private CheckBox chkStartup;
        private Button btnSave;
        private Button btnTest;
        private Label lblKey;
        private Label lblSeconds;
        private Label lblStatus;
        private TrackBar trackBrightness;
        private Label lblBrightness;
        private Panel pnlLog;
        private TextBox txtLog;
        private MenuStrip menuStrip;
        private Icon appIcon;

        private const string VERSION = "1.2.0";
        private bool logExpanded = false;

        private string[] texts_en = new string[] {
            "NoBright - Settings",
            "Activation key:",
            "Hold duration (0 = instant):",
            "Manual brightness control:",
            "Start with Windows",
            "Test Toggle",
            "Save",
            "✓ Saved",
            "Language",
            "English",
            "Spanish",
            "Theme",
            "Light",
            "Dark",
            "Event Log",
            "Show Event Log",
            "Help",
            "GitHub Repository",
            "About",
            "Application started successfully",
            "Current brightness:",
            "Testing brightness toggle...",
            "Brightness state:",
            "DARK",
            "NORMAL",
            "Settings saved:",
            "Brightness manually adjusted:",
            "Automatic startup enabled",
            "Automatic startup disabled",
            "WARNING: Cannot control brightness on this device",
            "Lock screen detected - switching to overlay mode",
            "Session unlocked - switching to brightness mode",
            "Smooth transition completed",
            "Show Log",
            "Hide Log",
            // About dialog
            "About NoBright",
            "A lightweight Windows utility for instant screen brightness control—perfect for late-night sessions or sudden wake-ups. It runs silently in your system tray, staying out of your way until you need it.",
            "License: GPL-3.0",
            "Close"
        };

        private string[] texts_es = new string[] {
            "NoBright - Configuración",
            "Tecla de activación:",
            "Duración de pulsación (0 = instantáneo):",
            "Control manual de brillo:",
            "Iniciar con Windows",
            "Probar Toggle",
            "Guardar",
            "✓ Guardado",
            "Idioma",
            "Inglés",
            "Español",
            "Tema",
            "Claro",
            "Oscuro",
            "Registro de Eventos",
            "Mostrar Registro de Eventos",
            "Ayuda",
            "Repositorio GitHub",
            "Acerca de",
            "Aplicación iniciada correctamente",
            "Brillo actual:",
            "Probando toggle de brillo...",
            "Estado del brillo:",
            "OSCURO",
            "NORMAL",
            "Configuración guardada:",
            "Brillo ajustado manualmente:",
            "Inicio automático activado",
            "Inicio automático desactivado",
            "ADVERTENCIA: No se puede controlar el brillo en este equipo",
            "Pantalla bloqueada detectada - cambiando a modo overlay",
            "Sesión desbloqueada - cambiando a modo brillo",
            "Transición suave completada",
            "Mostrar Registro",
            "Ocultar Registro",
            // About dialog
            "Acerca de NoBright",
            "Una utilidad ligera de Windows para control instantáneo del brillo—perfecta para sesiones nocturnas o despertares repentinos. Se ejecuta silenciosamente en la bandeja del sistema, sin molestar hasta que la necesites.",
            "Licencia: GPL-3.0",
            "Cerrar"
        };

        public string[] currentTexts;

        public ConfigForm()
        {
            // Cargar icono
            try
            {
                appIcon = new Icon("icon.ico");
            }
            catch
            {
                try
                {
                    appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                catch
                {
                    appIcon = SystemIcons.Application;
                }
            }

            // Determinar idioma por defecto (sistema o inglés)
            if (Properties.Settings.Default.Language == -1)
            {
                string systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                Properties.Settings.Default.Language = systemLang == "es" ? 1 : 0;
                Properties.Settings.Default.Save();
            }

            int lang = Properties.Settings.Default.Language;
            currentTexts = lang == 0 ? texts_en : texts_es;
            
            InitializeComponent();
            CreateMenuStrip();
            LoadSettings();
            ApplyTheme(Properties.Settings.Default.DarkMode);
            
            if (Properties.Settings.Default.EventLogEnabled)
            {
                LogMessage(currentTexts[19]);
                
                if (Program.CanControlBrightness())
                {
                    int current = Program.GetCurrentBrightness();
                    LogMessage($"{currentTexts[20]} {current}%");
                    trackBrightness.Value = current;
                }
                else
                {
                    LogMessage(currentTexts[29]);
                }
            }
            else if (Program.CanControlBrightness())
            {
                int current = Program.GetCurrentBrightness();
                trackBrightness.Value = current;
            }
        }

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();

            // Menú Idioma
            ToolStripMenuItem langMenu = new ToolStripMenuItem(currentTexts[8]);
            ToolStripMenuItem langEN = new ToolStripMenuItem(currentTexts[9]);
            ToolStripMenuItem langES = new ToolStripMenuItem(currentTexts[10]);
            
            // Marcar idioma actual
            if (Properties.Settings.Default.Language == 0)
                langEN.Checked = true;
            else
                langES.Checked = true;
            
            langEN.Click += (s, e) => ChangeLanguage(0);
            langES.Click += (s, e) => ChangeLanguage(1);
            
            langMenu.DropDownItems.Add(langEN);
            langMenu.DropDownItems.Add(langES);

            // Menú Tema
            ToolStripMenuItem themeMenu = new ToolStripMenuItem(currentTexts[11]);
            ToolStripMenuItem themeLight = new ToolStripMenuItem(currentTexts[12]);
            ToolStripMenuItem themeDark = new ToolStripMenuItem(currentTexts[13]);
            
            // Marcar tema actual
            if (Properties.Settings.Default.DarkMode)
                themeDark.Checked = true;
            else
                themeLight.Checked = true;
            
            themeLight.Click += (s, e) => {
                Properties.Settings.Default.DarkMode = false;
                Properties.Settings.Default.Save();
                ApplyTheme(false);
                themeLight.Checked = true;
                themeDark.Checked = false;
            };
            themeDark.Click += (s, e) => {
                Properties.Settings.Default.DarkMode = true;
                Properties.Settings.Default.Save();
                ApplyTheme(true);
                themeDark.Checked = true;
                themeLight.Checked = false;
            };
            
            themeMenu.DropDownItems.Add(themeLight);
            themeMenu.DropDownItems.Add(themeDark);

            // Menú Registro - ahora funciona como toggle
            ToolStripMenuItem logMenu = new ToolStripMenuItem(currentTexts[14]);
            ToolStripMenuItem logToggle = new ToolStripMenuItem(currentTexts[15]);
            logToggle.CheckOnClick = true;
            logToggle.Checked = logExpanded;
            logToggle.Click += (s, e) => {
                BtnToggleLog_Click(s, e);
                logToggle.Checked = logExpanded;
                
                // Habilitar el log cuando se muestra
                if (logExpanded && !Properties.Settings.Default.EventLogEnabled)
                {
                    Properties.Settings.Default.EventLogEnabled = true;
                    Properties.Settings.Default.Save();
                }
            };
            logMenu.DropDownItems.Add(logToggle);

            // Menú Ayuda
            ToolStripMenuItem helpMenu = new ToolStripMenuItem(currentTexts[16]);
            ToolStripMenuItem helpGitHub = new ToolStripMenuItem(currentTexts[17]);
            ToolStripMenuItem helpAbout = new ToolStripMenuItem(currentTexts[18]);
            
            helpGitHub.Click += (s, e) => {
                Process.Start(new ProcessStartInfo("https://github.com/peyoker/NoBright") { UseShellExecute = true });
            };
            helpAbout.Click += (s, e) => {
                AboutForm about = new AboutForm(
                    new string[] { currentTexts[35], currentTexts[36], currentTexts[37], currentTexts[38] },
                    Properties.Settings.Default.DarkMode,
                    appIcon
                );
                about.ShowDialog();
            };
            
            helpMenu.DropDownItems.Add(helpGitHub);
            helpMenu.DropDownItems.Add(helpAbout);

            menuStrip.Items.Add(langMenu);
            menuStrip.Items.Add(themeMenu);
            menuStrip.Items.Add(logMenu);
            menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void ChangeLanguage(int langIndex)
        {
            if (Properties.Settings.Default.Language != langIndex)
            {
                Properties.Settings.Default.Language = langIndex;
                Properties.Settings.Default.Save();
                
                MessageBox.Show(
                    langIndex == 0 
                        ? "Language changed to English. Please restart the application." 
                        : "Idioma cambiado a Español. Por favor reinicia la aplicación.",
                    "NoBright",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void InitializeComponent()
        {
            this.Text = currentTexts[0];
            this.Size = new Size(450, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = appIcon;

            int yPos = 50;

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
                "Escape",
                "Arrow Up", "Arrow Down", "Arrow Left", "Arrow Right",
                "Left Windows", "Right Windows",
                "Pause/Break",
                "Scroll Lock",
                "Print Screen",
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

            Label lblManual = new Label();
            lblManual.Text = currentTexts[3];
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
            chkStartup.Text = currentTexts[4];
            chkStartup.Location = new Point(20, yPos);
            chkStartup.Size = new Size(200, 20);
            this.Controls.Add(chkStartup);

            yPos += 35;

            btnTest = new Button();
            btnTest.Text = currentTexts[5];
            btnTest.Location = new Point(20, yPos);
            btnTest.Size = new Size(125, 30);
            btnTest.Click += BtnTest_Click;
            this.Controls.Add(btnTest);

            btnSave = new Button();
            btnSave.Text = currentTexts[6];
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

            yPos += 35;

            // Versión pequeña en la esquina
            Label lblVersion = new Label();
            lblVersion.Text = $"v{VERSION}";
            lblVersion.Location = new Point(370, yPos);
            lblVersion.Size = new Size(50, 15);
            lblVersion.Font = new Font(lblVersion.Font.FontFamily, 7);
            lblVersion.ForeColor = Color.Gray;
            lblVersion.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lblVersion);

            // Panel de log (inicialmente oculto)
            pnlLog = new Panel();
            pnlLog.Location = new Point(20, yPos + 5);
            pnlLog.Size = new Size(400, 150);
            pnlLog.Visible = false;

            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Dock = DockStyle.Fill;
            txtLog.ReadOnly = true;
            pnlLog.Controls.Add(txtLog);

            this.Controls.Add(pnlLog);

            this.FormClosing += ConfigForm_FormClosing;
        }

        private void BtnToggleLog_Click(object sender, EventArgs e)
        {
            logExpanded = !logExpanded;
            pnlLog.Visible = logExpanded;
            
            if (logExpanded)
            {
                this.Height = 550;
            }
            else
            {
                this.Height = 380;
            }
        }

        private void ApplyTheme(bool darkMode)
        {
            Color backColor, foreColor, logBackColor, logForeColor, menuBackColor, menuForeColor;

            if (darkMode)
            {
                backColor = Color.FromArgb(30, 30, 30);
                foreColor = Color.White;
                logBackColor = Color.FromArgb(45, 45, 45);
                logForeColor = Color.White;
                menuBackColor = Color.FromArgb(45, 45, 45);
                menuForeColor = Color.White;
            }
            else
            {
                backColor = SystemColors.Control;
                foreColor = SystemColors.ControlText;
                logBackColor = Color.White;
                logForeColor = Color.Black;
                menuBackColor = SystemColors.Control;
                menuForeColor = SystemColors.ControlText;
            }

            this.BackColor = backColor;
            this.ForeColor = foreColor;
            txtLog.BackColor = logBackColor;
            txtLog.ForeColor = logForeColor;

            // Actualizar menú con colores oscuros
            if (menuStrip != null)
            {
                menuStrip.BackColor = menuBackColor;
                menuStrip.ForeColor = menuForeColor;
                
                // Aplicar a todos los items del menú
                foreach (ToolStripMenuItem item in menuStrip.Items)
                {
                    item.BackColor = menuBackColor;
                    item.ForeColor = menuForeColor;
                    
                    // Aplicar a subitems
                    foreach (ToolStripItem subItem in item.DropDownItems)
                    {
                        subItem.BackColor = menuBackColor;
                        subItem.ForeColor = menuForeColor;
                    }
                    
                    // Aplicar al dropdown
                    item.DropDown.BackColor = menuBackColor;
                    item.DropDown.ForeColor = menuForeColor;
                }
            }
        }

        private void TrackBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblBrightness.Text = $"{trackBrightness.Value}%";
            Program.SetBrightness(trackBrightness.Value);
            LogMessage($"{currentTexts[26]} {trackBrightness.Value}%");
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
            LogMessage(currentTexts[21]);
            Program.ToggleBrightness();
            string state = Program.brightnessIsLow ? currentTexts[23] : currentTexts[24];
            LogMessage($"{currentTexts[22]} {state}");
            
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

            lblStatus.Text = currentTexts[7];
            LogMessage($"{currentTexts[25]} {cmbKey.Text}, {nudSeconds.Value}s");
            
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
                    LogMessage(currentTexts[27]);
                }
                else
                {
                    key.DeleteValue("NoBright", false);
                    LogMessage(currentTexts[28]);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }
    }

    static class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);

        // Usar múltiples métodos de detección para mayor fiabilidad
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, out RAWINPUT pData, ref uint pcbSize, uint cbSizeHeader);

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWINPUTDATA data;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RAWINPUTDATA
        {
            [FieldOffset(0)]
            public RAWMOUSE mouse;
            [FieldOffset(0)]
            public RAWKEYBOARD keyboard;
            [FieldOffset(0)]
            public RAWHID hid;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWKEYBOARD
        {
            public ushort MakeCode;
            public ushort Flags;
            public ushort Reserved;
            public ushort VKey;
            public uint Message;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWMOUSE
        {
            public ushort usFlags;
            public uint ulButtons;
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RAWHID
        {
            public uint dwSizeHid;
            public uint dwCount;
        }

        public static bool brightnessIsLow = false;
        static int savedBrightness = 50;
        static NotifyIcon trayIcon;
        static ConfigForm configForm;
        static DateTime keyPressStart = DateTime.MinValue;
        static bool wasPressed = false;
        static DarkOverlay overlay = null;
        static bool checkingKey = true;

        static int[] keyCodes = { 0xA2, 0xA3, 0xA4, 0xA5, 0xA0, 0xA1, 0x1B, 0x26, 0x28, 0x25, 0x27, 0x5B, 0x5C, 0x13, 0x91, 0x2C, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7A, 0x7B };

        private static bool IsKeyPressed(int vKey)
        {
            // Usar GetAsyncKeyState que funciona a nivel más bajo
            short keyState = GetAsyncKeyState(vKey);
            bool isPressed = (keyState & 0x8000) != 0;
            
            // Doble verificación con GetKeyState para mayor fiabilidad
            if (!isPressed)
            {
                short alternateState = GetKeyState(vKey);
                isPressed = (alternateState & 0x8000) != 0;
            }
            
            return isPressed;
        }

        private static void MonitorKeyPress()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            
            // Aumentar la frecuencia de polling a 10ms para mejor detección
            const int POLL_INTERVAL = 10;
            
            while (checkingKey)
            {
                try
                {
                    int selectedKey = keyCodes[Math.Max(0, Math.Min(Properties.Settings.Default.KeyIndex, keyCodes.Length - 1))];
                    double holdSeconds = Properties.Settings.Default.HoldSeconds;
                    
                    bool keyPressed = IsKeyPressed(selectedKey);

                    if (keyPressed && !wasPressed)
                    {
                        wasPressed = true;
                        keyPressStart = DateTime.Now;
                        
                        configForm?.LogMessage($"Key detected: {selectedKey:X}");
                        
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

                    Thread.Sleep(POLL_INTERVAL);
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }

        static bool IsRunAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RestartAsAdmin()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Verb = "runas";

            try
            {
                Process.Start(startInfo);
                Application.Exit();
            }
            catch
            {
                MessageBox.Show(
                    "NoBright requires administrator privileges to control brightness.\nPlease run as administrator.",
                    "Administrator Rights Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                Application.Exit();
            }
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

        public static async void ToggleBrightness()
        {
            if (!brightnessIsLow)
            {
                // Activar modo oscuro
                savedBrightness = GetCurrentBrightness();
                
                // Intentar controlar brillo por WMI primero
                SetBrightness(0);
                await Task.Delay(200); // Esperar para verificar
                
                int checkBrightness = GetCurrentBrightness();
                bool wmiWorked = (checkBrightness <= 10); // Si está cerca de 0, funcionó
                
                // Si WMI no funcionó, activar overlay (pantalla bloqueada o monitor no compatible)
                if (!wmiWorked)
                {
                    configForm?.LogMessage("Hardware control unavailable, using overlay mode");
                    if (overlay == null)
                    {
                        overlay = new DarkOverlay();
                    }
                    overlay.SetOpacitySafe(0.95);
                    overlay.ShowSafe();
                }
                else
                {
                    configForm?.LogMessage("✓ Brightness reduced to minimum");
                }
                
                brightnessIsLow = true;
            }
            else
            {
                // Restaurar modo normal
                
                // Si el overlay está visible, ocultarlo
                if (overlay != null && overlay.Visible)
                {
                    overlay.HideSafe();
                    configForm?.LogMessage("✓ Overlay removed");
                }
                
                // Restaurar brillo WMI
                SetBrightness(savedBrightness);
                configForm?.LogMessage($"✓ Brightness restored to {savedBrightness}%");
                
                brightnessIsLow = false;
            }
        }

        [STAThread]
        static void Main()
        {
            // Verificar permisos de administrador
            if (!IsRunAsAdmin())
            {
                RestartAsAdmin();
                return;
            }

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

            trayIcon.ShowBalloonTip(2000, "NoBright", 
                "Application started.\nWorks everywhere, even on lock screen!", 
                ToolTipIcon.Info);

            configForm.Show();

            // Iniciar monitoreo de teclas con alta prioridad
            Thread monitorThread = new Thread(MonitorKeyPress);
            monitorThread.IsBackground = true;
            monitorThread.Priority = ThreadPriority.Highest;
            monitorThread.Start();

            Application.Run();
            
            // Detener monitoreo al salir
            checkingKey = false;
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
        [global::System.Configuration.DefaultSettingValueAttribute("-1")]
        public int Language
        {
            get { return ((int)(this["Language"])); }
            set { this["Language"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("true")]
        public bool DarkMode
        {
            get { return ((bool)(this["DarkMode"])); }
            set { this["DarkMode"] = value; }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("false")]
        public bool EventLogEnabled
        {
            get { return ((bool)(this["EventLogEnabled"])); }
            set { this["EventLogEnabled"] = value; }
        }
    }
}
