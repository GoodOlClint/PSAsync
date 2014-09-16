using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Threading;
using System.Management.Automation.Runspaces;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.New, "RunspacePool")]
    public class NewRunspacePool : PSCmdlet
    {
        [Parameter()]
        public int PoolSize { get; set; }

        [Parameter()]
        public SwitchParameter PassThru { get; set; }

        [Parameter()]
        public ApartmentState ApartmentState { get; set; }

        [Parameter()]
        public TimeSpan? CleanupInterval { get; set; }

        [Parameter()]
        public PSThreadOptions ThreadOptions { get; set; }
        

        protected override void ProcessRecord()
        {
            PSRunspace.Instance.Settings = new RunspaceSettings();

            if (this.PoolSize != 0)
            { PSRunspace.Instance.Settings.PoolSize = this.PoolSize; }
            
            PSRunspace.Instance.Initalize();

            if (this.ApartmentState != System.Threading.ApartmentState.Unknown)
            { PSRunspace.Instance.Settings.ApartmentState = this.ApartmentState; }

            if (this.CleanupInterval != null)
            { PSRunspace.Instance.Settings.CleanupInterval = (TimeSpan)this.CleanupInterval; }

            if (this.ThreadOptions != PSThreadOptions.Default)
            { PSRunspace.Instance.Settings.ThreadOptions = this.ThreadOptions; }

            if (this.PassThru.IsPresent)
            { WriteObject(PSRunspace.Instance.Settings); }
        }
    }
}
