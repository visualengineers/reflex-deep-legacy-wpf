using Prism.Events;

namespace PhysicsSimulation.Events
{
    public class PhysicsSimBenchmarkUpdated : PubSubEvent<BenchmarkData>
    {
    }

    public struct BenchmarkData
    {
        public int NumSamples;
        public int TotalTime;
        public float AverageTime;
    }
}
