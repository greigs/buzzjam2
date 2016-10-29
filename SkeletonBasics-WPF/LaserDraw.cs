﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Laser;
using NAudio.Dsp;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;


namespace LaserDisplay
{
    public class LaserDraw
    {

        public DrawingContext DrawingContext { get; set; }


        private double ry = 0;
        double rx = 0, rz = 0;

        private bool processFOV = true;

        private bool enableLaser = false;



        double w = 640.0, h = 480.0;
        double globalScale = 0.1;

        double fov = 340;

        double drawScale = 0.01;
        double drawOffsetX = 100;
        double drawOffsetY = 400;

        public double laserxoffset = 2000;
        public double laseryoffset = 0;
        double laserscale = 55;
        int maxDistanceBetweenLaserPoints = 1500;

        double yRotationIncrement = 1.0;
        double xRotationIncrement = 0.1;
        double zRotationIncrement = 0.2;


        
        private readonly List<MyShape> scene = new List<MyShape>();
        private static DAC _laser;
        public float audioMaxVal = 1.0f;


        public LaserDraw()
        {
            CreateScene();
        }

        public double degToRad(double d)
        {
            return d*Math.PI/180.0;
        }

        public List<Point3D> TranslateAndTransform(List<Point3D> points, double rx, double ry, double rz, bool processFOV, double localScale)
        {
            var p = new List<Point3D>();

            for (var i = 0; i < points.Count; i++)
            {
                double x, y, z, tx, ty, tz;

                tx = points[i].X * globalScale;
                ty = points[i].Y * globalScale;
                tz = points[i].Z * globalScale;


                //if (inverty)
                //{
                //    ty = h - ty;
                //}

                // rotate about z axis
                x = tx*Math.Cos(rz) - ty*Math.Sin(rz);
                y = ty*Math.Cos(rz) + tx*Math.Sin(rz);
                z = tz;
                tx = x;
                ty = y;
                tz = z;

                // rotate about x axis
                x = tx;
                y = ty*Math.Cos(rx) - tz*Math.Sin(rx);
                z = tz*Math.Cos(rx) + ty*Math.Sin(rx);
                tx = x;
                ty = y;
                tz = z;

                // rotate about y axis
                x = tx*Math.Cos(ry) - tz*Math.Sin(ry);
                y = ty;
                z = tz*Math.Cos(ry) + tx*Math.Sin(ry);
                tx = x;
                ty = y;
                tz = z;
                
                x = x*localScale;
                y = y*localScale;
                z = z*localScale;

                if (processFOV)
                {
                    var scale = fov/(fov + z);
                    x = (w / 2.0) + x*scale;
                    y = (h / 2.0) + y*scale;
                    //z = z* scale;
                }
                else
                {
                    x = x + (w / 2.0);
                    y = y + (h / 2.0);
                }

                p.Add( new Point3D(x: x, y: y, z: z));
            }

            return p;
        }

        public void CreateScene()
        {
            Point3D[] triangle1Points =
            {
                new Point3D(x: 0.0, y: -100.0, z: 0.0),
                new Point3D(x: -100.0, y: 100.0, z: -60.0),
                new Point3D(x: 100.0, y: 100.0, z: -60.0)
            };

            List<MyShape> shapes = new List<MyShape>();

            for (double scale = 1.0; scale < 1.5; scale += 0.1)
            {
                shapes.Add(new MyShape()
                {
                    Points = triangle1Points.ToList(),
                    Scale = scale
                });
            }
            scene.AddRange(shapes);
        }
        
        public void DrawFrame()
        {
            DrawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, w, h));

            if (_laser == null && enableLaser)
            {
               _laser = DAC.Initialize(ControllerTypes.RiyaUSB, ControllerType.RiyaUSB);
            }
 
            Thread.Sleep(5);
            
            List<Point3D> points = new List<Point3D>();
            
            globalScale = 1.0f;

            foreach (var shape in scene)
            {
                Combine(points, TranslateAndTransform(shape.Points, degToRad(rx), degToRad(ry), degToRad(rz), processFOV, shape.Scale));
                Combine(points, TranslateAndTransform(shape.Points, degToRad(rx), degToRad(ry + 120.0), degToRad(rz), processFOV, shape.Scale));
                Combine(points, TranslateAndTransform(shape.Points, degToRad(rx), degToRad(ry + 240.0), degToRad(rz), processFOV, shape.Scale));
            }
            
            var laserFrame = CreateLaserFrame(points);

