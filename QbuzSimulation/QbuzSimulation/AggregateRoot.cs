﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QbuzSimulation
{
    //Basis klasse om events op te slaan en de passende apply methode aan te roepen
    public abstract class AggregateRoot
    {
        private readonly List<Event> _events = new List<Event>();
        public readonly Guid Id = Guid.NewGuid();

        protected void ApplyChange(Event @event)
        {
            ApplyChange(@event, true);
        }

        protected void ApplyChange(Event @event, bool isNew)
        {
            Console.WriteLine("Applying event {0} to {1}-{2} at {3}", @event.Name, GetType().Name, Id, @event.TimeStamp);
            this.AsDynamic().Apply(@event);
            if(isNew) _events.Add(@event);
        }
    }
}
