using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
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
            this._TestTex = Driftoid.CreateSolidTexture(256, 0.1f, Color.FromArgb(255, 100, 100), Color.FromArgb(255, 200, 200));
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            new View(new Vector(-1.0, -1.0), this._TestRot, Math.Sin(this._TestRot) + 1.2).Setup((double)this.Width / (double)this.Height);
            View.DrawTexturedSquare(new Vector(0.0, 0.0), 1.0);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            this._TestRot += updatetime;
        }

        private double _TestRot;
        private int _TestTex;
    }
}