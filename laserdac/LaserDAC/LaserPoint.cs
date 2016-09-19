using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using Point = System.Windows.Point;

namespace Laser
{
    /// <summary>
    /// Defines a point for a laser which is used in <see cref="DAC"/>.
    /// </summary>
    public class LaserPoint
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public Point Location { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color { get; set; }

        public bool Draw { get; set; }

        public LaserPoint(System.Windows.Point pPoint, Boolean visible)
        {
            this.Location = pPoint; 
            this.Color = Color.FromArgb(0, (visible ? 255 : 0), 0);
            this.Draw = visible;            
        }

        public LaserPoint(Point pPoint, Color color, Boolean visible)
        {
            this.Location = pPoint;
            this.Color = visible ? color : Color.FromArgb(0, 0, 0);
            this.Draw = visible;
        }

        public LaserPoint(LaserPoint punto)
        {
            this.Location = punto.Location;
            this.Color = punto.Draw ? punto.Color : Color.FromArgb(0, 0, 0);
            this.Draw = punto.Draw;
        }
    }
}
