﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Laser;

namespace LaserDisplay
{
    public static class ScreenRenderer
    {


        private static System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 2);


        public static void DrawToScreen(LaserPoint[] points, double drawScale, double drawOffsetX, double drawOffsetY, DrawingContext dc)
        {

            // begin stroke
            PathFigure myPathFigure = new PathFigure();
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();



            for (var i = 0; i < points.Length; i++)
            {

                var sx = (points[i].Location.X * drawScale) + drawOffsetX;
                var sy = (points[i].Location.Y * drawScale) + drawOffsetY;

                if (i == 0)
                {
                    //DrawingContext.moveTo(sx, sy);
                    myPathFigure.StartPoint = new Point(sx, sy);
                }
                else
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = new Point(sx, sy);
                    myPathSegmentCollection.Add(myLineSegment);
                }

            }

            // end stroke
            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;


            Path myPath = new Path()
            {
                Fill = null
            };
            myPath.Stroke = System.Windows.Media.Brushes.White;
            myPath.Data = myPathGeometry;

            dc.DrawGeometry(null, pen, myPathGeometry);

        }
    }
}