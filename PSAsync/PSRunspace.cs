using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace PSAsync
{
    public sealed class PSRunspace
    {
        #region Singleton Instance
        private static volatile PSRunspace instance;
        private static object syncRoot = new object();
        public static PSRunspace Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        { instance = new PSRunspace(); }
                    }
                }
                return instance;
            }
        }
        #endregion

        private Semaphore WorkLimit;

        private PSRunspace()
        {
            this.PoolSize = 20;
            this.IsOpen = false;
            this.JobQueue = new ConcurrentQueue<AsyncJob>();
        }

        private RunspacePool pool;

        public bool IsOpen { get; private set; }
        public int PoolSize { get; set; }

        public void Open()
        {
            if (!this.IsOpen)
            {
                this.IsOpen = true;
                if (this.pool == null)
                {
                    this.pool = RunspaceFactory.CreateRunspacePool(1, this.PoolSize);
                    this.WorkLimit = new Semaphore(this.PoolSize, this.PoolSize);
                    Thread t = new Thread(this.StartJobs);
                    t.Start();
                }
                this.pool.Open();
            }
        }

        private void StartJobs()
        {
            if (this.JobQueue.Count > 0)
            {
                this.WorkLimit.WaitOne();
                AsyncJob data;
                if (this.JobQueue.TryDequeue(out data))
                {
                    data.StartJob();
                    data.StateChanged += data_StateChanged;
                }
            }
            Thread.Sleep(250);
            this.StartJobs();
        }

        void data_StateChanged(object sender, JobStateEventArgs e)
        {
            if (e.JobStateInfo.State == JobState.Completed)
            { this.WorkLimit.Release(); }
        }

        public PowerShell NewPipeline()
        {
            this.Open();
            var pipeline = PowerShell.Create();
            pipeline.RunspacePool = this.pool;
            return pipeline;
        }

        private ConcurrentQueue<AsyncJob> JobQueue;
        public void AddJob(AsyncJob Job)
        { this.JobQueue.Enqueue(Job); }

        public void Close()
        {
            if (!this.IsOpen)
            {
                this.IsOpen = false;
                if (null != this.pool)
                {
                    this.pool.Close();
                    this.pool.Dispose();
                    this.pool = null;
                }
            }
        }

        ~PSRunspace()
        {
            if (this.IsOpen)
            { this.Close(); }
        }
    }
}
