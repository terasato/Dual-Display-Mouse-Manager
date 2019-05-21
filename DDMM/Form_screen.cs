using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DDMM
{
    public partial class FormScreen : Form
    {
        private const int RectWidth = 20;

        public Color BorderColor;
        public string FormName;

        public FormScreen()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.FillRectangle(new SolidBrush(BorderColor), 0, 0, Width, Height);
            g.FillRectangle(new SolidBrush(TransparencyKey), RectWidth, RectWidth, Width - RectWidth * 2,
                Height - RectWidth * 2);

            g.FillRectangle(new SolidBrush(Color.White), 35, 35, 125, 35);
            var
                str = FormName; // + " (" + this.Left.ToString() + ", " + this.Right.ToString() + ", " + this.Top.ToString() + ", " + this.Bottom.ToString() + ")";
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            g.DrawString(str, new Font("Verdana", 16, FontStyle.Bold), new SolidBrush(Color.Black), 40, 40);
        }
    }
}