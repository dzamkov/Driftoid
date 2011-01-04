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
            this.VSync = VSyncMode.Off;
            this.WindowState = WindowState.Maximized;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ColorMaterial);

            this._Area = new Area();

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    this._Area.Spawn(
                        Driftoid.Make(
                            PrimitiveDriftoid.GetConstructor((PrimitiveType)y), 
                            new Vector((double)x * 3.0, (double)y * 3.0 - 7.5)) as LinkedDriftoid);
                }
            }

            this._Area.Spawn(
                Driftoid.Make(
                    NucleusDriftoid.GetConstructor(this._Player = new Player(Color.RGB(1.0, 0.0, 0.0))),
                    new Vector(-8.0, 0.0)) as LinkedDriftoid);
            this._Area.Spawn(
                Driftoid.Make(
                    WeightedDriftoid.GetConstructor(0),
                    new Vector(-8.0, 7.0)) as LinkedDriftoid);
            this._Starfield = Starfield.CreateDefault(512, 5);

            this._View = new View(new Vector(), 0.0, 0.1);


            this.Mouse.ButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (e.Button == MouseButton.Left)
                {
                    Vector pos = this.MouseWorldPosition;
                    this._Dragged = this._Area.Pick(pos);
                }
                if (e.Button == MouseButton.Right)
                {
                    Vector pos = this.MouseWorldPosition;
                    LinkedDriftoid ldr = this._Area.Pick(pos) as LinkedDriftoid;
                    if (ldr != null)
                    {
                        this._Area.TryDelink(this._Player, ldr);
                    }
                }
            };

            this.Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                // Test reaction
                if (e.Key == Key.F)
                {
                    Vector mousepos = this.MouseWorldPosition;
                    LinkedDriftoid ldr = this._Area.Pick(mousepos);
                    if (ldr != null)
                    {
                        if (ldr.ReactionClear && this._Area.HasLinkControl(this._Player, ldr))
                        {
                            //DriftoidConstructor product = Recipe.Master.GetProduct(new Structure(ldr));
                            DriftoidConstructor product = PrimitiveDriftoid.GetConstructor(PrimitiveType.Sulfur);
                            if (product != null)
                            {
                                Reaction r = new Reaction()
                                {
                                    Product = product,
                                    Target = ldr
                                };
                                this._Area.BeginReaction(r);
                            }
                        }
                    }
                }
            };
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

            Visual.SetupDraw();
            foreach (Visual v in this._Area.GetVisuals())
            {
                v.Draw();
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

            Vector mousepos = this.MouseWorldPosition;

            // Drift command
            if (this._Dragged != null)
            {
                if (this.Mouse[MouseButton.Left])
                {
                    this._Area.IssueCommand(this._Player, new DriftCommand()
                    {
                        TargetDriftoid = this._Dragged,
                        TargetPosition = mousepos
                    });
                }
                else
                {
                    this._Area.CancelDriftCommand(this._Player);
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
        private Driftoid _Dragged;
        private Area _Area;
    }
}