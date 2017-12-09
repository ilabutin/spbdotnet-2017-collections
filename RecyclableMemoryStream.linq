<Query Kind="Program">
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <NuGetReference>Microsoft.IO.RecyclableMemoryStream</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Attributes.Jobs</Namespace>
  <Namespace>Microsoft.IO</Namespace>
</Query>

void Main()
{
	BenchmarkDotNet.Running.BenchmarkRunner.Run(typeof(MemoryStreamBenchmark));
}


//                                  Method |  Size |          Mean |       Error |      StdDev |     Gen 0 |     Gen 1 |     Gen 2 |  Allocated |
//---------------------------------------- |------ |--------------:|------------:|------------:|----------:|----------:|----------:|-----------:|
//                        TestMemoryStream |    10 |      4.136 us |   0.0314 us |   0.0278 us |   10.0937 |         - |         - |    31891 B |
//              TestRecyclableMemoryStream |    10 |     47.386 us |   0.1886 us |   0.1672 us |   41.6260 |   41.6260 |   41.6260 |   136209 B |
// TestRecyclableMemoryStreamCommonManager |    10 |      6.205 us |   0.0259 us |   0.0230 us |    0.0839 |         - |         - |      272 B |
//                        TestMemoryStream |   100 |     92.004 us |   1.1146 us |   0.9881 us |   41.6260 |   41.6260 |   41.6260 |   261574 B |
//              TestRecyclableMemoryStream |   100 |     97.643 us |   0.8578 us |   0.7163 us |   41.6260 |   41.6260 |   41.6260 |   136209 B |
// TestRecyclableMemoryStreamCommonManager |   100 |     33.167 us |   0.4088 us |   0.3192 us |         - |         - |         - |      272 B |
//                        TestMemoryStream |  1000 |    908.700 us |  22.0189 us |  64.5775 us |  374.6094 |  333.8542 |  333.3984 |  2097313 B |
//              TestRecyclableMemoryStream |  1000 |    708.848 us |   6.7628 us |   5.6473 us |  333.0078 |  333.0078 |  333.0078 |  1054177 B |
// TestRecyclableMemoryStreamCommonManager |  1000 |    229.052 us |   2.9773 us |   2.7850 us |         - |         - |         - |      477 B |
//                        TestMemoryStream | 10000 | 13,495.566 us | 145.6441 us | 136.2356 us | 2015.6250 | 1984.3750 | 1984.3750 | 33554693 B |
//              TestRecyclableMemoryStream | 10000 |  6,471.392 us | 128.2926 us | 217.8506 us | 3148.2319 | 3148.2319 |  682.1546 | 10368941 B |
// TestRecyclableMemoryStreamCommonManager | 10000 |  2,659.371 us |  26.5190 us |  24.8058 us |         - |         - |         - |     2626 B |

// Define other methods and classes here
[InProcessAttribute]
[MemoryDiagnoser]
public class MemoryStreamBenchmark
{
	[Params(10, 100, 1000, 10000)]
	public int Size = 0;

	private byte[] buffer = new byte[1024];
	private RecyclableMemoryStreamManager cmm = new RecyclableMemoryStreamManager();

	[Benchmark]
	public void TestMemoryStream()
	{
		using (var m = new MemoryStream())
		{
			for (int i = 0; i < Size; i++)
			{
				m.Write(buffer, 0, buffer.Length);
			}
		}
	}

	[Benchmark]
	public void TestRecyclableMemoryStream()
	{
		using (var m = new RecyclableMemoryStream(new RecyclableMemoryStreamManager()))
		{
			for (int i = 0; i < Size; i++)
			{
				m.Write(buffer, 0, buffer.Length);
			}
		}
	}

	[Benchmark]
	public void TestRecyclableMemoryStreamCommonManager()
	{
		using (var m = new RecyclableMemoryStream(cmm))
		{
			for (int i = 0; i < Size; i++)
			{
				m.Write(buffer, 0, buffer.Length);
			}
		}
	}
}
