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
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Carbon, 3.0, 0.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Oxygen, 0.0, 3.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Hydrogen, 2.0, 4.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Hydrogen, 0.0, -3.0));
            this._Area.Spawn(PrimitiveDriftoid.QuickCreate(PrimitiveDriftoidType.Hydrogen, -2.0, -4.0));
            this._Area.Spawn(new NucleusDriftoid(
                this._Player = new Player(Color.FromArgb(255, 0, 0)), new DriftoidState(new Vector(-4.0, 1.0))));
            this._Starfield = Starfield.CreateDefault(512, 5);

            this._View = new View(new Vector(), 0.0, 0.1);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Starfield.Background);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


            double aspect = this.AspectRatio;
            this._Starfield.Draw(this._View, aspect);
            this._View.Setup(aspect);

            Driftoid.SetupDraw();
            foreach (Driftoid d in this._Area.Driftoids)
            {
                d.Draw();
            }

            this.SwapBuffers();
        }

        /// <summary>
        /// Gets the aspect ration of the window.
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return (double)this.Width / (double)this.Height;
            }
        }

        /// <summary>
        /// Gets the position of the mouse in the world.
        /// </summary>
        public Vector MouseWorldPosition
        {
            get
            {
                Vector mousepos = new Vector(
                (double)this.Mouse.X / (double)this.Width,
                (double)this.Mouse.Y / (double)this.Height);
                return this._View.ToWorld(this.AspectRatio, mousepos);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;

            if (Mouse[MouseButton.Left])
            {
                Vector mousepos = this.MouseWorldPosition;
                if (!this._MouseLastState)
                {
                    // Pick a driftoid to move
                    this._Dragged = this._Area.Pick(mousepos);
                    this._MouseLastState = true;
                }

                // Drift command
                if (this._Dragged != null)
                {
                    this._Area.IssueCommand(this._Player, new DriftCommand()
                    {
                        TargetDriftoid = this._Dragged,
                        TargetPosition = mousepos
                    });
                }
            }
            else
            {
                if (this._MouseLastState)
                {
                    // Stop
                    this._Area.CancelDriftCommand(this._Player);
                    this._MouseLastState = false;
                }
            }

            this._Area.Update(updatetime);

            if (this.Keyboard[Key.Q]) this._View.Zoom *= 0.99;
            if (this.Keyboard[Key.E]) this._View.Zoom *= 1.01;
            if (this.Keyboard[Key.R]) this._View.Angle += updatetime;
            if (this.Keyboard[Key.T]) this._View.Angle -= updatetime;

            double movespeed = updatetime * 5.0;
            if (this.Keyboard[Key.W]) this._View.Center.Y += movespeed;
            if (this.Keyboard[Key.S]) this._View.Center.Y -= movespeed;
            if (this.Keyboard[Key.A]) this._View.Center.X -= movespeed;
            if (this.Keyboard[Key.D]) this._View.Center.X += movespeed;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }


        private Starfield _Starfield;
        private View _View;
        private Player _Player;
        private bool _MouseLastState;
        private Driftoid _Dragged;
        private Area _Area;
    }
}