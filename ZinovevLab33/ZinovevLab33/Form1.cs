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
        }


        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            ViewObject.Draw();
            glControl1.SwapBuffers();
        }

    }
}

