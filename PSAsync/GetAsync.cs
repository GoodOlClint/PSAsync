using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Get, "Async")]
    [CmdletBinding(DefaultParameterSetName = "Default")]
    public class GetAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "State", Mandatory = true)]
        public JobState State { get; set; }

        [Parameter(ParameterSetName = "Default")]
        public List<AsyncJob> InputObject { get; set; }

        protected override void ProcessRecord()
        {
            AsyncJob[] jobs;
            if (this.ParameterSetName == "ID")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Id.Contains(j.Id)).ToArray(); }
            else if (this.ParameterSetName == "Name")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Name.Contains(j.Name)).ToArray(); }
            else if (this.ParameterSetName == "State")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => j.JobStateInfo.State == this.State).ToArray(); }
            else
            { jobs = PSRunspace.Instance.JobQueue.ToArray(); }
            WriteObject(jobs);
        }
    }
}
