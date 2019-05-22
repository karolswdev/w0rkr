using System;
using System.Collections.Generic;
using System.Text;
using w0rkr.Jobs;

namespace w0rkr.Main
{
   public interface IJobStatus
   {
      string Job { get; }
      JobStatus Status { get; }
   }
}
