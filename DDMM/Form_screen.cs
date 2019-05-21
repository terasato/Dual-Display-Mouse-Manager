using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DDMM
{
    public partial class Form_screen : Form
    {
        private const int rectwidth = 20;

        public Color borderColor;
        public string FormName;

        public Form_screen()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var G = e.Graphics;
            G.FillRectangle(new SolidBrush(borderColor), 0, 0, Width, Height);
            G.FillRectangle(new SolidBrush(TransparencyKey), rectwidth, rectwidth, Width - rectwidth * 2,
                Height - rectwidth * 2);

            G.FillRectangle(new SolidBrush(Color.White), 35, 35, 125, 35);
            var
                str = FormName; // + " (" + this.Left.ToString() + ", " + this.Right.ToString() + ", " + this.Top.ToString() + ", " + this.Bottom.ToString() + ")";
            G.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            G.DrawString(str, new Font("Verdana", 16, FontStyle.Bold), new SolidBrush(Color.Black), 40, 40);
        }
    }
}