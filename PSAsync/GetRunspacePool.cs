using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Get, "RunspacePool")]
    public class GetRunspacePool : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject(PSRunspace.Instance.Settings);
        }
    }
}
