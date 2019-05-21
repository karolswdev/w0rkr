using Microsoft.Extensions.Configuration;
using w0rkr.Main;

namespace w0rkr.Jobs
{
   public interface IJob
   {
      string Name { get; }
      IConfigurationLoadResult LoadConfig(IConfiguration config);
      void Start();
      void Stop();
      JobStatus GetStatus();
      void SetExecutor(Executor executor);
   }
}
