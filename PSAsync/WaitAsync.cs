using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Wait, "Async")]
    [CmdletBinding(DefaultParameterSetName = "SessionIdParameterSet")]
    public class WaitAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "SessionIdParameterSet", Position = 1, Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "FilterParameterSet", Position = 1, Mandatory = true)]
        public Hashtable Filter { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet", Position = 1, Mandatory = true)]
        public Guid[] InstanceId { get; set; }

        [Parameter(ParameterSetName = "JobParameterSet", Position = 1, Mandatory = true, ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

        [Parameter(ParameterSetName = "NameParameterSet", Position = 1, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "StateParameterSet", Position = 1, Mandatory = true)]
        public JobState State { get; set; }

        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "FilterParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "JobParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public SwitchParameter Any { get; set; }

        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "FilterParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "JobParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public SwitchParameter Force { get; set; }

        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "FilterParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "JobParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public int Timeout { get; set; }

        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "FilterParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "JobParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public SwitchParameter ShowProgress { get; set; }

        public WaitAsync()
        {
            this.Timeout = -1;
            this.cts = new CancellationTokenSource();
            this.Queue = new Queue<AsyncJob>();
            this.queueLock = new object();
        }

        public List<AsyncJob> JobQueue;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.JobQueue = PSRunspace.Instance.JobQueue.Select(j => j.Value).ToList();
        }

        protected override void ProcessRecord()
        {
            List<AsyncJob> jobs = new List<AsyncJob>();
            if (this.ParameterSetName == "SessionIdParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Id.Contains(j.Value.Id)
                              select j.Value);
            }

            if (this.ParameterSetName == "NameParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Name.Contains(j.Value.Name)
                              select j.Value);
            }

            if (this.ParameterSetName == "StateParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where j.Value.JobStateInfo.State == this.State
                              select j.Value);
            }

            if (this.ParameterSetName == "InstanceIdParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.InstanceId.Contains(j.Value.InstanceId)
                              select j.Value);
            }

            if (this.ParameterSetName == "JobParameterSet")
            {
                jobs.AddRange(this.Job);
            }

            if (!this.Force.IsPresent)
            {
                var tempJobs = from j in jobs
                               where j.JobStateInfo.State != JobState.Disconnected
                               where j.JobStateInfo.State != JobState.Suspended
                               select j;
                jobs = tempJobs.ToList();
            }
            if (this.Filter != null)
            {
                var props = typeof(AsyncJob).GetProperties();
                var tempJobs = jobs.ConvertAll(j => j);
                jobs.Clear();
                foreach (DictionaryEntry pair in this.Filter)
                {
                    var myProp = (from prop in props
                                  where prop.Name == pair.Key.ToString()
                                  select prop).First();
                    jobs.AddRange(from j in tempJobs
                                  where myProp.GetValue(j, null) == pair.Value
                                  select j);

                }
            }


            foreach (var job in jobs)
            {
                if (job.JobStateInfo.State == JobState.Completed ||
                    job.JobStateInfo.State == JobState.Failed ||
                    job.JobStateInfo.State == JobState.Stopped)
                {
                    try
                    { WriteObject(Job); }
                    catch (PipelineStoppedException ex)
                    { }
                    continue;
                }

                var action = new Action<object>((o) =>
                {
                    var Job = (AsyncJob)o;
                    while (Job.JobStateInfo.State == JobState.NotStarted)
                    {
                        //DoNothing
                        Thread.Sleep(100);
                    }
                    if (Job.Finished.WaitOne(this.Timeout))
                    {
                        lock (queueLock)
                        { this.Queue.Enqueue(Job); }
                    }
                });
                Task.Factory.StartNew(action, job, this.cts.Token);
                threadCount++;
            }
        }

        CancellationTokenSource cts;
        private Queue<AsyncJob> Queue { get; set; }
        private object queueLock { get; set; }
        private double threadCount;

        protected override void EndProcessing()
        {
            double readCount = 0;
            if (this.Any.IsPresent)
            {
                while (readCount == 0)
                {
                    while (Queue.Count > 0)
                    {
                        AsyncJob Job;
                        lock (queueLock)
                        { Job = this.Queue.Dequeue(); }
                        try
                        { WriteObject(Job); }
                        catch (PipelineStoppedException ex)
                        { }
                        readCount++;
                    }
                }
            }
            else if (this.ShowProgress.IsPresent)
            {
                var progress = new ProgressRecord(1, "Wating for Async Threads", string.Format("{0} out of {1} complete", readCount, threadCount));
                var watch = new Stopwatch();
                watch.Start();
                while (readCount < threadCount)
                {
                    while (Queue.Count > 0)
                    {
                        AsyncJob Job;
                        lock (queueLock)
                        { Job = this.Queue.Dequeue(); }
                        try
                        { WriteObject(Job); }
                        catch (PipelineStoppedException ex)
                        { }
                        readCount++;
                        progress.PercentComplete = (int)(readCount / threadCount * 100);
                        progress.StatusDescription = string.Format("{0} out of {1} complete", readCount, threadCount);
                    }
                    var running = PSRunspace.Instance.JobQueue.Where(j => j.Value.JobStateInfo.State == JobState.Running);
                    progress.CurrentOperation = string.Format("{0}/{1} threads currently running, {2} elapsed", running.Count(), PSRunspace.Instance.Settings.PoolSize, watch.Elapsed);
                    try
                    { WriteProgress(progress); }
                    catch (PipelineStoppedException ex)
                    { }
                    Thread.Sleep(100);
                }
            }
            else
            {
                while (readCount < threadCount)
                {
                    while (Queue.Count > 0)
                    {
                        AsyncJob Job;
                        lock (queueLock)
                        { Job = this.Queue.Dequeue(); }
                        try
                        { WriteObject(Job); }
                        catch (PipelineStoppedException ex)
                        { }
                        readCount++;
                    }
                    Thread.Sleep(100);
                }
            }
            if (readCount != threadCount)
            { this.cts.Cancel(); }
        }
    }
}
