using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Remove, "Async")]
    [CmdletBinding(DefaultParameterSetName = "Job")]
    public class RemoveAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "ID", Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "Name", Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "Job", ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

        protected override void ProcessRecord()
        {
            //TODO: This should fail if the job is currently running unless -Force is specified
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
            {
                AsyncJob nullJob;
                PSRunspace.Instance.JobQueue.TryRemove(j.InstanceId, out nullJob);
                nullJob.Dispose();
            }
        }
    }
}
