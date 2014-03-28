using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV.Structure;
using Emgu.CV;
using DirectionRecognition;

namespace DirectionSampleTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            this.textBox_reuslt.Text = "先載入圖片";
            this.pictureBox_input.Image = null;
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Image<Bgr, byte> img = new Image<Bgr, byte>(o.FileName);
                this.pictureBox_input.Image = img.ToBitmap();
                Rectangle lb = new Rectangle(165, 355, 27, 67);
                Rectangle lt = new Rectangle(165, 55, 27, 67);
                Rectangle rb = new Rectangle(445, 350, 27, 67);
                Rectangle rt = new Rectangle(445, 60, 27, 67);
                double min1 = 0;
                double max1 = 10;
                double min2 = 160;
                double max2 = 180;
                int ED = 3;
                DR dr = new BinaryHistogram(lb, lt, rb, rt, min1, max1, min2, max2, ED);
                DR.Direction result = dr.Detect(img);
                if (result == DR.Direction.Right)
                    this.textBox_reuslt.Text = "正常方向";
                else if (result == DR.Direction.Reverse)
                    this.textBox_reuslt.Text = "相反方向";
                else
                    this.textBox_reuslt.Text = "例外";

            }
        }
    }
}
