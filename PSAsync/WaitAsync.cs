using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading;
using System.Collections;
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
            Tasks = new List<Task<AsyncJob>>();
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
                jobs.AddRange(from j in this.JobQueue
                              where this.Id.Contains(j.Id)
                              select j);
            }

            if (this.ParameterSetName == "NameParameterSet")
            {
                jobs.AddRange(from j in this.JobQueue
                              where this.Name.Contains(j.Name)
                              select j);
            }

            if (this.ParameterSetName == "StateParameterSet")
            {
                jobs.AddRange(from j in this.JobQueue
                              where j.JobStateInfo.State == this.State
                              select j);
            }

            if (this.ParameterSetName == "InstanceIdParameterSet")
            {
                jobs.AddRange(from j in this.JobQueue
                              where this.InstanceId.Contains(j.InstanceId)
                              select j);
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
                { continue; }
                this.Tasks.Add(Task<AsyncJob>.Factory.StartNew((o) =>
                      {
                          var Job = (AsyncJob)o;
                          Job.Finished.WaitOne();
                          return Job;
                      }, job));
            }
        }

        List<Task<AsyncJob>> Tasks;

        protected override void EndProcessing()
        {
            var tasks = this.Tasks.ToArray();
            if (this.Any.IsPresent)
            {
                int ret = Task.WaitAny(this.Tasks.ToArray(), this.Timeout);
                var t = this.Tasks[ret];
                WriteObject(t.Result);
            }
            else if (this.ShowProgress.IsPresent)
            {
                double totalCount = this.Tasks.Count;
                int completedCount = 0;
                while (this.Tasks.Count > 0)
                {
                    var progress = new ProgressRecord(1, "Wating for Async Threads", string.Format("Waiting for {0} threads", this.Tasks.Count));
                    progress.PercentComplete = (int)(completedCount / totalCount * 100);
                    var running = this.JobQueue.Where(j => j.JobStateInfo.State == JobState.Running);
                    progress.CurrentOperation = string.Format("{0} threads currently running", running.Count());
                    WriteProgress(progress);
                    int ret = Task.WaitAny(this.Tasks.ToArray(), 100);
                    if (ret != -1)
                    {
                        var t = this.Tasks[ret];
                        this.Tasks.RemoveAt(ret);
                        WriteObject(t.Result);
                        completedCount++;
                    }
                }
            }
            else
            {
                if (Task.WaitAll(this.Tasks.ToArray(), this.Timeout))
                {
                    foreach (var t in this.Tasks)
                    { WriteObject(t.Result); }
                }
            }
            this.JobQueue.Clear();
        }
    }
}
