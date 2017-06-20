using System;
using System.Collections;
using System.Collections.Generic;

namespace U3D.Threading.Tasks
{
    public class Task<TResult> : Task
    {
        public TResult Result { get; private set; }
		
		// internal helper function breaks out logic used by TaskCompletionSource
		public Task()
			: base()
		{
			Result = default(TResult);
		}
		
		public bool TrySetResult(TResult result)
		{
			if (IsCompleted) return false;
			
			if (m_state == TState.Created)
				m_state = TState.Running;
			Result = result;
			m_state = TState.Successful;
			return true;
		}
		public bool TrySetError(Exception e)
		{
			if (IsCompleted) return false;
			
			if(m_state== TState.Created)
				m_state = TState.Running;
			Exception = new AggregateException(e);
			m_state = TState.Faulted;
			return true;
		}
		// end public functions for TaskCompletionSource

		protected Task(Func<TResult> f)
            : base()
        {
            Result = default(TResult);
			m_action= () => {
				Result = f();
			};
        }

		public static Task<TResult> Run(Func<TResult> action)
		{
			Task<TResult> t = new Task<TResult>(action);
			t.RunAsync ();
			return t;
		}
        public static Task<TResult> RunInMainThread(Func<TResult> action)
        {
            Dispatcher.Initialize();
            return Dispatcher.instance.TaskToMainThread(action);
        }

        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
            return Task.Run(() =>
            {
                this.Wait();
                continuationAction(this);
            });
        }
        public Task ContinueInMainThreadWith(Action<Task<TResult>> continuationAction)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                this.Wait();
                Dispatcher.instance.ToMainThread(() =>
                {
                    try
                    {
                        continuationAction(this);
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        tcs.SetError(e);
                    }
                });
            });
            return tcs.Task;
        }

        internal void SetIsRunning()
        {
            m_state = TState.Running;
        }
    }
}