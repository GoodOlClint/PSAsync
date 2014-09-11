using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Start, "Async")]
    [CmdletBinding(DefaultParameterSetName = "ScriptBlock")]
    public class StartAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "Arguments", Position = 1, Mandatory = true)]
        [Parameter(ParameterSetName = "Parameters", Position = 1, Mandatory = true)]
        [Parameter(ParameterSetName = "ScriptBlock", Position = 1, Mandatory = true)]
        [Alias("Script", "Code")]
        public ScriptBlock ScriptBlock { get; set; }

        [Parameter()]
        [Alias("Init")]
        public ScriptBlock InitializationScript { get; set; }

        [Parameter(ParameterSetName = "Arguments", Position = 2, Mandatory = true)]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "Parameters", Position = 2, Mandatory = true)]
        public Hashtable Parameters { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ScriptBlock sc = this.ScriptBlock;

            if (this.InitializationScript != null)
            { sc = ScriptBlock.Create(this.InitializationScript.ToString() + this.ScriptBlock.ToString()); }
            AsyncJob job;
            if (this.ParameterSetName == "Arguments")
            { job = new AsyncJob(sc, this.ArgumentList); }
            else if (this.ParameterSetName == "Parameters")
            { job = new AsyncJob(sc, this.Parameters); }
            else
            { job = new AsyncJob(sc); }
            PSRunspace.Instance.AddJob(job);
            WriteObject(job);
        }
    }
}
