<Query Kind="Program">
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <Namespace>System.Collections.Concurrent</Namespace>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
  <Namespace>BenchmarkDotNet.Attributes.Jobs</Namespace>
</Query>

void Main()
{
	var result = BenchmarkDotNet.Running.BenchmarkRunner.Run<AllocationBenchmark>();
}

// Results
//          Method | iterations |         Mean |      Error |     StdDev |    Gen 0 |    Gen 1 |    Gen 2 |  Allocated |
//---------------- |----------- |-------------:|-----------:|-----------:|---------:|---------:|---------:|-----------:|
//        TestList |       1024 |     4.965 us |  0.0503 us |  0.0470 us |   2.6321 |        - |        - |    8.12 KB |
// TestChunkedList |       1024 |    17.189 us |  0.3208 us |  0.2679 us |  24.9939 |        - |        - |   78.35 KB |
//        TestList |      10240 |    52.329 us |  0.5674 us |  0.5030 us |  41.6260 |        - |        - |  128.43 KB |
// TestChunkedList |      10240 |   111.086 us |  1.2799 us |  1.1346 us |  24.9023 |        - |        - |   78.35 KB |
//        TestList |     102400 | 1,460.084 us | 22.7264 us | 21.2583 us | 498.0469 | 253.9063 | 244.1406 | 1026.16 KB |
// TestChunkedList |     102400 | 1,078.433 us | 14.0452 us | 13.1379 us |  89.8438 |  29.2969 |        - |  469.63 KB |

[MemoryDiagnoser]
[InProcess]
public class AllocationBenchmark
{
	[Params(1024, 10*1024, 100 * 1024)]
	public int iterations = 0;

	private SimpleAllocations sa = new SimpleAllocations();

	[Benchmark]
	public void TestList()
	{
		sa.TestList(iterations);
	}

	[Benchmark]
	public void TestChunkedList()
	{
		sa.TestChunkedList(iterations);
	}
}

public class SimpleAllocations
{
	private List<int> list;
	private ChunkedIntList chunkedList;

	public void TestList(int elements)
	{
		list = new List<int>();
		for (int i = 0; i < elements; i++)
		{
			list.Add(i);
		}
	}

	public void TestChunkedList(int elements)
	{
		chunkedList = new ChunkedIntList();
		for (int i = 0; i < elements; i++)
		{
			chunkedList.Add(i);
		}
	}
}


public class ChunkedIntList
{
	private const int ChunkSize = 20000;
	private static SimpleObjectPool<int[]> pool = new UserQuery.SimpleObjectPool<int[]>(() => new int[ChunkSize], 10);
	private List<int[]> chunks = new List<int[]>();
	private int length = 0;

	private int GetChunkIndex(int offset)
	{
		return offset / ChunkSize;
	}
	
	private int GetOffsetInChunk(int offset)
	{
		return offset % ChunkSize;
	}
	
	public int this[int index] { get 
	{
		return chunks[GetChunkIndex(index)][GetOffsetInChunk(index)];
	} }

	public int Count => length;

	public bool IsReadOnly => true;

	public void Add(int item)
	{
		if (length % ChunkSize == 0)
		{
			chunks.Add(pool.TakeObject());
		}
		chunks[GetChunkIndex(length)][GetOffsetInChunk(length)] = item;
		length++;
	}

	public void Clear()
	{
		chunks.Clear();
		length = 0;
	}
}

// Define other methods and classes here
public class SimpleObjectPool<T>
{
	private readonly ConcurrentBag<T> pool;
	private readonly Func<T> factory;
	private readonly int maxObjectsInPool;

	public SimpleObjectPool(Func<T> factory, int maxObjectsInPool)
	{
		pool = new ConcurrentBag<T>();
		this.factory = factory;
		this.maxObjectsInPool = maxObjectsInPool;
	}

	public T TakeObject()
	{
		T result;
		if (pool.TryTake(out result))
		{
			return result;
		}

		return factory();
	}

	public void ReturnObject(T value)
	{
		if (pool.Count < maxObjectsInPool)
		{
			pool.Add(value);
		}
	}
}