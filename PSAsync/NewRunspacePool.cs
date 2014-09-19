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

        [Parameter()]
        public string[] Modules { get; set; }


        protected override void ProcessRecord()
        {
            RunspaceSettings settings = new RunspaceSettings();

            if (this.PoolSize != 0)
            { settings.PoolSize = this.PoolSize; }

            if (this.ApartmentState != System.Threading.ApartmentState.Unknown)
            { settings.ApartmentState = this.ApartmentState; }

            if (this.CleanupInterval != null)
            { settings.CleanupInterval = (TimeSpan)this.CleanupInterval; }

            if (this.ThreadOptions != PSThreadOptions.Default)
            { settings.ThreadOptions = this.ThreadOptions; }

            if (this.Modules != null)
            { settings.InitialSessionState.ImportPSModule(this.Modules); }
            
            PSRunspace.Instance.LoadSettings(settings);

            if (this.PassThru.IsPresent)
            { WriteObject(settings); }
        }
    }
}
