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
            this.WindowState = WindowState.Maximized;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ColorMaterial);

            this._Area = new Area();
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Carbon, 0.0, 0.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Nitrogen, 3.0, 0.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Oxygen, 0.0, 3.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Hydrogen, 2.0, 4.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Iron, 0.0, -3.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Sulfur, -2.0, -4.0));
            this._Area.Spawn(new NucleusDriftoid(
                this._Player = new Player(Color.FromArgb(255, 255, 0)), new DriftoidState(new Vector(-4.0, 1.0))));
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

            Driftoid.SetupDraw();
            foreach (Driftoid d in this._Area.Driftoids)
            {
                d.Draw();
            }

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            this._Area.Update(updatetime);

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
        private Player _Player;
        private Area _Area;
    }
}