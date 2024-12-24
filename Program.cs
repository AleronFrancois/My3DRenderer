using OpenTK;
using OpenTK.Graphics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

public class Renderer : GameWindow
{
    private struct CubeInstance
    {
        public Matrix4 ModelMatrix;
        public Vector3 Color;
        public int Vao;
    }

    private List<CubeInstance> cubes;
    private int shaderProgram;
    private int vao;

    private Matrix4 projection, view;

    private struct Camera
    {
        public Vector3 Position;
        public Vector3 Front;
        public Vector3 Up;
        public float Speed;
        public float Yaw;
        public float Pitch;
        public float Sensitivity;
    }

    private Camera camera;
    private float lastX, lastY;
    private bool firstMouse = true;

    public Renderer(GameWindowSettings windowSettings, NativeWindowSettings nativeWindowSettings) : base(windowSettings, nativeWindowSettings)
    {
        camera = new Camera
        {
            Position = new Vector3(0, 0, 5),
            Front = new Vector3(0, 0, -1),
            Up = new Vector3(0, 1, 0),
            Speed = 0.001f,
            Yaw = -90.0f,
            Pitch = 0.0f,
            Sensitivity = 0.05f
        };

        cubes = new List<CubeInstance>();
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);

        InitializeShaders();

        AddCube(new Vector3(1, 0, -5), new Vector3(1, 1, 1), new Vector3(0, 1, 0));
        AddCube(new Vector3(0, 0, -5), new Vector3(1, 1, 1), new Vector3(1, 0, 0));

        // Set window to grab mouse cursor
        this.CursorState = CursorState.Hidden;  // Hides the cursor
        this.CursorState = CursorState.Grabbed; // Grabs the cursor
    }

    private void InitializeGeometry()
    {
        var cube = Geometry.CreateCube();
        vao = cube.vao;
    }

    private void InitializeShaders()
    {
        Shader shader = new Shader();
        shaderProgram = shader.CreateShaderProgram();
        GL.UseProgram(shaderProgram);
    }

    private void AddCube(Vector3 position, Vector3 scale, Vector3 color)
    {
        var (vao, vbo, ebo) = Geometry.CreateCube(); // Create a new VAO for each cube
        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);
        cubes.Add(new CubeInstance { ModelMatrix = model, Color = color, Vao = vao });
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused) return;

        HandleKeyboardInput();
        HandleMouseInput();
        view = Matrix4.LookAt(camera.Position, camera.Position + camera.Front, camera.Up);
    }

    private void HandleMouseInput()
{
    var mouse = MouseState;
    if (firstMouse)
    {
        lastX = mouse.X;
        lastY = mouse.Y;
        firstMouse = false;
    }

    float offsetX = mouse.X - lastX;
    float offsetY = lastY - mouse.Y; // Reversed because y-coordinates go from bottom to top
    lastX = mouse.X;
    lastY = mouse.Y;

    offsetX *= camera.Sensitivity;
    offsetY *= camera.Sensitivity;

    camera.Yaw += offsetX;
    camera.Pitch += offsetY;

    // Clamping the pitch to prevent flipping
    if (camera.Pitch > 89.0f) camera.Pitch = 89.0f;
    if (camera.Pitch < -89.0f) camera.Pitch = -89.0f;

    // Updating the camera's front vector
    Vector3 front;
    front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
    front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(camera.Pitch));
    front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(camera.Yaw)) * Math.Cos(MathHelper.DegreesToRadians(camera.Pitch)));
    camera.Front = Vector3.Normalize(front);
    }

    private void HandleKeyboardInput()
    {
        var keyboard = KeyboardState;

        if (keyboard.IsKeyDown(Keys.W)) camera.Position += camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.S)) camera.Position -= camera.Speed * camera.Front;
        if (keyboard.IsKeyDown(Keys.A)) camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.D)) camera.Position += Vector3.Normalize(Vector3.Cross(camera.Front, camera.Up)) * camera.Speed;
        if (keyboard.IsKeyDown(Keys.Q)) camera.Position += camera.Speed * camera.Up;
        if (keyboard.IsKeyDown(Keys.E)) camera.Position -= camera.Speed * camera.Up;

        if (keyboard.IsKeyDown(Keys.Escape)) Close();

        // Toggle cursor visibility 
        if (keyboard.IsKeyDown(Keys.D1)) {
            if (this.CursorState == CursorState.Grabbed) {
                // If cursor is currently grabbed, ungrab and unhide it
                this.CursorState = CursorState.Normal;  // Unhides and ungrabs the cursor
            } else {
                // If cursor is currently not grabbed, grab it and hide it
                this.CursorState = CursorState.Hidden;  // Hides the cursor
                this.CursorState = CursorState.Grabbed; // Grabs the cursor
            }
        }
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        RenderScene();
        SwapBuffers();
    }

    private void RenderScene()
    {
        GL.UseProgram(shaderProgram);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);

        for (int i = 0; i < cubes.Count; i++)
        {
            var cube = cubes[i];  // Access the cube by reference
            GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "model"), false, ref cube.ModelMatrix);
            GL.Uniform3(GL.GetUniformLocation(shaderProgram, "objectColor"), cube.Color);
            GL.BindVertexArray(cube.Vao);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);
        }
    }

    public static void Main()
    {
        var windowSettings = new GameWindowSettings();
        var nativeWindowSettings = new NativeWindowSettings { Size = new Vector2i(1000, 600), Title = "OpenTK Renderer" };

        using (var renderer = new Renderer(windowSettings, nativeWindowSettings))
        {
            renderer.Run();
        }
    }
}