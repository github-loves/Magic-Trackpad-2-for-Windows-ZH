using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

namespace AmtPtpControlPanel
{
    public partial class Main : Form
    {
        private bool _loading = true;
        private bool _english;
        private int _lastGoodStopPressure = 0;
        private int _lastGoodStopSize = -1;

        private System.Windows.Forms.Timer _saveDebounce;
        private System.Windows.Forms.Timer _batteryDelay;
        private System.Windows.Forms.Timer _batteryPoll;

        private System.Drawing.Icon _trayIcon;
        private bool _trayExit;
        private bool _balloonShown;
        private bool _startMinimized;

        public Main()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            _startMinimized = args.Any(a => string.Equals(a, "/tray", StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(a, "-tray", StringComparison.OrdinalIgnoreCase));

            if (_startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }

            _saveDebounce = new System.Windows.Forms.Timer();
            _saveDebounce.Interval = 500;
            _saveDebounce.Tick += SaveDebounce_Tick;

            ToolTip langTip = new ToolTip();
            langTip.SetToolTip(btnLang, "\u5207\u6362\u8bed\u8a00 / Switch language");
        }

        //=================
        // Init / load
        //=================

        private void Main_Load(object sender, EventArgs e)
        {
            LoadSettingsIntoControls();
            RepositionStopRows();
            UpdateModeUi();
            UpdateStopEditors();
            _loading = false;
            ApplyLanguage();

            _batteryDelay = new System.Windows.Forms.Timer();
            _batteryDelay.Interval = 500;
            _batteryDelay.Tick += delegate(object s, EventArgs ev)
            {
                ((System.Windows.Forms.Timer)s).Stop();
                RefreshBattery(false);
                _batteryPoll.Start();
            };

            _batteryPoll = new System.Windows.Forms.Timer();
            _batteryPoll.Interval = 60000;
            _batteryPoll.Tick += delegate(object s, EventArgs ev)
            {
                RefreshBattery(false);
            };

            _batteryDelay.Start();

            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = null;
            UpdateTrayIcon(-1, false);

            if (_startMinimized)
            {
                this.Hide();
            }
        }

