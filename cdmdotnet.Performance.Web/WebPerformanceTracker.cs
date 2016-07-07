using System;
using System.Web;

namespace cdmdotnet.Performance.Web
{
	public class WebPerformanceTracker : IPerformanceTracker
	{
		public void ProcessActionStart()
		{
			try
			{
				// ActionInfo encapsulates all the info about the action being invoked
				ActionInfo info = CreateActionInfo();

				// PerformanceTracker is the object that tracks performance and is attached to the request
				PerformanceTracker tracker = new PerformanceTracker(info);

				// Store this on the request
				string contextKey = GetUniqueContextKey(ConfigInfo.Value.ProcessId.ToString());
				HttpContext.Current.Items.Add(contextKey, tracker);

				// Process the action start - this is what starts the timer and increments any
				// required counters before the action executes
				tracker.ProcessActionStart();
			}
			catch (UnauthorizedAccessException) { }
			catch (Exception) { }
		}

		public void ProcessActionComplete(bool unhandledExceptionFlag)
		{
			try
			{
				// This is the unique key the PerformanceTracker object would be stored under
				string contextKey = GetUniqueContextKey(ConfigInfo.Value.ProcessId.ToString());

				// Check if there is an object on the request.  If not, must not be tracking performance
				// for this action, so just go ahead and return
				if (!HttpContext.Current.Items.Contains(contextKey))
				{
					return;
				}

				// If we are here, we are tracking performance.  Extract the object from the request and call
				// ProcessActionComplete.  This will stop the stopwatch and update the performance metrics
				PerformanceTracker tracker = HttpContext.Current.Items[contextKey] as PerformanceTracker;

				if (tracker != null)
				{
					tracker.ProcessActionComplete(unhandledExceptionFlag);
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (Exception) { }
		}

		#region Helper Methdos

		/// <summary>
		/// Helper method to create the ActionInfo object containing the info about the action that is getting called
		/// </summary>
		/// <returns>An ActionInfo object that contains all the information pertaining to what action is being executed</returns>
		private ActionInfo CreateActionInfo()
		{
			var parameters = HttpContext.Current.Request.QueryString;
			string parameterString = String.Join(",", parameters);

			int processId = ConfigInfo.Value.ProcessId;
			String categoryName = ConfigInfo.Value.PerformanceCategoryName;
			String httpMethod = HttpContext.Current.Request.HttpMethod;

			ActionInfo info = new ActionInfo(processId, categoryName, "API", HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.AbsolutePath, httpMethod, parameterString);

			return info;
		}


		/// <summary>
		/// Helper method to form the key that will be used to store/retrieve the PerformanceTracker object
		/// off if the HttpContext
		/// </summary>
		/// <remarks>
		/// To minimize any chance of collisions, this method concatenates the full name of this class
		/// with the UniqueID of the MVC action to get a unique key to use
		/// </remarks>
		/// <param name="actionUniqueId">A String of the unique id assigned by ASP.NET to the MVC action</param>
		/// <returns>A Strin suitable to be used for the key</returns>
		private String GetUniqueContextKey(String actionUniqueId)
		{
			return GetType().FullName + ":" + actionUniqueId;
		}

		#endregion
	}
}