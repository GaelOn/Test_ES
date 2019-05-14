using System.Threading;
using System;
using ToyConsole.TestQ;
using Domain.Base.Mock.CommunicationQueue;
using System.Threading.Tasks;

namespace ToyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestMockQScenario1();
            var mre = TestSynchroPingPong(10);
            mre.WaitOne();
            Console.ReadKey();
        }

        static void TestMockQScenario1()
        {
            var q = new MockQ();
            var exchangeName        = "Test";
            var publisher           = q.GetMessagePublisher(exchangeName);
            var handlerRegistration = q.GetHandlerRegister(exchangeName);

            var are = new AutoResetEvent(false);
            var msgProcessor   = new MessageProducer(publisher);
            Task.Factory.StartNew(msgProcessor.Run);
            Thread.Sleep(10000);
            var messageHandler = new MessageProcessor(handlerRegistration, are);
            are.WaitOne();
            Console.WriteLine("Back to main thread");
            Console.ReadKey();
        }

        static void TestMockQScenario2()
        {
            var q = new MockQ();
            var exchangeName = "Test";
            var publisher = q.GetMessagePublisher(exchangeName);
            var handlerRegistration = q.GetHandlerRegister(exchangeName);

            var are = new AutoResetEvent(false);
            var messageHandler = new MessageProcessor(handlerRegistration, are);
            var msgProcessor   = new MessageProducer(publisher);
            Task.Factory.StartNew(msgProcessor.Run);
            are.WaitOne();
            Console.WriteLine("Back to main thread");
            Console.ReadKey();
        }

        static ManualResetEvent TestSynchroPingPong(int max)
        {
            var mre1 = new ManualResetEvent(true);
            var mre2 = new ManualResetEvent(false);
            var mre3 = new ManualResetEvent(false);
            void PrintEvenNumber()
            {
                for (int it = 0; it < max; it++)
                {
                    if(it%2 == 0)
                    {
                        mre1.WaitOne();
                        Console.WriteLine(it);
                        mre2.Set();
                        mre1.Reset();
                    }
                }
            }
            void PrintOddNumber()
            {
                for (int it = 0; it < max; it++)
                {
                    if (it % 2 == 1)
                    {
                        mre2.WaitOne();
                        Console.WriteLine(it);
                        mre1.Set();
                        mre2.Reset();
                    }
                }
            }
            var t1 = Task.Factory.StartNew(PrintEvenNumber);
            var t2 = Task.Factory.StartNew(PrintOddNumber);
            var tasks = new[] { t1, t2 };
            Task.WhenAll(tasks).ContinueWith((t)=> mre3.Set());
            return mre3;
        }
    }
}
