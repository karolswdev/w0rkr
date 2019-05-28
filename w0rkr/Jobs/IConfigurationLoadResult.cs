namespace w0rkr.Jobs
{
   public interface IConfigurationLoadResult
   {
      bool Status { get; }
      string Message { get; }
   }
}