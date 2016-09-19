using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QbuzzSimulation
{
    public class ScheduledEvent
    {
        public Event Event { get; set; }
        public AggregateRoot Target { get; set; }
    }
}