        private bool IsFirstRun()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key =
                    Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\MagicTrackpad2Tray"))
                {
                    return key == null || key.GetValue("FirstRunDone") == null;
                }
            }
            catch { return true; }
        }

        private void MarkFirstRunDone()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key =
                    Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\MagicTrackpad2Tray"))
                {
                    key.SetValue("FirstRunDone", 1);
                }
            }
            catch { }
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            _saveDebounce.Stop();
            _saveDebounce.Dispose();
            if (_batteryDelay != null) _batteryDelay.Dispose();
            if (_batteryPoll != null) _batteryPoll.Dispose();
            if (notifyIcon != null) notifyIcon.Visible = false;
            if (_trayIcon != null) _trayIcon.Dispose();
        }

        private void LoadSettingsIntoControls()
        {
            Int32 buttonDisabled = 0;
            Int32 feedbackClick = 0x060617;
            Int32 feedbackRelease = 0x000014;
            Int32 stopPressure = 0;
            Int32 stopSize = -1;
            Int32 ignoreButtonFinger = 1;
            Int32 ignoreNearFingers = 1;
            Int32 palmRejection = 1;

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\WUDF\Services\AmtPtpDeviceUsbUm\Parameters"))
                {
                    delStringRefInt32Void get = (string name, ref Int32 output) =>
                    {
                        try
                        {
                            output = (Int32)key.GetValue(name);
                        }
                        catch
                        {
                        }
                    };

                    get("ButtonDisabled", ref buttonDisabled);
                    get("FeedbackClick", ref feedbackClick);
                    get("FeedbackRelease", ref feedbackRelease);
                    get("StopPressure", ref stopPressure);
                    get("StopSize", ref stopSize);
                    get("IgnoreButtonFinger", ref ignoreButtonFinger);
                    get("IgnoreNearFingers", ref ignoreNearFingers);
                    get("PalmRejection", ref palmRejection);
                }
            }
            catch
            {
            }

            if (buttonDisabled != 0)
            {
                tglDisable.Checked = true;
                sldFeedback.Value = 0;
            }
            else if (feedbackClick == 0xffffff && feedbackRelease == 0xffffff)
            {
                tglMaximum.Checked = true;
                sldFeedback.Value = 100;
            }
            else
                sldFeedback.Value = FeedbackClickToValue(feedbackClick);

            if (stopPressure == -1 && stopSize == -1)
                radStopNone.Checked = true;
            else if (stopPressure != -1)
            {
                radStopPressure.Checked = true;
                _lastGoodStopPressure = stopPressure;
                txtStopPressure.Text = stopPressure.ToString();
            }
            else
            {
                radStopSize.Checked = true;
                _lastGoodStopSize = stopSize;
                txtStopSize.Text = stopSize.ToString();
            }

            tglButtonFinger.Checked = ignoreButtonFinger != 0;
            tglNearFingers.Checked = ignoreNearFingers != 0;
            tglPalmRejection.Checked = palmRejection != 0;
            tglAutoStart.Checked = AutoStartTaskExists();
        }

        //=================
        // Layout helpers
        //=================

        private void RepositionStopRows()
        {
            txtStopPressure.Location = new Point(radStopPressure.Right + 6, radStopPressure.Top - 2);
            lblPressureUnit.Location = new Point(txtStopPressure.Right + 6, radStopPressure.Top + 2);

            txtStopSize.Location = new Point(radStopSize.Right + 6, radStopSize.Top - 2);
            lblSizeUnit.Location = new Point(txtStopSize.Right + 6, radStopSize.Top + 2);
        }

        //=================
        // Language
        //=================

        private static readonly string[] StringsZh = new string[]
        {
            "\u89e6\u6478\u677f\u53cd\u9988\u5f3a\u5ea6",                                                                                                    // 0 card feedback
            "\u5b8c\u5168\u7981\u7528\u89e6\u611f\u53cd\u9988\u4e0e\u529b\u611f\u6309\u538b\u6309\u94ae",                                                  // 1 disable
            "\u6700\u5927\u89e6\u611f\u53cd\u9988\uff08\u54d2\u54d2\u611f\u5f3a\u70c8\uff0c\u58f0\u97f3\u5f88\u5927\uff01\uff09",                          // 2 maximum
            "\u5f53\u624b\u6307\u79bb\u5f00\u89e6\u6478\u677f\u65f6\uff1a",                                                                                  // 3 card stop
            "\u4e0d\u8fdb\u884c\u4efb\u4f55\u64cd\u4f5c",                                                                                                    // 4 none
            "\u5f53\u6309\u538b\u529b\u5ea6\u5c0f\u4e8e\u7b49\u4e8e\u4ee5\u4e0b",                                                                            // 5 pressure
            "\u4e2a\u5355\u4f4d\u65f6\u3002\uff080 \u8868\u793a\u65e0\u538b\u529b\uff0c\u63a8\u8350 0\uff09",                                                // 6 unit pressure
            "\u5f53\u89e6\u6478\u533a\u57df\u5c3a\u5bf8\u5c0f\u4e8e\u7b49\u4e8e\u4ee5\u4e0b",                                                                // 7 size
            "\u4e2a\u5355\u4f4d\u65f6\u3002\uff08\u63a8\u8350\u503c 7\uff09",                                                                                // 8 unit size
            "\u5176\u4ed6\u9009\u9879",                                                                                                                      // 9 card other
            "\u5ffd\u7565\u60ac\u505c\u5728\u89e6\u6478\u677f\u4e0a\u65b9\u3001\u672a\u63a5\u89e6\u8868\u9762\u7684\u624b\u6307\u7684\u8f93\u5165",          // 10 near fingers
            "\u5ffd\u7565\u7528\u4e8e\u70b9\u6309\u529b\u611f\u6309\u538b\u6309\u94ae\u7684\u90a3\u6839\u624b\u6307\u7684\u8f93\u5165\uff08\u4f8b\u5982\u7528\u62c7\u6307\u70b9\u6309\u3001\u98df\u6307\u79fb\u52a8\u6307\u9488\uff0c\u5bf9\u62d6\u52a8\u64cd\u4f5c\u5f88\u6709\u7528\uff09", // 11 button finger
            "\u624b\u638c\u9632\u8bef\u89e6\uff08Palm Rejection\uff09",                                                                                      // 12 palm
            "Windows \u89e6\u6478\u677f\u8bbe\u7f6e",                                                                                                          // 13 settings button
            "\u8f7b",                                                                                                                                          // 14 min label
            "\u91cd"                                                                                                                                           // 15 max label
        };

        private static readonly string[] StringsEn = new string[]
        {
            "Haptic Feedback Strength",
            "Disable haptic feedback and force touch button entirely",
            "Maximum haptic feedback (very clicky and loud!)",
            "When you lift your finger from the trackpad:",
            "Do nothing",
            "Stop the pointer if the pressure is less than or equal to",
            "units. (0 means no pressure; recommended 0)",
            "Stop the pointer if the touch area size is less than or equal to",
            "units. (recommended value 7)",
            "Other options",
            "Ignore input from fingers hovering above the trackpad surface",
            "Ignore input from the finger pressing the force touch button (thumb presses, index moves - great for dragging)",
            "Palm Rejection",
            "Windows Touchpad Settings",
            "Light",
            "Firm"
        };

        private void btnLang_Click(object sender, EventArgs e)
        {
            _english = !_english;
            ApplyLanguage();
            RepositionStopRows();
        }

        private void ApplyLanguage()
        {
            string[] s = _english ? StringsEn : StringsZh;

            cardFeedback.Text = s[0];
            tglDisable.Text = s[1];
            tglMaximum.Text = s[2];
            cardStop.Text = s[3];
            radStopNone.Text = s[4];
            radStopPressure.Text = s[5];
            lblPressureUnit.Text = s[6];
            radStopSize.Text = s[7];
            lblSizeUnit.Text = s[8];
            cardOther.Text = s[9];
            tglNearFingers.Text = s[10];
            tglButtonFinger.Text = s[11];
            tglPalmRejection.Text = s[12];
            btnTouchpadSettings.Text = s[13];
            sldFeedback.MinText = s[14];
            sldFeedback.MaxText = s[15];

            UpdateTrayMenuText();

            LayoutBattery();
            RepositionStopRows();
        }

        // right-align the percentage label against the battery icon so they never overlap
        private void LayoutBattery()
        {
            Size ts = TextRenderer.MeasureText(lblBattery.Text, lblBattery.Font);
            int x = batteryIcon.Left - ts.Width - 6;
            int minX = btnLang.Right + 10;
            if (x < minX) x = minX;
            lblBattery.Location = new Point(x, 16);
        }

        //=================
        // UI events
        //=================

        private void ctlTouchpadSettings_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("ms-settings:devices-touchpad");
            }
            catch
            {
            }
        }

        private void sldFeedback_ValueChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            ScheduleSave();
        }

        private void ctlModeOptions_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;

            ToggleSwitch t = sender as ToggleSwitch;
            if (t != null && t.Checked)
            {
                if (t == tglDisable && tglMaximum.Checked)
                    tglMaximum.Checked = false;
                else if (t == tglMaximum && tglDisable.Checked)
                    tglDisable.Checked = false;
            }

            UpdateModeUi();
            ScheduleSave();
        }

        private void UpdateModeUi()
        {
            sldFeedback.Enabled = !(tglDisable.Checked || tglMaximum.Checked);
        }

        private void ctlStop_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            UpdateStopEditors();
            ScheduleSave();
        }

        private void UpdateStopEditors()
        {
            txtStopPressure.Enabled = radStopPressure.Checked;
            txtStopSize.Enabled = radStopSize.Checked;
        }

        private void StopValue_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            ScheduleSave();
        }

        private void OtherOption_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            ScheduleSave();
        }

        //=================
        // Saving
        //=================

        private void ScheduleSave()
        {
            _saveDebounce.Stop();
            _saveDebounce.Start();
        }

        private void SaveDebounce_Tick(object sender, EventArgs e)
        {
            _saveDebounce.Stop();
            SaveSettingsNow();
        }

        private int ParseStopValue(TextBox box, int fallback, ref bool changed)
        {
            int v;
            if (Int32.TryParse(box.Text.Trim(), out v) && v >= 0)
            {
                changed = true;
                return v;
            }
            return fallback;
        }

        private void SaveSettingsNow()
        {
            Int32 buttonDisabled;
            Int32 feedbackClick;
            Int32 feedbackRelease;

            if (tglDisable.Checked)
            {
                buttonDisabled = 1;
                feedbackClick = 0;
                feedbackRelease = 0;
            }
            else if (tglMaximum.Checked)
            {
                buttonDisabled = 0;
                feedbackClick = 0xffffff;
                feedbackRelease = 0xffffff;
            }
            else
            {
                buttonDisabled = 0;

                // continuous mapping, piecewise-linear through the three
                // factory presets: Light(0x040415/0x000010) -> Medium(0x060617/0x000014) -> Firm(0x08081e/0x020218)
                double t = sldFeedback.Value / 100.0;
                int cb, cl, rh, rm, rl;
                if (t <= 0.5)
                {
                    double u = t / 0.5;
                    cb = LerpInt(0x04, 0x06, u);
                    cl = LerpInt(0x15, 0x17, u);
                    rh = LerpInt(0x00, 0x00, u);
                    rm = LerpInt(0x00, 0x00, u);
                    rl = LerpInt(0x10, 0x14, u);
                }
                else
                {
                    double u = (t - 0.5) / 0.5;
                    cb = LerpInt(0x06, 0x08, u);
                    cl = LerpInt(0x17, 0x1e, u);
                    rh = LerpInt(0x00, 0x02, u);
                    rm = LerpInt(0x00, 0x02, u);
                    rl = LerpInt(0x14, 0x18, u);
                }

                feedbackClick = (cb << 16) | (cb << 8) | cl;
                feedbackRelease = (rh << 16) | (rm << 8) | rl;
            }

            bool changedP = false;
            bool changedS = false;
            Int32 stopPressure;
            Int32 stopSize;

            if (radStopNone.Checked)
            {
                stopPressure = -1;
                stopSize = -1;
                _lastGoodStopPressure = 0;
                _lastGoodStopSize = -1;
            }
            else if (radStopPressure.Checked)
            {
                stopPressure = ParseStopValue(txtStopPressure, _lastGoodStopPressure, ref changedP);
                if (changedP) _lastGoodStopPressure = stopPressure;
                stopSize = -1;
            }
            else
            {
                stopPressure = -1;
                stopSize = ParseStopValue(txtStopSize, _lastGoodStopSize, ref changedS);
                if (changedS) _lastGoodStopSize = stopSize;
            }

            Int32 ignoreButtonFinger = tglButtonFinger.Checked ? 1 : 0;
            Int32 ignoreNearFingers = tglNearFingers.Checked ? 1 : 0;
            Int32 palmRejection = tglPalmRejection.Checked ? 1 : 0;

            try
            {
                Action<string, string> save = (string key, string name) =>
                {
                    using (RegistryKey keyServices = Registry.LocalMachine.OpenSubKey(key, true))
                    using (RegistryKey keyAmtPtpDeviceUsbUm = keyServices.CreateSubKey(name, true))
                    using (RegistryKey keyParameters = keyAmtPtpDeviceUsbUm.CreateSubKey("Parameters", true))
                    {
                        keyParameters.SetValue("ButtonDisabled", buttonDisabled);
                        keyParameters.SetValue("FeedbackClick", feedbackClick);
                        keyParameters.SetValue("FeedbackRelease", feedbackRelease);
                        keyParameters.SetValue("StopPressure", stopPressure);
                        keyParameters.SetValue("StopSize", stopSize);
                        keyParameters.SetValue("IgnoreButtonFinger", ignoreButtonFinger);
                        keyParameters.SetValue("IgnoreNearFingers", ignoreNearFingers);
                        keyParameters.SetValue("PalmRejection", palmRejection);
                    }
                };

                save(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\WUDF\Services", "AmtPtpDeviceUsbUm");
                save(@"SYSTEM\CurrentControlSet\Services", "AmtPtpHidFilter");
            }
            catch
            {
                return;
            }

            UsbDevice.RestartDevices();
            BtDevice.SendIoctl(BtDevice.IOCTL_RELOAD_SETTINGS);
        }

        private static int LerpInt(int a, int b, double u)
        {
            return a + (int)Math.Round((b - a) * u);
        }

        // inverse map: FeedbackClick -> gauge value 0..100 (approximate for non-preset values)
        private double FeedbackClickToValue(int feedbackClick)
        {
            int cl = feedbackClick & 0xff;
            if (cl <= 0x15) return 0.0;
            if (cl >= 0x1e) return 100.0;
            if (cl <= 0x17)
                return ((double)(cl - 0x15)) / (0x17 - 0x15) * 50.0;
            return 50.0 + ((double)(cl - 0x17)) / (0x1e - 0x17) * 50.0;
        }

        //=================
        // Battery
        //=================

        private bool IsTrackpadOnCable()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    @"SELECT PNPDeviceID FROM Win32_PnPEntity WHERE PNPDeviceID LIKE 'USB\\VID_05AC&PID_0265%' OR PNPDeviceID LIKE 'USB\\VID_05AC&PID_0324%'"))
                {
                    return searcher.Get().Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void RefreshBattery(bool showErrors)
        {
            batteryIcon.Charging = IsTrackpadOnCable();

            uint level;
            if (BtDevice.SendIoctl(BtDevice.IOCTL_GET_BATTERY, out level, showErrors) && level <= 100)
            {
                lblBattery.Text = level.ToString() + "%";
                batteryIcon.Percent = (int)level;
                UpdateTrayIcon((int)level, batteryIcon.Charging);
            }
            else
            {
                lblBattery.Text = "--%";
                batteryIcon.Percent = -1;
                UpdateTrayIcon(-1, batteryIcon.Charging);
            }

            LayoutBattery();
        }

        //=================
        // Tray icon (phone-style battery with the percentage drawn inside)
        //=================

        private void UpdateTrayMenuText()
        {
            trayShow.Text = _english ? "Show Window" : "显示窗口";
            trayExit.Text = _english ? "Exit" : "退出";
            trayExit.ForeColor = Color.FromArgb(255, 59, 48);
        }

        private void UpdateTrayIcon(int percent, bool charging)
        {
            const int S = 128;
            using (Bitmap bmp = new Bitmap(S, S))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // 竖向电池：瘦长机身 + 顶部小电极帽，避免像桶
                int bodyW = 48, bodyH = 96;
                int bx = (S - bodyW) / 2;
                int by = 20;
                Rectangle body = new Rectangle(bx, by, bodyW, bodyH);

                // 顶部电极（白色实心帽），比机身窄，明显凸出
                int nubW = 24, nubH = 16;
                Rectangle nub = new Rectangle((S - nubW) / 2, by - 12, nubW, nubH);

                Color white = Color.White;
                Color edge = Color.FromArgb(55, 59, 67);

                // 1) 白色机身实心填充，保证轮廓完整闭合（不再用描边，避免底部被裁/发灰）
                using (GraphicsPath bodyPath = Shapes.RoundedRect(body, 12))
                using (SolidBrush wb = new SolidBrush(white))
                    g.FillPath(wb, bodyPath);

                // 电量颜色阈值
                Color level;
                if (percent < 0) level = Color.FromArgb(190, 194, 201);
                else if (percent > 50) level = Palette.Green;
                else if (percent >= 10) level = Color.FromArgb(255, 204, 0);
                else level = Palette.Red;

                // 2) 内胆留白（空电部分=白色，不再用深灰），精准彩色填充从底部升起（向内缩 8px = 粗白框）
                int pad = 8;
                Rectangle inner = new Rectangle(body.X + pad, body.Y + pad, body.Width - pad * 2, body.Height - pad * 2);
                using (GraphicsPath innerPath = Shapes.RoundedRect(inner, 5))
                {
                    if (percent > 0)
                    {
                        int fh = (int)Math.Round(inner.Height * percent / 100.0);
                        if (fh > 0)
                        {
                            var st = g.Save();
                            g.SetClip(innerPath);
                            g.FillRectangle(new SolidBrush(level), new Rectangle(inner.X, inner.Bottom - fh, inner.Width, fh));
                            g.Restore(st);
                        }
                    }
                    else if (percent < 0)
                    {
                        // 未知电量：内胆浅灰，区别于空电(纯白)
                        g.FillPath(new SolidBrush(Color.FromArgb(190, 194, 201)), innerPath);
                    }
                }

                // 3) 电极帽（白色实心，盖在机身顶部）
                using (GraphicsPath nubPath = Shapes.RoundedRect(nub, 3))
                using (SolidBrush b = new SolidBrush(white))
                    g.FillPath(b, nubPath);

                // 4) 细深色描边，保证在浅色任务栏上也清晰（仅 1.5px 外轮廓，不是灰色填充）
                using (GraphicsPath bodyPath = Shapes.RoundedRect(body, 12))
                using (Pen p = new Pen(edge, 1.5f))
                    g.DrawPath(p, bodyPath);
                using (GraphicsPath nubPath = Shapes.RoundedRect(nub, 3))
                using (Pen p = new Pen(edge, 1.5f))
                    g.DrawPath(p, nubPath);

                if (charging)
                {
                    PointF c = new PointF(body.X + body.Width / 2f, body.Y + body.Height / 2f);
                    float s = body.Width * 0.5f;
                    PointF[] bolt = new PointF[]
                    {
                        new PointF(c.X + s * 0.12f, c.Y - s * 0.42f),
                        new PointF(c.X - s * 0.30f, c.Y + s * 0.08f),
                        new PointF(c.X - s * 0.02f, c.Y + s * 0.08f),
                        new PointF(c.X - s * 0.16f, c.Y + s * 0.46f),
                        new PointF(c.X + s * 0.32f, c.Y - s * 0.06f),
                        new PointF(c.X + s * 0.02f, c.Y - s * 0.06f)
                    };
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 44, 52)))
                        g.FillPolygon(b, bolt);
                }

                IntPtr hIcon = bmp.GetHicon();
                Icon newIcon = Icon.FromHandle(hIcon);
                if (_trayIcon != null) _trayIcon.Dispose();
                _trayIcon = newIcon;
                notifyIcon.Icon = _trayIcon;
            }

            notifyIcon.Text = percent >= 0
                ? string.Format("Magic Trackpad 2 — 电量 {0}%{1}", percent, charging ? " (充电中)" : "")
                : "Magic Trackpad 2 — 电量未知";
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            trayShow_Click(sender, e);
        }

        private void trayShow_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private void trayExit_Click(object sender, EventArgs e)
        {
            _trayExit = true;
            this.Close();
        }

        private void ctlAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            SetAutoStart(tglAutoStart.Checked);
        }

        //=================
        // Silent autostart via Task Scheduler (elevated, no UAC prompt at logon)
        //=================

        private const string AutoStartTaskName = "MagicTrackpad2TrayAutostart";

        private bool AutoStartTaskExists()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(
                    "schtasks.exe", "/Query /TN \"" + AutoStartTaskName + "\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetAutoStart(bool enable)
        {
            try
            {
                string exe = Application.ExecutablePath;
                string args;
                if (enable)
                    args = "/Create /TN \"" + AutoStartTaskName + "\" /TR \"\\\"" + exe + "\\\" /tray\" /SC ONLOGON /RL HIGHEST /F";
                else
                    args = "/Delete /TN \"" + AutoStartTaskName + "\" /F";

                ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", args)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                }
            }
            catch
            {
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == FormWindowState.Minimized)
                this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

        private delegate void delStringRefInt32Void(string _1, ref Int32 _2);
    }

    //=========
    // Helpers
    //=========

    public class ButtonWait : IDisposable
    {
        public ButtonWait(Button btn)
        {
            _btn = btn;
            Cursor.Current = Cursors.WaitCursor;
            Application.UseWaitCursor = true;
            if (btn.FindForm() != null && btn.FindForm().Controls["ctlFocusHack"] != null)
                btn.FindForm().Controls["ctlFocusHack"].Focus();
            _btn.Enabled = false;
            Application.DoEvents();
        }

        public void Dispose()
        {
            Cursor.Current = Cursors.Default;
            Application.UseWaitCursor = false;
            _btn.Enabled = true;
            _btn.Focus();
        }

        private Button _btn;
    }

    //=================
    // Low level stuff
    //=================

    public class UsbDevice
    {
        public static bool RestartDevices(Action action = null)
        {
            Guid guid = new Guid("4a5064e5-7d39-41d1-a0e4-81097edce967"); // <-- driver device interface

            bool success = false;
            IntPtr deviceInfoSet = EnableDevices(false, guid, INVALID_HANDLE_VALUE, ref success);

            if (action != null)
                action();

            if (success)
                EnableDevices(true, guid, deviceInfoSet, ref success);

            if (deviceInfoSet != INVALID_HANDLE_VALUE)
                SetupDiDestroyDeviceInfoList(deviceInfoSet);

            return success;
        }

        public static IntPtr EnableDevices(bool enable, Guid guid, IntPtr deviceInfoSetOverride, ref bool success)
        {
            IntPtr deviceInfoSet = deviceInfoSetOverride != INVALID_HANDLE_VALUE ? deviceInfoSetOverride :
                SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (deviceInfoSet == INVALID_HANDLE_VALUE)
                return INVALID_HANDLE_VALUE;

            uint index = 0;

            while (true)
            {
                SP_DEVINFO_DATA devInfo = new SP_DEVINFO_DATA();
                devInfo.cbSize = (UInt32)Marshal.SizeOf(devInfo);
                if (!SetupDiEnumDeviceInfo(deviceInfoSet, index, ref devInfo))
                    break;
                else
                    index++;

                SP_PROPCHANGE_PARAMS propChange = new SP_PROPCHANGE_PARAMS();
                propChange.ClassInstallHeader = new SP_CLASSINSTALL_HEADER();
                propChange.ClassInstallHeader.cbSize = (UInt32)Marshal.SizeOf(propChange.ClassInstallHeader);
                propChange.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE;
                propChange.Scope = DICS_FLAG_GLOBAL;
                propChange.StateChange = enable ? DICS_ENABLE : DICS_DISABLE;

                if (SetupDiSetClassInstallParams(deviceInfoSet, ref devInfo, ref propChange, Marshal.SizeOf(propChange)))
                {
                    if (SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, deviceInfoSet, ref devInfo))
                    {
                        success = true;
                    }
                }
            }

            return deviceInfoSet;
        }

        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        const int DIGCF_DEFAULT = 0x1;
        const int DIGCF_PRESENT = 0x2;
        const int DIGCF_ALLCLASSES = 0x4;
        const int DIGCF_PROFILE = 0x8;
        const int DIGCF_DEVICEINTERFACE = 0x10;

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(
           ref Guid ClassGuid,
           IntPtr Enumerator,
           IntPtr hwndParent,
           int Flags
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList
        (
                IntPtr DeviceInfoSet
        );

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            public UInt32 cbSize;
            public Guid ClassGuid;
            public UInt32 DevInst;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref SP_PROPCHANGE_PARAMS ClassInstallParams, int ClassInstallParamsSize);

        [StructLayout(LayoutKind.Sequential)]
        struct SP_CLASSINSTALL_HEADER
        {
            public UInt32 cbSize;
            public UInt32 InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER ClassInstallHeader;
            public UInt32 StateChange;
            public UInt32 Scope;
            public UInt32 HwProfile;
        }

        const uint DIF_PROPERTYCHANGE = 0x12;
        const uint DICS_ENABLE = 1;
        const uint DICS_DISABLE = 2;
        const uint DICS_FLAG_GLOBAL = 1;

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiCallClassInstaller(
             UInt32 InstallFunction,
             IntPtr DeviceInfoSet,
             ref SP_DEVINFO_DATA DeviceInfoData
        );
    }

    class BtDevice
    {
        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const uint METHOD_BUFFERED = 0;
        private const uint FILE_ANY_ACCESS = 0;

        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return (deviceType << 16) | (access << 14) | (function << 2) | method;
        }

        public static readonly uint IOCTL_RELOAD_SETTINGS = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x800, METHOD_BUFFERED, FILE_ANY_ACCESS);
        public static readonly uint IOCTL_GET_BATTERY = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED, FILE_ANY_ACCESS);

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        public static bool SendIoctl(uint code, bool showMessageBox = false)
        {
            uint unusedResult;
            return ExecuteIoctl(code, out unusedResult, false, showMessageBox);
        }

        public static bool SendIoctl(uint code, out uint result, bool showMessageBox = false)
        {
            return ExecuteIoctl(code, out result, true, showMessageBox);
        }

        private static bool ExecuteIoctl(uint code, out uint result, bool expectData, bool showMessageBox)
        {
            result = 0;
            SafeFileHandle hDevice = null;
            IntPtr pOutBuffer = IntPtr.Zero;

            try
            {
                hDevice = CreateFile(
                    @"\\.\AmtPtpControlDeviceUm",
                    GENERIC_READ | GENERIC_WRITE,
                    FILE_SHARE_READ | FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    OPEN_EXISTING,
                    0,
                    IntPtr.Zero
                );

                if (hDevice.IsInvalid)
                {
                    if (showMessageBox)
                    {
                        MessageBox.Show(string.Format("\u6253\u5f00\u8bbe\u5907\u5931\u8d25\u3002\u9519\u8bef\u4ee3\u7801\uff1a{0}", Marshal.GetLastWin32Error()));
                    }
                    return false;
                }

                uint outBufferSize = 0;
                if (expectData)
                {
                    outBufferSize = sizeof(uint);
                    pOutBuffer = Marshal.AllocHGlobal((int)outBufferSize);
                }

                uint bytesReturned;
                bool success = DeviceIoControl(
                    hDevice,
                    code,
                    IntPtr.Zero,
                    0,
                    pOutBuffer,
                    outBufferSize,
                    out bytesReturned,
                    IntPtr.Zero
                );

                if (success && expectData)
                {
                    result = (uint)Marshal.ReadInt32(pOutBuffer);
                }

                if (!success && showMessageBox)
                {
                    MessageBox.Show(string.Format("DeviceIoControl \u8c03\u7528\u5931\u8d25\u3002\u9519\u8bef\u4ee3\u7801\uff1a{0}", Marshal.GetLastWin32Error()));
                }

                return success;
            }
            finally
            {
                if (pOutBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pOutBuffer);
                }
                if (hDevice != null)
                {
                    hDevice.Dispose();
                }
            }
        }
    }

    // Frosted "liquid glass" renderer for the tray context menu
    internal class GlassMenuRenderer : ToolStripProfessionalRenderer
    {
        public GlassMenuRenderer()
        {
            RoundedEdges = false;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);

            // translucent frosted glass body so the DWM blur shows through (iOS 26 feel)
            using (System.Drawing.Drawing2D.LinearGradientBrush br = new System.Drawing.Drawing2D.LinearGradientBrush(
                rect, Color.FromArgb(205, 248, 250, 252), Color.FromArgb(180, 232, 236, 242), System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(br, rect);
            }

            // top sheen highlight (liquid glass reflection)
            using (System.Drawing.Drawing2D.LinearGradientBrush sheen = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, rect.Width, 16), Color.FromArgb(255, 255, 255, 200), Color.FromArgb(255, 255, 255, 0), System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                g.FillRectangle(sheen, 0, 0, rect.Width, 16);
            }

            // subtle inner light ring + outer hairline border
            using (Pen inner = new Pen(Color.FromArgb(255, 255, 255, 220), 1))
                g.DrawRectangle(inner, 1f, 1f, rect.Width - 2, rect.Height - 2);
            using (Pen outer = new Pen(Color.FromArgb(200, 204, 212), 1))
                g.DrawRectangle(outer, 0.5f, 0.5f, rect.Width - 1, rect.Height - 1);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(6, e.Item.Bounds.Y + 3, e.ToolStrip.Width - 12, e.Item.Bounds.Height - 6);
            bool destructive = !e.Item.ForeColor.IsEmpty;
            Color fill;
            if (destructive)
                fill = e.Item.Selected ? Color.FromArgb(255, 89, 78) : Color.FromArgb(255, 59, 48);
            else
                fill = e.Item.Selected ? Color.FromArgb(64, 126, 198) : Color.FromArgb(232, 236, 242);
            using (System.Drawing.Drawing2D.GraphicsPath path = Shapes.RoundedRect(r, 10))
            using (SolidBrush b = new SolidBrush(fill))
                g.FillPath(b, path);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            bool destructive = !e.Item.ForeColor.IsEmpty;
            if (e.Item.Selected || destructive)
                e.TextColor = Color.White;
            else
                e.TextColor = Color.FromArgb(30, 30, 35);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            ToolStripMenuItem tsmi = e.Item as ToolStripMenuItem;
            if (tsmi == null || !tsmi.Checked) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(e.ToolStrip.Padding.Left + 4, e.Item.Bounds.Y + e.Item.Bounds.Height / 2 - 5, 11, 10);
            using (Pen p = new Pen(Color.FromArgb(0, 122, 255), 2))
            {
                g.DrawLines(p, new Point[]
                {
                    new Point(r.Left + 1, r.Top + 5),
                    new Point(r.Left + 4, r.Top + 8),
                    new Point(r.Left + 10, r.Top + 1)
                });
            }
        }
    }
}
