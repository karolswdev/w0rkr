using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using w0rkr.Configuration;
using w0rkr.Helpers;
using w0rkr.Jobs;

namespace w0rkr.Main
{
   public class Executor
   {
      private IList<Type> _supportedJobs;
      private readonly StartupOptions _options;
      private readonly IConfiguration _config;
      private IEnumerable<string> _tasks;
      private readonly IList<IJob> _jobs;

      public Executor(StartupOptions options)
      {
         _options = options;
         WriteString("Starting Executor");
         _config = new ConfigurationBuilder().AddJsonFile("w0rkr.config.json").Build();
         _jobs = new List<IJob>();
      }

      public void Start()
      {
         LoadExternalJobs();
         LoadJobs();
         PrintJobs();
         LoadTasks();
         Work();
      }

      private void LoadTasks()
      {
         WriteString("Loading tasks");
         var tasks = _config.GetSection("Jobs").GetChildren().ToArray().Select(t => t.Value);
         var enumerable = tasks as string[] ?? tasks.ToArray();
         _tasks = enumerable;
         WriteString($"Found tasks to do: {string.Join(",", _tasks)}", ConsoleColor.Red);
      }

      public void Work()
      {
            var work = GenerateWorkMappingsForTasks();

            foreach (var match in work)
            {
               WriteString($"{match.RequestedTask} -> {match.SupportedType}", ConsoleColor.Red);
            }
            WriteString("----------");

            foreach (var match in work)
            {
               var job = (IJob) Activator.CreateInstance(match.SupportedType);
               job.SetExecutor(this);
               WriteString($"Instantiated {match.SupportedType}");
               _jobs.Add(job);
            }

            foreach (var job in _jobs)
            {
               WriteString($"Writing configuration into {job.Name}");
               var result = job.LoadConfig(_config);
               WriteString($"Configuration load result is {result.Status}. The message is {result.Message}");
               if (result.Status)
               {
                  WriteString($"Starting job {job.Name}", ConsoleColor.Red);
                  Task.Run(() =>
                  {
                     job.Start();
                  });

               }
            }

      }

      private IList<JobMatch> GenerateWorkMappingsForTasks()
      {
         var retList = new List<JobMatch>();
         foreach (var task in _tasks)
         {
            retList.AddRange(_supportedJobs.Where(j => j.ToString().ToLower().Contains(task.ToLower())).Select(t => new JobMatch() { RequestedTask = task, SupportedType = t}));
         }

         return retList;
      }

      private void PrintJobs()
      {
         if (_options.Quiet)
         {
            return;
         }

         foreach (var supportedJob in _supportedJobs)
         {
            WriteString($"Found {supportedJob}");
         }
      }

      #region "Loading of plugins and internal jobs"
      private void LoadJobs()
      {
         WriteString("Loading support for jobs");
         _supportedJobs = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IJob).IsAssignableFrom(p)).Where(p => !p.IsInterface).ToList();
      }

      private void LoadExternalJobs()
      {
         WriteString("Loading external job plug-ins");
         if (!Directory.Exists("./jobs"))
         {
            WriteString("No jobs/ directory. Cannot load external job plug-ins.");
            return;
         }

         var libraries = Directory.GetFiles("./jobs", "*.dll");

         foreach (var file in libraries)
         {
            var fi = new FileInfo(file);
            System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(fi.FullName);
         }

      }
      #endregion

      #region "Helper function to output to console"

      public void WriteString(string text, ConsoleColor color = ConsoleColor.Gray)
      {
         if (_options.Quiet)
         {
            return;
         }
         Console.ForegroundColor = color;
         Console.WriteLine($"[{DateTime.Now}]: {text}");
         Console.ResetColor();
      }

      #endregion

   }
}
