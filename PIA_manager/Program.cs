using System;
using System.IO;
using System.Diagnostics;

namespace PIA
{
    //Original author Shukhrat Nekbaev: https://www.privateinternetaccess.com/forum/discussion/2286/pia-from-a-fixed-location-in-windows
    class PIA_Runner
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            var workingDir = Environment.CurrentDirectory;
            var rubyFilesDir = Path.Combine(workingDir, "pia_ruby_files");

            Console.WriteLine("Working directory: {0}", workingDir);
            Console.WriteLine("Ruby files directory: {0}", rubyFilesDir);

            string rubyExecutable = Path.Combine(rubyFilesDir, "bin", "rubyw.exe");
            string rubySourceFile = Path.Combine(rubyFilesDir, "src", "pia_manager.rb");
            string rubyArgs = string.Format("\"{0}\" --run", rubySourceFile);

            Console.WriteLine("Running: {0} {1}", rubyExecutable, rubyArgs);

            var info = new ProcessStartInfo(rubyExecutable, rubyArgs)
            {
                WorkingDirectory = workingDir
            };

            Process.Start(info);
        }
    }
}