using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public Main()
        {
            InitializeComponent();

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
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            _saveDebounce.Stop();
            _saveDebounce.Dispose();
            if (_batteryDelay != null) _batteryDelay.Dispose();
            if (_batteryPoll != null) _batteryPoll.Dispose();
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
                tglDisable.Checked = true;
            else if (feedbackClick == 0xffffff && feedbackRelease == 0xffffff)
                tglMaximum.Checked = true;
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
            }
            else
            {
                lblBattery.Text = "--%";
                batteryIcon.Percent = -1;
            }

            LayoutBattery();
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
}
