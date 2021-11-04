using SimpleCircuit.Diagnostics;
using SimpleCircuit.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SimpleCircuit
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                // Just show some help
                Console.WriteLine(Properties.Resources.Help);
                return;
            }

            // Make a list of files and target filenames
            List<Job> jobs = new();
            string css = null;
            var logger = new Logger();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                    case "-out":
                    case "-output":
                        if (jobs.Count == 0)
                            logger.Post(new DiagnosticMessage(SeverityLevel.Error, "SC001", string.Format(Properties.Resources.NoInput, args[i])));
                        i++;
                        if (i >= args.Length)
                            break;
                        jobs[^1].Output = Path.GetFileName(args[i]);
                        jobs[^1].OutputDirectory = Path.GetDirectoryName(args[i]);
                        break;

                    case "-defaultcss":
                        if (css == null)
                            css = GraphicalCircuit.DefaultStyle;
                        else
                            css += Environment.NewLine + GraphicalCircuit.DefaultStyle;
                        break;

                    case "-nocss":
                        css = "";
                        break;

                    case "-css":
                        i++;
                        if (!File.Exists(args[i]))
                            logger.Post(new DiagnosticMessage(SeverityLevel.Error, "SC002", string.Format(Properties.Resources.FilenameNotFound, args[i])));
                        else
                        {
                            string ncss = File.ReadAllText(args[i]);
                            if (css == null)
                                css = ncss;
                            else
                                css = css + Environment.NewLine + ncss;
                        }
                        break;

                    default:
                        // Add a new job for the filename
                        bool found = false;
                        foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), args[i]))
                        {
                            found = true;
                            jobs.Add(new() { Input = Path.GetFileName(args[i]), InputDirectory = Path.GetDirectoryName(args[i]) });
                        }
                        if (!found)
                            logger.Post(new DiagnosticMessage(SeverityLevel.Error, "SC002", string.Format(Properties.Resources.FilenameNotFound, args[i])));
                        break;
                }
            }

            if (jobs.Count == 0)
            {
                // No jobs
                logger.Post(new DiagnosticMessage(SeverityLevel.Warning, "SC003", Properties.Resources.NoJobs));
                return;
            }

            // Handle the jobs
            foreach (var job in jobs)
            {
                var context = new ParsingContext
                {
                    Diagnostics = logger
                };
                using var sr = new StreamReader(job.Input);
                var lexer = new Lexer(sr.ReadToEnd());
                Parser.Parser.Parse(lexer, context);

                // Render the document
                if (css != null)
                    context.Circuit.Style = css;
                var doc = context.Circuit.Render(logger);

                if (job.Output == null)
                {
                    // Create a filename
                    job.OutputDirectory = job.InputDirectory;
                    job.Output = Path.GetFileNameWithoutExtension(job.Input) + ".svg";
                }
                else
                {
                    if (!Directory.Exists(job.OutputDirectory))
                        Directory.CreateDirectory(job.OutputDirectory);
                }
                using var sw = new StreamWriter(Path.Combine(job.OutputDirectory, job.Output));
                using var xml = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = false });
                doc.WriteTo(xml);
            }

            logger.Post(new DiagnosticMessage(SeverityLevel.Info, "SC000", string.Format(Properties.Resources.Complete, jobs.Count)));
        }
    }
}
