
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DbConnectorTestCases
{
    public static partial class Program
    {
        private const string ConString = @"Data Source=SFK-A90\SQLEXPRESS;Initial Catalog=OrmLite;Persist Security Info=True;User ID=sa;Password=sfka90";

        static void Main()
        {
            ExecuteFirstSet();

            Console.ReadLine();
        }

        /// <summary>
        /// Executes the set of methods (methods which does not do any mapping) for testing
        /// </summary>
        static void ExecuteFirstSet()
        {
            var executionTime = new List<string>();

            for (var i = 0; i < 5; i++)
            {
                var stopwatch = Stopwatch.StartNew();

                Console.WriteLine("======= TEST CASES STARTED =======");

                SetDefaults();

                ParametrizedQueries();

                QueryResultsetTest();

                TableValuedParms();

                DataTableTest();

                DataSetTest();

                Console.WriteLine("\n======= TEST CASES ENDS =======");

                stopwatch.Stop();

                executionTime.Add(stopwatch.GetTimeString());
            }

            foreach (var item in executionTime)
            {
                Console.WriteLine("\nTime took to complete tests - {0}", item);
            }
        }

        /// Reference :- http://stackoverflow.com/questions/16130232/find-execution-time-of-a-method
        /// <summary>
        /// Gets the time elapsed to know the execution time
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <param name="numberofDigits">Precision</param>
        /// <returns></returns>
        public static string GetTimeString(this Stopwatch stopwatch, int numberofDigits = 1)
        {
            var time = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
            if (time > 1)
                return Math.Round(time, numberofDigits) + " s";
            if (time > 1e-3)
                return Math.Round(1e3 * time, numberofDigits) + " ms";
            if (time > 1e-6)
                return Math.Round(1e6 * time, numberofDigits) + " µs";
            if (time > 1e-9)
                return Math.Round(1e9 * time, numberofDigits) + " ns";
            return stopwatch.ElapsedTicks + " ticks";
        }

    }

}
