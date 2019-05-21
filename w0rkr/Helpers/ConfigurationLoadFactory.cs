using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using w0rkr.Jobs;

namespace w0rkr.Helpers
{
   public static class ConfigurationLoadFactory
   {
      public static IConfigurationLoadResult Get(bool result, string errorDescription = "")
      {
         return new ConfigurationLoadResult(result, errorDescription);
      }
   }
}
