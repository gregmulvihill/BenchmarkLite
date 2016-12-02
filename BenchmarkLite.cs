using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// .NET 2.0 does not include a parameter-less action delegate, so BenchmarkLite has its own actions
/// </summary>
public delegate void BenchmarkLiteAction();

/// <summary>
/// .NET 2.0 does not include a parameter-less action delegate, so BenchmarkLite has its own actions
/// </summary>
public delegate void BenchmarkLiteAction<T>(T oT);

/// <summary>
/// A lightweight and simple benchmark tool.  
/// </summary>
[DebuggerStepThrough]
public class BenchmarkLite
{
	List<Record> _oActionList = new List<Record>();
	string _sTitle = null;

	/// <summary>
	/// A static instance of BenchmarkLite for quick/easy access
	/// </summary>
	public static BenchmarkLite Instance = new BenchmarkLite();

	/// <summary>
	/// Details of the action benchmark
	/// </summary>
	public struct Record
	{
		internal BenchmarkLiteAction<int> Action;
		internal BenchmarkLiteAction SetupAction;
		internal BenchmarkLiteAction CleanupAction;
		internal int OrderPercent;
		internal int OrderList;

		/// <summary>
		/// The label for this benchmark
		/// </summary>
		public string Label;
		/// <summary>
		/// Total elapsed seconds for this benchmark
		/// </summary>
		public double ElapsedSeconds;
		/// <summary>
		/// Percent comparison against best benchmark
		/// </summary>
		public double Percent;
	}

	/// <summary>
	/// A list of the results for custom processing/viewing
	/// </summary>
	public List<Record> Results
	{
		get { return _oActionList; }
	}

	/// <summary>
	/// Reset all benchmark values
	/// </summary>
	/// <returns></returns>
	public BenchmarkLite Reset()
	{
		_oActionList.Clear();

		return this;
	}

	/// <summary>
	/// Set the title of the benchmark
	/// </summary>
	/// <param name="sTitle">The title of the benchmark report</param>
	/// <returns></returns>
	public BenchmarkLite Title(string sTitle)
	{
		_sTitle = sTitle;

		return this;
	}

	/// <summary>
	/// Add an action to benchmark
	/// </summary>
	/// <param name="oAction">The action to benchmark</param>
	/// <param name="oSetupAction">The action to execute before beginning to benchmark</param>
	/// <param name="oCleanupAction">The action to execute after benchmark has been recorded</param>
	/// <param name="sLabel">The label of the action to benchmark</param>
	/// <returns></returns>
	public BenchmarkLite Add(
		BenchmarkLiteAction oAction,
		BenchmarkLiteAction oSetupAction = null,
		BenchmarkLiteAction oCleanupAction = null,
		string sLabel = null)
	{
		return Add(x => oAction(), oSetupAction, oCleanupAction, sLabel);
	}

	/// <summary>
	/// Add an action to benchmark with instance count
	/// </summary>
	/// <param name="oAction">The action to benchmark</param>
	/// <param name="oSetupAction">The action to execute before beginning to benchmark</param>
	/// <param name="oCleanupAction">The action to execute after benchmark has been recorded</param>
	/// <param name="sLabel">The label of the action to benchmark</param>
	/// <returns></returns>
	public BenchmarkLite Add(
		BenchmarkLiteAction<int> oAction,
		BenchmarkLiteAction oSetupAction = null,
		BenchmarkLiteAction oCleanupAction = null,
		string sLabel = null)
	{
		if (null == oSetupAction) oSetupAction = () => { };
		if (null == oCleanupAction) oCleanupAction = () => { };

		var oRecord = new Record
		{
			Action = oAction,
			Label = sLabel,
			SetupAction = oSetupAction,
			CleanupAction = oCleanupAction,
			ElapsedSeconds = 0,
		};

		_oActionList.Add(oRecord);

		return this;
	}

