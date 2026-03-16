using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZinovevLab1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max) 
                return max; 
            return value;
        }
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R,
                                             255 -  sourceColor.G,
                                             255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
            Color resultColor = Color.FromArgb(
                                        Clamp(intensity, 0, 255),
                                        Clamp(intensity, 0, 255),
                                        Clamp(intensity, 0, 255));
            return resultColor;
        }
        public Color calculateTmpPixelColor(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor((Bitmap)sourceImage, x, y);
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double intensity = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;
            int k = 10;
            Color resultColor = Color.FromArgb( 
                Clamp((int)(intensity + 2 * k), 0, 255),
                Clamp((int)(intensity + 0.5 * k), 0, 255),
                Clamp((int)(intensity - 1 * k), 0, 255));
            return resultColor;
        }
    }

    class BrightnessUpFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int c = 30;
            Color resultColor = Color.FromArgb(
                Clamp(sourceColor.R + c, 0, 255),
                Clamp(sourceColor.G + c, 0, 255),
                Clamp(sourceColor.B + c, 0, 255));
            return resultColor;
        }

    }

    class GrayWorldFilter : Filters
    {
        private double cR;
        private double cG;
        private double cB;

        public void calculateAvgCoefficient(Bitmap sourceImage)
        {
            double avgR = 0;
            double avgG = 0;
            double avgB = 0;
            double average;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color tmpSourceColor = sourceImage.GetPixel(i, j);
                    avgR += tmpSourceColor.R;
                    avgG += tmpSourceColor.G;
                    avgB += tmpSourceColor.B;
                }
            }

            int n = sourceImage.Width * sourceImage.Height;
            avgR /= n;
            avgG /= n;
            avgB /= n;
            average = (avgR + avgG + avgB) / 3;

            cR = (avgR > 0) ? (average / avgR) : 1;
            cG = (avgG > 0) ? (average / avgG) : 1;
            cB = (avgB > 0) ? (average / avgB) : 1;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(
                Clamp((int)(sourceColor.R * cR), 0, 255),
                Clamp((int)(sourceColor.G * cG), 0, 255),
                Clamp((int)(sourceColor.B * cB), 0, 255));
            return resultColor;
        }
    }

    class LinearStretchingFilter : Filters
    {
        private int minY = 255;
        private int maxY = 0;
        
        public void calculateMaxMinIntensity(Bitmap sourceImage)
        {
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    int Y = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
                    if (Y > maxY) maxY = Y;
                    if (Y < minY) minY = Y;
                }
            }
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double intensity = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;
            double newIntensity = (intensity - minY) * 255.0 / (maxY - minY);
            newIntensity = Clamp((int)newIntensity, 0, 255);
            double scale = (intensity > 0) ? (newIntensity / intensity) : 1;

            Color resultColor = Color.FromArgb(
                Clamp((int)(sourceColor.R * scale), 0, 255),
                Clamp((int)(sourceColor.G * scale), 0, 255),
                Clamp((int)(sourceColor.B * scale), 0, 255));
            
            return resultColor;
        }
        
    }

    class GlassFilter : Filters
    {
        Random random = new Random();
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        { 
            int xx = (int)(x + (random.NextDouble() - 0.5) * 10);
            int yy = (int)(y + (random.NextDouble() - 0.5) * 10);
            xx = Clamp(xx, 0, sourceImage.Width - 1);
            yy = Clamp(yy, 0, sourceImage.Height - 1);
            Color sourceColor = sourceImage.GetPixel(xx, yy);
            Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
            return resultColor;
        }
    }

    class ShiftFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int xx = x + 50;
            int yy = y;

            if (xx < 0 || xx >= sourceImage.Width || yy < 0 || yy >= sourceImage.Height)
            {
                return Color.Black;  
            }
            Color sourceColor = sourceImage.GetPixel(xx, yy);
            Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
            return resultColor;
        }
    }

    class RotationFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int x0 = sourceImage.Width / 2;
            int y0 = sourceImage.Height / 2;
            int xx = (int)((x - x0)*Math.Cos(Math.PI / 4) - (y - y0)*Math.Sin(Math.PI / 4) + x0);
            int yy = (int)((x - x0) * Math.Sin(Math.PI / 4) + (y - y0) * Math.Cos(Math.PI / 4) + y0);

            if (xx < 0 || xx >= sourceImage.Width || yy < 0 || yy >= sourceImage.Height)
            {
                return Color.Black;
            }
            Color sourceColor = sourceImage.GetPixel(xx, yy);
            Color resultColor = Color.FromArgb(sourceColor.R, sourceColor.G, sourceColor.B);
            return resultColor;
        }
    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x+k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y+l, 0,sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

                }
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY); 
                }
            }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius ; i++) 
                for (int j = -radius; j <= radius ; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class SobelFilter : MatrixFilter
    {
        private float[,] kernelX;
        private float[,] kernelY;
        public SobelFilter()
        {
            kernelY = new float [,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
            kernelX = new float [,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = 1;
            int radiusY = 1;
            float gxR = 0, gyR = 0;
            float gxG = 0, gyG = 0;
            float gxB = 0, gyB = 0;
            int resultR = 0, resultG = 0, resultB = 0;
            Color lastColor = sourceImage.GetPixel(Clamp(x - radiusX, 0, sourceImage.Width - 1), Clamp(y - radiusY, 0, sourceImage.Height - 1));
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    if (x + k < 0) continue;
                    if (y + k < 0) continue;
                    if(x + k > sourceImage.Width) continue;
                    if(y + k > sourceImage.Height) continue;
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + k, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    if(neighborColor == lastColor && lastColor == Color.Black) continue;
                    
                    if (neighborColor.R != 0)
                    {
                        gxR += neighborColor.R * kernelX[k + radiusX, l + radiusY];
                        gyR += neighborColor.R * kernelY[k + radiusX, l + radiusY];
                    }
                    if (neighborColor.G != 0)
                    {
                        gxG += neighborColor.G * kernelX[k + radiusX, l + radiusY];
                        gyG += neighborColor.G * kernelY[k + radiusX, l + radiusY];
                    }
                    if (neighborColor.B != 0)
                    {
                        gxB += neighborColor.B * kernelX[k + radiusX, l + radiusY];
                        gyB += neighborColor.B * kernelY[k + radiusX, l + radiusY];
                    }
                    lastColor = neighborColor;

                }
            if (!(gxR == 0 && gyR == 0)) resultR = Clamp((int)Math.Sqrt(gxR * gxR + gyR * gyR), 0, 255);
            if (!(gxG == 0 && gyG == 0)) resultG = Clamp((int)Math.Sqrt(gxG * gxG + gyG * gyG), 0, 255);
            if (!(gxB == 0 && gyB == 0)) resultB = Clamp((int)Math.Sqrt(gxB * gxB + gyB * gyB), 0, 255);

            return Color.FromArgb(resultR, resultG, resultB);
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            kernel = new float[3, 3] { { 0, -1, 0}, { -1, 5, -1}, { 0, -1, 0} };
        }
    }

    class EmbossingFilter : MatrixFilter
    {
        public EmbossingFilter()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        }
        public int ShiftAndNormalization(int value)
        {
            return ((value + 255) / 2);
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

                }
            resultR = ShiftAndNormalization((int)resultR);
            resultG = ShiftAndNormalization((int)resultG);
            resultB = ShiftAndNormalization((int)resultB);
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap grayImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            GrayScaleFilter filter = new GrayScaleFilter();
            for (int i = 0; i < sourceImage.Width; i++)
            {
               worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    grayImage.SetPixel(i, j, filter.calculateTmpPixelColor(sourceImage, i, j));
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(grayImage, i, j));
                }
            }

            return resultImage;
        }
    }

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter() 
        {
            kernel = new float[9, 9];
            for (int i = 0; i < kernel.GetLength(1); i++)
            {
                for (int j = 0; j < kernel.GetLength(0); j++)
                {
                    if (i == j) kernel[i, j] = 1.0f / kernel.GetLength(1);
                    else kernel[i, j] = 0;
                }
            }
        }
    }

    class DilationFilter : MatrixFilter
    {
        public DilationFilter() 
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }

        public void setKernel(float[,] currentKernel)
        {
            kernel = currentKernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            int maxIntensity = 0;
            int resR = 0, resG = 0, resB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    if (((k + radiusX) >= kernel.GetLength(0)) || ((k + radiusX) < 0)) continue;
                    if (((l + radiusY) >= kernel.GetLength(1)) || ((l + radiusY) < 0)) continue;

                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    int intensity = (int)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);

                    if ((kernel[k + radiusX, l + radiusY] != 0) && (intensity > maxIntensity))
                    {
                        maxIntensity = intensity;
                        resR = neighborColor.R;
                        resG = neighborColor.G; 
                        resB = neighborColor.B;
                    }
                }

            return Color.FromArgb(resR, resG, resB);
        }

        public Color calculateTmpPixelColor(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor((Bitmap)sourceImage, x, y);
        }
    }

    class ErosionFilter : MatrixFilter
    {
        public ErosionFilter()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }
        public void setKernel(float[,] currentKernel)
        {
            kernel = currentKernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            int minIntensity = 255;
            int resR = 255, resG = 255, resB = 255;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    if (((k + radiusX) >= kernel.GetLength(0)) || ((k + radiusX) < 0)) continue;
                    if (((l + radiusY) >= kernel.GetLength(1)) || ((l + radiusY) < 0)) continue;

                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    int intensity = (int)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);

                    if ((kernel[k + radiusX, l + radiusY] != 0) && (intensity < minIntensity))
                    {
                        minIntensity = intensity;
                        resR = neighborColor.R;
                        resG = neighborColor.G;
                        resB = neighborColor.B;
                    }
                }

            return Color.FromArgb(resR, resG, resB);
        }

        public Color calculateTmpPixelColor(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor((Bitmap)sourceImage, x, y);
        }
    }

    class OpeningFilter : MatrixFilter
    {
        public OpeningFilter()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }
        public void setKernel(float[,] currentKernel)
        {
            kernel = currentKernel;
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap erosImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            ErosionFilter erosionFilter = new ErosionFilter();
            DilationFilter dilationFilter = new DilationFilter();   
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    erosImage.SetPixel(i, j, erosionFilter.calculateTmpPixelColor(sourceImage, i, j));
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, dilationFilter.calculateTmpPixelColor(erosImage, i, j));
                }
            }

            return resultImage;
        }

        public Color calculateTmpPixelColor(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor((Bitmap)sourceImage, x, y);
        }
    }

    class ClosingFilter : MatrixFilter
    {
        public ClosingFilter()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }
        public void setKernel(float[,] currentKernel)
        {
            kernel = currentKernel;
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap dilImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            ErosionFilter erosionFilter = new ErosionFilter();
            DilationFilter dilationFilter = new DilationFilter();
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    dilImage.SetPixel(i, j, dilationFilter.calculateTmpPixelColor(sourceImage, i, j));
                }
            }

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, erosionFilter.calculateTmpPixelColor(dilImage, i, j));
                }
            }

            return resultImage;
        }

        public Color calculateTmpPixelColor(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor((Bitmap)sourceImage, x, y);
        }
    }

    class TopHatFilter : MatrixFilter
    {
        public TopHatFilter()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }
        public void setKernel(float[,] currentKernel)
        {
            kernel = currentKernel;
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap closeImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            ClosingFilter closeFilter = new ClosingFilter();

            closeImage = closeFilter.processImage(sourceImage, worker);
            
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress(50 + (int)((float)i / resultImage.Width * 50));
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    Color tmpColor = closeImage.GetPixel(i, j);
                    Color resColor = Color.FromArgb(Clamp(sourceColor.R - tmpColor.R, 0, 255), 
                                              Clamp(sourceColor.G - tmpColor.G, 0, 255),
                                              Clamp(sourceColor.B - tmpColor.B, 0, 255));
                    resultImage.SetPixel(i, j, resColor);     
                }
            }
            
            return resultImage;
        }

    }
    
    class MedianFilter : MatrixFilter
    {
        public MedianFilter()
        {
            kernel = new float[5, 5];
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            List<(int intensity, Color color)> neighborColors = new List<(int, Color)>();

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    int intensity = (int)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);

                    neighborColors.Add((intensity, neighborColor));
                }
            neighborColors.Sort((a, b) => a.intensity.CompareTo(b.intensity));

            int medianIndex = neighborColors.Count / 2;
            Color medianColor = neighborColors[medianIndex].color;

            return medianColor;
        }
    }
}
