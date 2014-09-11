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

        [Parameter(ParameterSetName = "Default", Mandatory = true, ValueFromPipeline = true)]
        public List<AsyncJob> InputObject { get; set; }

        protected override void ProcessRecord()
        {
            AsyncJob[] jobs;
            if (this.ParameterSetName == "ID")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Id.Contains(j.Id)).ToArray(); }
            else if (this.ParameterSetName == "Name")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Name.Contains(j.Name)).ToArray(); }
            else
            { jobs = PSRunspace.Instance.JobQueue.ToArray(); }

            foreach (AsyncJob j in jobs)
            { WriteObject(j.GetJob()); }
        }
    }
}
