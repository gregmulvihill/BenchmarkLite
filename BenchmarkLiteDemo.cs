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