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
        public AsyncJob Job { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            Job.Finished.WaitOne();
        }
    }
}
