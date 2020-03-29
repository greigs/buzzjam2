﻿//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Laser;

namespace LaserDisplay
{
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Drawing group for rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        private static DAC _laser;
        public static LaserDraw _laserDraw;
        private static bool exiting;
        private DateTime frameDisplayTimestamp = DateTime.UtcNow;
        private int frameCount = 0;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            _laserDraw = new LaserDraw();
            InitializeComponent();
        }
        

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;


            //slider.Value = _laserDraw.laserxoffset;


            var audio = new AudioIn(_laserDraw);
            audio.Start();

            LaserPoint[] frameSegments = null;
            int frameSegmentIndex = 0;
            bool drawInSegments = false;
            while (Application.Current != null && !exiting)
            {
                var take = frameSegments == null ? 0 : 5;
                var newFrame = !drawInSegments || (frameSegments == null || frameSegmentIndex >= frameSegments.Length - take - 1);


                using (DrawingContext dc = newFrame? this.drawingGroup.Open() : this.drawingGroup.Append())
                {

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
                    {
                        _laserDraw.DrawingContext = dc;
                        if (newFrame)
                        {
                            // Update UI elements
                            
                            _laserDraw.UpdateFrame();
                            _laserDraw.UpdateAnimation();
                            _laserDraw.ClearFrame();
                            frameSegments = _laserDraw.GetFrameSegments();
                            take = !drawInSegments ? frameSegments.Length : take;
                            frameSegmentIndex = 0;
                        }

                        _laserDraw.DrawLaserSegments(frameSegments.ToArray());//.Skip(frameSegmentIndex).Take(take).ToArray());
                        frameSegmentIndex += take;

                        frameCount++;
                        if (DateTime.UtcNow.AddSeconds(-1) >  frameDisplayTimestamp)
                        {
                            frameDisplayTimestamp = DateTime.UtcNow;
                            Console.WriteLine(frameCount + "fps");
                            frameCount = 0;
                        }

                    })).Wait();
                }
            }
            audio.Stop();
        }
        


        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            exiting = true;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _laserDraw.laserxoffset = e.NewValue * -1;
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _laserDraw.laseryoffset = e.NewValue * -1;
        }
    }
}