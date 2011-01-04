using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;


namespace Driftoid
{
    /// <summary>
    /// Describes how and where to draw an object on the field. Contains functions for drawing.
    /// </summary>
    public abstract class Visual
    {
        /// <summary>
        /// Draws the visual to the current graphics context.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Prepares the current GL context for drawing. Should be called after setting up the view.
        /// </summary>
        public static void SetupDraw()
        {
            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Draws a texture relative to this driftoid.
        /// </summary>
        public static void DrawTexture(int ID, Vector Center, double Size, double Angle)
        {
            Texture.Bind2D(ID);
            GL.PushMatrix();
            GL.Translate(Center.X, Center.Y, 0.0);
            GL.Scale(Size, Size, 1.0);
            GL.Rotate(Angle, 0.0, 0.0, 1.0);
            GL.Begin(BeginMode.Quads);
            GL.Vertex2(-1.0f, -1.0f); GL.TexCoord2(0f, 0f);
            GL.Vertex2(-1.0f, 1.0f); GL.TexCoord2(1f, 0f);
            GL.Vertex2(1.0f, 1.0f); GL.TexCoord2(1f, 1f);
            GL.Vertex2(1.0f, -1.0f); GL.TexCoord2(0f, 1f);
            GL.End();
            GL.PopMatrix();
        }
    }

    /// <summary>
    /// A dynamicly changing visual.
    /// </summary>
    public abstract class Effect : Visual
    {
        /// <summary>
        /// Updates the state of the effect by the given time.
        /// </summary>
        public abstract void Update(double Time, IEffectInterface Interface);
    }

    /// <summary>
    /// An interface given to effects as they update.
    /// </summary>
    public interface IEffectInterface
    {
        /// <summary>
        /// Deletes the current effect.
        /// </summary>
        void Delete();

        /// <summary>
        /// Spawns a new effect.
        /// </summary>
        void Spawn(Effect Effect);
    }

    /// <summary>
    /// A simple visual that draws an oriented texture in the specified location.
    /// </summary>
    public class SimpleVisual : Visual
    {
        public SimpleVisual(int TextureID, Vector Center, double Size, double Angle)
        {
            this._ID = TextureID;
            this._Center = Center;
            this._Size = Size;
            this._Angle = Angle;
        }

        /// <summary>
        /// Copies for the visual from a driftoid.
        /// </summary>
        public SimpleVisual(int TextureID, Driftoid Driftoid)
        {
            this._ID = TextureID;
            this._Center = Driftoid.Position;
            this._Size = Driftoid.Radius;
            this._Angle = Driftoid.Angle;
        }

        public override void Draw()
        {
            Visual.DrawTexture(this._ID, this._Center, this._Size, this._Angle);
        }

        private int _ID;
        private Vector _Center;
        private double _Size;
        private double _Angle;
    }
}
