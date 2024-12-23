using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class Geometry
{
    /// <summary>
    /// Creates a cube VAO
    /// </summary>
    public static (int vao, int vbo, int ebo) CreateCube() {
        float[] vertices = {
            // Front face
            -1.0f, -1.0f, -1.0f, 
            1.0f, -1.0f, -1.0f, 
            1.0f,  1.0f, -1.0f, 
            -1.0f,  1.0f, -1.0f, 
            // Back face 
            -1.0f, -1.0f,  1.0f, 
            1.0f, -1.0f,  1.0f, 
            1.0f,  1.0f,  1.0f, 
            -1.0f,  1.0f,  1.0f  
        };

        ushort[] indices = {
            0, 1, 2,  2, 3, 0,  
            4, 5, 6,  6, 7, 4,  
            0, 1, 5,  5, 4, 0,  
            2, 3, 7,  7, 6, 2,  
            0, 3, 7,  7, 4, 0,  
            1, 2, 6,  6, 5, 1   
        };

        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        int vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        int ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(ushort), indices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);

        return (vao, vbo, ebo);
    }






    /// <summary>
    /// Creates a ground VAO
    /// </summary>
    public static (int vao, int vbo, int ebo) CreateGround() {
        float[] groundVertices = {
            // Front face
            -15.0f, -8.0f, -15.0f,  // Vertex 0
            15.0f, -8.0f, -15.0f,   // Vertex 1
            15.0f,  -7.0f, -15.0f,   // Vertex 2
            -15.0f,  -7.0f, -15.0f,  // Vertex 3
            
            // Back face
            -15.0f, -8.0f,  15.0f,  // Vertex 4
            15.0f, -8.0f,  15.0f,   // Vertex 5
            15.0f,  -7.0f,  15.0f,   // Vertex 6
            -15.0f,  -7.0f,  15.0f   // Vertex 7
        };

        ushort[] groundIndices = {
            // Front face
            0, 1, 2,  2, 3, 0,  
            
            // Back face
            4, 5, 6,  6, 7, 4,  
            
            // Connecting the sides
            0, 1, 5,  5, 4, 0,  
            2, 3, 7,  7, 6, 2,  
            0, 3, 7,  7, 4, 0,  
            1, 2, 6,  6, 5, 1   
        };

        int groundVao = GL.GenVertexArray();
        GL.BindVertexArray(groundVao);

        int groundVbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, groundVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, groundVertices.Length * sizeof(float), groundVertices, BufferUsageHint.StaticDraw);

        int groundEbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, groundEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, groundIndices.Length * sizeof(ushort), groundIndices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);

        return (groundVao, groundVbo, groundEbo);
    }
}