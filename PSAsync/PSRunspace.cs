using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSAsync
{
    public sealed class PSRunspace
    {
        #region Singleton Instance
        private static volatile PSRunspace instance;
        private static object syncRoot = new object();
        public static PSRunspace Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        { instance = new PSRunspace(); }
                    }
                }
                return instance;
            }
        }
        #endregion

        private PSRunspace()
        {
            this.PoolSize = 20;
            this.IsOpen = false;
        }

        private RunspacePool pool;

        public bool IsOpen { get; private set; }
        public int PoolSize { get; set; }

        public void Open()
        {
            if (!this.IsOpen)
            {
                this.IsOpen = true;
                if (null != this.pool)
                { this.pool = RunspaceFactory.CreateRunspacePool(1, this.PoolSize); }
                this.pool.Open();
            }
        }

        public PowerShell NewPipeline()
        {
            var pipeline = PowerShell.Create();
            pipeline.RunspacePool = this.pool;
            return pipeline;
        }

        public void Close()
        {
            if (!this.IsOpen)
            {
                this.IsOpen = false;
                if (null != this.pool)
                {
                    this.pool.Close();
                    this.pool.Dispose();
                    this.pool = null;
                }
            }
        }

        ~PSRunspace()
        {
            if (this.IsOpen)
            { this.Close(); }
        }
    }
}
