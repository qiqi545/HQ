using Microsoft.Extensions.Primitives;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public static class AB
    {
        /// <summary>
        /// Returns whether the current identity is in an alternate group than the control group. 
        /// Useful for simple AB tests with two alternatives. 
        /// </summary>
        /// <example>
        /// if(AB.Test("My experiment"))
        /// {
        ///     // Show this   
        /// }
        /// else
        /// {
        ///     // Show that
        /// }
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static bool Test(string experiment)
        {
            return Group(experiment) != 1;
        }

        /// <summary>
        /// Returns the value of the current identity's experiment group. Groups are 1-based.
        /// </summary>
        /// <example>
        /// switch(AB.Group("My experiment"))
        /// {
        ///     case 1:
        ///         // Show this
        ///         break;
        ///     case 2:
        ///         // Show that
        ///         break;
        ///     case 3:
        ///         // Show the other thing
        ///         break;
        /// }
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static int Group(string experiment)
        {
            var exp = GetExperiment(experiment);
            return exp?.Alternative ?? 1;
        }

        /// <summary>
        /// Returns the value of the current experiment choice, as determined by the experiment identity.
        /// </summary>
        /// <example>
        /// <p>
        ///     Hello, @AB.Value("Polite or rude")!
        /// </p>
        /// </example>
        /// <param name="experiment"></param>
        /// <returns></returns>
        public static StringValues Value(string experiment)
        {
            var exp = GetExperiment(experiment);
            var choice = exp == null ? "?" : exp.CurrentAlternative;
            return new StringValues(choice.ToString());
        }

        /// <summary>
        /// Returns a specified value for the current experiment choice.
        /// You must provide at least as many values as experiment alternatives, otherwise this call is equivalent to <see cref="Value"/>.
        /// Extraneous values are ignored.
        /// </summary>
        /// <example>
        /// <p>
        ///     Hello, @AB.OneOf("Polite or rude", "smarty-pants", "dumby")!
        /// </p>
        /// </example>
        /// <param name="experiment"></param>
        /// <param name="values"> </param>
        /// <returns></returns>
        public static StringValues OneOf(string experiment, params string[] values)
        {
            var exp = GetExperiment(experiment);
            if (exp == null) return new StringValues("?");
            return values.Length < exp.Alternatives ? Value(experiment) : new StringValues(values[exp.Alternative - 1]);
        }

        private static Experiment GetExperiment(string experiment)
        {
            var exp = Experiments.All[new ExperimentKey(experiment)];
            return exp;
        }
    }
}
