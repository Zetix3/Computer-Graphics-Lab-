using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZinovevLab1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Stack<Image> st = new Stack<Image>();
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
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
                st.Clear();
            }
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
           Filters filter = new InvertFilter();
           
           backgroundWorker1.RunWorkerAsync(filter);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
            {
                Image imageCopy = new Bitmap(image);
                st.Push(imageCopy);
                image = newImage;
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (!e.Cancelled)
            {
                
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void гауссToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличениеЯркостиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessUpFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void собельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            dialog.DefaultExt = "png";
            dialog.AddExtension = true;
            dialog.FileName = "image.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    image.Save(dialog.FileName);
                }
                catch (Exception ex) {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (st.Count > 0)
            {
                Image imageCopy = st.Pop();
                image = (Bitmap)imageCopy;
                pictureBox1.Image = imageCopy;
                pictureBox1.Refresh();
            }
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                GrayWorldFilter filter = new GrayWorldFilter();
                filter.calculateAvgCoefficient(image);
                backgroundWorker1.RunWorkerAsync(filter);
            }
            catch{ }
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LinearStretchingFilter filter = new LinearStretchingFilter();
                filter.calculateMaxMinIntensity(image);
                backgroundWorker1.RunWorkerAsync(filter);
            }
            catch { }
        }

        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new ShiftFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new RotationFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new EmbossingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void motionBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionBlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void расширениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DilationFilter filter = new DilationFilter();
            filter.setKernel(currentKernel);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void сужениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErosionFilter filter = new ErosionFilter();
            filter.setKernel(currentKernel);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void открытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpeningFilter filter = new OpeningFilter();
            filter.setKernel(currentKernel);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void закрытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClosingFilter filter = new ClosingFilter();
            filter.setKernel(currentKernel);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopHatFilter filter = new TopHatFilter();
            filter.setKernel(currentKernel);
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void медианныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void диагональныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new DiagonalFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }



        private void точечныеToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private float[,] currentKernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };

        private void структурныйЭлементToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (KernelInputForm inputForm = new KernelInputForm())
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    currentKernel = inputForm.Kernel;

                    int size = currentKernel.GetLength(0);
                    MessageBox.Show($"Структурный элемент {size}x{size} загружен!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        
    }

    public class KernelInputForm : Form
    {
        private NumericUpDown nudSize;      
        private TextBox txtKernel;           
        private Button btnOk;                 
        private Button btnCancel;             
        
        public float[,] Kernel { get; private set; }

        
        public KernelInputForm()
        {
            InitializeComponents(); 
        }

        private void InitializeComponents()
        {
            
            this.Text = "Ввод структурного элемента";     
            this.Size = new Size(400, 250);                
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;                       
            this.MinimizeBox = false;                       
            this.StartPosition = FormStartPosition.CenterParent; 

            Label lblSize = new Label()
            {
                Text = "Размер матрицы:",   
                Location = new Point(20, 20), 
                Size = new Size(120, 25)     
            };

            nudSize = new NumericUpDown()
            {
                Location = new Point(150, 20), 
                Size = new Size(60, 25),       
                Minimum = 1,                    
                Maximum = 9,                     
                Value = 3                        
            };
            
            Label lblKernel = new Label()
            {
                Text = "Элементы матрицы:",
                Location = new Point(20, 60),
                Size = new Size(120, 25)
            };

           
            txtKernel = new TextBox()
            {
                Location = new Point(20, 90),
                Size = new Size(340, 25),
                Text = "0 1 0 1 1 1 0 1 0" 
            };

            btnOk = new Button()
            {
                Text = "OK",
                Location = new Point(200, 160),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK 
            };
            btnOk.Click += BtnOk_Click; 

            
            btnCancel = new Button()
            {
                Text = "Отмена",
                Location = new Point(285, 160),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel 
            };

            
            this.Controls.AddRange(new Control[] {
                lblSize, nudSize,        
                lblKernel, txtKernel,     
                btnOk, btnCancel          
            });
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                int size = (int)nudSize.Value;

                string[] parts = txtKernel.Text.Trim().Split(new char[] { ' ', ',' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != size * size)
                {
                    MessageBox.Show($"Нужно ввести {size * size} чисел через пробел",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None; 
                    return;
                }

                Kernel = new float[size, size];

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        
                        if (!float.TryParse(parts[i * size + j], out float value))
                        {
                            MessageBox.Show($"Ошибка в числе {parts[i * size + j]}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.DialogResult = DialogResult.None;
                            return;
                        }

                        if (value != 0 && value != 1)
                        {
                            MessageBox.Show("Можно вводить только 0 или 1",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.DialogResult = DialogResult.None;
                            return;
                        }

                        Kernel[i, j] = value;
                    }
                }

                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
