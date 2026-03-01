using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace ZinovevLab1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
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
                    int idY = Clamp(y+k, 0,sourceImage.Height - 1);
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
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + k, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    gxR += neighborColor.R * kernelX[k + radiusX, l + radiusY];
                    gxG += neighborColor.G * kernelX[k + radiusX, l + radiusY];
                    gxB += neighborColor.B * kernelX[k + radiusX, l + radiusY];

                    gyR += neighborColor.R * kernelY[k + radiusX, l + radiusY];
                    gyG += neighborColor.G * kernelY[k + radiusX, l + radiusY];
                    gyB += neighborColor.B * kernelY[k + radiusX, l + radiusY];

                }
            int resultR = Clamp((int)Math.Sqrt(gxR * gxR + gyR * gyR), 0, 255);
            int resultG = Clamp((int)Math.Sqrt(gxG * gxG + gyG * gyG), 0, 255);
            int resultB = Clamp((int)Math.Sqrt(gxB * gxB + gyB * gyB), 0, 255);
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

}
