using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZinovevLab33
{
    public partial class Form1 : Form
    {
        View ViewObject = new View();

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            
            ViewObject.InitShaders();
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            
            if (glControl1.Width > 0 && glControl1.Height > 0)
            {
                GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
                ViewObject.UpdateResolution(glControl1.Width, glControl1.Height);
                glControl1.Invalidate(); 
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            ViewObject.UpdateResolution(glControl1.Width, glControl1.Height);
            groupBox1.Text = $"Depth: {trackBar1.Value}";
            ViewObject.Draw();
            glControl1.SwapBuffers();
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            ViewObject.depth_value = trackBar1.Value;
            groupBox1.Text = $"Depth: {trackBar1.Value}";
            glControl1.Invalidate();
        }
    }
}

