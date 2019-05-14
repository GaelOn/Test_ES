namespace ToyConsole.TestQ
{
    public class CountMessage
    {
        public int Value { get; }

        public CountMessage(int value) => Value = value;

        public override string ToString() => $"{Value}";
    }
}
