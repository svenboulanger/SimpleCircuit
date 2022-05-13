using SimpleCircuit.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleCircuit
{
    public class Program
    {
        /// <summary>
        /// Main entry point of the program.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            // Create jobs for each argument
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("No files specified");
                return;
            }
            var logger = new ConsoleDiagnosticLogger();

            // Create a simple job
            var jobs = ReadJobs(args, out bool interactiveMode);
            if (jobs.Count > 0)
                DoJobs(jobs, logger).GetAwaiter().GetResult();
            if (interactiveMode)
                InteractiveMode(logger);

#if DEBUG
            Console.WriteLine("Finished. Press any key to continue...");
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// Starts a number of jobs to be converted.
        /// </summary>
        /// <param name="jobs">The jobs to convert.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public static async Task DoJobs(IReadOnlyList<Job> jobs, IDiagnosticHandler diagnostics)
        {
            if (jobs == null || jobs.Count == 0)
                return;

            // Let's try to do as much as possible in parallel here
            ChromiumTextFormatter formatter = null;
            var formatterTask = Task.Run(() => formatter = new ChromiumTextFormatter());
            try
            {
                var tasks = new Task[jobs.Count];
                for (int i = 0; i < jobs.Count; i++)
                    tasks[i] = jobs[i].Compute();
                Task.WaitAll(tasks);

                // Render all the completed tasks
                await formatterTask;
                if (formatter == null)
                    diagnostics?.Post(new DiagnosticMessage(SeverityLevel.Warning, "SC001", "Could not create formatter"));
                for (int i = 0; i < jobs.Count; i++)
                {
                    jobs[i].DisplayMessages(diagnostics);
                    if (!jobs[i].HasErrors)
                        await jobs[i].Render(formatter, diagnostics);
                }
            }
            finally
            {
                // Dispose of our created formatter
                formatter?.Dispose();
            }
        }

        /// <summary>
        /// Starts an interactive mode.
        /// </summary>
        /// <param name="diagnostics">The diagnostics message handler.</param>
        public static void InteractiveMode(IDiagnosticHandler diagnostics)
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Write("> ");
                string arguments = Console.ReadLine();
                string[] args = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length == 1 && StringComparer.OrdinalIgnoreCase.Equals(args[0], "quit"))
                    keepGoing = false;
                else
                {
                    var jobs = ReadJobs(args, out _);
                    DoJobs(jobs, diagnostics).GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Reads a number of jobs from arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="interactiveMode">If <c>true</c>, the arguments specify that interactive mode should start; otherwise, <c>false</c>.</param>
        /// <returns>A list of jobs to execute.</returns>
        private static IReadOnlyList<Job> ReadJobs(string[] args, out bool interactiveMode)
        {
            interactiveMode = false;
            List<Job> jobs = new List<Job>();
            Job currentJob = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-out":
                        if (currentJob != null && i + 1 < args.Length)
                            currentJob.OutputFilename = args[++i];
                        break;

                    case "-css":
                        if (currentJob != null && i + 1 < args.Length)
                            currentJob.CssFilename = args[++i];
                        break;

                    case "-cli":
                        interactiveMode = true;
                        break;

                    default:
                        if (currentJob != null)
                            jobs.Add(currentJob);
                        currentJob = new Job()
                        {
                            Filename = args[i]
                        };
                        break;
                }
            }
            if (currentJob != null)
                jobs.Add(currentJob);
            return jobs;
        }
    }
}
