using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AmtPtpControlPanel
{
    // Apple-style palette
    internal static class Palette
    {
        internal static readonly Color PageBackground = Color.FromArgb(242, 242, 247);
        internal static readonly Color CardBorder = Color.FromArgb(229, 229, 234);
        internal static readonly Color Label = Color.FromArgb(28, 28, 30);
        internal static readonly Color SecondaryLabel = Color.FromArgb(142, 142, 147);
        internal static readonly Color Separator = Color.FromArgb(209, 209, 214);
        internal static readonly Color Blue = Color.FromArgb(0, 122, 255);
        internal static readonly Color Green = Color.FromArgb(52, 199, 89);
        internal static readonly Color Red = Color.FromArgb(255, 59, 48);
    }

    // Shared helpers
    internal static class Shapes
    {
        internal static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();

            if (bounds.Width < d || bounds.Height < d)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    // Rounded card container
    public class Card : GroupBox
    {
        public Card()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            BackColor = Palette.PageBackground;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBack = Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Palette.PageBackground;
            g.Clear(parentBack);

            bool hasTitle = !string.IsNullOrEmpty(Text);

            Rectangle borderRect;
            if (hasTitle)
            {
                Size textSize = TextRenderer.MeasureText(Text, Font);
                borderRect = new Rectangle(
                    8,
                    textSize.Height / 2,
                    Math.Max(0, Width - 17),
                    Math.Max(0, Height - textSize.Height / 2 - 8));
            }
            else
            {
                borderRect = new Rectangle(8, 6, Math.Max(0, Width - 17), Math.Max(0, Height - 14));
            }

            using (GraphicsPath path = Shapes.RoundedRect(borderRect, 8))
            using (Pen pen = new Pen(Palette.CardBorder))
            {
                g.DrawPath(pen, path);
            }

            if (hasTitle)
            {
                using (Font boldFont = new Font(Font, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, Text, boldFont, new Point(16, 3), Palette.Label);
                }
            }
        }
    }

    // iOS-style toggle switch with text on the right; entire control is clickable
    public class ToggleSwitch : Control
    {
        private bool _checked;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                OnCheckedChanged(EventArgs.Empty);
            }
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler h = CheckedChanged;
            if (h != null) h(this, e);
        }

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick, true);
            BackColor = Palette.PageBackground;
            ForeColor = Palette.Label;
            Cursor = Cursors.Hand;
            Size = new Size(400, 30);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int sw = 42, sh = 24;

            Color trackColor;
            if (!Enabled) trackColor = Color.FromArgb(225, 225, 225);
            else trackColor = _checked ? Palette.Green : Palette.Separator;

            Rectangle track = new Rectangle(0, (Height - sh) / 2, sw, sh);
            using (GraphicsPath path = Shapes.RoundedRect(track, sh / 2))
            using (SolidBrush b = new SolidBrush(trackColor))
            {
                g.FillPath(b, path);
            }

            int knobD = sh - 4;
            int kx = _checked ? track.Right - knobD - 2 : track.Left + 2;
            Rectangle knob = new Rectangle(kx, track.Y + 2, knobD, knobD);
            using (SolidBrush b = new SolidBrush(Color.White))
            {
                g.FillEllipse(b, knob);
            }
            using (Pen p = new Pen(Color.FromArgb(210, 210, 210)))
            {
                g.DrawEllipse(p, knob);
            }

            Color textColor = Enabled ? ForeColor : Palette.SecondaryLabel;
            Rectangle textRect = new Rectangle(sw + 12, 0, Math.Max(0, Width - sw - 12), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (Enabled) Checked = !Checked;
        }
    }

    // Horizontal capsule slider (iOS Control Center style): continuous value 0..100,
    // drag anywhere inside the control, blue fill from the left, percentage centered.
    public class CapsuleSlider : Control
    {
        private double _value = 50;
        private string _minText = "";
        private string _maxText = "";
        private bool _dragging;

        public event EventHandler ValueChanged;

        public double Value
        {
            get { return _value; }
            set
            {
                double v = Math.Max(0.0, Math.Min(100.0, value));
                if (Math.Abs(_value - v) < 0.0001) return;
                _value = v;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        public string MinText
        {
            get { return _minText; }
            set { _minText = value == null ? "" : value; Invalidate(); }
        }

        public string MaxText
        {
            get { return _maxText; }
            set { _maxText = value == null ? "" : value; Invalidate(); }
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler h = ValueChanged;
            if (h != null) h(this, e);
        }

        public CapsuleSlider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            BackColor = Palette.PageBackground;
            Cursor = Cursors.Hand;
            Size = new Size(727, 68);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        private Rectangle CapsuleRect()
        {
            return new Rectangle(0, 0, Math.Max(0, Width), 42);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBack = Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Color.White;
            g.Clear(parentBack);

            Rectangle cap = CapsuleRect();

            // track
            using (GraphicsPath path = Shapes.RoundedRect(cap, cap.Height / 2))
            using (SolidBrush b = new SolidBrush(Enabled ? Color.FromArgb(233, 233, 236) : Color.FromArgb(240, 240, 242)))
            {
                g.FillPath(b, path);
            }

            // fill
            int fillW = (int)Math.Round(cap.Width * _value / 100.0);
            if (fillW > 0)
            {
                if (fillW < cap.Height) fillW = cap.Height; // keep the pill fully rounded at tiny widths
                if (fillW > cap.Width) fillW = cap.Width;
                Rectangle fill = new Rectangle(cap.X, cap.Y, fillW, cap.Height);
                using (GraphicsPath path = Shapes.RoundedRect(fill, cap.Height / 2))
                using (SolidBrush b = new SolidBrush(Enabled ? Palette.Blue : Palette.SecondaryLabel))
                {
                    g.FillPath(b, path);
                }
            }

            // centered percentage
            string text = ((int)Math.Round(_value)).ToString() + "%";
            using (Font boldFont = new Font(Font.FontFamily, 11F, FontStyle.Bold))
            {
                Color c;
                if (!Enabled) c = Palette.SecondaryLabel;
                else c = _value >= 50.0 ? Color.White : Palette.Label;
                TextRenderer.DrawText(g, text, boldFont, cap, c,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }

            // min/max captions under the capsule
            int labelY = cap.Bottom + 5;
            if (!string.IsNullOrEmpty(_minText))
                TextRenderer.DrawText(g, _minText, Font, new Point(1, labelY), Palette.SecondaryLabel);
            if (!string.IsNullOrEmpty(_maxText))
            {
                Size ms = TextRenderer.MeasureText(_maxText, Font);
                TextRenderer.DrawText(g, _maxText, Font, new Point(Width - ms.Width - 1, labelY), Palette.SecondaryLabel);
            }
        }

        private void UpdateFromX(int x)
        {
            Value = x / (double)Math.Max(1, Width) * 100.0;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled) return;
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                UpdateFromX(e.X);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging && Capture && Enabled) UpdateFromX(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!Enabled) return;
            Value = _value + (e.Delta > 0 ? 3.0 : -3.0);
        }
    }

    // iOS Settings grouped-list cell: white rounded block, left-aligned label,
    // blue chevron on the right, subtle hover/press highlight, no border stroke.
    public class CellButton : Control
    {
        private bool _hover;
        private bool _pressed;

        public CellButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick, true);
            BackColor = Color.White;
            ForeColor = Palette.Label;
            Cursor = Cursors.Hand;
            Size = new Size(783, 48);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // no plate: blend into the page background unless highlighted
            Color parentBack = Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Color.White;
            g.Clear(parentBack);

            if (_hover || _pressed)
            {
                Rectangle hl = new Rectangle(0, 0, Width - 1, Height - 1);
                using (GraphicsPath path = Shapes.RoundedRect(hl, 10))
                using (SolidBrush b = new SolidBrush(_pressed ? Color.FromArgb(225, 225, 230) : Color.FromArgb(238, 238, 242)))
                {
                    g.FillPath(b, path);
                }
            }

            // label
            Rectangle textRect = new Rectangle(18, 0, Math.Max(0, Width - 60), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, !Enabled ? Palette.SecondaryLabel : ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);

            // chevron
            int cy = Height / 2;
            Point[] chevron = new Point[]
            {
                new Point(Width - 30, cy - 7),
                new Point(Width - 21, cy),
                new Point(Width - 30, cy + 7)
            };
            using (Pen p = new Pen(!Enabled ? Palette.SecondaryLabel : Palette.Blue, 1.8F))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.LineJoin = LineJoin.Round;
                g.DrawLines(p, chevron);
            }
        }
    }

    // Globe icon button (language switch)
    public class GlobeButton : Control
    {
        private bool _hover;

        public GlobeButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.StandardClick |
                     ControlStyles.StandardDoubleClick, true);
            BackColor = Color.FromArgb(242, 242, 247);
            ForeColor = Palette.Blue;
            Cursor = Cursors.Hand;
            Size = new Size(36, 36);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBack = Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Color.White;
            g.Clear(parentBack);

            if (_hover)
            {
                Rectangle hoverRect = new Rectangle(1, 1, Width - 3, Height - 3);
                using (GraphicsPath path = Shapes.RoundedRect(hoverRect, 7))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(235, 235, 240)))
                {
                    g.FillPath(b, path);
                }
            }

            int cx = Width / 2;
            int cy = Height / 2;
            int r = Math.Min(Width, Height) / 2 - 7;

            using (Pen p = new Pen(ForeColor, 1.6F))
            {
                g.DrawEllipse(p, cx - r, cy - r, r * 2, r * 2);

                // vertical meridian ellipses
                g.DrawEllipse(p, cx - r / 2, cy - r, r, r * 2);
                g.DrawLine(p, cx, cy - r, cx, cy + r);

                // horizontal latitude lines
                g.DrawLine(p, cx - r, cy, cx + r, cy);
                double chord = Math.Sin(Math.PI / 3.0);
                int dy = (int)(r * 0.5);
                int half = (int)(r * chord);
                g.DrawLine(p, cx - half, cy - dy, cx + half, cy - dy);
                g.DrawLine(p, cx - half, cy + dy, cx + half, cy + dy);
            }
        }
    }

    // Small battery glyph with level fill and charging bolt
    public class BatteryIcon : Control
    {
        private int _percent = -1;
        private bool _charging;

        public int Percent
        {
            get { return _percent; }
            set { _percent = value; Invalidate(); }
        }

        public bool Charging
        {
            get { return _charging; }
            set { _charging = value; Invalidate(); }
        }

        public BatteryIcon()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint, true);
            BackColor = Palette.PageBackground;
            Size = new Size(26, 13);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null && Parent.BackColor != Color.Transparent ? Parent.BackColor : Color.White);

            Rectangle body = new Rectangle(0, 1, Width - 5, Height - 3);
            using (Pen p = new Pen(Palette.SecondaryLabel))
            {
                using (GraphicsPath path = Shapes.RoundedRect(body, 3))
                {
                    g.DrawPath(p, path);
                }
                Brush capB = new SolidBrush(Palette.SecondaryLabel);
                g.FillRectangle(capB, body.Right, Height / 2 - 3, 3, 6);
                capB.Dispose();
            }

            if (_percent > 0)
            {
                Color fillColor = _charging ? Palette.Green :
                    (_percent <= 20 ? Palette.Red : Palette.Green);
                int fillW = (int)((body.Width - 4) * (_percent / 100f));
                if (fillW > 1)
                {
                    Rectangle fill = new Rectangle(body.X + 2, body.Y + 2, fillW, body.Height - 4);
                    using (GraphicsPath path = Shapes.RoundedRect(fill, 1))
                    using (SolidBrush b = new SolidBrush(fillColor))
                    {
                        g.FillPath(b, path);
                    }
                }
            }

            if (_charging)
            {
                Point[] bolt = new Point[]
                {
                    new Point(body.Width * 58 / 100, body.Top),
                    new Point(body.Width * 30 / 100, body.Height * 60 / 100),
                    new Point(body.Width * 48 / 100, body.Height * 60 / 100),
                    new Point(body.Width * 40 / 100, body.Bottom),
                    new Point(body.Width * 72 / 100, body.Height * 35 / 100),
                    new Point(body.Width * 52 / 100, body.Height * 35 / 100)
                };
                using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 214, 10)))
                {
                    g.FillPolygon(b, bolt);
                }
                using (Pen p = new Pen(Color.FromArgb(120, 90, 0)))
                {
                    g.DrawPolygon(p, bolt);
                }
            }
        }
    }
}
