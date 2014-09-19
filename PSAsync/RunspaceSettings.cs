using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;

namespace PSAsync
{
    public class RunspaceSettings
    {
        public RunspaceSettings()
        {
            int nulInt;
            int poolSize;
            ThreadPool.GetMinThreads(out poolSize, out nulInt);
            this.PoolSize = poolSize;
            this.InitialSessionState = InitialSessionState.CreateDefault();
            this.ApartmentState = System.Threading.ApartmentState.Unknown;
            this.CleanupInterval = new TimeSpan(0, 15, 0);
            this.ThreadOptions = PSThreadOptions.Default;
        }

        public RunspaceSettings(RunspacePool pool)
            : this()
        {
            if (pool == null)
            { return; }
            this.ApartmentState = pool.ApartmentState;
            this.CleanupInterval = pool.CleanupInterval;
            this.InitialSessionState = pool.InitialSessionState;
            this.InstanceId = pool.InstanceId;
            this.IsDisposed = pool.IsDisposed;
            this.PoolSize = pool.GetMaxRunspaces();
            this.RunspacePoolAvailability = pool.RunspacePoolAvailability;
            this.RunspacePoolStateInfo = pool.RunspacePoolStateInfo;
            this.ThreadOptions = pool.ThreadOptions;
        }

        internal RunspacePool ToPool(PSHost Host)
        {
            RunspacePool pool = RunspaceFactory.CreateRunspacePool(1, this.PoolSize, this.InitialSessionState, Host);
            //RunspacePool pool = RunspaceFactory.CreateRunspacePool(1, this.PoolSize);
            pool.ApartmentState = this.ApartmentState;
            pool.CleanupInterval = this.CleanupInterval;
            pool.ThreadOptions = this.ThreadOptions;
            return pool;
        }
        public int PoolSize { get; set; }
        public ApartmentState ApartmentState
        {
            get;
            set;
        }
        public TimeSpan CleanupInterval
        {
            get;
            set;
        }

        public PSThreadOptions ThreadOptions
        {
            get;
            set;
        }

        #region Read Only Properties
        public Guid InstanceId
        {
            get;
            protected set;
        }

        public bool IsDisposed
        {
            get;
            protected set;
        }

        public InitialSessionState InitialSessionState
        {
            get;
            protected set;
        }

        public RunspacePoolAvailability RunspacePoolAvailability
        {
            get;
            protected set;
        }

        public RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get;
            protected set;
        }
        #endregion

    }
}
