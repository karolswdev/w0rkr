using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.Extensions.Configuration;
using w0rkr.Helpers;

namespace w0rkr.Jobs
{
   public class MoveFileJob : IJob
   {
      public string Name => "MoveFile";

      #region "MoveFile configuration specific fields"

      private string _fromDirectory;
      private string _toDirectory;
      private int _scanInterval;
      private string _fileFilter;

      #endregion

      #region "Status related logic"
         
      #endregion

      public IConfigurationLoadResult LoadConfig(IConfiguration config)
      {
         if (String.IsNullOrEmpty(config["MoveFile:FromDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the FromDirectory is not set for this job type.");
         }

         if (String.IsNullOrEmpty(config["MoveFile:ToDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ToDirectory is not set for this job type.");
         }

         if (String.IsNullOrEmpty(config["MoveFile:ScanInterval"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ScanInterval is not set for this job type.");
         }

         if (String.IsNullOrEmpty(config["MoveFile:FileFilter"])) 
         {
            return ConfigurationLoadFactory.Get(false, "the FileFilter is not set for this job type.");
         }

         return ConfigurationLoadFactory.Get(true, "All configuration loaded and ready to work.");
      }
   }
}
