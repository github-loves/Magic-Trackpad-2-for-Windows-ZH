
namespace AmtPtpControlPanel
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnLang = new AmtPtpControlPanel.GlobeButton();
            this.batteryIcon = new AmtPtpControlPanel.BatteryIcon();
            this.lblBattery = new System.Windows.Forms.Label();
            this.cardFeedback = new AmtPtpControlPanel.Card();
            this.sldFeedback = new AmtPtpControlPanel.CapsuleSlider();
            this.cardModes = new AmtPtpControlPanel.Card();
            this.tglDisable = new AmtPtpControlPanel.ToggleSwitch();
            this.tglMaximum = new AmtPtpControlPanel.ToggleSwitch();
            this.cardStop = new AmtPtpControlPanel.Card();
            this.radStopNone = new System.Windows.Forms.RadioButton();
            this.radStopPressure = new System.Windows.Forms.RadioButton();
            this.txtStopPressure = new System.Windows.Forms.TextBox();
            this.lblPressureUnit = new System.Windows.Forms.Label();
            this.radStopSize = new System.Windows.Forms.RadioButton();
            this.txtStopSize = new System.Windows.Forms.TextBox();
            this.lblSizeUnit = new System.Windows.Forms.Label();
            this.cardOther = new AmtPtpControlPanel.Card();
            this.tglNearFingers = new AmtPtpControlPanel.ToggleSwitch();
            this.tglButtonFinger = new AmtPtpControlPanel.ToggleSwitch();
            this.tglPalmRejection = new AmtPtpControlPanel.ToggleSwitch();
            this.btnTouchpadSettings = new AmtPtpControlPanel.CellButton();
            this.pnlHeader.SuspendLayout();
            this.cardFeedback.SuspendLayout();
            this.cardModes.SuspendLayout();
            this.cardStop.SuspendLayout();
            this.cardOther.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(242, 242, 247);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnLang);
            this.pnlHeader.Controls.Add(this.batteryIcon);
            this.pnlHeader.Controls.Add(this.lblBattery);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(815, 52);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Magic Trackpad 2";
            // 
            // btnLang
            // 
            this.btnLang.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.btnLang.Location = new System.Drawing.Point(636, 8);
            this.btnLang.Name = "btnLang";
            this.btnLang.Size = new System.Drawing.Size(36, 36);
            this.btnLang.TabIndex = 1;
            this.btnLang.Click += new System.EventHandler(this.btnLang_Click);
            // 
            // batteryIcon
            // 
            this.batteryIcon.Location = new System.Drawing.Point(724, 20);
            this.batteryIcon.Name = "batteryIcon";
            this.batteryIcon.Size = new System.Drawing.Size(26, 13);
            this.batteryIcon.TabIndex = 2;
            // 
            // lblBattery
            // 
            this.lblBattery.AutoSize = true;
            this.lblBattery.ForeColor = System.Drawing.Color.FromArgb(60, 60, 67);
            this.lblBattery.Location = new System.Drawing.Point(736, 16);
            this.lblBattery.Name = "lblBattery";
            this.lblBattery.TabIndex = 3;
            this.lblBattery.Text = "--%";
            // 
            // cardFeedback
            // 
            this.cardFeedback.Controls.Add(this.sldFeedback);
            this.cardFeedback.Location = new System.Drawing.Point(16, 62);
            this.cardFeedback.Name = "cardFeedback";
            this.cardFeedback.Size = new System.Drawing.Size(783, 124);
            this.cardFeedback.TabStop = false;
            // 
            // sldFeedback
            // 
            this.sldFeedback.Location = new System.Drawing.Point(28, 40);
            this.sldFeedback.Name = "sldFeedback";
            this.sldFeedback.Size = new System.Drawing.Size(727, 68);
            this.sldFeedback.ValueChanged += new System.EventHandler(this.sldFeedback_ValueChanged);
            // 
            // cardModes
            // 
            this.cardModes.Controls.Add(this.tglDisable);
            this.cardModes.Controls.Add(this.tglMaximum);
            this.cardModes.Location = new System.Drawing.Point(16, 196);
            this.cardModes.Name = "cardModes";
            this.cardModes.Size = new System.Drawing.Size(783, 120);
            this.cardModes.TabStop = false;
            // 
            // tglDisable
            // 
            this.tglDisable.Location = new System.Drawing.Point(28, 26);
            this.tglDisable.Name = "tglDisable";
            this.tglDisable.Size = new System.Drawing.Size(727, 30);
            this.tglDisable.CheckedChanged += new System.EventHandler(this.ctlModeOptions_CheckedChanged);
            // 
            // tglMaximum
            // 
            this.tglMaximum.Location = new System.Drawing.Point(28, 70);
            this.tglMaximum.Name = "tglMaximum";
            this.tglMaximum.Size = new System.Drawing.Size(727, 30);
            this.tglMaximum.CheckedChanged += new System.EventHandler(this.ctlModeOptions_CheckedChanged);
            // 
            // cardStop
            // 
            this.cardStop.Controls.Add(this.radStopNone);
            this.cardStop.Controls.Add(this.radStopPressure);
            this.cardStop.Controls.Add(this.txtStopPressure);
            this.cardStop.Controls.Add(this.lblPressureUnit);
            this.cardStop.Controls.Add(this.radStopSize);
            this.cardStop.Controls.Add(this.txtStopSize);
            this.cardStop.Controls.Add(this.lblSizeUnit);
            this.cardStop.Location = new System.Drawing.Point(16, 326);
            this.cardStop.Name = "cardStop";
            this.cardStop.Size = new System.Drawing.Size(783, 150);
            this.cardStop.TabStop = false;
            // 
            // radStopNone
            // 
            this.radStopNone.AutoSize = true;
            this.radStopNone.Location = new System.Drawing.Point(28, 42);
            this.radStopNone.Name = "radStopNone";
            this.radStopNone.UseVisualStyleBackColor = true;
            this.radStopNone.CheckedChanged += new System.EventHandler(this.ctlStop_CheckedChanged);
            // 
            // radStopPressure
            // 
            this.radStopPressure.AutoSize = true;
            this.radStopPressure.Location = new System.Drawing.Point(28, 84);
            this.radStopPressure.Name = "radStopPressure";
            this.radStopPressure.UseVisualStyleBackColor = true;
            this.radStopPressure.CheckedChanged += new System.EventHandler(this.ctlStop_CheckedChanged);
            // 
            // txtStopPressure
            // 
            this.txtStopPressure.Location = new System.Drawing.Point(340, 82);
            this.txtStopPressure.Name = "txtStopPressure";
            this.txtStopPressure.Size = new System.Drawing.Size(51, 25);
            this.txtStopPressure.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtStopPressure.TextChanged += new System.EventHandler(this.StopValue_TextChanged);
            // 
            // lblPressureUnit
            // 
            this.lblPressureUnit.AutoSize = true;
            this.lblPressureUnit.ForeColor = System.Drawing.Color.FromArgb(142, 142, 147);
            this.lblPressureUnit.Location = new System.Drawing.Point(398, 85);
            this.lblPressureUnit.Name = "lblPressureUnit";
            // 
            // radStopSize
            // 
            this.radStopSize.AutoSize = true;
            this.radStopSize.Location = new System.Drawing.Point(28, 120);
            this.radStopSize.Name = "radStopSize";
            this.radStopSize.UseVisualStyleBackColor = true;
            this.radStopSize.CheckedChanged += new System.EventHandler(this.ctlStop_CheckedChanged);
            // 
            // txtStopSize
            // 
            this.txtStopSize.Location = new System.Drawing.Point(340, 118);
            this.txtStopSize.Name = "txtStopSize";
            this.txtStopSize.Size = new System.Drawing.Size(51, 25);
            this.txtStopSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtStopSize.TextChanged += new System.EventHandler(this.StopValue_TextChanged);
            // 
            // lblSizeUnit
            // 
            this.lblSizeUnit.AutoSize = true;
            this.lblSizeUnit.ForeColor = System.Drawing.Color.FromArgb(142, 142, 147);
            this.lblSizeUnit.Location = new System.Drawing.Point(398, 121);
            this.lblSizeUnit.Name = "lblSizeUnit";
            // 
            // cardOther
            // 
            this.cardOther.Controls.Add(this.tglNearFingers);
            this.cardOther.Controls.Add(this.tglButtonFinger);
            this.cardOther.Controls.Add(this.tglPalmRejection);
            this.cardOther.Location = new System.Drawing.Point(16, 486);
            this.cardOther.Name = "cardOther";
            this.cardOther.Size = new System.Drawing.Size(783, 162);
            this.cardOther.TabStop = false;
            // 
            // tglNearFingers
            // 
            this.tglNearFingers.Location = new System.Drawing.Point(28, 34);
            this.tglNearFingers.Name = "tglNearFingers";
            this.tglNearFingers.Size = new System.Drawing.Size(727, 30);
            this.tglNearFingers.CheckedChanged += new System.EventHandler(this.OtherOption_CheckedChanged);
            // 
            // tglButtonFinger
            // 
            this.tglButtonFinger.Location = new System.Drawing.Point(28, 76);
            this.tglButtonFinger.Name = "tglButtonFinger";
            this.tglButtonFinger.Size = new System.Drawing.Size(727, 30);
            this.tglButtonFinger.CheckedChanged += new System.EventHandler(this.OtherOption_CheckedChanged);
            // 
            // tglPalmRejection
            // 
            this.tglPalmRejection.Location = new System.Drawing.Point(28, 118);
            this.tglPalmRejection.Name = "tglPalmRejection";
            this.tglPalmRejection.Size = new System.Drawing.Size(727, 30);
            this.tglPalmRejection.CheckedChanged += new System.EventHandler(this.OtherOption_CheckedChanged);
            // 
            // btnTouchpadSettings
            // 
            this.btnTouchpadSettings.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.btnTouchpadSettings.Location = new System.Drawing.Point(16, 744);
            this.btnTouchpadSettings.Name = "btnTouchpadSettings";
            this.btnTouchpadSettings.Size = new System.Drawing.Size(783, 48);
            this.btnTouchpadSettings.Click += new System.EventHandler(this.ctlTouchpadSettings_Click);
            // 
            // cardStartup
            // 
            this.cardStartup = new AmtPtpControlPanel.Card();
            this.tglAutoStart = new AmtPtpControlPanel.ToggleSwitch();
            this.cardStartup.SuspendLayout();
            // 
            // tglAutoStart
            // 
            this.tglAutoStart.Location = new System.Drawing.Point(28, 34);
            this.tglAutoStart.Name = "tglAutoStart";
            this.tglAutoStart.Size = new System.Drawing.Size(727, 30);
            this.tglAutoStart.Text = "开机自启（静默托盘）";
            this.tglAutoStart.CheckedChanged += new System.EventHandler(this.ctlAutoStart_CheckedChanged);
            // 
            // cardStartup
            // 
            this.cardStartup.Controls.Add(this.tglAutoStart);
            this.cardStartup.Location = new System.Drawing.Point(16, 658);
            this.cardStartup.Name = "cardStartup";
            this.cardStartup.Size = new System.Drawing.Size(783, 76);
            this.cardStartup.TabIndex = 0;
            this.cardStartup.Text = "启动选项";
            this.cardStartup.ResumeLayout(false);
            // 
            // tray components
            // 
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.trayShow = new System.Windows.Forms.ToolStripMenuItem();
            this.trayExit = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trayExit});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(140, 70);
            // 
            // trayShow
            // 
            this.trayShow.Name = "trayShow";
            this.trayShow.Size = new System.Drawing.Size(139, 24);
            this.trayShow.Text = "显示窗口";
            this.trayShow.Click += new System.EventHandler(this.trayShow_Click);
            // 
            // trayExit
            // 
            this.trayExit.Name = "trayExit";
            this.trayExit.Size = new System.Drawing.Size(139, 24);
            this.trayExit.Text = "退出";
            this.trayExit.Click += new System.EventHandler(this.trayExit_Click);
            // 
            // notifyIcon
            // 
            this.trayMenu.Renderer = new AmtPtpControlPanel.GlassMenuRenderer();
            this.trayMenu.ForeColor = System.Drawing.Color.FromArgb(30, 30, 35);
            this.trayMenu.ShowImageMargin = false;
            this.trayMenu.ShowCheckMargin = true;
            this.trayMenu.DropShadowEnabled = true;
            this.notifyIcon.ContextMenuStrip = this.trayMenu;
            this.notifyIcon.Text = "Magic Trackpad 2";
            this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 808);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.cardFeedback);
            this.Controls.Add(this.cardModes);
            this.Controls.Add(this.cardStop);
            this.Controls.Add(this.cardOther);
            this.Controls.Add(this.cardStartup);
            this.Controls.Add(this.btnTouchpadSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.BackColor = System.Drawing.Color.FromArgb(242, 242, 247);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.ShowIcon = false;
            this.ForeColor = System.Drawing.Color.FromArgb(28, 28, 30);
            this.Text = "";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.cardFeedback.ResumeLayout(false);
            this.cardModes.ResumeLayout(false);
            this.cardStop.ResumeLayout(false);
            this.cardStop.PerformLayout();
            this.cardOther.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private AmtPtpControlPanel.GlobeButton btnLang;
        private AmtPtpControlPanel.BatteryIcon batteryIcon;
        private System.Windows.Forms.Label lblBattery;
        private AmtPtpControlPanel.Card cardFeedback;
        private AmtPtpControlPanel.CapsuleSlider sldFeedback;
        private AmtPtpControlPanel.Card cardModes;
        private AmtPtpControlPanel.ToggleSwitch tglDisable;
        private AmtPtpControlPanel.ToggleSwitch tglMaximum;
        private AmtPtpControlPanel.Card cardStop;
        private System.Windows.Forms.RadioButton radStopNone;
        private System.Windows.Forms.RadioButton radStopPressure;
        private System.Windows.Forms.TextBox txtStopPressure;
        private System.Windows.Forms.Label lblPressureUnit;
        private System.Windows.Forms.RadioButton radStopSize;
        private System.Windows.Forms.TextBox txtStopSize;
        private System.Windows.Forms.Label lblSizeUnit;
        private AmtPtpControlPanel.Card cardOther;
        private AmtPtpControlPanel.ToggleSwitch tglNearFingers;
        private AmtPtpControlPanel.ToggleSwitch tglButtonFinger;
        private AmtPtpControlPanel.ToggleSwitch tglPalmRejection;
        private AmtPtpControlPanel.CellButton btnTouchpadSettings;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem trayShow;
        private System.Windows.Forms.ToolStripMenuItem trayExit;
        private AmtPtpControlPanel.Card cardStartup;
        private AmtPtpControlPanel.ToggleSwitch tglAutoStart;
    }
}
