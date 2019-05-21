using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Configuration;
using w0rkr.Jobs;
using w0rkr.Main;

namespace w0rkr.JobExample
{
   public class HelloWorldJob : IJob
   {
      public string Name => "HelloWorld";

      private JobStatus _status;
      private Executor _executor;

      public HelloWorldJob()
      {
         _status = JobStatus.Pending;
      }

      public void SetExecutor(Executor executor)
      {
         _executor = executor;
      }

      public IConfigurationLoadResult LoadConfig(IConfiguration config)
      {
         return new ConfigurationLoadResult(true, "All good!");
      }

      public void Start()
      {
         _status = JobStatus.Working;
         Console.WriteLine("Hello World!");
         _status = JobStatus.Pending;
      }

      public void Stop()
      {
         _status = JobStatus.Stopped;
      }

      public JobStatus GetStatus()
      {
         return _status;
      }
   }
}
