using SimpleCircuit.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SimpleCircuit
{
    public partial class Program
    {
        /// <summary>
        /// Main entry point of the program.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            // Make sure the current thread culture is invariant
            // There were issues where numbers were exported with a comma instead of a dot
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

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
                DoJobs(jobs, logger);
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
        public static void DoJobs(IReadOnlyList<Job> jobs, IDiagnosticHandler diagnostics)
        {
            if (jobs == null || jobs.Count == 0)
                return;

            // Let's try to do as much as possible in parallel here
            for (int i = 0; i < jobs.Count; i++)
                jobs[i].Compute();

            // Render all the completed tasks
            for (int i = 0; i < jobs.Count; i++)
            {
                jobs[i].DisplayMessages(diagnostics);
                if (!jobs[i].HasErrors)
                    jobs[i].Render(diagnostics);
            }
        }

        /// <summary>
        /// Starts an interactive mode.
        /// </summary>
        /// <param name="diagnostics">The diagnostics message handler.</param>
        public static void InteractiveMode(IDiagnosticHandler diagnostics)
        {
            var regex = Arguments();

            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Write("> ");
                string? arguments = Console.ReadLine();
                string[] args = regex.Matches(arguments ?? string.Empty).Cast<Match>().Select(m => m.Groups["value"].Value).ToArray();
                if (args.Length == 0 ||
                    args.Length == 1 && (
                        StringComparer.OrdinalIgnoreCase.Equals(args[0], "quit") ||
                        StringComparer.OrdinalIgnoreCase.Equals(args[0], "exit")))
                {
                    keepGoing = false;
                }
                else
                {
                    var jobs = ReadJobs(args, out _);
                    DoJobs(jobs, diagnostics);
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
            var jobs = new List<Job>();
            Job? currentJob = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--version":
                        // Show the version of SimpleCircuit.Lib
                        var version = typeof(SvgDrawing).Assembly.GetName().Version;
                        Console.WriteLine("SimpleCircuit v" + version);
                        break;

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

                        // Get the full filename
                        string filename = args[i];
                        if (!Path.IsPathRooted(args[i]))
                            filename = Path.Combine(Directory.GetCurrentDirectory(), filename);
                        currentJob = new Job()
                        {
                            Filename = filename
                        };
                        break;
                }
            }
            if (currentJob != null)
                jobs.Add(currentJob);
            return jobs;
        }

        [GeneratedRegex(@"""(?<value>[^""]+)""|(?<value>[^\s]+)")]
        private static partial Regex Arguments();
    }
}
