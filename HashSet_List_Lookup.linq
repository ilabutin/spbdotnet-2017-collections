<Query Kind="Program">
  <NuGetReference>BenchmarkDotNet</NuGetReference>
  <Namespace>BenchmarkDotNet.Attributes.Jobs</Namespace>
  <Namespace>BenchmarkDotNet.Attributes</Namespace>
</Query>

void Main()
{
	BenchmarkDotNet.Running.BenchmarkRunner.Run(typeof(DictionaryBenchmark));
}

// Results
//        Method |    N |        Mean |      Error |     StdDev |
//-------------- |----- |------------:|-----------:|-----------:|
// LookupHashSet |   10 |    791.4 us |  3.1327 us |  2.7771 us |
//    LookupList |   10 |    318.0 us |  0.9956 us |  0.9313 us |
// LookupHashSet |  100 |    954.8 us |  2.9117 us |  2.7236 us |
//    LookupList |  100 |  1,844.3 us |  5.2175 us |  4.8804 us |
// LookupHashSet | 1000 |    954.8 us |  2.5680 us |  2.4021 us |
//    LookupList | 1000 | 16,235.2 us | 35.8425 us | 29.9301 us |

// Define other methods and classes here
[InProcess]
public class DictionaryBenchmark
{
	private HashSet<string> hashSet;
	private List<string> valuesToSearch;
	private List<string> values;

	private const string text =
			"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
		;

	[Params(10, 100, 1000)]
	public int N = 1;

	public int LookupN = 10000;

	[Setup]
	public void Setup()
	{
		Random r = new Random(54);
		hashSet = new HashSet<string>();
		values = new List<string>(N);
		for (int i = 0; i < N; i++)
		{
			int position = r.Next(text.Length);
			int len = r.Next(text.Length - position);
			string s = text.Substring(position, len);
			values.Add(s);
			hashSet.Add(s);
		}
		valuesToSearch = new List<string>(LookupN);
		for (int i = 0; i < valuesToSearch.Capacity; i++)
		{
			int valIndex = r.Next(values.Count);
			valuesToSearch.Add(values[valIndex]);
		}
	}

	[Benchmark]
	public bool LookupHashSet()
	{
		bool exists = false;
		foreach (string v in valuesToSearch)
		{
			exists |= hashSet.Contains(v);
		}
		return exists;
	}

	[Benchmark]
	public bool LookupList()
	{
		bool exists = false;
		foreach (string v in valuesToSearch)
		{
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i] == v)
				{
					exists = true;
					break;
				}
			}
		}
		return exists;
	}
}
