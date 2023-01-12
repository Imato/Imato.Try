using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Imato.Try
{
    public class Execution
    {
        protected Action<Exception> defaultOnError = (Exception ex) => throw ex;
        private List<Func<CancellationToken, Task>> onExecuteAsync = null!;
        private List<Action> onExecute = null!;
        protected List<Action<Exception>> onError = null!;
        internal TryOptions Options { get; set; } = new TryOptions();

        internal void AddFunction(Func<Task> func)
        {
            if (onExecuteAsync == null)
            {
                onExecuteAsync = new List<Func<CancellationToken, Task>>();
            }
            onExecuteAsync.Add((CancellationToken token) => func());
        }

        internal void AddFunction(Func<CancellationToken, Task> func)
        {
            if (onExecuteAsync == null)
            {
                onExecuteAsync = new List<Func<CancellationToken, Task>>();
            }
            onExecuteAsync.Add(func);
        }

        internal void AddFunction(Action func)
        {
            if (onExecute == null)
            {
                onExecute = new List<Action>();
            }
            onExecute.Add(func);
        }

        internal void AddOnError(Action<Exception> handler)
        {
            if (onError == null)
            {
                onError = new List<Action<Exception>>();
            }
            onError.Add(handler);
        }

        protected async Task OnError(Exception ex)
        {
            if (Options.RetryCount == 1 && Options.ErrorOnFail)
            {
                if (onError != null)
                {
                    foreach (var error in onError)
                    {
                        error(ex);
                    }
                }
                else
                {
                    defaultOnError(ex);
                }
            }

            Options.RetryCount--;
            if (Options.Delay > 0 && Options.RetryCount > 0)
            {
                await Task.Delay(Options.Delay);
            }
        }

        /// <summary>
        /// Execute added function(s)
        /// </summary>
        /// <exception cref="Exception"></exception>
        public async Task ExecuteAsync()
        {
            if (onExecuteAsync == null && onExecute == null)
            {
                throw new Exception("Add Function(s) to execute first");
            }

            var hasError = false;
            while (Options.RetryCount > 0)
            {
                if (onExecuteAsync != null)
                {
                    foreach (var execution in onExecuteAsync)
                    {
                        try
                        {
                            await execution(Token);
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            await OnError(ex);
                        }
                    }
                }

                if (onExecute != null)
                {
                    foreach (var execution in onExecute)
                    {
                        try
                        {
                            execution();
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            await OnError(ex);
                        }
                    }
                }

                if (!hasError) return;
            }
        }

        /// <summary>
        /// Execute added function(s)
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Execute()
        {
            ExecuteAsync().Wait();
        }

        /// <summary>
        /// Get new token
        /// </summary>
        public CancellationToken Token
        {
            get
            {
                var source = new CancellationTokenSource();
                source.CancelAfter(Options.Timeout);
                return source.Token;
            }
        }
    }

    public class Execution<T> : Execution
    {
        private List<Func<CancellationToken, Task<T>>> onExecuteAsync = null!;
        private List<Func<T>> onExecute = null!;
        internal T Default { get; set; } = default;

        internal void AddFunction(Func<Task<T>> func)
        {
            if (onExecuteAsync == null)
            {
                onExecuteAsync = new List<Func<CancellationToken, Task<T>>>();
            }
            onExecuteAsync.Add((_) => func());
        }

        internal void AddFunction(Func<CancellationToken, Task<T>> func)
        {
            if (onExecuteAsync == null)
            {
                onExecuteAsync = new List<Func<CancellationToken, Task<T>>>();
            }
            onExecuteAsync.Add(func);
        }

        internal void AddFunction(Func<T> func)
        {
            if (onExecute == null)
            {
                onExecute = new List<Func<T>>();
            }
            onExecute.Add(func);
        }

        /// <summary>
        /// Get result from pipeline function(s)
        /// </summary>
        /// <returns>T Result of added function(s)</returns>
        /// <exception cref="EmptyResultException">Then has not result</exception>
        public async Task<T> GetResultNotEmptyAsync()
        {
            var resutl = await GetResultAsync();
            if (resutl != null) return resutl;
            if (Default != null) return Default;
            throw new EmptyResultException();
        }

        /// <summary>
        /// Get result from pipeline function(s)
        /// </summary>
        /// <returns>T Result of added function(s)</returns>
        /// <exception cref="EmptyResultException">Then has not result</exception>
        public T GetResultNotEmpty()
        {
            var resutl = GetResult();
            if (resutl != null) return resutl;
            if (Default != null) return Default;
            throw new EmptyResultException();
        }

        /// <summary>
        /// Get result from pipeline function(s)
        /// </summary>
        /// <returns>T Result of added function(s)</returns>
        /// <exception cref="SetupExecutionException"></exception>
        public async Task<T> GetResultAsync()
        {
            if (onExecuteAsync == null && onExecute == null)
            {
                throw new SetupExecutionException("Add Function(s) to execute first");
            }

            if (onExecuteAsync != null)
            {
                foreach (var execution in onExecuteAsync)
                {
                    while (Options.RetryCount > 0)
                    {
                        try
                        {
                            return await execution(Token);
                        }
                        catch (Exception ex)
                        {
                            await OnError(ex);
                        }
                    }
                }
            }

            if (onExecute != null)
            {
                foreach (var execution in onExecute)
                {
                    while (Options.RetryCount > 0)
                    {
                        try
                        {
                            return execution();
                        }
                        catch (Exception ex)
                        {
                            await OnError(ex);
                        }
                    }
                }
            }

            return Default;
        }

        /// <summary>
        /// Get result from pipeline function(s)
        /// </summary>
        /// <returns>T Result of added function(s)</returns>
        /// <exception cref="SetupExecutionException"></exception>
        public T GetResult()
        {
            return GetResultAsync().Result;
        }
    }
}