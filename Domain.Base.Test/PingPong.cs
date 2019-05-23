using System.Threading;

namespace Domain.Base.EventSourcedAggregateRepository.Test
{
    public partial class EventSourcedAggregateRepositoryTest
    {
        public class PingPong
        {
            private ManualResetEvent _ping = new ManualResetEvent(true);
            private ManualResetEvent _pong = new ManualResetEvent(false);

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

            public void SetOnlyPong() => _pong.Set();
        }
    }
}