	/// <summary>
	/// Benchmark all actions
	/// </summary>
	/// <param name="nIterations">The number of iterations to execute each action</param>
	/// <param name="nWarmups">The number of iterations to execute each action before recording timestamps</param>
	/// <param name="nThreadCount">The number of threads to use to benchmark each action</param>
	/// <returns></returns>
	public BenchmarkLite Run(
		int nIterations = 1,
		int nWarmups = 0,
		int nThreadCount = 0)
	{
		var nMinIndex = 0;

		for (var nIndex = 0; nIndex < _oActionList.Count; nIndex++)
		{
			var oRecord = _oActionList[nIndex];

			var oAction = oRecord.Action;
			var oSetupAction = oRecord.SetupAction;
			var oCleanupAction = oRecord.CleanupAction;

			if (nWarmups > 0)
			{
				oSetupAction();

				ExecuteActions(nWarmups, nThreadCount, oAction);

				oCleanupAction();
			}

			var nTimestampHead = Stopwatch.GetTimestamp();
			{
				oSetupAction();

				ExecuteActions(nIterations, nThreadCount, oAction);

				oCleanupAction();
			}
			var nTimestampTail = Stopwatch.GetTimestamp();

			oRecord.ElapsedSeconds = (double)(nTimestampTail - nTimestampHead) / Stopwatch.Frequency;
			_oActionList[nIndex] = oRecord;

			if (_oActionList[nIndex].ElapsedSeconds < _oActionList[nMinIndex].ElapsedSeconds) nMinIndex = nIndex;
		}

		for (var nIndex = 0; nIndex < _oActionList.Count; nIndex++)
		{
			var oRecord = _oActionList[nIndex];

			oRecord.OrderList = nIndex;
			oRecord.Percent = oRecord.ElapsedSeconds / _oActionList[nMinIndex].ElapsedSeconds;

			_oActionList[nIndex] = oRecord;
		}

		_oActionList.Sort((a, b) => a.Percent.CompareTo(b.Percent));

		for (var nIndex = 0; nIndex < _oActionList.Count; nIndex++)
		{
			var oRecord = _oActionList[nIndex];
			oRecord.OrderPercent = nIndex;
			_oActionList[nIndex] = oRecord;
		}

		_oActionList.Sort((a, b) => a.OrderList.CompareTo(b.OrderList));

		return this;
	}

	static void ExecuteActions(
		int nIterations,
		int nThreadCount,
		BenchmarkLiteAction<int> oAction)
	{
		var oIterateAction = new ThreadStart(() =>
		{
			for (var i = 0; i < nIterations; i++)
			{
				oAction(i);
			}
		});

		if (nThreadCount > 1)
		{
			var oThreadList = new List<Thread>();

			for (var i = 0; i < nThreadCount; i++)
			{
				oThreadList.Add(new Thread(oIterateAction));
			}

			oThreadList.ForEach(x => x.Start());
			oThreadList.ForEach(x => x.Join());
		}
		else
		{
			oIterateAction();
		}
	}

	/// <summary>
	/// Show benchmark results
	/// </summary>
	/// <param name="bOutputToDebug">Output to the debugger, on by default</param>
	/// <param name="bOutputToConsole">Output to the console, on by default</param>
	/// <returns></returns>
	public string ShowResults(
		bool bOutputToDebug = true,
		bool bOutputToConsole = true)
	{
		System.Text.StringBuilder oOutput = new System.Text.StringBuilder();

		if (!string.IsNullOrEmpty(_sTitle))
		{
			var nLength = (68 - _sTitle.Length) / 2 - 1;
			var sPadding = new string('-', nLength);
			oOutput.AppendLine($"{sPadding} {_sTitle} {sPadding}");
		}
		else
		{
			oOutput.AppendLine(new string('-', 68));
		}

		var nCount = (double)_oActionList.Count;

		oOutput.AppendLine($" {"Label",39}  = {"Ave Sec",7}   {"Percent",8} {"Order",5}");

		for (var i = 0; i < nCount; i++)
		{
			var nOrder = _oActionList[i].OrderPercent;
			var nTimeS = _oActionList[i].ElapsedSeconds;
			var nPercent = _oActionList[i].Percent * 100.0;
			var sLabel = _oActionList[i].Label ?? i.ToString();

			var sResult = $"[{sLabel,39}] = {nTimeS / nCount,7:N3} {nPercent,9:N2}% {nOrder,5}";
			oOutput.AppendLine(sResult);
		}

		Reset();  //not sure about this....  use default parameter? or rely on user to call clear?

		if (bOutputToConsole) Console.WriteLine(oOutput);
		if (bOutputToDebug) System.Diagnostics.Debug.WriteLine(oOutput);

		return oOutput.ToString();
	}
}