namespace w0rkr.Jobs
{
   public class ConfigurationLoadResult : IConfigurationLoadResult
   {
      public ConfigurationLoadResult(bool status, string message)
      {
         Message = message;
         Status = status;
      }

      public bool Status { get; }
      public string Message { get; }
   }
}