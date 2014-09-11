using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    public class AsyncJob : Job
    {
        public AsyncJob(ScriptBlock Script)
        {
            this.Pipeline = PSRunspace.Instance.NewPipeline();
            this.Pipeline.AddScript(Script.ToString());
            this.Pipeline.InvocationStateChanged += Pipeline_InvocationStateChanged;
            this.PSJobTypeName = "RunspaceJob";
        }

        void Pipeline_InvocationStateChanged(object sender, PSInvocationStateChangedEventArgs e)
        {
            switch (e.InvocationStateInfo.State)
            {
                case PSInvocationState.Completed:
                    this.SetJobState(JobState.Completed);
                    break;
                case PSInvocationState.Failed:
                    this.SetJobState(JobState.Failed);
                    break;
                case PSInvocationState.NotStarted:
                    this.SetJobState(JobState.NotStarted);
                    break;
                case PSInvocationState.Running:
                    this.SetJobState(JobState.Running);
                    break;
                case PSInvocationState.Stopped:
                    this.SetJobState(JobState.Stopped);
                    break;
                case PSInvocationState.Stopping:
                    this.SetJobState(JobState.Stopping);
                    break;
            }
        }

        public AsyncJob(ScriptBlock Script, object[] Arguments)
            : this(Script)
        {
            foreach (var arg in Arguments)
            { this.Pipeline.AddArgument(arg); }
        }

        public AsyncJob(ScriptBlock Script, Hashtable Parameters)
            : this(Script)
        {
            this.Pipeline.AddParameters(Parameters);
        }

        private IAsyncResult AsyncResults { get; set; }
        private PowerShell Pipeline { get; set; }
        private bool hasRead = false;

        public bool IsFinished { get { return this.AsyncResults.IsCompleted; } }

        public override bool HasMoreData
        { get { return !this.hasRead; } }

        public override string Location
        { get { return Environment.MachineName; } }

        public override string StatusMessage
        { get { return this.Pipeline.InvocationStateInfo.State.ToString(); } }

        public override void StopJob()
        { this.Pipeline.Stop(); }

        public void StartJob()
        { this.AsyncResults = this.Pipeline.BeginInvoke(); }

        public PSDataCollection<PSObject> GetJob()
        {
            this.hasRead = true;
            PSDataCollection<PSObject> data;
            data = this.Pipeline.EndInvoke(this.AsyncResults);
            foreach(var err in this.Pipeline.Streams.Error)
            { data.Add(new PSObject(err)); }
            this.Dispose();
            return data;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Pipeline.Dispose();
                this.Pipeline = null;
                this.AsyncResults = null;
            }
            base.Dispose(disposing);
        }
    }
}
