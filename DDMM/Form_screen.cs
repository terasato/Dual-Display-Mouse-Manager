using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DDMM
{
    public partial class Form_screen : Form
    {
        public Form_screen()
        {
            InitializeComponent();
        }

        public Color borderColor;
        public String FormName;
        private const int rectwidth = 20;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics G = e.Graphics;
            G.FillRectangle(new SolidBrush(this.borderColor), 0, 0, this.Width, this.Height);
            G.FillRectangle(new SolidBrush(this.TransparencyKey), rectwidth, rectwidth, this.Width-rectwidth*2, this.Height-rectwidth*2);

            G.FillRectangle(new SolidBrush(Color.White), 35, 35, 125, 35);
            String str = FormName; // + " (" + this.Left.ToString() + ", " + this.Right.ToString() + ", " + this.Top.ToString() + ", " + this.Bottom.ToString() + ")";
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            G.DrawString(str, new Font("Verdana", 16, FontStyle.Bold), new SolidBrush(Color.Black), 40, 40);
            
        }

    }
}
