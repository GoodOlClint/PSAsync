using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Stop, "Async")]
    [CmdletBinding(DefaultParameterSetName = "Job")]
    public class StopAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "Job", ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

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
            else if (this.Job != null)
            {
                jobs = from j in this.Job
                       select j;
            }
            else
            {
                jobs = from j in PSRunspace.Instance.JobQueue
                       select j.Value;
            }

            foreach (var j in jobs)
            { j.StopJob(); }
        }
    }
}
