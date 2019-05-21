using System;
using System.Collections.Generic;
using System.Text;

namespace w0rkr.Jobs
{
   public class ConfigurationLoadResult : IConfigurationLoadResult
   {
      public bool Status { get; }
      public string Message { get; }

      public ConfigurationLoadResult(bool status, string message)
      {
         Message = message;
         Status = status;
      }
   }
}
