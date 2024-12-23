using OpenTK; // Drives the program
using OpenTK.Graphics; // Handles graphics
using OpenTK.Windowing.Common;  // Handles window and input
using OpenTK.Windowing.Desktop; // Handles desktop windowing
using OpenTK.Graphics.OpenGL4;  // Handles OpenGL functions
using OpenTK.Mathematics;       // Handles vectors and matrices mathematics
using OpenTK.Windowing.GraphicsLibraryFramework; // Handles GLFW windowing and input handling
using System; // For console logs
using ImGuiNET; // For GUI

public class Renderer : GameWindow
{
    // Handles position and movement of the camera
    private Vector3 cameraPosition = new Vector3(0, 0, 5); // Camera position in 3D space
    private Vector3 cameraFront = new Vector3(0, 0, -1); // Direction the camera is facing
    private Vector3 cameraUp = new Vector3(0, 1, 0); // Y-axis of camera direction 
    private float cameraSpeed = 0.001f; // Camera speed value

    // Handles mouse movement 
    private float yaw = -90.0f;
    private float pitch = 0.0f;

    float sensitivity = 0.05f; // Mouse sensitivity

    // Center point of mouse relative to window size
    private float lastX;
    private float lastY;
    
    private bool firstMouse = true; // Checks for the first mouse movement

    // Projection and view matrices for camera transformations
    private Matrix4 projection;
    private Matrix4 view;

    // OpenGL objects for shader program and vertex array object (VAO)
    private int shaderProgram;
    private int vao;
    private int groundVao;

    // Constructor to initialise the window
    public Renderer(GameWindowSettings windowSettings, NativeWindowSettings nativeWindowSettings) : base(windowSettings, nativeWindowSettings) { }

    




    /// <OnLoad>
    /// Handles OpenGL projection matrices and geometry
    /// Prepares the shader program and enables mouse capture
    /// </OnLoad>
    protected override void OnLoad() {
        
        base.OnLoad();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f); // Background color
        GL.Enable(EnableCap.DepthTest); // Enables depth testing to correctly render 3D objects

