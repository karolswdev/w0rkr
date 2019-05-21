using System;
using System.Runtime.InteropServices;
using Figgle;
using w0rkr.Configuration;

namespace w0rkr
{
   internal static class Program
   {
      private static StartupOptions _startupOptions;
      private static void Main(string[] args)
      {
         _startupOptions = Args.Configuration.Configure<StartupOptions>().CreateAndBind(args);
         ShowSplashScreen();
         Console.ReadKey();
      }

      #region "UI"

      private static void ShowSplashScreen()
      {
         if (!_startupOptions.Quiet)
         {
            Console.WriteLine(FiggleFonts.Slant.Render("w0rkr"));
            Console.WriteLine($"Detected OS: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Started {DateTime.Now}");
         }
      }

      #endregion

   }
}
