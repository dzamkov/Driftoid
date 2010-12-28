using OpenTK;
using OpenTK.Input;
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
            this._TestTex = Driftoid.CreateSolidTexture(256, 0.1f, Color.FromArgb(255, 200, 200), Color.FromArgb(255, 240, 240));
            this._Starfield = Starfield.CreateDefault(512, 5);   
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Starfield.Background);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            View v = new View(this._Pos, this._Rot, this._Zoom);
            double aspect = (double)this.Width / (double)this.Height;
            this._Starfield.Draw(v, aspect);
            v.Setup(aspect);
            Texture.Bind2D(this._TestTex);
            View.DrawTexturedSquare(new Vector(0.0, 0.0), 1.0);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            if (this.Keyboard[Key.Q]) this._Zoom *= 0.99;
            if (this.Keyboard[Key.E]) this._Zoom *= 1.01;
            if (this.Keyboard[Key.R]) this._Rot += updatetime;
            if (this.Keyboard[Key.T]) this._Rot -= updatetime;

            double movespeed = updatetime * 5.0;
            if (this.Keyboard[Key.W]) this._Pos.Y += movespeed;
            if (this.Keyboard[Key.S]) this._Pos.Y -= movespeed;
            if (this.Keyboard[Key.A]) this._Pos.X -= movespeed;
            if (this.Keyboard[Key.D]) this._Pos.X += movespeed;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private Starfield _Starfield;
        private Vector _Pos;
        private double _Zoom = 0.1;
        private double _Rot = 0.0;
        private int _TestTex;
    }
}