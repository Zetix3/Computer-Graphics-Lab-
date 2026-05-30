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

namespace ZinovevLab3
{
    public partial class Form1 : Form
    {
        View ViewObject = new View();
        
        public Form1()
        {
           
            InitializeComponent();
            
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            
            glControl1.MakeCurrent();          
            ViewObject.InitShaders();               
            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);
            glControl1.Invalidate();
            
        }
            

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            
            glControl1.MakeCurrent();
            ViewObject.Draw();
            glControl1.SwapBuffers();
        }
    }
}
