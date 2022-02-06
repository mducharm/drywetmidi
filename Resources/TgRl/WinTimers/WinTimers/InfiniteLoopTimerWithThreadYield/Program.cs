using Common;

namespace InfiniteLoopTimerWithThreadYield
{
    class Program
    {
        static void Main(string[] args)
        {
            TimerChecker.Check(new Timer());
        }
    }
}
