#region

using System;
using System.Windows.Threading;

#endregion

namespace SimpleTTSReader
{
    internal class UpdateChecker
    {
        private readonly DispatcherTimer _updateTimer = new DispatcherTimer();

        public UpdateChecker()
        {
            _updateTimer.Tick += (sender, args) => ClickOnceHelper.CheckForUpdates(true);
            _updateTimer.Interval = new TimeSpan(1, 0, 0);
        }

        public void Start()
        {
            if (ClickOnceHelper.IsUpdateable)
                _updateTimer.Start();
        }

        public void Stop()
        {
            _updateTimer.Stop();
        }
    }
}