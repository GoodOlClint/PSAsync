using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Stop, "Async")]
    [CmdletBinding(DefaultParameterSetName = "Default")]
    public class StopAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "Default", ValueFromPipeline = true)]
        public AsyncJob[] InputObject { get; set; }

        protected override void ProcessRecord()
        {
            AsyncJob[] jobs;
            if (this.ParameterSetName == "ID")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Id.Contains(j.Id)).ToArray(); }
            else if (this.ParameterSetName == "Name")
            { jobs = PSRunspace.Instance.JobQueue.Where(j => this.Name.Contains(j.Name)).ToArray(); }
            else if (this.InputObject != null)
            { jobs = this.InputObject; }
            else
            { jobs = PSRunspace.Instance.JobQueue.ToArray(); }

            foreach (AsyncJob j in jobs)
            { j.StopJob(); }
        }
    }
}
