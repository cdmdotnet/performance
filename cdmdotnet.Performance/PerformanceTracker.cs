using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using cdmdotnet.Performance.Metrics;

namespace cdmdotnet.Performance
{
	public class PerformanceTracker : IPerformanceTracker
	{
		public PerformanceTracker(ActionInfo info)
		{
			ActionInfo = info;
		}

		#region Member Variables

		/// <summary>
		/// Hold info about the action being tracked
		/// </summary>
		private ActionInfo ActionInfo { get; set; }

		/// <summary>
		/// Stopwatch to time how long the action took
		/// </summary>
		private Stopwatch Stopwatch { get; set; }

		/// <summary>
		/// Collection of all the performance metrics to be tracked
		/// </summary>
		private List<PerformanceMetricBase> PerformanceMetrics { get; set; }

		#endregion


		public void ProcessActionStart()
		{
			try
			{
				Stopwatch = Stopwatch.StartNew();

				// Use the factory class to get all of the performance metrics that are being tracked
				// for MVC Actions
				PerformanceMetrics = PerformanceMetricFactory.GetPerformanceMetrics(ActionInfo);

				// Iterate through each metric and call the OnActionStart() method
				// Start off a task to do this so it can it does not block and minimized impact to the user
				Task task = Task.Factory.StartNew(() =>
				{
					try
					{
						if (PerformanceMetrics != null)
						{
							foreach (PerformanceMetricBase performanceMetric in PerformanceMetrics)
							{
								performanceMetric.OnActionStart();
							}
						}
					}
					catch { }
				});
			}
			catch (Exception ex)
			{
				string message = string.Format("Exception {0} occurred PerformanceTracker.ProcessActionStart().  Message {1}\nStackTrace {2}", ex.GetType().FullName, ex.Message, ex.StackTrace);
				Trace.WriteLine(message);
			}
		}



		public void ProcessActionComplete(bool unhandledExceptionFlag)
		{
			try
			{
				// Stop the stopwatch
				Stopwatch.Stop();

				// Iterate through each metric and call the OnActionComplete() method
				// Start off a task to do this so it can it does not block and minimized impact to the user
				Task task = Task.Factory.StartNew(() =>
				{
					try
					{
						if (PerformanceMetrics != null)
						{
							foreach (PerformanceMetricBase performanceMetric in PerformanceMetrics)
							{
								performanceMetric.OnActionComplete(Stopwatch.ElapsedTicks, unhandledExceptionFlag);
							}
						}
					}
					catch { }
				});
			}
			catch (Exception ex)
			{
				string message = string.Format("Exception {0} occurred PerformanceTracker.ProcessActionComplete().  Message {1}\nStackTrace {2}", ex.GetType().FullName, ex.Message, ex.StackTrace);
				Trace.WriteLine(message);
			}
		}
	}
}