namespace MyServiceBus.Domains
{
    public class MetricPerSecond
    {
        public int Value { get; private set; }
        
        public int InternalValue { get; private set; }


        public void EventHappened()
        {
            InternalValue++;
        }

        public void OneSecondTimer()
        {
            Value = InternalValue;
            InternalValue = 0;
        }
    }
}