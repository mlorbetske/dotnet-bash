using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConsoleApplication
{
    public enum NextCompletionType
    {
        None,
        WordList,
        DirectoryList,
        FileList,
        Suppress
    }

    public class CommandOption
    {
        private Func<string, IEnumerable<string>> _nextArg;
    
        private Func<string, bool> _keepCompleting;
        private object getNewTypeCompletions;
        private string v1;
        private string v2;

        public CommandOption(params string[] forms)
            : this(NextCompletionType.None, forms)
        {
        }

        public CommandOption(NextCompletionType nextType, params string[] forms)
        {
            NextType = nextType;
            Forms = forms;
        }

        public CommandOption(Func<string, IEnumerable<string>> nextArg, params string[] forms)
            : this (nextArg, s => true, forms)
        {
        }

        public CommandOption(Func<string, IEnumerable<string>> nextArg, Func<string, bool> keepCompleting, params string[] forms)
        {
            NextType = NextCompletionType.WordList;
            Forms = forms;
            _nextArg = nextArg;
            _keepCompleting = keepCompleting;
        }

        public CommandOption(object getNewTypeCompletions, string v1, string v2)
        {
            this.getNewTypeCompletions = getNewTypeCompletions;
            this.v1 = v1;
            this.v2 = v2;
        }

        public NextCompletionType NextType { get; }

        public IEnumerable<string> Forms { get; }

        public bool Include(string[] args)
        {
            foreach (string item in args)
            {
                if(Forms.Contains(item.Trim()))
                {
                    return false;
                }
            }

           return true; 
        }

        public bool TryHandlePositionalArg(string[] args, out IEnumerable<string> nextArg)
        {
            if(_nextArg != null && _keepCompleting != null && (Forms.Contains(args[args.Length - 1].Trim()) || Forms.Contains(args[args.Length - 2].Trim())) && _keepCompleting(args[args.Length - 1]))
            {
                nextArg = _nextArg(args[args.Length - 1]);

                if(nextArg.Count() == 1 && nextArg.First() == args[args.Length - 1])
                {
                    return false;
                }

                return true;
            }

            if (Forms.Contains(args[args.Length - 1].Trim()) && _nextArg == null)
            {
                nextArg = Enumerable.Empty<string>();
                return NextType != NextCompletionType.None;
            }

            nextArg = null;
            return false;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            IEnumerable<string> options = null;
            NextCompletionType completionType = NextCompletionType.None;

            switch (args[1])
            {
                case "build":
                    options = GetBuildOpts(args, out completionType);
                    break;
                case "new":
                    options = GetNewOpts(args, out completionType);
                    break;
                case "restore":
                    options = GetRestoreOpts(args, out completionType);
                    break;
                case "publish":
                    options = GetPublishOpts(args, out completionType);
                    break;
                case "run":
                    options = GetRunOpts(args, out completionType);
                    break;
                case "test":
                    options = GetTestOpts(args, out completionType);
                    break;
                case "pack":
                    options = GetPackOpts(args, out completionType);
                    break;
                case "help":
                    options = GetHelpOpts(args, out completionType);
                    break;
            }

            if (options != null)
            {
                switch (completionType)
                {
                    case NextCompletionType.WordList:
                        string wordList = string.Join(" ", options);
                        Console.WriteLine($"WORDS:{wordList}");
                        break;
                    case NextCompletionType.DirectoryList:
                        Console.WriteLine("DIRS:");
                        break;
                    case NextCompletionType.FileList:
                        Console.WriteLine("FILES:");
                        break;
                }
            }
        }

        private static IEnumerable<string> GetPackOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(NextCompletionType.DirectoryList, "-o", "--output"),
                new CommandOption("--no-build"),
                new CommandOption(NextCompletionType.DirectoryList, "-b", "--build-base-path"),
                new CommandOption(GetCommonBuildConfigurations, "-c", "--configuration"),
                new CommandOption(NextCompletionType.Suppress, "--version-suffix"),
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetTestOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-?", "-h", "--help"),
                new CommandOption(NextCompletionType.Suppress, "--parentProcessId"),
                new CommandOption(NextCompletionType.Suppress, "--port"),
                new CommandOption(GetCommonBuildConfigurations, "-c", "--configuration"),
                new CommandOption(NextCompletionType.DirectoryList, "-o", "--output"),
                new CommandOption(NextCompletionType.DirectoryList, "-b", "--build-base-path"),
                new CommandOption(NextCompletionType.Suppress, "-f", "--framework"), //TODO: Suggest some frameworks
                new CommandOption(GetCompatibleRuntimeList, "-r", "--runtime"), //TODO: Suggest the current RID
                new CommandOption("--no-build")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetCompatibleRuntimeList(string arg)
        {
            ProcessStartInfo psi = new ProcessStartInfo("dotnet", "--info");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            Process p = Process.Start(psi);
            p.WaitForExit();
            string allText = p.StandardOutput.ReadToEnd();
            return new []{allText.Substring(allText.IndexOf("RID:") + 4).Trim()};
        }

        private static IEnumerable<string> GetRunOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(NextCompletionType.Suppress, "-f", "--framework"), //TODO: Suggest some frameworks
                new CommandOption(GetCommonBuildConfigurations, "-c", "--configuration"),
                new CommandOption(NextCompletionType.DirectoryList, "-p", "--project")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetPublishOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(NextCompletionType.Suppress, "-f", "--framework"), //TODO: Suggest some frameworks
                new CommandOption(GetCompatibleRuntimeList, "-r", "--runtime"), //TODO: Suggest the current RID
                new CommandOption(NextCompletionType.DirectoryList, "-b", "--build-base-path"),
                new CommandOption(NextCompletionType.DirectoryList, "-o", "--output"),
                new CommandOption(NextCompletionType.Suppress, "--version-suffix"),
                new CommandOption(GetCommonBuildConfigurations, "-c", "--configuration"),
                new CommandOption("--native-subdirectory"),
                new CommandOption("--no-build")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetRestoreOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(NextCompletionType.None, "--force-english-output"),
                new CommandOption(NextCompletionType.DirectoryList, "-s", "--source"),
                new CommandOption(NextCompletionType.DirectoryList, "--packages"),
                new CommandOption("--disable-parallel"),
                new CommandOption(NextCompletionType.DirectoryList, "-f", "--fallbacksource"),
                new CommandOption(NextCompletionType.FileList, "--configfile"),
                new CommandOption("--no-cache"),
                new CommandOption("--infer-runtimes"),
                new CommandOption(GetRestoreVerbosityOptions, "-v", "--verbosity"),
                new CommandOption("--ignore-failed-sources")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetRestoreVerbosityOptions(string arg)
        {
            return new[]{"Debug", "Verbose", "Information", "Minimal", "Warning", "Error"};
        }

        private static IEnumerable<string> GetNewOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(GetNewLangCompletions, "-l", "--lang"),
                new CommandOption(GetNewTypeCompletions, "-t", "--type"),
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetNewTypeCompletions(string arg)
        {
            return new[]{"Console"};
        }

        private static IEnumerable<string> GetNewLangCompletions(string arg)
        {
            return new[]{"C#", "F#"};
        }

        private static IEnumerable<string> GetHelpOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption(NextCompletionType.Suppress, "new"),
                new CommandOption(NextCompletionType.Suppress, "restore"),
                new CommandOption(NextCompletionType.Suppress, "build"),
                new CommandOption(NextCompletionType.Suppress, "publish"),
                new CommandOption(NextCompletionType.Suppress, "run"),
                new CommandOption(NextCompletionType.Suppress, "test"),
                new CommandOption(NextCompletionType.Suppress, "pack")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetBuildOpts(string[] args, out NextCompletionType completionType)
        {
            CommandOption[] opts = new CommandOption[]
            {
                new CommandOption("-h", "--help"),
                new CommandOption(NextCompletionType.DirectoryList, "-o", "--output"),
                new CommandOption(NextCompletionType.DirectoryList, "-b", "--build-base-path"),
                new CommandOption(NextCompletionType.Suppress, "-f", "--framework"), //TODO: Suggest some frameworks
                new CommandOption(GetCompatibleRuntimeList, "-r", "--runtime"), //TODO: Suggest the current RID
                new CommandOption(GetCommonBuildConfigurations, "-c", "--configuration"),
                new CommandOption(NextCompletionType.Suppress, "--version-suffix"),
                new CommandOption("--build-profile"),
                new CommandOption("--no-incremental"),
                new CommandOption("--no-dependencies")
            };

            return ProcessOptions(args, opts, out completionType);
        }

        private static IEnumerable<string> GetCommonBuildConfigurations(string arg)
        {
            return new[]{"Debug", "Release"};
        }

        private static IEnumerable<string> ProcessOptions(string[] args, CommandOption[] opts, out NextCompletionType completionType)
        {
            List<string> allArgs = new List<string>();

            foreach (CommandOption item in opts)
            {
                if (!item.Include(args))
                {
                    IEnumerable<string> extended;
                    if(item.TryHandlePositionalArg(args, out extended))
                    {
                        completionType = item.NextType;
                        return extended;
                    }
                }
                else
                {
                    allArgs.AddRange(item.Forms);
                }
            }

            completionType = NextCompletionType.WordList;
            return allArgs;
        }

        private static IEnumerable<string> SuggestCurrentRid()
        {
            throw new NotImplementedException();
        }
    }
}
