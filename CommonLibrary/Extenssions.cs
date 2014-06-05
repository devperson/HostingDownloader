using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CommonLibrary
{
    public class Extenssions
    {
        public static void InvokeAfterSec(double sec, Action action)
        {
            DispatcherTimer t = new DispatcherTimer();
            t.Interval = TimeSpan.FromSeconds(sec);
            t.Tick += (s, e) =>
            {
                t.Stop();
                action();
            };
            t.Start();
        }

        
    }
}
