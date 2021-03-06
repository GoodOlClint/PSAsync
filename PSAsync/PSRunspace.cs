﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace PSAsync
{
    public sealed class PSRunspace : IDisposable
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

        #region Public Properties
        public RunspaceSettings Settings { get { return new RunspaceSettings(this.pool); } }
        public bool IsOpen { get; private set; }
        #endregion

        #region Internal Properties
        internal RunspacePool pool { get; set; }
        internal ConcurrentDictionary<Guid, AsyncJob> JobQueue { get; set; }
        #endregion

        #region Private Properties
        private Semaphore WorkLimit { get; set; }
        #endregion

        #region Constructor
        private PSRunspace()
        {
            this.IsOpen = false;
            this.JobQueue = new ConcurrentDictionary<Guid, AsyncJob>();
        }
        #endregion

        #region Public Methods
        public void LoadSettings(RunspaceSettings settings, PSHost host)
        {
            if (!this.IsOpen)
            { this.pool = settings.ToPool(host); }
        }

        public PowerShell NewPipeline()
        {
            this.Open();
            var pipeline = PowerShell.Create();
            pipeline.RunspacePool = this.pool;
            return pipeline;
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

        public void AddJob(AsyncJob Job)
        { this.JobQueue.TryAdd(Job.InstanceId, Job); }

        public void Close()
        {
            if (!this.IsOpen)
            {
                this.IsOpen = false;
                if (this.pool != null)
                {
                    this.pool.Close();
                    this.pool.Dispose();
                    this.pool = null;
                }
                if (this.JobQueue != null)
                {
                    this.JobQueue.Clear();
                    this.JobQueue = null;
                }
            }
        }
        #endregion

        #region Internal Methods
        internal void Initalize()
        {
            if (!this.IsOpen)
            {
                if (this.pool == null)
                { this.pool = RunspaceFactory.CreateRunspacePool(1, this.Settings.PoolSize); }
                this.WorkLimit = new Semaphore(this.Settings.PoolSize, this.Settings.PoolSize);
                Thread t = new Thread(this.StartJobs);
                t.Start();
            }
        }
        #endregion

        #region Private Methods
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
                Thread.Sleep(20);
            }
        }
        #endregion

        #region Event Handlers
        void data_StateChanged(object sender, JobStateEventArgs e)
        {
            if (e.JobStateInfo.State != JobState.Running)
            { this.WorkLimit.Release(); }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        { this.Dispose(true); }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
                this.WorkLimit.Dispose();
            }
        }

        ~PSRunspace()
        { this.Dispose(false); }
        #endregion
    }
}
