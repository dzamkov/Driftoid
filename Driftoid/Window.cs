using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// Main game window.
    /// </summary>
    public class Window : GameWindow
    {
        public Window() : base(640, 480, GraphicsMode.Default, "Driftoid")
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ColorMaterial);
            this._TestTex = Starfield.CreateStarfieldTexture(256, 20, 0.2f, 0.5f, 0.01f, Color.Gray, new System.Random());
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            
            GL.LoadIdentity();
            GL.Scale(0.5f, 0.5f, 1.0f);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(0f, 1f);
            GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
            GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(1f, 0f);
            GL.End();

            

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
        }

        private int _TestTex;
    }
}