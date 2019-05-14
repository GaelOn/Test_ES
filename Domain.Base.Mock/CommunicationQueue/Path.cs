namespace Domain.Base.Mock.CommunicationQueue
{
    class Path
    {
        public string Exchange { get; }
        public string QueueName { get; }

        public Path(string exchange, string queueName)
        {
            Exchange = exchange;
            QueueName = queueName;
        }

        public override string ToString() => $"{Exchange}.{QueueName}";
    }
}
