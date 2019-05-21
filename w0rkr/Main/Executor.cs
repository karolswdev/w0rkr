using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using w0rkr.Configuration;
using w0rkr.Jobs;

namespace w0rkr.Main
{
   public class Executor
   {
      private IList<Type> _supportedJobs;
      private readonly StartupOptions _options;
      private readonly IConfigurationRoot _config;

      public Executor(StartupOptions options)
      {
         _options = options;
         WriteString("Starting Executor");
      }

      public void Start()
      {
         LoadExternalJobs();
         LoadJobs();
      }

      public void PrintJobs()
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

      private void WriteString(string text)
      {
         if (_options.Quiet)
         {
            return;
         }

         Console.WriteLine($"[{DateTime.Now}]: {text}");
      }

      #endregion


   }
}
