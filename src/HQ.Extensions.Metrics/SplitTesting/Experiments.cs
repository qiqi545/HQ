using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public static class Experiments
    {
        public static readonly ConcurrentDictionary<ExperimentKey, Experiment> Inner = new ConcurrentDictionary<ExperimentKey, Experiment>();

        public static IDictionary<ExperimentKey, Experiment> All => new ReadOnlyDictionary<ExperimentKey, Experiment>(Inner);

        public static IDictionary<ExperimentKey, Experiment> AllSorted => new ReadOnlyDictionary<ExperimentKey, Experiment>(new SortedDictionary<ExperimentKey, Experiment>(Inner));
    }
}
