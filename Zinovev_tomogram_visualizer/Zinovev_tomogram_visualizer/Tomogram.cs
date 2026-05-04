using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
namespace Zinovev_tomogram_visualizer
{
    class Bin
    {
        public static int X, Y, Z;
        public static short[] array;
        public int MAX;
        public Bin() { }

        public void readBIN(string path)
        {
            if(File.Exists(path))
            {
                BinaryReader reader = 
                    new BinaryReader(File.Open(path, FileMode.Open));

                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                Z = reader.ReadInt32();

                
                MAX = Z - 1;
                int arraySize = X * Y * Z;
                array = new short[arraySize];
                for (int i  = 0; i < arraySize; ++i)
                {
                    array[i] = reader.ReadInt16();
                }

            }
        }
    }

    class View
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public void SetupView(int width, int height)
        {
            GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Bin.X, 0, Bin.Y, -1, 1);
            GL.Viewport(0, 0, width, height);
        }
        public int min = 0;
        public int max = 2000;
        Color TransferFunction(short value)
        {
            if (max <= min) return Color.FromArgb(255, 0, 0, 0);
            int newVal = Clamp((value - min) * 255 / (max - min), 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }

        Color ConstTransferFunction(short value)
        {
            int newVal = Clamp(value * 255 / 2000, 0, 255);
            return Color.FromArgb(255, newVal, newVal, newVal);
        }

        
        public void DrawQuads(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Begin(BeginMode.Quads);
            for (int x_coords = 0; x_coords < Bin.X - 1; x_coords++)
                for (int y_coords = 0; y_coords < Bin.Y - 1; y_coords++)
                {
                    short value;
                    value = Bin.array[x_coords + y_coords * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coords, y_coords);

                    value = Bin.array[x_coords + (y_coords + 1) * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coords, y_coords + 1);

                    value = Bin.array[x_coords + 1 + (y_coords + 1) * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coords + 1, y_coords + 1);

                    value = Bin.array[x_coords + 1 + y_coords * Bin.X
                        + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x_coords + 1, y_coords);
                }
            GL.End();
        }

        public void DrawHalfTF(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            short value;
            for (int y = 0; y < Bin.Y - 1; y++)
            {
                GL.Begin(BeginMode.QuadStrip);

                for (int x = 0; x < (Bin.X/2); x++)
                {
                    value = Bin.array[x + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x, y);

                    value = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x, y + 1);
                }

                for (int x = Bin.X / 2; x < Bin.X; x++)
                {
                    value = Bin.array[x + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(ConstTransferFunction(value));
                    GL.Vertex2(x, y);

                    value = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(ConstTransferFunction(value));
                    GL.Vertex2(x, y + 1);
                }
                GL.End();
            }
        }
        public void DrawQuadStrip(int layerNumber)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            short value;
            for (int y = 0; y < Bin.Y - 1; y++)
            {
                GL.Begin(BeginMode.QuadStrip);

                for (int x = 0; x < Bin.X; x++)
                {
                    value = Bin.array[x + y * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x, y);
                    
                    value = Bin.array[x + (y + 1) * Bin.X + layerNumber * Bin.X * Bin.Y];
                    GL.Color3(TransferFunction(value));
                    GL.Vertex2(x, y + 1);
                }

                GL.End();
            }
        }

        

        Bitmap textureImage;
        int VBOtexture;
        public void Load2DTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);
            BitmapData data = textureImage.LockBits(
                new System.Drawing.Rectangle(0, 0, textureImage.Width, textureImage.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            textureImage.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            ErrorCode Er = GL.GetError();
            string str = Er.ToString();

        }

        public void generateTextureImage(int layerNumber)
        {
            textureImage = new Bitmap(Bin.X, Bin.Y);
            for (int i = 0; i < Bin.X; ++i)
                for (int j = 0; j < Bin.Y; ++j)
                {
                    int pixelNumber = i + j * Bin.X + layerNumber * Bin.X * Bin.Y;
                    textureImage.SetPixel(i, j, TransferFunction(Bin.array[pixelNumber]));
                }
        }

        public void DrawTexture()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, VBOtexture);

            GL.Begin(BeginMode.Quads);
            GL.Color3(Color.White);
            GL.TexCoord2(0f, 0f);
            GL.Vertex2(0, 0);
            GL.TexCoord2(0f, 1f);
            GL.Vertex2(0, Bin.Y);
            GL.TexCoord2(1f, 1f);
            GL.Vertex2(Bin.X, Bin.Y);
            GL.TexCoord2(1f, 0f);
            GL.Vertex2(Bin.X, 0);
            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }



    }
    internal class Tomogram
    {
    }
}
