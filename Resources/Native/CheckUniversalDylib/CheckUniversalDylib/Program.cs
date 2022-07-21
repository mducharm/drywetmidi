using System.Runtime.InteropServices;

namespace CheckUniversalDylib
{
    internal class Program
    {
        [DllImport("universal", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Foo();

        static void Main(string[] args)
        {
            Console.WriteLine($"Foo called: {Foo()}");
            Console.WriteLine("Everything fine! Press any key to exit the program...");
            Console.ReadKey();
        }
    }
}