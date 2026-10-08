using System;
using System.Drawing;
using System.Windows.Forms;

namespace dsz
{
    public class VolumeSlider : Control
    {
        private float _volume = 1.0f;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public float Volume
        {
            get { return _volume; }
            set { 
                _volume = Math.Max(0f, Math.Min(1f, value)); 
                Invalidate(); 
                VolumeChanged?.Invoke(this, EventArgs.Empty); 
            }
        }

        public event EventHandler VolumeChanged;

        private bool isDragging = false;

        public VolumeSlider()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(100, 20);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background line (light grey)
            int lineY = this.Height / 2;
            using (Pen bgPen = new Pen(Color.FromArgb(180, 180, 180), 2))
            {
                bgPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                bgPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(bgPen, 10, lineY, this.Width - 10, lineY);
            }

            // Foreground line (darker grey)
            int thumbX = 10 + (int)(Volume * (this.Width - 20));
            using (Pen fgPen = new Pen(Color.FromArgb(150, 150, 150), 4))
            {
                fgPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                fgPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                if (thumbX > 10)
                {
                    g.DrawLine(fgPen, 10, lineY, thumbX, lineY);
                }
            }

            // Thumb (vertical rectangle)
            int thumbWidth = 8;
            int thumbHeight = 16;
            Rectangle thumbRect = new Rectangle(thumbX - (thumbWidth / 2), lineY - (thumbHeight / 2), thumbWidth, thumbHeight);
            using (SolidBrush thumbBrush = new SolidBrush(Color.FromArgb(150, 150, 150)))
            {
                g.FillRectangle(thumbBrush, thumbRect);
            }
        }

        private void UpdateVolumeFromMouse(int mouseX)
        {
            int range = this.Width - 20;
            int adjustedX = mouseX - 10;
            float newVol = (float)adjustedX / range;
            Volume = newVol;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                UpdateVolumeFromMouse(e.X);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isDragging)
            {
                UpdateVolumeFromMouse(e.X);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}
