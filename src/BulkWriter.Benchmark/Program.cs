using BenchmarkDotNet.Running;

namespace BulkWriter.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            DbHelpers.SetupDb();
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
