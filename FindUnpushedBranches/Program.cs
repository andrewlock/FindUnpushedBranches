using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindUnpushedBranches
{
    class Program
    {
        static string[] SkipDirectories = { "node_modules" };
        static void Main(string[] args)
        {
            var baseDirectory = args.Length == 0
                ? Directory.GetCurrentDirectory()
                : args[0];

            if (!Directory.Exists(baseDirectory))
            {
                Console.WriteLine("Unable to search folders");
                Console.WriteLine($"The directory '{baseDirectory}' does not exist");
            }

            Console.WriteLine($"Searching {baseDirectory} for Git repos");

            FindGitFolders(baseDirectory);
        }

        private static void FindGitFolders(string baseDirectory, int level = 0)
        {
            var prefix = new string(' ', level);
            var currentDirectory = Path.GetFileName(baseDirectory);

            if (SkipDirectories.Contains(currentDirectory, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            Console.Write(prefix);
            Console.Write(currentDirectory);
            Console.WriteLine("/");

            var directories = Directory.GetDirectories(baseDirectory);
            var isGitRepo = directories
                .Select(dir => Path.GetFileName(dir))
                .Any(dir => string.Equals(".git", dir, StringComparison.OrdinalIgnoreCase));

            if (isGitRepo)
            {
                PrintUnpushedBranches(baseDirectory, prefix);
            }
            else
            {
                foreach (var directory in directories)
                {
                    FindGitFolders(directory, level + 1);
                }
            }
        }

        private static void PrintUnpushedBranches(string directory, string prefix)
        {
            var previousColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "log --branches --not --remotes --simplify-by-decoration --decorate --oneline",
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                Console.Write(prefix);
                Console.WriteLine(process.StandardOutput.ReadLine());
            }

            Console.ForegroundColor = previousColour;
        }
    }
}
