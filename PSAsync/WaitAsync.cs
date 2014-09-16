using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading;
using System.Collections;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Wait, "Async")]
    [CmdletBinding(DefaultParameterSetName = "SessionIdParameterSet")]
    public class WaitAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "SessionIdParameterSet", Position = 1, Mandatory = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "FilterParameterSet", Position = 1, Mandatory = true)]
        public Hashtable Filter { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet", Position = 1, Mandatory = true)]
        public Guid[] InstanceId { get; set; }

        [Parameter(ParameterSetName = "JobParameterSet", Position = 1, Mandatory = true)]
        public AsyncJob[] Job { get; set; }

        [Parameter(ParameterSetName = "NameParameterSet", Position = 1, Mandatory = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "StateParameterSet", Position = 1, Mandatory = true)]
        public JobState State { get; set; }

        protected override void BeginProcessing()
        { waitHandles = new List<WaitHandle>(); }

        List<WaitHandle> waitHandles;
        protected override void ProcessRecord()
        { waitHandles.Add(this.Job.Select(j => j.Finished).First()); }

        protected override void EndProcessing()
        {
            foreach (var handle in this.waitHandles)
            { handle.WaitOne(); }
        }
    }
}