        // Set up projection matrix for a perspective view
        projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), ClientSize.X / (float)ClientSize.Y, 0.1f, 100.0f);

        // Set the initial view matrix for the camera
        view = Matrix4.LookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);

        // Create cube geometry using Geometry.cs
        var cube = Geometry.CreateCube();
        vao = cube.vao; // Initialize the cube geometry

        // Create ground geometry using Geometry.cs
        var ground = Geometry.CreateGround();
        groundVao = ground.vao; // Initialize the ground geometry

        // Create shader program
        shaderProgram = CreateShaderProgram();
        GL.UseProgram(shaderProgram);

        // Set window to grab mouse cursor
        this.CursorVisible = false; // Hides the cursor
        this.CursorGrabbed = true; // Grabs the cursor

        // Finds the center of the screen relative to the resolution
        lastX = ClientSize.X / 2;
        lastY = ClientSize.Y / 2;
    }






    /// <OnUpdateFrame>
    /// Handles input updates for WASD movement
    /// Updates the view matrix based on the current camera position
    /// </OnUpdateFrame>
    protected override void OnUpdateFrame(FrameEventArgs e) {

        base.OnUpdateFrame(e);

        if (!IsFocused) return;

        var keyboard = KeyboardState;

        // Control camera using WASD keys
        if (keyboard.IsKeyDown(Keys.W)) 
            cameraPosition += cameraSpeed * cameraFront; // Move forward
        if (keyboard.IsKeyDown(Keys.S)) 
            cameraPosition -= cameraSpeed * cameraFront; // Move backward
        if (keyboard.IsKeyDown(Keys.A)) 
            cameraPosition -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed; // Move left
        if (keyboard.IsKeyDown(Keys.D)) 
            cameraPosition += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed; // Move right
        if (keyboard.IsKeyDown(Keys.Q)) 
            cameraPosition += cameraSpeed * cameraUp; // Move up
        if (keyboard.IsKeyDown(Keys.E))
            cameraPosition -= cameraSpeed * cameraUp; // Move down

        // Close window on hotkey
        if (keyboard.IsKeyDown(Keys.Escape)) {
            Close();
        }

        // Toggle cursor visibility and grabbing on 'R' key press
        if (keyboard.IsKeyDown(Keys.D1)) {
            if (this.CursorGrabbed) {
                // If cursor is currently grabbed, ungrab and unhide it
                this.CursorVisible = true;  // Unhide the cursor
                this.CursorGrabbed = false; // Ungrab the cursor
            } else {
                // If cursor is currently not grabbed, grab it and hide it
                this.CursorVisible = false; // Hide the cursor
                this.CursorGrabbed = true;  // Grab the cursor
            }
        }

        // Update the view matrix relative to camera position and direction
        view = Matrix4.LookAt(cameraPosition, cameraPosition + cameraFront, cameraUp);
    }






    /// <OnMouseMove>
    /// Process mouse movement to update the cameras yaw and pitch
    /// Prevents the camera from flipping with clamped pitch values
    /// </OnMouseMove>
    protected override void OnMouseMove(MouseMoveEventArgs e) {

        base.OnMouseMove(e);

        if (!IsFocused) return;

        if (firstMouse) {
            lastX = e.Position.X;
            lastY = e.Position.Y;
            firstMouse = false;
        }

        // Calculate mouse movement offsets
        float xOffset = e.Position.X - lastX;
        float yOffset = lastY - e.Position.Y; 
        lastX = e.Position.X;
        lastY = e.Position.Y;

        // Handles mouse sensitivity
        xOffset *= sensitivity;
        yOffset *= sensitivity;

        // Update yaw and pitch to avoid camera flipping
        pitch += yOffset;
        yaw += xOffset;
        pitch = Math.Clamp(pitch, -89.0f, 89.0f);

        // Calculate the new camera front vector based on yaw and pitch
        Vector3 front;
        front.X = (float)Math.Cos(MathHelper.DegreesToRadians(yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(pitch));
        front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = (float)Math.Sin(MathHelper.DegreesToRadians(yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(pitch));

        // Normalise the front vector
        cameraFront = Vector3.Normalize(front);
    }






    /// <OnRenderFrame>
    /// Renders the scene by clearing buffers and drawing objects
    /// Updates the shader program with the latest projection and view matrices
    /// </OnRenderFrame>
    protected override void OnRenderFrame(FrameEventArgs e) { 

        base.OnRenderFrame(e);

        // Clear the color and depth buffer
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        int colorLocation = GL.GetUniformLocation(shaderProgram, "objectColor");

        GL.UseProgram(shaderProgram);

        // Pass the projection and view matrices to the shader program
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shaderProgram, "view"), false, ref view);

        // Draw the cube
        GL.Uniform3(colorLocation, 0.0f, 0.0f, 1.0f); // Red color
        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);

        // Draw the ground
        GL.Uniform3(colorLocation, 0.0f, 1.0f, 0.0f); // Green color
        GL.BindVertexArray(groundVao);
        GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedShort, 0);

        // Swap buffers to display the frame
        SwapBuffers();
    }






    /// <CreateShaderProgram>
    /// Creates and links the shader program using vertex and fragment shaders
    /// Returns the shader program after successful compilation and linking
    /// </CreateShaderProgram>
    private int CreateShaderProgram() { 

        string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;

            uniform mat4 projection;
            uniform mat4 view;

            void main() {
                gl_Position = projection * view * vec4(aPos, 1.0);
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





    /// <Main>
    /// Entry point for the application that initialises and runs the renderer
    /// Configures window settings and starts the main loop
    /// </Main>
    public static void Main() {

        var windowSettings = new GameWindowSettings();
        var nativeWindowSettings = new NativeWindowSettings { 
            Size = new Vector2i(800, 560),
            Title = "OpenTK Renderer"
        };

        using (var renderer = new Renderer(windowSettings, nativeWindowSettings)) {
            renderer.Run();
        }
    }
}