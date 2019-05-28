using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using w0rkr.Helpers.Factories;
using w0rkr.Jobs;
using w0rkr.Main;

namespace w0rkr.JobExample
{
   public class MoveFileJob : IJob
   {
      private IExecutor _executor;

      #region "Status related fields"

      private JobStatus _status;

      #endregion

      #region "Break token"

      private bool _stop;

      #endregion

      public MoveFileJob()
      {
         _status = JobStatus.Pending;
      }

      public string Name => "MoveFile";

      public JobStatus GetStatus()
      {
         return _status;
      }

      public void SetExecutor(IExecutor executor)
      {
         _executor = executor;
         _executor.SendMessage(this, "Successfully coupled with executor", MessageType.Verbose);
      }

      public IConfigurationLoadResult LoadConfig(IConfiguration config)
      {
         _status = JobStatus.CorruptConfiguration;

         #region "fromDirectory checks"

         if (String.IsNullOrEmpty(config["Tasks:MoveFile:fromDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the FromDirectory is not set for this job type.");
         }

         _fromDirectory = config["Tasks:MoveFile:fromDirectory"];

         if (!Directory.Exists(_fromDirectory))
         {
            return ConfigurationLoadFactory.Get(false, "the FromDirectory is not found on this filesystem.");
         }

         #endregion

         #region "toDirectory checks"

         _toDirectory = config["Tasks:MoveFile:toDirectory"];

         if (!Directory.Exists(_toDirectory))
         {
            return ConfigurationLoadFactory.Get(false, "the ToDirectory is not found on this filesystem.");
         }

         if (String.IsNullOrEmpty(config["Tasks:MoveFile:toDirectory"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ToDirectory is not set for this job type.");
         }

         #endregion

         #region "scanInterval checks"

         if (String.IsNullOrEmpty(config["Tasks:MoveFile:scanInterval"]))
         {
            return ConfigurationLoadFactory.Get(false, "the ScanInterval is not set for this job type.");
         }

         if (!Int32.TryParse(config["Tasks:MoveFile:scanInterval"], out int interval))
         {
            return ConfigurationLoadFactory.Get(false, "the ScanInterval is not correct.");
         }

         _scanInterval = interval;

         #endregion

         #region "fileFilter checks"

         if (String.IsNullOrEmpty(config["Tasks:MoveFile:fileFilter"]))
         {
            return ConfigurationLoadFactory.Get(false, "the FileFilter is not set for this job type.");
         }

         _fileFilter = config["Tasks:MoveFile:fileFilter"];

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
                  _executor.SendMessage(this, $"Moving file {file}", MessageType.Information);
                  File.Move(file, $"{_toDirectory}\\{fi.Name}");
                  _executor.SendMessage(this, $"Filed moved {file}", MessageType.Information);
               }
               catch (Exception)
               {
                  _executor.SendMessage(this, "Error when moving file. Job stopping.", MessageType.Error);
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

      #region "MoveFile configuration specific fields"

      private string _fromDirectory;
      private string _toDirectory;
      private int _scanInterval;
      private string _fileFilter;

      #endregion
   }
}