using System.Threading;

namespace Domain.Base.Test.TestHelper
{
    public class PingPong
    {
        private readonly ManualResetEvent _ping = new ManualResetEvent(true);
        private readonly ManualResetEvent _pong = new ManualResetEvent(false);

        public void Ping()
        {
            _pong.Set();
            _ping.Reset();
            _ping.WaitOne();
        }

        public void Pong()
        {
            _ping.Set();
            _pong.Reset();
            _pong.WaitOne();
        }

        public void WaitPing() => _ping.WaitOne();

        public void WaitPong() => _pong.WaitOne();

        public void SetOnlyPong() => _pong.Set();
    }
}
