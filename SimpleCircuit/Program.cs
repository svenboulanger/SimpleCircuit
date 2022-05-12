using SimpleCircuit.Diagnostics;
using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;
using System.IO;
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
            List<Job> jobs = new List<Job>();
            for (int i = 0; i < args.Length; i++)
            {
                string filename = args[i];
                jobs.Add(new Job()
                {
                    Filename = filename
                });
            }

            // Create a simple job
            Start(jobs.ToArray(), new ConsoleDiagnosticLogger());

            Console.WriteLine("Finished. Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Starts a number of jobs to be converted.
        /// </summary>
        /// <param name="jobs">The jobs to convert.</param>
        /// <param name="diagnostics">The diagnostics handler.</param>
        public static void Start(Job[] jobs, IDiagnosticHandler diagnostics)
        {
            if (jobs == null || jobs.Length == 0)
                return;

            Cef.EnableWaitForBrowsersToClose();
            var settings = new CefSettings()
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
            };
            CefSharpSettings.ShutdownOnExit = true;
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

            // Start the jobs
            var tasks = new Task[jobs.Length];
            for (int i = 0; i < jobs.Length; i++)
            {
                var task = jobs[i].Execute(diagnostics);
                tasks[i] = task;
            }
            Task.WaitAll(tasks);
        }
    }
}
