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

        protected override void ProcessRecord()
        {
            List<AsyncJob> jobs = new List<AsyncJob>();
            if (this.ParameterSetName == "ID")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Id.Contains(j.Value.Id)
                              select j.Value);
            }
            
            if (this.ParameterSetName == "Name")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Name.Contains(j.Value.Name)
                              select j.Value);
            }
            
            if(this.ParameterSetName == "Job")
            { jobs.AddRange(this.Job); }

            foreach (AsyncJob j in jobs)
            {
                WriteObject(j.GetJob(false));
            }
        }
    }
}
