using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DogsonkiMessenger.Networking
{
    public class ThreadCallback
    {
        protected Stopwatch sw = new Stopwatch();

        public ThreadCallback()
        {
            sw.Start();
        }
    }
}
