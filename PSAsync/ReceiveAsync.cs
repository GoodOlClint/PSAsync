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

        [Parameter(ParameterSetName = "Default", ValueFromPipeline = true)]
        public List<AsyncJob> InputObject { get; set; }

        protected override void ProcessRecord()
        {
            IEnumerable<AsyncJob> jobs;
            if (this.ParameterSetName == "ID")
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       where this.Id.Contains(j.Value.Id)
                       select j.Value;
            }
            else if (this.ParameterSetName == "Name")
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       where this.Name.Contains(j.Value.Name)
                       select j.Value;
            }
            else
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       select j.Value;
            }

            foreach (AsyncJob j in jobs)
            { WriteObject(j.GetJob()); }
        }
    }
}
