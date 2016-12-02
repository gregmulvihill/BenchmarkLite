# BenchmarkLite
A lightweight and simple benchmark tool.

Use:

```C#
using System;
using System.Threading;

public class BenchmarkLiteDemo
{
	public static void Run()
	{
		var oRandom = new Random();

		BenchmarkLite.Instance
			.Title("Benchmark 1")
			.Add(() =>
			{
				Thread.Sleep(oRandom.Next(10));
			})
			.Add(() =>
			{
				Thread.Sleep(oRandom.Next(10));
			})
			.Add((x) =>
			{
				Console.Write($"{x}          \r");
				Thread.Sleep(oRandom.Next(Math.Min(x, 10)));
			})
			.Add((x) =>
			{
				Console.Write($"{x}          \r");
				Thread.Sleep(oRandom.Next(Math.Min(x, 10)));
			})
			.Run(250, 1, Environment.ProcessorCount * 2 / 3)
			.ShowResults(false, true);

		BenchmarkLite.Instance.Reset();

		BenchmarkLite.Instance
			.Title("Benchmark 2")
			.Add(() =>
			{
				Thread.Sleep(oRandom.Next(10));
			},
			() => { Console.WriteLine("Setup action 0"); },
			() => { Console.WriteLine("Cleanup action 0"); },
			"Action 0")
			.Add(() =>
			{
				Thread.Sleep(oRandom.Next(10));
			})
			.Add((x) =>
			{
				Console.Write($"{x}          \r");
				Thread.Sleep(oRandom.Next(Math.Min(x, 10)));
			},
			() => { Console.WriteLine("Setup action 2"); },
			() => { Console.WriteLine("Cleanup action 2"); },
			"Action 2")
			.Add((x) =>
			{
				Console.Write($"{x}          \r");
				Thread.Sleep(oRandom.Next(Math.Min(x, 10)));
			})
			.Run(250, 1, Environment.ProcessorCount * 2 / 3)
			.ShowResults(true, true);

		Console.WriteLine("Press any key to continue");
		Console.ReadKey();
	}
}
```

Output:

<pre>
--------------------------- Benchmark 1 ---------------------------
                                   Label  = Ave Sec    Percent Order
[                                      0] =   0.301    102.25%     1
[                                      1] =   0.295    100.00%     0
[                                      2] =   0.302    102.50%     2
[                                      3] =   0.313    106.29%     3

Setup action 0
Cleanup action 0
Setup action 0
Cleanup action 0
Setup action 2
Cleanup action 2
Setup action 2
Cleanup action 2
--------------------------- Benchmark 2 ---------------------------
                                   Label  = Ave Sec    Percent Order
[                               Action 0] =   0.296    102.58%     2
[                                      1] =   0.289    100.00%     0
[                               Action 2] =   0.310    107.41%     3
[                                      3] =   0.296    102.32%     1

Press any key to continue
</pre>
