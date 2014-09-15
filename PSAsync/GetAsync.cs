using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Get, "Async")]
    [CmdletBinding(DefaultParameterSetName = "SessionIdParameterSet")]
    public class GetAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "SessionIdParameterSet", Position = 1)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet", Position = 1)]
        public Guid[] InstanceID { get; set; }

        [Parameter(ParameterSetName = "NameParameterSet", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "StateParameterSet", Mandatory = true)]
        public JobState State { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public DateTime? After { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public DateTime? Before { get; set; }

        protected override void ProcessRecord()
        {

            List<AsyncJob> jobs = new List<AsyncJob>();
            if (this.Id != null)
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
                              where this.InstanceID.Contains(j.Value.InstanceId)
                              select j.Value);
            }

            if (jobs.Count == 0)
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              select j.Value);
            }

            if ((this.After != null) && (this.Before != null))
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               where j.PSEndTime <= this.Before
                               select j;
                jobs = tempJobs.ToList();
            }
            else if (this.After != null)
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               select j;
                jobs = tempJobs.ToList();
            }
            else if (this.Before != null)
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               select j;
                jobs = tempJobs.ToList();
            }


            foreach (var job in jobs)
            { WriteObject(job); }
        }
    }
}
