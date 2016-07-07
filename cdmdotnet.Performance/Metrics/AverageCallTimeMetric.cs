using System;
using System.Diagnostics;

namespace cdmdotnet.Performance.Metrics
{
    /// <summary>
    /// Performance Metric that updates the counters that track the average time a method took
    /// </summary>
    public class AverageCallTimeMetric : PerformanceMetricBase
    {

        public AverageCallTimeMetric(ActionInfo info)
            : base(info)
        {
            String categoryName = actionInfo.PerformaneCounterCategory;
            String instanceName = actionInfo.InstanceName;
            _averageTimeCounter = InitializeCounter(categoryName, CounterName, instanceName);
            _baseCounter = InitializeCounter(categoryName, BaseCounterName, instanceName);
        }


        /// <summary>
        /// Constant defining the name of the average time counter
        /// </summary>
        /// <remarks>
        /// This is the counter name that will show up in perfmon
        /// </remarks>
        public const String CounterName = "Average Time per Call";

        /// <summary>
        /// Constant defining the name of the base counter to use
        /// </summary>
        public const String BaseCounterName = "Average Time per Call Base";


        #region Member Variables

        private readonly PerformanceCounter _averageTimeCounter;
        private readonly PerformanceCounter _baseCounter;

        #endregion


        /// <summary>
        /// Method called by the custom action filter after the action completes
        /// </summary>
        /// <remarks>
        /// This method increments the Average Time per Call counter by the number of ticks
        /// the action took to complete and the base counter is incremented by 1 (this is
        /// done in the PerfCounterUtil.IncrementTimer() method).  
        /// </remarks>
        /// <param name="elapsedTicks">A long of the number of ticks it took to complete the action</param>
        public override void OnActionComplete(long elapsedTicks, bool exceptionThrown)
        {
            _averageTimeCounter.IncrementBy(elapsedTicks);
            _baseCounter.Increment();
        }

        /// <summary>
        /// Disposes of the two PerformanceCounter objects when the metric object is disposed
        /// </summary>
        public override void Dispose()
        {
            _averageTimeCounter.Dispose();
            _baseCounter.Dispose();
        }
    }
}
