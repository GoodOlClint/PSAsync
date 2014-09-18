using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Collections;

namespace PSAsync
{
    [Cmdlet(VerbsCommon.Get, "Async")]
    [CmdletBinding(DefaultParameterSetName = "SessionIdParameterSet")]
    public class GetAsync : PSCmdlet
    {
        [Parameter(ParameterSetName = "SessionIdParameterSet",
            Position = 1,
            ValueFromPipelineByPropertyName = true)]
        public int[] Id { get; set; }

        [Parameter(ParameterSetName = "CommandParameterSet",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Command { get; set; }

        [Parameter(ParameterSetName = "FilterParameterSet",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public Hashtable Filter { get; set; }

        [Parameter(ParameterSetName = "InstanceIdParameterSet",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public Guid[] InstanceID { get; set; }

        [Parameter(ParameterSetName = "NameParameterSet",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "StateParameterSet",
            Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public JobState State { get; set; }

        [Parameter(ParameterSetName = "CommandParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public DateTime? After { get; set; }

        [Parameter(ParameterSetName = "CommandParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public DateTime? Before { get; set; }

        [Parameter(ParameterSetName = "CommandParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public SwitchParameter HasMoreData { get; set; }

        [Parameter(ParameterSetName = "CommandParameterSet")]
        [Parameter(ParameterSetName = "InstanceIdParameterSet")]
        [Parameter(ParameterSetName = "SessionIdParameterSet")]
        [Parameter(ParameterSetName = "NameParameterSet")]
        [Parameter(ParameterSetName = "StateParameterSet")]
        public int Newest { get; set; }

        protected override void ProcessRecord()
        {

            List<AsyncJob> jobs = new List<AsyncJob>();
            if (this.Id != null)
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Id.Contains(j.Value.Id)
                              select j.Value);
            }

            if (this.ParameterSetName == "NameParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Name.Contains(j.Value.Name)
                              select j.Value);
            }

            if (this.ParameterSetName == "StateParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where j.Value.JobStateInfo.State == this.State
                              select j.Value);
            }

            if (this.ParameterSetName == "InstanceIdParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.InstanceID.Contains(j.Value.InstanceId)
                              select j.Value);
            }

            if (this.ParameterSetName == "CommandParameterSet")
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              where this.Command.Contains(j.Value.Command)
                              select j.Value);
            }

            if (jobs.Count == 0)
            {
                jobs.AddRange(from j in PSRunspace.Instance.JobQueue
                              select j.Value);
            }

            if ((this.After != null) && (this.Before != null))
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               where j.PSEndTime <= this.Before
                               select j;
                jobs = tempJobs.ToList();
            }
            else if (this.After != null)
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               select j;
                jobs = tempJobs.ToList();
            }
            else if (this.Before != null)
            {
                var tempJobs = from j in jobs
                               where j.PSEndTime >= this.After
                               select j;
                jobs = tempJobs.ToList();
            }

            if (this.HasMoreData.IsPresent)
            {
                var tempJobs = from j in jobs
                               where j.HasMoreData == true
                               select j;
                jobs = tempJobs.ToList();
            }

            if (this.Newest > 0)
            {
                var tempJobs = from j in jobs
                               orderby j.PSEndTime descending
                               select j;
                jobs = tempJobs.Take(this.Newest).ToList();
            }

            if (this.Filter != null)
            {
                var props = typeof(AsyncJob).GetProperties();
                var tempJobs = jobs.ConvertAll(j => j);
                jobs.Clear();
                foreach (DictionaryEntry pair in this.Filter)
                {
                    var myProp = (from prop in props
                                  where prop.Name == pair.Key.ToString()
                                  select prop).First();
                    jobs.AddRange(from j in tempJobs
                                  where myProp.GetValue(j, null) == pair.Value
                                  select j);

                }
            }

            foreach (var job in jobs.OrderBy(j => j.Id))
            { WriteObject(job); }
        }
    }
}
