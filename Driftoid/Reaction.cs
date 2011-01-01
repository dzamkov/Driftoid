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
        /// The amount of time in seconds it takes for a transitional state to be absorbed or transformed.
        /// </summary>
        public const double CooldownTime = 0.2;
    }

    /// <summary>
    /// The kind of a driftoid that is entering a transitional state.
    /// </summary>
    public class ReactionWarmupKind : Kind
    {
        public ReactionWarmupKind(double ReactionTime, DriftoidConstructor Product, Kind Source)
        {
            this._Product = Product;
            this._Source = Source;
            this._FinishTime = ReactionTime;
            this._Wait = true;
            this._Aborted = false;
        }

        public override double LinkVisibility
        {
            get
            {
                return 1.0 - this._Time / this._FinishTime;
            }
        }

        public override Kind OnUpdate(LinkedDriftoid Driftoid, double Time, IDriftoidInterface Interface)
        {
            if (this._Wait)
            {
                if (this._SafeAborted)
                {
                    // Safe abort, nothing is lost
                    return this._Source;
                }

                bool ready = true;
                foreach (LinkedDriftoid child in Driftoid.LinkedChildren)
                {
                    if (!(child.Kind is ReactionCooldownKind))
                    {
                        ready = false;
                    }
                }
                if (ready)
                {
                    foreach (LinkedDriftoid child in Driftoid.LinkedChildren)
                    {
                        ((ReactionCooldownKind)child.Kind)._Wait = false;
                    }
                    this._Wait = false;
                }
            }
            else
            {
                this._Time += Time;
                if (this._Time > this._FinishTime)
                {
                    if (this._Product == null)
                    {
                        ReactionCooldownKind rck = new ReactionCooldownKind();

                        // If the reaction can't continue, this driftoid is absorbed with no gain.
                        if (this._Aborted)
                        {
                            rck._Wait = false;
                        }

                        return rck;
                    }
                    else
                    {
                        return new ReactionTransformKind(this._Product);
                    }
                }
            }
            this._Source = this._Source.OnUpdate(Driftoid, Time, Interface);
            return this;
        }

        public override void Draw(Driftoid Driftoid)
        {
            SetupTextures();
            this._Source.Draw(Driftoid);
            GL.Color4(1.0, 1.0, 1.0, this._Time / this._FinishTime);
            Driftoid.DrawTexture(_WhiteTextureID, 1.0, 0.0);
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

        public override Kind OnParentDelink(LinkedDriftoid Driftoid, LinkedDriftoid Parent)
        {
            if (this._Product == null)
            {
                this._Aborted = true;
            }
            return this;
        }

        public override Kind OnChildDelink(LinkedDriftoid Driftoid, LinkedDriftoid Child)
        {
            if (this._Wait)
            {
                this._SafeAborted = true;
                LinkedDriftoid cur = Driftoid.LinkedParent;
                ReactionWarmupKind rwk;
                while (cur != null && (rwk = cur.Kind as ReactionWarmupKind) != null)
                {
                    rwk._SafeAborted = true;
                    cur = cur.LinkedParent;
                }
            }
            return this;
        }

        public static void SetupTextures()
        {
            if (!_TexturesSetup)
            {
                _WhiteTextureID = new Driftoid.SolidDrawer()
                {
                    BorderColor = Color.RGB(1.0, 1.0, 1.0),
                    BorderSize = 0.0,
                    InteriorColor = Color.RGB(1.0, 1.0, 1.0)
                }.CreateTexture(256);
                _TexturesSetup = true;
            }
        }

        private static bool _TexturesSetup;
        internal static int _WhiteTextureID;

        private bool _SafeAborted;
        private bool _Aborted;
        private bool _Wait;
        private double _Time;
        private double _FinishTime;
        private DriftoidConstructor _Product;
        private Kind _Source;
    }

    /// <summary>
    /// The kind of a driftoid as it disappears (from absorbtion? or something like that).
    /// </summary>
    public class ReactionCooldownKind : Kind
    {
        public ReactionCooldownKind()
        {
            this._Wait = true;
        }

        public override double LinkVisibility
        {
            get
            {
                return 0.0;
            }
        }

        public override void Draw(Driftoid Driftoid)
        {
            GL.Color4(1.0, 1.0, 1.0, 1.0 - this._Time / Reaction.CooldownTime);
            Driftoid.DrawTexture(ReactionWarmupKind._WhiteTextureID, 1.0, 0.0);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
        }

        public override Kind OnUpdate(LinkedDriftoid Driftoid, double Time, IDriftoidInterface Interface)
        {
            if (!this._Wait)
            {
                this._Time += Time;
            }
            if (this._Time > Reaction.CooldownTime)
            {
                Interface.Delete();
            }
            return this;
        }

        public override bool AllowLink(int Index, LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        public override bool AllowLink(LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        internal bool _Wait;
        private double _Time;
    }

    /// <summary>
    /// A kind for a driftoid that is changing from a transitional state into a concrete driftoid.
    /// </summary>
    public class ReactionTransformKind : Kind
    {
        public ReactionTransformKind(DriftoidConstructor Product)
        {
            this._Product = Product;
        }

        public override double LinkVisibility
        {
            get
            {
                return this._Time / Reaction.CooldownTime;
            }
        }

        public override void Draw(Driftoid Driftoid)
        {
            if (this._Final == null)
            {
                Driftoid.DrawTexture(ReactionWarmupKind._WhiteTextureID, 1.0, 0.0);
            }
            else
            {
                this._Final.Draw(Driftoid);
                GL.Color4(1.0, 1.0, 1.0, 1.0 - this._Time / Reaction.CooldownTime);
                Driftoid.DrawTexture(ReactionWarmupKind._WhiteTextureID, 1.0, 0.0);
                GL.Color4(1.0, 1.0, 1.0, 1.0);
            }
        }

        public override Kind OnUpdate(LinkedDriftoid Driftoid, double Time, IDriftoidInterface Interface)
        {
            if (this._Final == null)
            {
                double massadjustrate = 100.0 * Time;
                double radiusadjustrate = 1.0 * Time;
                double curmass = Driftoid.Mass;
                double currad = Driftoid.Radius;
                if (curmass > this._Product.Mass) curmass -= massadjustrate;
                if (curmass < this._Product.Mass) curmass += massadjustrate;
                if (currad > this._Product.Radius) currad -= radiusadjustrate;
                if (currad < this._Product.Radius) currad += radiusadjustrate;
                bool ready = true;
                if (Math.Abs(curmass - this._Product.Mass) <= massadjustrate)
                {
                    curmass = this._Product.Mass;
                }
                else
                {
                    ready = false;
                }
                if (Math.Abs(currad - this._Product.Radius) <= radiusadjustrate)
                {
                    currad = this._Product.Radius;
                }
                else
                {
                    ready = false;
                }
                Interface.ChangeMass(curmass);
                Interface.ChangeRadius(currad);
                if (ready)
                {
                    this._Final = this._Product.Kind;
                }
            }
            else
            {
                this._Final.OnUpdate(Driftoid, Time, Interface);
                this._Time += Time;
                if (this._Time > Reaction.CooldownTime)
                {
                    return this._Final;
                }
            }
            return this;
        }

        public override bool AllowLink(int Index, LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        public override bool AllowLink(LinkedDriftoid This, LinkedDriftoid Other)
        {
            return false;
        }

        private Kind _Final;
        private DriftoidConstructor _Product;
        private double _Time;
    }
}