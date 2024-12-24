using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;


public class Shader
{
    /// <CreateShaderProgram>
    /// Creates and links the shader program using vertex and fragment shaders
    /// Returns the shader program after successful compilation and linking
    /// </CreateShaderProgram>
    public int CreateShaderProgram() { 

        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec3 aColor;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            out vec3 color;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPos, 1.0);
                color = aColor;
            }";

        string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;

            uniform vec3 objectColor; // Set a unique color for each object

            void main() {
                FragColor = vec4(objectColor, 1.0);
            }";

        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

        int shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);

        return shaderProgram;
    }






    /// <CompileShader>
    /// Compiles a shader of a specified type from the source code
    /// Logs any compilation errorss and returns the compiled shader program
    /// </CompileShader>
    private int CompileShader(ShaderType type, string source) {

        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        string infoLog = GL.GetShaderInfoLog(shader);
        if (!string.IsNullOrEmpty(infoLog)) {
            Console.WriteLine($"Error, compiling shader: {infoLog}");
        }

        return shader;
    }
}