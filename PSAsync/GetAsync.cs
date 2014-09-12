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

        [Parameter()]
        public DateTime After { get; set; }

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
            else if (this.ParameterSetName == "State")
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       where j.Value.JobStateInfo.State == this.State
                       select j.Value;
            }
            else if (this.After != null)
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       where j.Value.PSEndTime >= this.After
                       select j.Value;
            }
            else
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       select j.Value;
            }

            WriteObject(jobs);
        }
    }
}
