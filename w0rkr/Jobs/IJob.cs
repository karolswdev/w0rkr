using Microsoft.Extensions.Configuration;

namespace w0rkr.Jobs
{
   public interface IJob
   {
      string Name { get; }
      IConfigurationLoadResult LoadConfig(IConfigurationRoot config);
      void Start();
      void Stop();
      JobStatus GetStatus();
   }
}
