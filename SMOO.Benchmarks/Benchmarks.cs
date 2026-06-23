using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace SMOO.Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    [GlobalSetup]
    public void Setup()
    {

    }
}
