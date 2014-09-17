using System;
using System.Collections;
using System.Management.Automation;
using System.Threading;

namespace PSAsync
{
    public class AsyncJob : Job
    {
        #region Public Properties
        private bool hasMoreData = true;
        public override bool HasMoreData
        { get { return this.hasMoreData; } }

        public override string Location
        { get { return "localhost"; } }

        public override string StatusMessage
        { get { return this.Pipeline.InvocationStateInfo.State.ToString(); } }
        #endregion

        #region Private Properties
        private IAsyncResult AsyncResults { get; set; }
        private PowerShell Pipeline { get; set; }
        #endregion

        #region Constructors
        public AsyncJob(ScriptBlock Script)
            : base(Script.ToString())
        {
            this.Pipeline = PSRunspace.Instance.NewPipeline();
            this.Pipeline.AddCommand(Script.ToString());
            this.Pipeline.InvocationStateChanged += Pipeline_InvocationStateChanged;
            this.PSJobTypeName = "RunspaceJob";
            this.Name = string.Format("Async{0}", this.Id);
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
            foreach(DictionaryEntry param in Parameters)
            {
                this.Pipeline.AddParameter((string)param.Key, param.Value);
            }
        }


        #endregion

        #region Public Methods
        public override void StopJob()
        { this.Pipeline.Stop(); }

        public void StartJob()
        {
            this.AsyncResults = this.Pipeline.BeginInvoke();
        }

        public PSDataCollection<PSObject> GetJob(bool Keep)
        {
            PSDataCollection<PSObject> data = new PSDataCollection<PSObject>();
            if (HasMoreData)
            {
                data = this.Output;
                if (!Keep)
                { this.hasMoreData = false; }
            }
            return data;
        }
        #endregion

        #region Private Methods
        private void GetData()
        {
            this.Output = this.Pipeline.EndInvoke(this.AsyncResults);
            if (this.Output.Count == 0)
            { this.hasMoreData = false; }
            this.Error = this.Pipeline.Streams.Error;
            this.Warning = this.Pipeline.Streams.Warning;
            this.Verbose = this.Pipeline.Streams.Verbose;
        }
        #endregion

        #region Event Handlers
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
            if (e.InvocationStateInfo.State == PSInvocationState.Completed)
            {
                Thread t = new Thread(GetData);
                t.Start();
            }
        }
        #endregion

        #region IDisposable
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
        #endregion
    }
}
