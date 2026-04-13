using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenTK;

using OpenTK.Graphics.OpenGL;

namespace Zinovev_tomogram_visualizer
{
   
    public partial class Form1 : Form
    {
        Bin BinObject;
        View ViewObject;
        bool loaded = false;
        int currentLayer = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                BinObject.readBIN(str);
                ViewObject.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                ViewObject.DrawQuads(currentLayer);
                glControl1.SwapBuffers();
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
        }
    }
}
