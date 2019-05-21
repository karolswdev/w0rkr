using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using w0rkr.Helpers;
using w0rkr.Main;

namespace w0rkr.Jobs
{
   public class MoveFileJob : IJob
   {
      public string Name => "MoveFile";

      private Executor _executor;

      #region "MoveFile configuration specific fields"

      private string _fromDirectory;
      private string _toDirectory;
      private int _scanInterval;
      private string _fileFilter;

      #endregion

      #region "Status related fields"

      private JobStatus _status;

      #endregion

      #region "Break token"

      private bool _stop;

      #endregion

      public JobStatus GetStatus()
      {
         return _status;
      }

      public void SetExecutor(Executor executor)
      {
         _executor = executor;
      }

      public MoveFileJob()
      {
         _status = JobStatus.Pending;
      }

      public IConfigurationLoadResult LoadConfig(IConfigurationRoot config)
      {
         _status = JobStatus.CorruptConfiguration;

         #region "fromDirectory checks"

         if (String.IsNullOrEmpty(config["MoveFile:fromDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the FromDirectory is not set for this job type.");
         }

         _fromDirectory = config["MoveFile:fromDirectory"];

         if (!Directory.Exists(_fromDirectory))
         {
            return ConfigurationLoadFactory.Get(false, "the FromDirectory is not found on this filesystem.");
         }

         #endregion

         #region "toDirectory checks"

         _toDirectory = config["MoveFile:toDirectory"];

         if (!Directory.Exists(_toDirectory))
         {
            return ConfigurationLoadFactory.Get(false, "the ToDirectory is not found on this filesystem.");
         }

         if (String.IsNullOrEmpty(config["MoveFile:toDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ToDirectory is not set for this job type.");
         }

         #endregion

         #region "scanInterval checks"

         if (String.IsNullOrEmpty(config["MoveFile:scanInterval"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ScanInterval is not set for this job type.");
         }

         if (!Int32.TryParse(config["MoveFile:scanInterval"], out int interval))
         {
            return ConfigurationLoadFactory.Get(false, "the ScanInterval is not correct.");
         }

         _scanInterval = interval;

         #endregion

         #region "fileFilter checks"

         if (String.IsNullOrEmpty(config["MoveFile:fileFilter"])) 
         {
            return ConfigurationLoadFactory.Get(false, "the FileFilter is not set for this job type.");
         }

         _fileFilter = config["MoveFile:fileFilter"];

         #endregion

        _status = JobStatus.Pending;

         return ConfigurationLoadFactory.Get(true, "All configuration loaded and ready to work.");
      }

      public void Start()
      {
         _status = JobStatus.Starting;

         while (!_stop)
         {
            Thread.Sleep(_scanInterval);
            // In case stop was set while wait time was hit
            if (_stop)
            {
               break;
            }
            var files = Directory.GetFiles(_fromDirectory, _fileFilter);
            foreach (var file in files)
            {
               var fi = new FileInfo(file);
               try
               {
                  _executor.WriteString($"Moving file {file}");
                  File.Move(file, $"{_toDirectory}/{fi.Name}");
                  _executor.WriteString($"Filed moved {file}");
               }
               catch (Exception)
               {
                  _executor.WriteString("Error when moving file. Job stopping.");
                  _status = JobStatus.Crashed;
                  _stop = true;
               }
            }
         }
      }

      public void Stop()
      {
         _status = JobStatus.Stopped;
         _stop = true;
      }
   }
}
