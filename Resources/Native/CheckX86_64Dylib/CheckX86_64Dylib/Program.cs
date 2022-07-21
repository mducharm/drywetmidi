using System.Runtime.InteropServices;

namespace CheckX86_64Dylib
{
    internal class Program
    {
        [DllImport("x86_64", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Foo();

        static void Main(string[] args)
        {
            Console.WriteLine($"Foo called: {Foo()}");
            Console.WriteLine("Everything fine! Press any key to exit the program...");
            Console.ReadKey();
        }
    }
}