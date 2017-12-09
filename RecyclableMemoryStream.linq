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
