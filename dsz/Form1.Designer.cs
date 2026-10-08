namespace dsz
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            closeButton = new Panel();
            panel1 = new Panel();
            panel2 = new Panel();
            panel3 = new Panel();
            panel4 = new Panel();
            debugToolsPanel = new Panel();
            gearIcon = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)gearIcon).BeginInit();
            SuspendLayout();
            // 
            // closeButton
            // 
            closeButton.BackColor = Color.Transparent;
            closeButton.Location = new Point(121, 616);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(35, 34);
            closeButton.TabIndex = 1;
            closeButton.Click += closeButton_Click;
            closeButton.Paint += closeButton_Paint;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.BorderStyle = BorderStyle.Fixed3D;
            panel1.Location = new Point(185, 443);
            panel1.Name = "panel1";
            panel1.Size = new Size(371, 272);
            panel1.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Transparent;
            panel2.BorderStyle = BorderStyle.Fixed3D;
            panel2.Location = new Point(190, 53);
            panel2.Name = "panel2";
            panel2.Size = new Size(363, 271);
            panel2.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.BackColor = Color.Transparent;
            panel3.Location = new Point(121, 653);
            panel3.Name = "panel3";
            panel3.Size = new Size(35, 34);
            panel3.TabIndex = 2;
            // 
            // panel4
            // 
            panel4.BackColor = Color.Transparent;
            panel4.BackgroundImage = Properties.Resources.fast_forward_256;
            panel4.BackgroundImageLayout = ImageLayout.Stretch;
            panel4.Location = new Point(48, 726);
            panel4.Name = "panel4";
            panel4.Size = new Size(22, 23);
            panel4.TabIndex = 5;
            // 
            // debugToolsPanel
            // 
            debugToolsPanel.BackColor = Color.Transparent;
            debugToolsPanel.Cursor = Cursors.Hand;
            debugToolsPanel.Location = new Point(76, 726);
            debugToolsPanel.Name = "debugToolsPanel";
            debugToolsPanel.Size = new Size(22, 23);
            debugToolsPanel.TabIndex = 6;
            debugToolsPanel.Click += debugToolsPanel_Click;
            // 
            // gearIcon
            // 
            gearIcon.BackColor = Color.Transparent;
            gearIcon.Cursor = Cursors.Hand;
            gearIcon.ImageLocation = "Resources\\gear.jpg";
            gearIcon.Location = new Point(17, 723);
            gearIcon.Name = "gearIcon";
            gearIcon.Size = new Size(25, 28);
            gearIcon.SizeMode = PictureBoxSizeMode.Zoom;
            gearIcon.TabIndex = 4;
            gearIcon.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.ds;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(742, 771);
            ControlBox = false;
            Controls.Add(gearIcon);
            Controls.Add(debugToolsPanel);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(closeButton);
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)gearIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel closeButton;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel debugToolsPanel;
        private PictureBox gearIcon;
    }
}
