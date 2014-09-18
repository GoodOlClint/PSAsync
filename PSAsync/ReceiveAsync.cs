using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommunications.Receive, "Async")]
    [CmdletBinding(DefaultParameterSetName = "ID")]
    public class ReceiveAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "Job", ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

        public List<AsyncJob> JobQueue;
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.JobQueue = PSRunspace.Instance.JobQueue.Select(j => j.Value).ToList();
        }

        protected override void ProcessRecord()
        {
            List<AsyncJob> jobs = new List<AsyncJob>();
            if (this.ParameterSetName == "ID")
            {
                jobs.AddRange(from j in this.JobQueue
                              where this.Id.Contains(j.Id)
                              select j);
            }

            if (this.ParameterSetName == "Name")
            {
                jobs.AddRange(from j in this.JobQueue
                              where this.Name.Contains(j.Name)
                              select j);
            }

            if (this.ParameterSetName == "Job")
            { jobs.AddRange(this.Job); }

            foreach (AsyncJob j in jobs)
            {
                WriteObject(j.GetJob(false));
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            this.JobQueue.Clear();
        }
    }
}
