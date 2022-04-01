using System.Threading;

namespace Proge.Teams.Edu.Abstraction.Utils.Monitoring
{
    public class Counter
    {
        private int _value = 0;
        private int Value => _value;
        public void Increment() => Interlocked.Increment(ref _value);
        public void Add(int delta) => Interlocked.Add(ref _value, delta);

        public static implicit operator int(Counter c) => c.Value;
        public static Counter operator +(Counter c, int delta)
        {
            c.Add(delta);
            return c;
        }

        public static Counter operator ++(Counter c)
        {
            c.Increment();
            return c;
        }

        public override string ToString() => Value.ToString();
    }
}