using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZinovevLab33
{
    class View
    {
        int BasicProgramID;
        int BasicVertexShader;
        int BasicFragmentShader;
        int vbo_position;
        int attribute_vpos;
        int uniform_resolution;
        int uniform_depth;
        public int depth_value = 1;

        void loadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (System.IO.StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);
            Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public void InitShaders()
        {
            BasicProgramID = GL.CreateProgram();
            loadShader("..\\..\\raytracing.vert", ShaderType.VertexShader, BasicProgramID,
            out BasicVertexShader);
            loadShader("..\\..\\raytracing.frag", ShaderType.FragmentShader, BasicProgramID,
            out BasicFragmentShader);
            GL.LinkProgram(BasicProgramID);
            int status = 0;
            GL.GetProgram(BasicProgramID, GetProgramParameterName.LinkStatus, out status);
            Console.WriteLine(GL.GetProgramInfoLog(BasicProgramID));
            attribute_vpos = GL.GetAttribLocation(BasicProgramID, "vPosition");
            uniform_resolution = GL.GetUniformLocation(BasicProgramID, "uResolution");
            uniform_depth = GL.GetUniformLocation(BasicProgramID, "uDepth");

            Vector3[] vertdata = new Vector3[] {
            new Vector3(-1f, -1f, 0f),
            new Vector3( 1f, -1f, 0f),
            new Vector3( 1f,  1f, 0f),
            new Vector3(-1f,  1f, 0f) };

            GL.GenBuffers(1, out vbo_position);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length *
            Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(attribute_vpos);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);
            
        }

        public void Draw()
        {
            GL.UseProgram(BasicProgramID);
            GL.Uniform1(uniform_depth, depth_value);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void UpdateResolution(int width, int height)
        {
            GL.UseProgram(BasicProgramID);
            GL.Uniform2(uniform_resolution, (float)width, (float)height);  
            GL.UseProgram(0);
        }

    }
    internal class RayTracing
    {
    }
}