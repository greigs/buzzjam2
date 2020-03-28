using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace LaserDisplay
{
    class AudioIn
    {
        private readonly LaserDraw _laserDraw;

        // Other inputs are also usable. Just look through the NAudio library.
        private IWaveIn waveIn;
        private static int fftLength = 64; // NAudio fft wants powers of two!

        // There might be a sample aggregator in NAudio somewhere but I made a variation for my needs
        private readonly SampleAggregator _sampleAggregator = new SampleAggregator(fftLength);

        Queue<float> vals = new Queue<float>(5);

        public AudioIn(LaserDraw laserDraw)
        {
            _laserDraw = laserDraw;
            
        }

        public void Start()
        {
            _sampleAggregator.FftCalculated += new EventHandler<FftEventArgs>(FftCalculated);
            _sampleAggregator.PerformFFT = true;

            // Here you decide what you want to use as the waveIn.
            // There are many options in NAudio and you can use other streams/files.
            // Note that the code varies for each different source.
            waveIn = new WasapiCapture();

            waveIn.DataAvailable += OnDataAvailable;

            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveIn.StopRecording();
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (false) //this.InvokeRequired)
            {
                //this.BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                byte[] buffer = e.Buffer;
                int bytesRecorded = e.BytesRecorded;
                int bufferIncrement = waveIn.WaveFormat.BlockAlign;

                for (int index = 0; index < bytesRecorded; index += bufferIncrement)
                {
                    float sample32 = BitConverter.ToSingle(buffer, index);
                    _sampleAggregator.Add(sample32);
                }   
            }
        }

        void FftCalculated(object sender, FftEventArgs e)
        {
            var max = e.Result.Skip(0).Take(150).Average(x =>Math.Abs(x.X)  ) * 100;
            vals.Enqueue(max);
            if (vals.Count > 10)
            {
                vals.Dequeue();
            }
            var av = vals.Average();


            //var maxVal = (max * 100);
            //if (maxVal < 0.4f)
            //{
            //    maxVal = 0.4f;
            //}
            //else
            //{
            //    maxVal = 0.4f + (maxVal * 0.08f);
            //}


            //if (maxVal > 0.7f)
            //{
            //    maxVal = 0.7f;
            //}

            _laserDraw.audioMaxVal = av * 10f;
        }
    }
}
