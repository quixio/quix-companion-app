using System;
using System.Threading.Tasks;

namespace QuixTracker.Services
{
    public static class RetryService
    {
        /// <summary>
        ///  Executes the submitted actions with retries.
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="retries">Retry count (if set to less than 1, retries forever)</param>
        /// <param name="interval">Time interval between retries in ms.</param>
        /// <returns><see cref="Task" indicating asynchronous operation/></returns>
        public static async Task Execute(Func<Task> action, int retries = 5, int interval = 500, Action<Exception> errorHandler = null)
        {
            var retry = 0;
            start:
            try
            {
                retry++;
                await action();
            }
            catch (Exception ex)
            {
                if (retries > 0 && retry > retries)
                    throw;
                if (errorHandler != null)
                    errorHandler(ex);
                await Task.Delay(interval);
                goto start;
            }
        }

        /// <summary>
        ///  Executes the submitted actions with retries.
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="retries">Retry count (if set to less than 1, retries forever)</param>
        /// <param name="interval">Time interval between retries in ms.</param>
        /// <returns>Return value of the action.</returns>
        public static async Task<T> Execute<T>(Func<Task<T>> action, int retries = 5, int interval = 500, Action<Exception> errorHandler = null)
        {
            var retry = 0;
        start:
            try
            {
                retry++;
                return await action();
            }
            catch (Exception ex)
            {
                if (retries > 0 && retry > retries)
                    throw;
                if (errorHandler != null)
                    errorHandler(ex);
                await Task.Delay(interval);
                goto start;
            }
        }
    }
}
