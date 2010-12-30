using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A high-mass player-controlled driftoid that can apply force to other driftoids.
    /// </summary>
    public class NucleusDriftoid : LinkedDriftoid
    {
        public NucleusDriftoid(Player Player, DriftoidState MotionState)
            : base(new DriftoidConstructorInfo()
            {
                Mass = 20.0,
                Player = Player,
                MotionState = MotionState
            })
        {

        }

        public override int TextureID
        {
            get
            {
                int texid;
                if (!_Textures.TryGetValue(this.Player, out texid))
                {
                    _Textures[this.Player] = texid = _CreateTexture(this.Player);
                }
                return texid;
            }
        }

        private static int _CreateTexture(Player Player)
        {
            Color primary = Player.FadedColor;
            Color secondary = Player.SecondaryColor;
            int texsize = 256;
            float actualsize = (float)texsize;
            using (Bitmap bm = new Bitmap(texsize, texsize))
            {
                using (Graphics g = Graphics.FromImage(bm))
                {
                    Driftoid.DrawSolid(g, texsize, 0.1f, primary, secondary);

                    int dashamount = 9;
                    double dashdelta = Math.PI * 2.0 / (double)dashamount;
                    Vector center = new Vector(actualsize / 2.0f, actualsize / 2.0f);
                    using (Pen p = new Pen(primary, actualsize * 0.05f))
                    {
                        for (int t = 0; t < dashamount; t++)
                        {
                            double dashangle = dashdelta * (double)t;
                            Vector dashunit = Vector.Unit(dashangle) * (double)actualsize * 0.5;

                            g.DrawLine(p, dashunit * 0.3 + center, dashunit * 0.6 + center);
                        }
                    }
                }
                return Texture.Create(bm);
            }
        }

        private static readonly Dictionary<Player, int> _Textures = new Dictionary<Player, int>();

        /// <summary>
        /// Gets the maximum force magnitude the nucleus can apply.
        /// </summary>
        public double MaxForce
        {
            get
            {
                return 9.0;
            }
        }

        /// <summary>
        /// Gets the radius of the area of influence of this nucleus.
        /// </summary>
        public double MaxDistance
        {
            get
            {
                return 20.0;
            }
        }

        public override bool AllowLink(int Index, LinkedDriftoid PossibleChild)
        {
            return true;    
        }

        public override bool AllowChildLink(int Index, LinkedDriftoid PossibleChild, LinkedDriftoid Descendant)
        {
            return true;
        }

        internal void _Pull(double Time, Driftoid Target, Vector Position)
        {
            double dis = (Target.Position - this.Position).Length;
            if (dis < this.MaxDistance)
            {
                Vector vel = Target.MotionState.Velocity;
                double vellen = vel.Length;
                vel = vel * vellen;

                Vector gpos = Target.Position + vel * (Target.Mass / this.MaxForce);
                Vector vec = (Position - gpos);
                double mdis = vec.Length;
                Vector nvec = vec * (1.0 / mdis);
                vec = nvec * this.MaxForce;
                Target._ApplyForce(Time, vec);
                this._ApplyForce(Time, -vec);
            }
        }
    }

    /// <summary>
    /// A player-wide command that causes all the players nuclei to position (by applying force) another driftoid.
    /// </summary>
    public struct DriftCommand
    {
        /// <summary>
        /// The driftoid to be moved.
        /// </summary>
        public Driftoid TargetDriftoid;

        /// <summary>
        /// The position the driftoid should be moved to.
        /// </summary>
        public Vector TargetPosition;
    }
}