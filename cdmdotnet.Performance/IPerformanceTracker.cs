namespace cdmdotnet.Performance
{
	public interface IPerformanceTracker
	{
		void ProcessActionStart();
		void ProcessActionComplete(bool unhandledExceptionFlag);
	}
}