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
                    //double currentCount = this.Tasks.Count;
                    int ret = Task.WaitAny(this.Tasks.ToArray(), this.Timeout);
                    var t = this.Tasks[ret];
                    this.Tasks.RemoveAt(ret);
                    var progress = new ProgressRecord(1, "Wating for Async Threads", string.Format("{0} threads remaining", this.Tasks.Count));
                    progress.PercentComplete = (int)(completedCount / totalCount * 100);
                    WriteProgress(progress);
                    WriteObject(t.Result);
                    completedCount++;
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
        }
    }
}
