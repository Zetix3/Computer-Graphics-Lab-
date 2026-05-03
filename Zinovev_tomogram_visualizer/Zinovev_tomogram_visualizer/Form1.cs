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



namespace Zinovev_tomogram_visualizer
{
    
    public partial class Form1 : Form
    {
        Bin BinObject = new Bin();
        View ViewObject = new View();
        bool loaded = false;
        int currentLayer = 0;

        

        public Form1()
        {
            InitializeComponent();

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                BinObject.readBIN(str);
                trackBar1.Maximum = BinObject.MAX;
                ViewObject.SetupView(glControl1.Width, glControl1.Height);
                loaded = true;
                glControl1.Invalidate();
            }
        }
        bool needReload = false;
        int mode = 0;
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (loaded)
            {
                if (mode == 0)
                {
                    ViewObject.DrawQuads(currentLayer);
                }
                else if (mode == 1) 
                { 
                    if (needReload)
                    {
                        ViewObject.generateTextureImage(currentLayer);
                        ViewObject.Load2DTexture();
                        needReload = false;
                    }

                    ViewObject.DrawTexture();
                }
                else if (mode == 2)
                {
                    ViewObject.DrawQuadStrip(currentLayer);
                }

                glControl1.SwapBuffers();
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        int FrameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);

        void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                this.Text = String.Format("CT Visualizer (fps={0})", FrameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                FrameCount = 0;
            }
            FrameCount++;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                mode = 0;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                mode = 1;
            }
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                mode = 2;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            ViewObject.min = trackBar2.Value;

            int weight = trackBar3.Value;
            ViewObject.max = ViewObject.min + weight;

            needReload = true;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int weight = trackBar3.Value;
            ViewObject.max = weight + ViewObject.min;
            needReload = true;
        }

        
    }
}
