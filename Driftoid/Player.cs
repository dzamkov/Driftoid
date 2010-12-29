using System;
using System.Collections.Generic;
using System.Drawing;

namespace Driftoid
{
    /// <summary>
    /// A player in a game of driftoids. Players are able to control driftoids of their own alliance. They may not
    /// directly affect the game.
    /// </summary>
    public class Player
    {
        public Player(Color Color)
        {
            this._Color = Color;
        }

        /// <summary>
        /// Gets the color associated with this player.
        /// </summary>
        public Color Color
        {
            get
            {
                return this._Color;
            }
        }

        /// <summary>
        /// Gets a color very slightly faded from the player color.
        /// </summary>
        public Color FadedColor
        {
            get
            {
                float r = (float)this._Color.R / 255.0f;
                float g = (float)this._Color.G / 255.0f;
                float b = (float)this._Color.B / 255.0f;
                r = r * 0.5f + 0.1f;
                g = g * 0.5f + 0.1f;
                b = b * 0.5f + 0.1f;
                return Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
            }
        }

        /// <summary>
        /// Gets a color complementary to the player color. This color is usually less intense, and distinct, to
        /// the player color.
        /// </summary>
        public Color SecondaryColor
        {
            get
            {
                float r = (float)this._Color.R / 255.0f;
                float g = (float)this._Color.G / 255.0f;
                float b = (float)this._Color.B / 255.0f;

                if (Math.Abs(0.5 - r) > 0.3 || Math.Abs(0.5 - g) > 0.3 || Math.Abs(0.5 - b) > 0.3)
                {
                    r = r * 0.3f + 0.4f;
                    g = g * 0.3f + 0.4f;
                    b = b * 0.3f + 0.4f;
                }
                else
                {
                    r += 0.2f;
                    g += 0.2f;
                    b += 0.2f;
                }

                return Color.FromArgb((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
            }
        }

        private Color _Color;
    }

}