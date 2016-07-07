using System;
using System.Collections.Generic;
using System.Diagnostics;
using cdmdotnet.Performance.Metrics;

namespace cdmdotnet.Performance
{
	public static class PerformanceMetricFactory
	{

		#region Static Variables

		/// <summary>
		/// Dictionry of all the metrics that have been created through the life of the application
		/// </summary>
		private static Dictionary<ActionInfo, PerformanceMetricContainer> PerformanceMetrics { get; set; }

		/// <summary>
		/// List of Custom Metrics that have been registered by the app to 
		/// </summary>
		private static List<Func<PerformanceMetricBase>> CustomMetrics { get; set; }

		/// <summary>
		/// Object used for locking when we check to see if an ActionInfo already has its metrics created
		/// </summary>
		private static Object LockObject { get; set; }

		#endregion


		static PerformanceMetricFactory()
		{
			PerformanceMetrics = new Dictionary<ActionInfo, PerformanceMetricContainer>();
			CustomMetrics = new List<Func<PerformanceMetricBase>>();
			LockObject = new Object();
		}


		/// <summary>
		/// Gets a List of performance metrics that will be measured on the action whose data is 
		/// represented by the given action info
		/// </summary>
		/// <param name="info">An ActionInfo object that contains info about the action whose performance
		/// is being measured</param>
		/// <returns>A List of PerformanceMetricBase objects of the metrics to be measured on this action</returns>
		public static List<PerformanceMetricBase> GetPerformanceMetrics(ActionInfo info)
		{
			if (PerformanceMetrics.ContainsKey(info) == false)
			{
				lock (LockObject)
				{
					// Check Again
					if (PerformanceMetrics.ContainsKey(info) == false)
					{
						List<PerformanceMetricBase> metrics = CreateMetricsForAction(info);
						 PerformanceMetricContainer pmc = new PerformanceMetricContainer(info, metrics);
						PerformanceMetrics.Add(info, pmc);
					}
				}
			}

			return PerformanceMetrics[info].GetPerformanceMetrics();
		}



		private static List<PerformanceMetricBase> CreateMetricsForAction(ActionInfo actionInfo)
		{
			List<PerformanceMetricBase> metrics = new List<PerformanceMetricBase>();

			// Add the standard metrics
			try
			{
				metrics.Add(new TotalCallsMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new TotalElapsedTimeMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new DeltaCallsMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new DeltaElapsedTimeMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new AverageCallTimeMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new CallsPerSecondMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new CallsInProgressMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new LastCallElapsedTimeMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new TotalExceptionsThrownMetric(actionInfo));
			}
			catch (InvalidOperationException) { }
			try
			{
				metrics.Add(new DeltaExceptionsThrownMetric(actionInfo));
			}
			catch (InvalidOperationException) { }

			// Now add any custom metrics the user may have added
			foreach (var x in CustomMetrics)
			{
				PerformanceMetricBase customMetric = x();
				metrics.Add(customMetric);
			}

			return metrics;
		}

		public static void AddCustomPerformanceMetric(Func<PerformanceMetricBase> customMetricCreator)
		{
			CustomMetrics.Add(customMetricCreator);
		}

		/// <summary>
		/// Method to clean up the performance counters on application exit
		/// </summary>
		/// <remarks>
		/// This method should only be called on application exit
		/// </remarks>
		public static void CleanupPerformanceMetrics()
		{
			// We'll make sure no one is trying to add while we are doing this, but should not
			// really be an issue
			lock (LockObject)
			{
				foreach (var pmc in PerformanceMetrics.Values)
				{
					pmc.DisposePerformanceMetrics();
				}

				PerformanceMetrics.Clear();
				PerformanceCounter.CloseSharedResources();
			}
		}
	}
}
