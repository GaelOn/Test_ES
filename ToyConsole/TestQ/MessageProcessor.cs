using System;
using System.Threading;
using Domain.Base.Mock.CommunicationQueue;

namespace ToyConsole.TestQ
{
    public class MessageProcessor
    {
        AutoResetEvent _are;

        public MessageProcessor(IHandlerRegister register, AutoResetEvent are)
        {
            _are = are;

            register.RegisterHandler<CountMessage>(HandleCountMessage);
            register.RegisterHandler<EndMessage>(HandleEndMessage);
        }

        private void HandleCountMessage(CountMessage msg)
        {
            Console.WriteLine(msg.ToString());
            Console.WriteLine(Environment.NewLine);
        }

        private void HandleEndMessage(EndMessage msg)
        {
            Console.WriteLine("End of count");
            Console.WriteLine(Environment.NewLine);
            _are.Set();
        }
    }
}
