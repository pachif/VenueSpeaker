using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;
using System.ComponentModel;

namespace SoundLibrary
{
    public class SoundCube
    {
        private Queue<SoundEffect> queue;
        private bool isPlaying;
        private BackgroundWorker soundWorker; 

        public SoundCube()
        {
            queue = new Queue<SoundEffect>();
            soundWorker = new BackgroundWorker();
            soundWorker.DoWork += new DoWorkEventHandler(bw_DoWork);
        }

        public void AddToQueue(SoundEffect se) 
        {
            queue.Enqueue(se);
        }

        public void AddToQueue(Stream stream)
        {
            var se = SoundEffect.FromStream(stream);
            queue.Enqueue(se);
        }

        public void StartPlayingQueue()
        {
            soundWorker.RunWorkerAsync();

            if (isPlaying)
                return;
            
            while (queue.Count > 0)
            {
                SoundEffect se = queue.Dequeue();
                var timeSlepping = se.Duration;
                FrameworkDispatcher.Update();
                se.Play();
                isPlaying = true;
                System.Threading.Thread.Sleep(timeSlepping.Add(new TimeSpan(0, 0, 2)));
            }

            isPlaying = false;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            while (queue.Count > 0)
            {
                SoundEffect se = queue.Dequeue();
                var timeSlepping = se.Duration;
                FrameworkDispatcher.Update();
                se.Play();
                isPlaying = true;
                System.Threading.Thread.Sleep(timeSlepping.Add(new TimeSpan(0, 0, 2)));
            }
        }
    }
}
