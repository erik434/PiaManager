using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

/// <remarks>
/// This project started out using code from Shukhrat Nekbaev: https://www.privateinternetaccess.com/forum/discussion/2286/pia-from-a-fixed-location-in-windows
/// I've pretty much rewritten everything since then, so I think I'm clear to call this my own work at this point, but I'll leave the above as a reference.
/// </remarks>
namespace PIA_manager
{
    /// <remarks>
    /// Call this with no args to (re)start PIA - it will first kill an existing PIA process if one exists.
    /// Call this with the argument "--stop" to just kill an existing PIA process.
    /// </remarks>
    static class Program
    {
        const string PiaFilesDirName = "pia_ruby_files";

        static string rubyExecutable;
        static string rubySourceFile;
        static string piaWorkingDir;

        static void Main(string[] args)
        {
            //First check that we can find the PIA directory and ruby files
            FindPiaFiles();

            //Then run the desired command
            if (args.Length > 0 && args[0] == "--stop")
            {
                StopPia();
            }
            else
            {
                StopPia();
                StartPia();
            }
        }

        private static void StartPia()
        {
            string rubyArgs = $"\"{rubySourceFile}\" --run";
            Console.WriteLine($"Running: {rubyExecutable} {rubyArgs}");

            var info = new ProcessStartInfo(rubyExecutable, rubyArgs)
            {
                WorkingDirectory = piaWorkingDir
            };

            Process.Start(info);
        }

        private static void StopPia()
        {
            //Kill the PIA processes
            foreach (var process in Process.GetProcessesByName("rubyw"))
            {
                var cmdLine = process.GetCommandLine();
                if (cmdLine == null)
                {
                    Console.WriteLine($"Found ruby process with PID {process.Id} but couldn't retrieve command line");
                    continue;
                }

                Console.WriteLine($"Found Ruby process with PID: {process.Id}; cmd: {cmdLine}");

                if (!cmdLine.Contains(rubySourceFile, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Does not look like a ruby process running PIA - skipping");
                    continue;
                }

                //If we get down here, we found a rubyw process that is probably running the PIA manager code. It should be safe to kill it and its children.
                process.KillTree();
            }

            //Delete the .pid files - may be important for PIA to restart properly later?
            var logDir = Path.Combine(piaWorkingDir, "log");
            if (!Directory.Exists(logDir))
            {
                Console.WriteLine("No log directory found, skipping pid cleanup");
                return;
            }

            foreach(var pidFile in Directory.EnumerateFiles(logDir, "*.pid"))
            {
                Console.WriteLine($"Deleting pid file: {pidFile}");
                File.Delete(pidFile);
            }
        }

        private static void FindPiaFiles()
        {
            //Find the PIA files directory - first search where the .exe is running, and if that fails, try the current directory.
            var exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var searchDirs = new[] { exeDir, Environment.CurrentDirectory };
            piaWorkingDir = searchDirs.FirstOrDefault(DirContainsPiaFiles);
            if (piaWorkingDir == null)
            {
                throw new DirectoryNotFoundException("Failed to find PIA ruby files directory");
            }
            var rubyFilesDir = Path.Combine(piaWorkingDir, PiaFilesDirName);

            Console.WriteLine($"Working directory: {piaWorkingDir}");
            Console.WriteLine($"Ruby files directory: {rubyFilesDir}");

            rubyExecutable = Path.Combine(rubyFilesDir, "bin", "rubyw.exe");
            rubySourceFile = Path.Combine(rubyFilesDir, "src", "pia_manager.rb");

            if (!File.Exists(rubyExecutable)) throw new FileNotFoundException("Could not find ruby executable", rubyExecutable);
            if (!File.Exists(rubySourceFile)) throw new FileNotFoundException("Could not find pia_manager ruby file", rubySourceFile);
        }

        static bool DirContainsPiaFiles(string searchDir)
        {
            return Directory.EnumerateDirectories(searchDir, PiaFilesDirName, SearchOption.TopDirectoryOnly).Any();
        }
    }
}