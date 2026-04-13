using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;

using OpenTK.Graphics.OpenGL;
namespace Zinovev_tomogram_visualizer
{
    class Bin
    {
        public static int X, Y, Z;
        public static short[] array;
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

        Color TransferFunction(short value)
        {
            int min = 0;
            int max = 2000;
            int newVal = Clamp((value - min) * 255 / (max - min), 0, 255);
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


    }
    internal class Tomogram
    {
    }
}
