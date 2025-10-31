using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.GLNS;

public sealed class Shader : IDisposable
{
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath)
    {
        string vs = File.ReadAllText(vertexPath);
        string fs = File.ReadAllText(fragmentPath);

        int v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, vs);
        GL.CompileShader(v);
        GL.GetShader(v, ShaderParameter.CompileStatus, out int vStatus);
        if (vStatus == 0) throw new Exception($"Vertex compile error: {GL.GetShaderInfoLog(v)}");

        int f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, fs);
        GL.CompileShader(f);
        GL.GetShader(f, ShaderParameter.CompileStatus, out int fStatus);
        if (fStatus == 0) throw new Exception($"Fragment compile error: {GL.GetShaderInfoLog(f)}");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, v);
        GL.AttachShader(Handle, f);
        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus == 0) throw new Exception($"Program link error: {GL.GetProgramInfoLog(Handle)}");

        GL.DetachShader(Handle, v); GL.DetachShader(Handle, f);
        GL.DeleteShader(v); GL.DeleteShader(f);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetInt(string name, int value) => GL.Uniform1(GetLoc(name), value);
    public void SetFloat(string name, float value) => GL.Uniform1(GetLoc(name), value);
    public void SetVector3(string name, Vector3 v) => GL.Uniform3(GetLoc(name), v);
    public void SetMatrix4(string name, Matrix4 m)
    {
        GL.UniformMatrix4(GetLoc(name), false, ref m);
    }

    private int GetLoc(string name)
    {
        int loc = GL.GetUniformLocation(Handle, name);
        if (loc == -1) throw new Exception($"Uniform '{name}' not found.");
        return loc;
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
    }
}