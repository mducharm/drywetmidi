using System.Runtime.InteropServices;

namespace CheckArm64Dylib
{
    internal class Program
    {
        [DllImport("arm64", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Foo();

        static void Main(string[] args)
        {
            Console.WriteLine($"Foo called: {Foo()}");
            Console.WriteLine("Everything fine! Press any key to exit the program...");
            Console.ReadKey();
        }
    }
}