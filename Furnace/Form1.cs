using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Emgu.CV.UI;

namespace Furnace
{
    public partial class Form1 : Form
    {
        private Model _model;

        delegate void UpdateFrameHandler();

        public Form1(Model model)
        {
            _model = model;
            _model.OnFrameUpdate += ModelOnFrameUpdateHandler;

            InitializeComponent();

            _model.PlayPreAni();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _model.Execute();
        }

        private void UpdateFrame()
        {
            if (this.main_imageBox.InvokeRequired)
            {
                UpdateFrameHandler ufh = new UpdateFrameHandler(UpdateFrame);
                this.main_imageBox.Invoke(ufh);
            }
            else
                this.main_imageBox.Image = _model.GetFrame();
        }

        private void ModelOnFrameUpdateHandler(object sender, EventArgs e)
        {
            UpdateFrame();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _model.Close();
        }
    }
}
