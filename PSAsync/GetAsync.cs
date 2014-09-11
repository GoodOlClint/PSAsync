using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Get, "Async")]
    [CmdletBinding(DefaultParameterSetName = "Job")]
    public class GetAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "State", Mandatory = true)]
        public JobState State { get; set; }

        [Parameter(ParameterSetName = "Job", ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

        protected override void ProcessRecord()
        {
            IEnumerable<KeyValuePair<Guid, AsyncJob>> jobs;
            if (this.ParameterSetName == "ID")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Id.Contains(j.Value.Id)); }
            else if (this.ParameterSetName == "Name")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Name.Contains(j.Value.Name)); }
            else if (this.ParameterSetName == "State")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => j.Value.JobStateInfo.State == this.State); }
            else
            { jobs = PSRunspace.Instance.JobQueue; }
            WriteObject(jobs.Select(j => j.Value));
        }
    }
}
