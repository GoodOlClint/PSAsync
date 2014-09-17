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
        [Parameter(ParameterSetName = "ScriptBlock", Position = 1, Mandatory = true)]
        [Alias("Script", "Code")]
        public ScriptBlock ScriptBlock { get; set; }

        [Parameter(ParameterSetName = "FilePath", Position = 1, Mandatory = true)]
        [Alias("File", "Path")]
        public string FilePath { get; set; }

        [Parameter()]
        [Alias("Init")]
        public ScriptBlock InitializationScript { get; set; }

        [Parameter(ParameterSetName = "ScriptBlock", Position = 2)]
        [Parameter(ParameterSetName = "FilePath", Position = 2)]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "ScriptBlock", Position = 2)]
        [Parameter(ParameterSetName = "FilePath", Position = 2)]
        public Hashtable Parameters { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            ScriptBlock sc = this.ScriptBlock;

            if (!string.IsNullOrEmpty(this.FilePath))
            { sc = ScriptBlock.Create(this.FilePath); }

            if (this.InitializationScript != null)
            { sc = ScriptBlock.Create(this.InitializationScript.ToString() + sc.ToString()); }
            AsyncJob job;

            if (this.ArgumentList != null)
            { job = new AsyncJob(sc, this.ArgumentList); }
            if (this.Parameters != null)
            { job = new AsyncJob(sc, this.Parameters); }
            else
            { job = new AsyncJob(sc); }
            PSRunspace.Instance.AddJob(job);
            WriteObject(job);
        }
    }
}
