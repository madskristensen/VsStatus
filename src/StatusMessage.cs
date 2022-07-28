using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsStatus
{
    public class StatusMessage
    {
        public DateTime queriedTimestamp { get; set; }
        public Service[] services { get; set; }
    }

    public class Service
    {
        public Geography[] geographies { get; set; }
        public string name { get; set; }
        public State state { get; set; } = State.Resolved;
        public DateTime impactStartTimestamp { get; set; }
        public Severity severity { get; set; } = Severity.None;
    }

    public class Geography
    {
        public string geography { get; set; }
        public string name { get; set; }
        public State state { get; set; } = State.Resolved;
        public DateTime impactStartTimestamp { get; set; }
        public Severity severity { get; set; } = Severity.None;
    }

    public enum State
    {
        Resolved,
        Mitigated,
        Active,
    }

    public enum Severity
    {
        None,
        Advisory,
        Degraded,
        Unhealthy,
    }
}
