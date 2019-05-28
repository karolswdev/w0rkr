using System;
using System.Collections.Generic;
using System.Text;
using w0rkr.Configuration;
using w0rkr.Main;

namespace w0rkr.Helpers.Factories
{
   public static class ExecutorFactory {
      public static IExecutor Get(StartupOptions options)
      {
         return new Executor(options);
      }
   }
}
