using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Linq;
using System;

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
            this.IsOpen = false;
            this.JobQueue = new ConcurrentDictionary<Guid, AsyncJob>();
            this.Settings = new RunspaceSettings();
        }

        internal RunspacePool pool;
        public RunspaceSettings Settings { get; set; }

        public bool IsOpen { get; private set; }

        internal void Initalize()
        {
            if (!this.IsOpen)
            {
                this.pool = RunspaceFactory.CreateRunspacePool(1, this.Settings.PoolSize);
                this.WorkLimit = new Semaphore(this.Settings.PoolSize, this.Settings.PoolSize);
                Thread t = new Thread(this.StartJobs);
                t.Start();
            }
        }

        public void Open()
        {
            if (!this.IsOpen)
            {
                this.Initalize();
                this.IsOpen = true;
                this.pool.Open();
            }
        }

        private void StartJobs()
        {
            while (this.IsOpen)
            {
                var NewJobs = this.JobQueue.Where(j => j.Value.JobStateInfo.State == JobState.NotStarted).Select(j => j.Value).OrderBy(j => j.Id);
                if (NewJobs.Count() > 0)
                {
                    this.WorkLimit.WaitOne();
                    AsyncJob data = NewJobs.First();
                    data.StartJob();
                    data.StateChanged += data_StateChanged;
                }
                Thread.Sleep(250);
            }
        }

        void data_StateChanged(object sender, JobStateEventArgs e)
        {
            if (e.JobStateInfo.State != JobState.Running)
            { this.WorkLimit.Release(); }
        }

        public PowerShell NewPipeline()
        {
            this.Open();
            var pipeline = PowerShell.Create();
            pipeline.RunspacePool = this.pool;
            return pipeline;
        }

        internal ConcurrentDictionary<Guid, AsyncJob> JobQueue;
        public void AddJob(AsyncJob Job)
        { this.JobQueue.TryAdd(Job.InstanceId, Job); }

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
