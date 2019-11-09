namespace GrunCS
{
    using gui;
    using NDesk.Options;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConsoleManager.AttachOrCreateConsole();

            bool showHelp = false;
            bool showGui = false;
            bool showTokens = false;
            bool showTree = false;
            string configFileLocation = null;
            string sourceDirectoryOrAssemblyName = null;
            string grammarName = null;
            string startRuleName = null;
            string testFilePath = null;

            // parse command line args
            var options = new OptionSet
            {
                { "s|sourcePath=", "*.cs source directory or an assembly name", v => sourceDirectoryOrAssemblyName = v },
                { "g|grammarName=", "Antlr4 grammar name", v => grammarName = v },
                { "r|ruleName=", "Start rule name", v => startRuleName = v },
                { "f|testFilePath=", "Test file path", v => testFilePath = v },
                { "to|tokens", "Print the tokens", v => showTokens = true},
                { "tr|tree", "Print the tree", v => showTree = true },
                { "gi|gui", "Show the GUI", v => showGui = true },
                { "c|config", "Specify an explicit config file (gruncs.json) location", v => configFileLocation = v },
                { "h|help",  "show this message and exit", v => showHelp = true },
            };

            List<string> extraArgs = null;
            try
            {
                extraArgs = options.Parse(e.Args);
            }
            catch (OptionException exception)
            {
                this.PrintInputException(exception);
                this.Shutdown();
                return;
            }

            // if help flag present, quit out
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                this.Shutdown();
                return;
            }

            // load and parse
            try
            {
                Main main = new Main
                {
                    ShowTokens = showTokens,
                    ShowTree = showTree,
                    ConfigFileLocation = configFileLocation
                };

                main.Process(sourceDirectoryOrAssemblyName, grammarName, startRuleName, testFilePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine("There was an error when trying to parse.");
                Console.WriteLine(exception.Message);

                if (exception is ReflectionTypeLoadException reflectionException)
                {
                    foreach (var ex in reflectionException.LoaderExceptions)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                throw;
            }

            // don't allow the window to show if the gui flag isn't set
            if (!showGui)
            {
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ConsoleManager.DetachConsole();
            base.OnExit(e);
        }

        private void PrintInputException(Exception exception)
        {
            Console.Write("gruncs: ");
            Console.WriteLine(exception.Message);
            Console.WriteLine("Try `gruncs --help for more information.");
        }

        private new void Shutdown()
        {
            Current.Shutdown();
        }
    }
}