using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Driftoid
{
    /// <summary>
    /// Represents a transformation of a driftoid by absorbing others.
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// The driftoid to be transformed. The root in the reaction sequence.
        /// </summary>
        public LinkedDriftoid Target;

        /// <summary>
        /// A constructor for the product of the transformation.
        /// </summary>
        public DriftoidConstructor Product;

        /// <summary>
        /// The amount of time in seconds it takes to convert a normal reactant into a transitional state.
        /// </summary>
        public const double WarmupTime = 1.0;

        /// <summary>
        /// The amount of time in seconds it takes for a transitional state to be absorbed or transformed.
        /// </summary>
        public const double CooldownTime = 1.0;
    }

    /// <summary>
    /// The kind of a driftoid that is entering a transitional state.
    /// </summary>
    public class ReactionWarmupKind : Kind
    {
        public ReactionWarmupKind(Reaction Reaction, Kind Source)
        {
            this._Reaction = Reaction;
            this._Source = Source;
        }

        public override Kind Update(double Time)
        {
            this._Time += Time;
            this._Source.Update(Time);
            return this;
        }

        public override void Draw(Driftoid Driftoid)
        {
            SetupTextures();
            this._Source.Draw(Driftoid);
            GL.Color4(1.0, 1.0, 1.0, this._Time / Reaction.WarmupTime);
            Driftoid.DrawTexture(_InnerTextureID, 1.0, 0.0);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
        }

        public override double GetBorderWidth(Driftoid Driftoid)
        {
            return this._Source.GetBorderWidth(Driftoid);
        }

        public override bool AllowLink(int Index, LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        public override bool AllowLink(LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        public static void SetupTextures()
        {
            if (!_TexturesSetup)
            {
                _InnerTextureID = new Driftoid.SolidDrawer()
                {
                    BorderColor = Color.RGB(1.0, 1.0, 1.0),
                    BorderSize = 0.0,
                    InteriorColor = Color.RGB(1.0, 1.0, 1.0)
                }.CreateTexture(256);
                _TexturesSetup = true;
            }
        }

        private static bool _TexturesSetup;
        private static int _InnerTextureID;

        private double _Time;
        private Reaction _Reaction;
        private Kind _Source;
    }
}