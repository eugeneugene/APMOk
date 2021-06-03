using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace RecurrentTasks
{
    public class TaskRunner<TRunnable> : ITask<TRunnable>, IDisposable
        where TRunnable : IRunnable
    {
        private readonly EventWaitHandle runImmediately = new AutoResetEvent(false);

        private readonly ILogger logger;

        private Task mainTask;

        private CancellationTokenSource cancellationTokenSource;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskRunner{TRunnable}"/> class.
        /// </summary>
        /// <param name="loggerFactory">Фабрика для создания логгера</param>
        /// <param name="options">TaskOptions</param>
        /// <param name="serviceScopeFactory">Фабрика для создания Scope (при запуске задачи)</param>
        public TaskRunner(ILoggerFactory loggerFactory, TaskOptions<TRunnable> options, IServiceScopeFactory serviceScopeFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            logger = loggerFactory.CreateLogger($"{GetType().Namespace}.{nameof(TaskRunner<TRunnable>)}<{typeof(TRunnable).FullName}>");
            Options = options ?? throw new ArgumentNullException(nameof(options));
            ServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            RunStatus = new TaskRunStatus();
        }

        ~TaskRunner()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                runImmediately?.Dispose();
                cancellationTokenSource?.Dispose();
                if (mainTask != null)
                {
                    if (mainTask.Status == TaskStatus.RanToCompletion ||
                        mainTask.Status == TaskStatus.Faulted ||
                        mainTask.Status == TaskStatus.Canceled)
                        mainTask.Dispose();
                }
            }

            disposed = true;
        }

        /// <inheritdoc />
        public TaskRunStatus RunStatus { get; protected set; }

        /// <inheritdoc />
        public bool IsStarted => mainTask != null;

        /// <inheritdoc />
        public bool IsRunningRightNow { get; private set; }

        /// <inheritdoc />
        public TaskOptions Options { get; }

        /// <inheritdoc />
        public Type RunnableType => typeof(TRunnable);

        private IServiceScopeFactory ServiceScopeFactory { get; set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsStarted && Options.AutoStart)
                Start(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsStarted)
                Stop();

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Start()
        {
            Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Options.FirstRunDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(Options.FirstRunDelay), "First run delay can't be negative");

            if (Options.Interval < TimeSpan.Zero)
                throw new InvalidOperationException("Interval can't be negative");

            logger.LogInformation("<{0}>.Start() called...", RunnableType.Name);
            if (mainTask != null)
                throw new InvalidOperationException("Already started");

            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            mainTask = Task.Run(() => MainLoop(Options.FirstRunDelay, cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        public void Stop()
        {
            logger.LogInformation("<{0}>.Stop() called...", RunnableType.Name);
            if (mainTask == null)
                throw new InvalidOperationException("Can't stop without start");

            cancellationTokenSource.Cancel();
        }

        public void TryRunImmediately()
        {
            if (mainTask == null)
                throw new InvalidOperationException("Can't run without Start");

            runImmediately.Set();
        }

        protected void MainLoop(TimeSpan firstRunDelay, CancellationToken cancellationToken)
        {
            logger.LogInformation("<{0}> started. Running...", RunnableType.Name);
            var sleepInterval = firstRunDelay;
            var handles = new[] { cancellationToken.WaitHandle, runImmediately };
            while (true)
            {
                logger.LogDebug("<{0}> Sleeping for {1}...", RunnableType.Name, sleepInterval);
                RunStatus.NextRunTime = DateTimeOffset.Now.Add(sleepInterval);
                try
                {
                    WaitHandle.WaitAny(handles, sleepInterval);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    // must stop and quit
                    logger.LogWarning("CancellationToken signaled, stopping...");
                    return;
                }

                logger.LogDebug("<{0}> Creating scope...", RunnableType.Name);
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    if (Options.RunCulture != null)
                    {
                        logger.LogDebug("Switching to {0} CultureInfo...", Options.RunCulture.Name);
                        CultureInfo.CurrentCulture = Options.RunCulture;
                        CultureInfo.CurrentUICulture = Options.RunCulture;
                    }

                    try
                    {
                        var beforeRunResponse = OnBeforeRun(scope.ServiceProvider);

                        if (!beforeRunResponse)
                            logger.LogInformation("Task run cancelled (BeforeRun returned 'false')");
                        else
                        {
                            IsRunningRightNow = true;

                            var startTime = DateTimeOffset.Now;

                            var runnable = (TRunnable)scope.ServiceProvider.GetRequiredService(typeof(TRunnable));

                            logger.LogInformation("Calling <{0}>.Run()...", RunnableType.Name);
                            runnable.RunAsync(this, scope.ServiceProvider, cancellationToken).GetAwaiter().GetResult();
                            logger.LogInformation("Done.");

                            RunStatus.LastRunTime = startTime;
                            RunStatus.LastResult = TaskRunResult.Success;
                            RunStatus.LastSuccessTime = DateTimeOffset.Now;
                            RunStatus.FirstFailTime = DateTimeOffset.MinValue;
                            RunStatus.FailsCount = 0;
                            RunStatus.LastException = null;
                            IsRunningRightNow = false;

                            OnAfterRunSuccess(scope.ServiceProvider);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(0, ex, "Error in <{0}>.Run() (ignoring, see RunStatus.LastException or handle AfterRunFail event)", RunnableType.Name);
                        RunStatus.LastResult = TaskRunResult.Fail;
                        RunStatus.LastException = ex;
                        if (RunStatus.FailsCount == 0)
                            RunStatus.FirstFailTime = DateTimeOffset.Now;

                        RunStatus.FailsCount++;
                        IsRunningRightNow = false;

                        OnAfterRunFail(scope.ServiceProvider, ex);
                    }
                    finally
                    {
                        IsRunningRightNow = false;
                    }
                }

                if (Options.Interval.Ticks == 0)
                {
                    logger.LogWarning("Interval equal to zero. Stopping...");
                    cancellationTokenSource.Cancel();
                }
                else
                    sleepInterval = Options.Interval; // return to normal (important after first run only)
            }
        }

        /// <summary>
        /// Invokes BeforeRun handler (don't forget to call base.OnBeforeRun in override)
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> to be passed in event args</param>
        /// <returns>Return <b>falce</b> to cancel/skip task run</returns>
        protected virtual bool OnBeforeRun(IServiceProvider serviceProvider)
        {
            return Options.BeforeRun?.Invoke(serviceProvider, this).GetAwaiter().GetResult() ?? true;
        }

        /// <summary>
        /// Invokes AfterRunSuccess handler (don't forget to call base.OnAfterRunSuccess in override)
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> to be passed in event args</param>
        /// <remarks>
        /// Attention! Any exception, catched during AfterRunSuccess.Invoke, is written to error log and ignored.
        /// </remarks>
        protected virtual void OnAfterRunSuccess(IServiceProvider serviceProvider)
        {
            try
            {
                Options.AfterRunSuccess?.Invoke(serviceProvider, this).GetAwaiter().GetResult();
            }
            catch (Exception ex2)
            {
                logger.LogError(0, ex2, "Error while processing AfterRunSuccess event (ignored)");
            }
        }

        /// <summary>
        /// Invokes AfterRunFail handler - don't forget to call base.OnAfterRunSuccess in override
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> to be passed in event args</param>
        /// <param name="ex"><see cref="Exception"/> to be passes in event args</param>
        /// <remarks>
        /// Attention! Any exception, catched during AfterRunFail.Invoke, is written to error log and ignored.
        /// </remarks>
        protected virtual void OnAfterRunFail(IServiceProvider serviceProvider, Exception ex)
        {
            try
            {
                Options.AfterRunFail?.Invoke(serviceProvider, this, ex).GetAwaiter().GetResult();
            }
            catch (Exception ex2)
            {
                logger.LogError(0, ex2, "Error while processing AfterRunFail event (ignored)");
            }
        }
    }
}
