using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Wait, "Async")]
    public class WaitAsync : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true)]
        public AsyncJob[] Job { get; set; }

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
