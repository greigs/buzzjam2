using System;
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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;


namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    class StardisDraw
    {

        public DrawingContext DrawingContext { get; set; }


        private double ry = 0;
        double rx = 0, rz = 0;

        private System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 2);


        private bool processFOV = true;

        private bool enableLaser = true;



        double w = 640.0, h = 480.0;
        private double globalScale = 0.1;
        //var canvas = document.getElementById('foo');
        //canvas.width = w; canvas.height = h;
        //var DrawingContext = canvas.getContext('2d');

        double fov = 340;

        private double drawScale = 0.15;
        private double drawOffsetX = 0;
        private double drawOffsetY = 500;

        public static double laserxoffset = 2000;
        public static double laseryoffset = 0;
        double laserscale = 55;
        const int maxDistanceBetweenLaserPoints = 1500;


        static Point3D[] triangle =
        {
            new Point3D(x: 0.0, y: -100.0, z: 0.0),
            new Point3D(x: -100.0, y: 100.0, z: -60.0),
            new Point3D(x: 100.0, y: 100.0, z: -60.0)
        };

        List<Point3D> scene = new List<Point3D>(triangle);
        private static DAC _laser;


        public double degToRad(double d)
        {
            return d*Math.PI/180.0;
        }

        public List<Point3D> calculate(List<Point3D> points, double rx, double ry, double rz, bool processFOV, double localScale)
        {
            var p = new List<Point3D>();

            for (var i = 0; i < scene.Count; i++)
            {
                double x, y, z, tx, ty, tz;

                tx = points[i].X;
                ty = points[i].Y;
                tz = points[i].Z;


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



        public void draw(LaserPoint[] points)
        {
            
            // begin stroke
            PathFigure myPathFigure = new PathFigure();
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();



            for (var i = 0; i < points.Length; i++)
            {

                var sx = (points[i].Location.X * globalScale * drawScale) + drawOffsetX;
                var sy = (points[i].Location.Y * globalScale * drawScale) + drawOffsetY;

                if (i == 0)
                {
                    //DrawingContext.moveTo(sx, sy);
                    myPathFigure.StartPoint = new Point(sx,sy);
                }
                else
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = new Point( sx,  sy);
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

            DrawingContext.DrawGeometry(null, pen, myPathGeometry);

        }

        // Loop
        public void DrawLoop()
        {

            double yRotationIncrement = 1.0;
            double xRotationIncrement = 0.1;
            double zRotationIncrement = 0.2;

            DrawingContext.DrawRectangle(Brushes.Black, null, new Rect(0, 0, w, h));


            if (_laser == null && enableLaser)
            {
               _laser = DAC.Initialize(ControllerTypes.RiyaUSB, ControllerType.RiyaUSB);
            }
 
            Thread.Sleep(5);

            //var laserpoints = calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz), processFOV,1).ToArray();
            //var frame = CreateLaserFrame(laserpoints.ToList());
            //if (enableLaser)
            //{
            //    _laser.RenderFrames(frame);
            //}

            /*
                        draw(calculate(degToRad(rx), degToRad(ry), degToRad(rz), processFOV, 1).ToArray());
                        draw(calculate(degToRad(rx), degToRad(ry), degToRad(rz), processFOV, 1.1).ToArray());
                        draw(calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz), processFOV, 1).ToArray());
                        draw(calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz), processFOV, 1.1).ToArray());
                        draw(calculate(degToRad(rx), degToRad(ry + 240), degToRad(rz), processFOV, 1).ToArray());
                        draw(calculate(degToRad(rx), degToRad(ry + 240), degToRad(rz), processFOV, 1.1).ToArray());*/

            List<Point3D> points = new List<Point3D>();


            for (double d = 1.0; d < 1.5; d += 0.1)
            {
                Combine(points, calculate(scene, degToRad(rx), degToRad(ry), degToRad(rz), processFOV, d));
                Combine(points, calculate(scene, degToRad(rx), degToRad(ry + 120.0), degToRad(rz), processFOV, d));
                Combine(points, calculate(scene, degToRad(rx), degToRad(ry + 240.0), degToRad(rz), processFOV, d));
            }
            var allpoints = points;

            var laserFrames = CreateLaserFrame(allpoints);

            if (enableLaser)
            {
                _laser.RenderFrames(laserFrames);
            }

            draw(laserFrames);
            
            ry += yRotationIncrement;

            // these are crazy
            rx += xRotationIncrement;
            rz += zRotationIncrement;

        }

        private void Combine(List<Point3D> points, List<Point3D> point3Ds)
        {
            points.AddRange(point3Ds);
        }

        private LaserPoint[] CreateLaserFrame(List<Point3D> point3Ds)
        {
            point3Ds.Add(point3Ds.First());
            var points = convertPoints(point3Ds.ToArray());
            return points;


        }

        LaserPoint[] convertPoints(Point3D[] points)
        {

            List<LaserPoint> newPoints = new List<LaserPoint>();


            var i = 0;

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
                            var calcPoint = CalculatePoint(new Point(prevSegmentEndX, prevSegmentEndY),
                                new Point(nextpointX, nextpointY), maxDistanceBetweenLaserPoints);

                            var newP = new LaserPoint(new Point(calcPoint.X, calcPoint.Y), true);
                            newPoints.Add(newP);

                            prevSegmentEndX = calcPoint.X;
                            prevSegmentEndY = calcPoint.Y;
                        }
                    }
                }
                i++;
            }

            return newPoints.ToArray();

        }

        private static Point CalculatePoint(Point a, Point b, double distance)
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

}