            if (enableLaser)
            {
                _laser.RenderFrame(laserFrame);
            }

            ScreenRenderer.DrawToScreen(laserFrame,drawScale,drawOffsetX,drawOffsetY,DrawingContext);


        }

        public void UpdateAnimation()
        {

            ry += yRotationIncrement;

            // these are crazy
            //rx += xRotationIncrement;
            //rz += zRotationIncrement;
        }

        private void Combine(List<Point3D> points, List<Point3D> point3Ds)
        {
            points.AddRange(point3Ds);
        }

        private LaserPoint[] CreateLaserFrame(List<Point3D> point3Ds)
        {
            if (point3Ds.Any())
            {
                point3Ds.Add(point3Ds.First());
            }
            
            var points = ConvertToLaserPoints(point3Ds.ToArray());
            return points;


        }

        private LaserPoint[] ConvertToLaserPoints(Point3D[] points)
        {

            List<LaserPoint> newPoints = new List<LaserPoint>();


            var i = 0;
            Random r = new Random();
            while (i < points.Length)
            {
                
                var newPointX = (points[i].X + laserxoffset)*laserscale;
                var newPointY = (points[i].Y + laseryoffset)*laserscale;

                var newPoint = new LaserPoint(new System.Windows.Point(newPointX, newPointY), true);

                newPoints.Add(newPoint);


                if (i < (points.Length - 1))
                {
                    var nextPoint = points[i + 1];
                    var nextpointX = (nextPoint.X + laserxoffset) * laserscale;
                    var nextpointY = (nextPoint.Y + laseryoffset) * laserscale;

                    var distance = Math.Sqrt(Math.Pow(newPointX - nextpointX, 2) + Math.Pow(newPointY - nextpointY, 2));
                    if (distance > maxDistanceBetweenLaserPoints)
                    {
                        var numSegments = (distance/ maxDistanceBetweenLaserPoints) - 1;
                        double prevSegmentEndX = newPointX;
                        double prevSegmentEndY = newPointY;
                        for (int j=0; j< numSegments; j++)
                        {
                            var calcPoint = CreateInterpolatedPoint(new Point(prevSegmentEndX, prevSegmentEndY),
                                new Point(nextpointX, nextpointY), maxDistanceBetweenLaserPoints);
                            

                            // from the new point, calculate the vector back to the old point.
                            //var v = new Vector(calcPoint.X, calcPoint.Y);
                            //var v2 = new Vector(prevSegmentEndX, prevSegmentEndY);
                            //var v3 = v + v2;
                            //Vector perpVector;

                            //if (r.Next(0,1) > 0)
                            //{
                            //    perpVector = PerpendicularClockwise(v3);
                            //}
                            //else
                            //{
                            //    perpVector = PerpendicularCounterClockwise(v3);
                            //}
                            

                            //var calcPoint2 =  CreateInterpolatedPoint(new Point(v.X, v.Y),
                            //    new Point(perpVector.X, perpVector.Y), 5000);

                            //var newP = new LaserPoint(new Point(calcPoint2.X, calcPoint2.Y), true);                            

                            //var newP2 = new LaserPoint(new Point(calcPoint.X, calcPoint.Y), true);
                           
                            //newPoints.Add(newP2);

                            //newPoints.Add(newP);

                            prevSegmentEndX = calcPoint.X;
                            prevSegmentEndY = calcPoint.Y;

                        }
                    }
                }
                i++;
            }

            return newPoints.ToArray();

        }

        public static Vector PerpendicularClockwise(Vector vector)
        {
            return new Vector(-vector.Y, vector.X);
        }

        public static Vector PerpendicularCounterClockwise(Vector vector)
        {
            return new Vector(vector.Y, -vector.X);
        }

        private static Point CreateInterpolatedPoint(Point a, Point b, double distance)
        {

            // a. calculate the vector from o to g:
            double vectorX = b.X - a.X;
            double vectorY = b.Y - a.Y;

            // b. calculate the proportion of hypotenuse
            double factor = distance / Math.Sqrt(vectorX * vectorX + vectorY * vectorY);

            // c. factor the lengths
            vectorX *= factor;
            vectorY *= factor;

            // d. calculate and Draw the new vector,
            return new System.Windows.Point((a.X + vectorX), (a.Y + vectorY));
        }
    }

    public class MyShape
    {
     
        public List<Point3D> Points { get; set; }
        public double Scale { get; set; } = 1.0;
    }
}
