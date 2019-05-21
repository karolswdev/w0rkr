using System;
using System.Collections.Generic;
using System.Text;

namespace w0rkr.Jobs
{
   public interface IConfigurationLoadResult
   {
      bool Status { get; }
      string Message { get; }
   }
}
