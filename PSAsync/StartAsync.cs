using System.Collections;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsLifecycle.Start, "Async", DefaultParameterSetName = "ScriptBlock")]
    [OutputType(new[] { typeof(AsyncJob) })]
    public class StartAsync : PSCmdlet
    {
        [Alias("Script", "Code")]
        [Parameter(ParameterSetName = "ScriptBlock", Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public ScriptBlock ScriptBlock { get; set; }

        [Alias("File", "Path")]
        [Parameter(ParameterSetName = "FilePath", Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        [Parameter(ParameterSetName = "FilePath", Position = 1)]
        [Parameter(ParameterSetName = "ScriptBlock", Position = 1)]
        [Alias("Init")]
        public ScriptBlock InitializationScript { get; set; }

        [Parameter(ParameterSetName = "FilePath")]
        [Parameter(ParameterSetName = "ScriptBlock")]
        public object[] ArgumentList { get; set; }

        [Parameter(ParameterSetName = "FilePath")]
        [Parameter(ParameterSetName = "ScriptBlock")]
        public Hashtable Parameters { get; set; }

        [Parameter(ParameterSetName = "FilePath")]
        [Parameter(ParameterSetName = "ScriptBlock")]
        public string Name { get; set; }

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
            job.Name = this.Name;
            PSRunspace.Instance.AddJob(job);
            WriteObject(job);
        }
    }
}

