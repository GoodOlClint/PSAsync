using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace PSAsync
{
    public sealed class RunspaceSettings
    {
        public RunspaceSettings()
        {
            int nulInt;
            int poolSize;
            ThreadPool.GetMinThreads(out poolSize, out nulInt);
            this.PoolSize = poolSize;
        }

        public int PoolSize { get; set; }
        public ApartmentState ApartmentState
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.ApartmentState; }
                else
                { return System.Threading.ApartmentState.Unknown; }
            }
            set { PSRunspace.Instance.pool.ApartmentState = value; }
        }
        public TimeSpan CleanupInterval
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.CleanupInterval; }
                else
                { return new TimeSpan(); }
            }
            set { PSRunspace.Instance.pool.CleanupInterval = value; }
        }

        public PSThreadOptions ThreadOptions
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.ThreadOptions; }
                else
                { return PSThreadOptions.Default; }
            }
            set { PSRunspace.Instance.pool.ThreadOptions = value; }
        }

        #region Read Only Properties
        public Guid InstanceId
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.InstanceId; }
                else
                { return Guid.Empty; }
            }
        }

        public bool IsDisposed
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.IsDisposed; }
                else
                { return true; }
            }
        }

        public RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.ConnectionInfo; }
                else
                { return null; }
            }
        }

        public InitialSessionState InitialSessionState
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.InitialSessionState; }
                else
                { return null; }
            }
        }

        public RunspacePoolAvailability RunspacePoolAvailability
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.RunspacePoolAvailability; }
                else
                { return new RunspacePoolAvailability(); }
            }
        }

        public RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                if (PSRunspace.Instance.pool != null)
                { return PSRunspace.Instance.pool.RunspacePoolStateInfo; }
                else
                { return new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null); }
            }
        }
        #endregion

    }
}
