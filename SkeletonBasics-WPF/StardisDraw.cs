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

        private DrawingContext ctx;

        private double ry = 0;

        private System.Windows.Media.Pen pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.White, 3);

        private bool inverty = true;

        public StardisDraw(DrawingContext dc)
        {
            this.ctx = dc;
        }



        int w = 640, h = 480;
        private double globalScale = 1.0;
        //var canvas = document.getElementById('foo');
        //canvas.width = w; canvas.height = h;
        //var ctx = canvas.getContext('2d');

        double fov = 250;


        double laserxoffset = -670;
        double laseryoffset = -470;
        double laserscale = 22;
        const int maxDistanceBetweenLaserPoints = 500;


        static Point3D[] triangle =
        {
            new Point3D(x: 0.0, y: -130.0, z: 0.0),
            new Point3D(x: -100.0, y: 130.0, z: -60.0),
            new Point3D(x: 100.0, y: 130.0, z: -60.0)
        };

        List<Point3D> scene = new List<Point3D>(triangle);
        private static DAC _laser;


        public double degToRad(double d)
        {
            return d*Math.PI/180.0;
        }

        public List<Point3D> calculate(double rx, double ry, double rz)
        {
            var p = new List<Point3D>();

            for (var i = 0; i < scene.Count; i++)
            {
                double x, y, z, tx, ty, tz;

                tx = scene[i].X;
                ty = scene[i].Y;
                tz = scene[i].Z;

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

                if (inverty)
                {
                    y = h - y;
                }

                p.Add( new Point3D(x: x, y: y, z: z));
            }

            return p;
        }


        public void ProcessFOV(Point3D[] points)
        {
            for (var i = 0; i < points.Length; i++)
            {
                var scale = fov / (fov + points[i].Z);
                var sx = (w / 2) + points[i].X * scale;
                var sy = (h / 2) + points[i].Y * scale;

                points[i].X = sx;
                points[i].X = sy;
            }
        }


        public void draw(Point3D[] points, double localScale)
        {
            
            // begin stroke
            PathFigure myPathFigure = new PathFigure();
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

            
            for (var i = 0; i < points.Length; i++)
            {
                var scale = fov/(fov + points[i].Z);
                var sx = (w/2) + points[i].X*scale*localScale*globalScale;
                var sy = (h/2) + points[i].Y*scale*localScale*globalScale;

                if (i == 0)
                {
                    //ctx.moveTo(sx, sy);
                    myPathFigure.StartPoint = new Point(sx,sy);
                }
                else
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = new Point((int) sx, (int) sy);
                    myPathSegmentCollection.Add(myLineSegment);
                }

                if (i == points.Length - 1)
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = myPathFigure.StartPoint;
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
            myPath.StrokeThickness = 3;
            myPath.Data = myPathGeometry;

            ctx.DrawGeometry(null, pen, myPathGeometry);


            // begin stroke
            myPathFigure = new PathFigure();
            myPathSegmentCollection = new PathSegmentCollection();



            for (var i = 0; i < points.Length; i++)
            {
                var scale = fov/(fov + points[i].Z);
                var sx = (w/2) + points[i].X*scale*localScale*globalScale;
                var sy = (h/2) + points[i].Y*scale*localScale*globalScale;

                if (i == 0)
                {
                    myPathFigure.StartPoint = new Point(sx, sy);
                    //ctx.moveTo(sx, sy);
                }
                else
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = new Point((int) sx, (int) sy);
                    myPathSegmentCollection.Add(myLineSegment);
                }

                if (i == points.Length - 1)
                {
                    LineSegment myLineSegment = new LineSegment();
                    myLineSegment.Point = myPathFigure.StartPoint;
                    myPathSegmentCollection.Add(myLineSegment);
                }
            }

            // end stroke
            myPathFigure.Segments = myPathSegmentCollection;

            myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;


            myPath = new Path()
            {
                Fill = null,
            };
            myPath.Stroke = System.Windows.Media.Brushes.White;
            myPath.StrokeThickness = 3;
            myPath.Data = myPathGeometry;

            ctx.DrawGeometry(null, pen, myPathGeometry);

        }

        // Loop
        public void DrawLoop(int yRotationIncrement)
        {

           
            double rx = 0, rz = 0;

            ctx.DrawRectangle(Brushes.Black, null, new Rect(0, 0, w, h));


            if (_laser == null)
            {
                _laser = DAC.Initialize(ControllerTypes.RiyaUSB, ControllerType.RiyaUSB);

            }

            CreateLaserFrame(calculate(degToRad(rx), degToRad(ry), degToRad(rz)));

            Thread.Sleep(10);

            var laserpoints = calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz)).ToArray();
            //ProcessFOV(laserpoints);
            //CreateLaserFrame(laserpoints.ToList());

            draw(calculate(degToRad(rx), degToRad(ry), degToRad(rz)).ToArray(), 1);
            //draw(calculate(degToRad(rx), degToRad(ry), degToRad(rz)), 1.1);
            //draw(calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz)), 1);
            //draw(calculate(degToRad(rx), degToRad(ry + 120), degToRad(rz)), 1.1);
            //draw(calculate(degToRad(rx), degToRad(ry + 240), degToRad(rz)), 1);
            //draw(calculate(degToRad(rx), degToRad(ry + 240), degToRad(rz)), 1.1);
            

            ry += yRotationIncrement;

     
        }

        private void CreateLaserFrame(List<Point3D> point3Ds)
        {
            point3Ds.Add(point3Ds.First());
            var points = convertPoint(point3Ds.ToArray());
            _laser.RenderDemoFrames(points);

        }

        LaserPoint[] convertPoint(Point3D[] points)
        {

            List<LaserPoint> newPoints = new List<LaserPoint>();


            var i = 0;

            while (i < points.Length)
            {



                var newPointX = ((int) points[i].X + laserxoffset)*laserscale;
                var newPointY = ((int) points[i].Y + laseryoffset)*laserscale;

                var newPoint = new LaserPoint(new System.Drawing.Point((int)newPointX, (int)newPointY), true);

                newPoints.Add(newPoint);

                if (i < (points.Length - 1))
                {
                    var nextPoint = points[i + 1];
                    var nextpointX = (nextPoint.X + laserxoffset) * laserscale;
                    var nextpointY = (nextPoint.Y + laseryoffset) * laserscale;

                    var distance = Math.Sqrt(Math.Pow(newPointX - nextpointX, 2) + Math.Pow(newPointY - nextpointY, 2));
                    if (distance > maxDistanceBetweenLaserPoints)
                    {
                        var numSegments = distance/ maxDistanceBetweenLaserPoints;
                        double prevSegmentEndX = newPointX;
                        double prevSegmentEndY = newPointY;
                        for (int j=0; j< numSegments; j++)
                        {
                            var calcPoint = CalculatePoint(new System.Windows.Point(prevSegmentEndX, prevSegmentEndY),
                                new System.Windows.Point(nextpointX, nextpointY), maxDistanceBetweenLaserPoints);

                            var newP = new LaserPoint(new System.Drawing.Point((int)calcPoint.X, (int)calcPoint.Y), true);
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

        private static Point CalculatePoint(Point a, Point b, int distance)
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
            return new Point((int)(a.X + vectorX), (int)(a.Y + vectorY));
        }
    }

}
