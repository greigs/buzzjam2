﻿using System;
using System.Diagnostics;
using System.Linq;
using NAudio.Dsp; // The Complex and FFT are here!

class SampleAggregator
{
    // FFT
    public event EventHandler<FftEventArgs> FftCalculated;
    public bool PerformFFT { get; set; }

    // This Complex is NAudio's own! 
    private Complex[] fftBuffer;
    private FftEventArgs fftArgs;
    private int fftPos;
    private int fftLength;
    private int m;

    public SampleAggregator(int fftLength)
    {
        if (!IsPowerOfTwo(fftLength))
        {
            throw new ArgumentException("FFT Length must be a power of two");
        }
        this.m = (int)Math.Log(fftLength, 2.0);
        this.fftLength = fftLength;
        this.fftBuffer = new Complex[fftLength];
        this.fftArgs = new FftEventArgs(fftBuffer);
    }

    bool IsPowerOfTwo(int x)
    {
        return (x & (x - 1)) == 0;
    }

    public void Add(float value)
    {
        if (PerformFFT && FftCalculated != null)
        {
            // Remember the window function! There are many others as well.
            fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
            fftBuffer[fftPos].Y = 0; // This is always zero with audio.
            fftPos++;
            if (fftPos >= fftLength)
            {
                var average = fftBuffer.Average(x => Math.Abs(x.X));
                fftArgs.AverageVolume = average;
                fftPos = 0;
                FastFourierTransform.FFT(true, m, fftBuffer);
                FftCalculated(this, fftArgs);
            }
        }
    }
}

public class FftEventArgs : EventArgs
{
    [DebuggerStepThrough]
    public FftEventArgs(Complex[] result)
    {
        this.Result = result;
    }

    public float AverageVolume { get; set; }

    public Complex[] Result { get; private set; }
}