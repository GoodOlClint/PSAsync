﻿using System.Collections.Generic;
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

        private PSRunspace()
        {
            this.PoolSize = 50;
            this.IsOpen = false;
            this.JobQueue = new ConcurrentDictionary<Guid, AsyncJob>();
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
                {                    this.pool = RunspaceFactory.CreateRunspacePool(1, this.PoolSize);                }
                this.pool.Open();
            }
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
        {
            this.JobQueue.TryAdd(Job.InstanceId, Job);
            Job.StartJob();
        }

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
