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
   internal class Executor : IExecutor
   {
      #region "All ran/running/stopped jobs"

      private readonly IList<IJob> _jobs;

      #endregion

      public Executor(StartupOptions options)
      {
         // Store options passed by factory
         _options = options;
         // If user provided the /c switch, the below will not be empty and another file name will be used
         // to build the configuration
         if (String.IsNullOrEmpty(_options.ConfigFile))
         {
            WriteToConsole("Using default configuration file, no alternate file supplied.");
            _config = new ConfigurationBuilder().AddJsonFile("w0rkr.config.json").Build();
         }
         else
         {
            WriteToConsole("Using alternate configuration file.");
            _config = new ConfigurationBuilder().AddJsonFile(_options.ConfigFile).Build();
         }
         _jobs = new List<IJob>();
      }

      public void SendMessage(IJob from, string message, MessageType type = MessageType.Information)
      {
         switch (type)
         {
            case MessageType.Information:
               Console.ForegroundColor = ConsoleColor.White;
               break;
            case MessageType.Error:
               Console.ForegroundColor = ConsoleColor.Red;
               break;
            case MessageType.Verbose:
               Console.ForegroundColor = ConsoleColor.DarkGray;
               break;
            case MessageType.Warning:
               Console.ForegroundColor = ConsoleColor.DarkYellow;
               break;
            default:
               Console.ForegroundColor = ConsoleColor.White;
               break;
         }

         Console.WriteLine($"[{DateTime.Now}] -> [{from.Name}]: {message}");
         Console.ResetColor();
      }

      public void Start()
      {
         LoadExternalJobs();
         LoadJobs();
         PrintJobs();
         LoadTasks();
         Work();
      }

      public void Stop()
      {
         throw new NotImplementedException();
      }

      public IReadOnlyCollection<IJobStatus> GetJobStatus()
      {
         throw new NotImplementedException();
      }

      private void LoadTasks()
      {
         WriteToConsole("Loading tasks");
         var tasks = _config.GetSection("tasks").GetChildren().ToArray().Select(t => t.Value);
         var enumerable = tasks as string[] ?? tasks.ToArray();
         _tasks = enumerable;
         WriteToConsole($"Found tasks to do: {string.Join(",", _tasks)}", ConsoleColor.Red);
      }

      private void Work()
      {
         var work = GenerateWorkMappingsForTasks();

         foreach (var match in work)
         {
            WriteToConsole($"{match.RequestedTask} -> {match.SupportedType}", ConsoleColor.Red);
         }
         WriteToConsole("----------");

         foreach (var match in work)
         {
            var job = (IJob) Activator.CreateInstance(match.SupportedType);
            job.SetExecutor(this);
            WriteToConsole($"Instantiated {match.SupportedType}");
            _jobs.Add(job);
         }

         foreach (var job in _jobs)
         {
            WriteToConsole($"Writing configuration into {job.Name}");
            var result = job.LoadConfig(_config);
            WriteToConsole($"Configuration load result is {result.Status}. The message is {result.Message}");
            if (result.Status)
            {
               WriteToConsole($"Starting job {job.Name}", ConsoleColor.Red);
               Task.Run(() => { job.Start(); });
            }
         }
      }

      /// <summary>
      ///    Generates mappings between tasks to perform and actual loaded jobs.
      ///    Used to map and start any given units of work.
      ///    Essential method.
      /// </summary>
      /// <returns>A list of <see cref="JobMatch" /> that can then be used to instantiate units of work</returns>
      private IList<JobMatch> GenerateWorkMappingsForTasks()
      {
         var retList = new List<JobMatch>();
         foreach (var task in _tasks)
         {
            retList.AddRange(_supportedJobs.Where(j => j.ToString().ToLower().Contains(task.ToLower()))
               .Select(t => new JobMatch {RequestedTask = task, SupportedType = t}));
         }

         return retList;
      }

      /// <summary>
      ///    Prints the supported jobs into console.
      ///    Takes <see cref="StartupOptions" /> into consideration for quiet operations
      /// </summary>
      private void PrintJobs()
      {
         if (_options.Quiet)
         {
            return;
         }

         foreach (var supportedJob in _supportedJobs)
         {
            WriteToConsole($"Found {supportedJob}");
         }
      }

      #region "Helper function to output to console"

      public void WriteToConsole(string text, ConsoleColor color = ConsoleColor.Gray)
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

      #region "Jobs and tasks"

      private IList<Type> _supportedJobs;
      private IEnumerable<string> _tasks;

      #endregion

      #region "Startup options and configuration"

      private readonly StartupOptions _options;
      private readonly IConfiguration _config;

      #endregion

      #region "Loading of plugins and internal jobs"

      private void LoadJobs()
      {
         WriteToConsole("Loading support for jobs");
         _supportedJobs = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IJob).IsAssignableFrom(p)).Where(p => !p.IsInterface).ToList();
      }

      private void LoadExternalJobs()
      {
         WriteToConsole("Loading external job plug-ins");
         if (!Directory.Exists("./jobs"))
         {
            WriteToConsole("No jobs/ directory. Cannot load external job plug-ins.");
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
   }
}