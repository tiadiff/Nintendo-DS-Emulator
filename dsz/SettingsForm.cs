using System;
using System.Drawing;
using System.Windows.Forms;

namespace dsz
{
    public partial class SettingsForm : Form
    {
        private Form1 mainForm;
        private Label lblBrightness;
        private VolumeSlider trackBrightness;
        private Label lblFfSpeed;
        private ComboBox comboFfSpeed;
        private Label X;
        private Label lblGbaNotice;
        private CheckBox chkShowFps;

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public SettingsForm(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
            
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 25, 25));

            trackBrightness.Volume = 1.0f - (mainForm.BgDimAlpha / 255.0f);
            trackBrightness.VolumeChanged += (s, e) =>
            {
                mainForm.BgDimAlpha = (int)((1.0f - trackBrightness.Volume) * 255.0f);
                mainForm.Invalidate();
            };

            if (mainForm.FfSpeedMultiplier == 2) comboFfSpeed.SelectedIndex = 0;
            else if (mainForm.FfSpeedMultiplier == 3) comboFfSpeed.SelectedIndex = 1;
            else if (mainForm.FfSpeedMultiplier == 4) comboFfSpeed.SelectedIndex = 2;
            else comboFfSpeed.SelectedIndex = 3;

            comboFfSpeed.SelectedIndexChanged += (s, e) =>
            {
                if (comboFfSpeed.SelectedIndex == 0) mainForm.FfSpeedMultiplier = 2;
                else if (comboFfSpeed.SelectedIndex == 1) mainForm.FfSpeedMultiplier = 3;
                else if (comboFfSpeed.SelectedIndex == 2) mainForm.FfSpeedMultiplier = 4;
                else mainForm.FfSpeedMultiplier = 0;
            };

            chkShowFps.Checked = mainForm.ShowFps;
            chkShowFps.CheckedChanged += (s, e) =>
            {
                mainForm.ShowFps = chkShowFps.Checked;
                mainForm.UpdateFpsVisibility();
            };
        }

        private void InitializeComponent()
        {
            lblBrightness = new Label();
            trackBrightness = new VolumeSlider();
            lblFfSpeed = new Label();
            comboFfSpeed = new ComboBox();
            lblGbaNotice = new Label();
            chkShowFps = new CheckBox();
            X = new Label();
            SuspendLayout();
            // 
            // lblBrightness
            // 
            lblBrightness.AutoSize = true;
            lblBrightness.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBrightness.Location = new Point(24, 23);
            lblBrightness.Name = "lblBrightness";
            lblBrightness.Size = new Size(113, 15);
            lblBrightness.TabIndex = 1;
            lblBrightness.Text = "Background Brightness:";
            // 
            // trackBrightness
            // 
            trackBrightness.BackColor = Color.Transparent;
            trackBrightness.Location = new Point(143, 19);
            trackBrightness.Name = "trackBrightness";
            trackBrightness.Size = new Size(128, 25);
            trackBrightness.TabIndex = 0;
            // 
            // lblFfSpeed
            // 
            lblFfSpeed.AutoSize = true;
            lblFfSpeed.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFfSpeed.Location = new Point(24, 55);
            lblFfSpeed.Name = "lblFfSpeed";
            lblFfSpeed.Size = new Size(81, 15);
            lblFfSpeed.TabIndex = 3;
            lblFfSpeed.Text = "Fast Forward:";
            // 
            // comboFfSpeed
            // 
            comboFfSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFfSpeed.FlatStyle = FlatStyle.System;
            comboFfSpeed.FormattingEnabled = true;
            comboFfSpeed.Items.AddRange(new object[] { "2x (Balanced)", "3x", "4x (Fast)", "Max (Uncapped)" });
            comboFfSpeed.Location = new Point(111, 52);
            comboFfSpeed.Name = "comboFfSpeed";
            comboFfSpeed.Size = new Size(132, 23);
            comboFfSpeed.TabIndex = 2;
            // 
            // chkShowFps
            // 
            chkShowFps.AutoSize = true;
            chkShowFps.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkShowFps.Location = new Point(24, 85);
            chkShowFps.Name = "chkShowFps";
            chkShowFps.Size = new Size(150, 19);
            chkShowFps.TabIndex = 6;
            chkShowFps.Text = "Show FPS (On displays)";
            // 
            // lblGbaNotice
            // 
            lblGbaNotice.AutoSize = true;
            lblGbaNotice.ForeColor = Color.DimGray;
            lblGbaNotice.Location = new Point(24, 115);
            lblGbaNotice.Name = "lblGbaNotice";
            lblGbaNotice.Size = new Size(317, 15);
            lblGbaNotice.TabIndex = 4;
            lblGbaNotice.Text = "GameBoy ROMs are not supported by the emulator.";
            // 
            // X
            // 
            X.AutoSize = true;
            X.Cursor = Cursors.Hand;
            X.Font = new Font("Gill Sans MT", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            X.ForeColor = Color.Gray;
            X.Location = new Point(335, 7);
            X.Name = "X";
            X.Size = new Size(23, 23);
            X.TabIndex = 5;
            X.Text = "X";
            X.Click += X_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gainsboro;
            ClientSize = new Size(365, 145);
            ControlBox = false;
            Controls.Add(X);
            Controls.Add(chkShowFps);
            Controls.Add(trackBrightness);
            Controls.Add(lblBrightness);
            Controls.Add(comboFfSpeed);
            Controls.Add(lblFfSpeed);
            Controls.Add(lblGbaNotice);
            ForeColor = Color.FromArgb(64, 64, 64);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        private void X_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
