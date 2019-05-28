using w0rkr.Jobs;

namespace w0rkr.Helpers.Factories
{
   public static class ConfigurationLoadFactory
   {
      public static IConfigurationLoadResult Get(bool result, string errorDescription = "")
      {
         return new ConfigurationLoadResult(result, errorDescription);
      }
   }
}