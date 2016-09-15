using System;

namespace QbuzzSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO Input van .csv's accepteren

            //Run simulatie met tijd = 12 uur, f = 5, q = 5.
            var system = new System(43200, 4, 5);
            system.Run();

            //TODO output uit systeem events genereren

            Console.ReadLine();
        }
    }
}
