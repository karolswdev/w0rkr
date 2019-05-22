using System;
using System.Collections.Generic;
using System.Text;
using w0rkr.Jobs;

namespace w0rkr.Main
{
   public interface IExecutor
   {
      void SendMessage(IJob from, string message, MessageType type);
      void Start();
      void Stop();
      IReadOnlyCollection<IJobStatus> GetJobStatus();
   }
}
