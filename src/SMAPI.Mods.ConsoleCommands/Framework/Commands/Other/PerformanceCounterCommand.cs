using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.PerformanceCounter;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Other
{
    // ReSharper disable once UnusedType.Global
    internal class PerformanceCounterCommand : TrainerCommand
    {
        /// <summary>The command names and aliases</summary>
        private readonly Dictionary<SubCommand, string[]> SubCommandNames = new Dictionary<SubCommand, string[]>()
        {
            {SubCommand.Summary, new[] {"summary", "sum", "s"}},
            {SubCommand.Detail, new[] {"detail", "d"}},
            {SubCommand.Reset, new[] {"reset", "r"}},
            {SubCommand.Trigger, new[] {"trigger"}},
            {SubCommand.Examples, new[] {"examples"}},
            {SubCommand.Concepts, new[] {"concepts"}},
            {SubCommand.Help, new[] {"help"}},
        };

        /// <summary>The available commands enum</summary>
        private enum SubCommand
        {
            Summary,
            Detail,
            Reset,
            Trigger,
            Examples,
            Help,
            Concepts,
            None
        }

        /// <summary>Construct an instance.</summary>
        public PerformanceCounterCommand() : base("pc", PerformanceCounterCommand.GetDescription())
        {
        }

        /// <summary>Handle the command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="command">The command name.</param>
        /// <param name="args">The command arguments.</param>
        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            if (args.TryGet(0, "command", out string subCommandString, false))
            {
                SubCommand subSubCommand = this.ParseCommandString(subCommandString);

                switch (subSubCommand)
                {
                    case SubCommand.Summary:
                        this.HandleSummarySubCommand(monitor, args);
                        break;
                    case SubCommand.Detail:
                        this.HandleDetailSubCommand(monitor, args);
                        break;
                    case SubCommand.Reset:
                        this.HandleResetSubCommand(monitor, args);
                        break;
                    case SubCommand.Trigger:
                        this.HandleTriggerSubCommand(monitor, args);
                        break;
                    case SubCommand.Examples:
                        break;
                    case SubCommand.Concepts:
                        this.OutputHelp(monitor, SubCommand.Concepts);
                        break;
                    case SubCommand.Help:
                        if (args.TryGet(1, "command", out string commandString))
                            this.OutputHelp(monitor, this.ParseCommandString(commandString));
                        break;
                    default:
                        this.LogUsageError(monitor, $"Unknown command {subCommandString}");
                        break;
                }
            }
            else
                this.HandleSummarySubCommand(monitor, args);
        }

        /// <summary>Handles the summary sub command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleSummarySubCommand(IMonitor monitor, ArgumentParser args)
        {
            IEnumerable<PerformanceCounterCollection> data;

            if (!args.TryGet(1, "mode", out string mode, false))
            {
                mode = "important";
            }

            switch (mode)
            {
                case null:
                case "important":
                    data = SCore.PerformanceCounterManager.PerformanceCounterCollections.Where(p => p.IsImportant);
                    break;
                case "all":
                    data = SCore.PerformanceCounterManager.PerformanceCounterCollections;
                    break;
                default:
                    data = SCore.PerformanceCounterManager.PerformanceCounterCollections.Where(p =>
                        p.Name.ToLowerInvariant().Contains(mode.ToLowerInvariant()));
                    break;
            }

            double? threshold = null;

            if (args.TryGetDecimal(2, "threshold", out decimal t, false))
            {
                threshold = (double?) t;
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Summary:");
            sb.AppendLine(this.GetTableString(
                data: data,
                header: new[] {"Collection", "Avg Calls/s", "Avg Execution Time (Game)", "Avg Execution Time (Mods)", "Avg Execution Time (Game+Mods)"},
                getRow: item => new[]
                {
                    item.Name,
                    item.GetAverageCallsPerSecond().ToString(),
                    this.FormatMilliseconds(item.GetGameAverageExecutionTime(), threshold),
                    this.FormatMilliseconds(item.GetModsAverageExecutionTime(), threshold),
                    this.FormatMilliseconds(item.GetAverageExecutionTime(), threshold)
                },
                true
            ));

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        /// <summary>Handles the detail sub command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleDetailSubCommand(IMonitor monitor, ArgumentParser args)
        {
            var collections = new List<PerformanceCounterCollection>();
            TimeSpan averageInterval = TimeSpan.FromSeconds(60);
            double? thresholdMilliseconds = null;
            string sourceFilter = null;

            if (args.TryGet(1, "collection", out string collectionName))
            {
                collections.AddRange(SCore.PerformanceCounterManager.PerformanceCounterCollections.Where(
                    collection => collection.Name.ToLowerInvariant().Contains(collectionName.ToLowerInvariant())));

                if (args.IsDecimal(2) && args.TryGetDecimal(2, "threshold", out decimal value, false))
                {
                    thresholdMilliseconds = (double?) value;
                }
                else
                {
                    if (args.TryGet(2, "source", out string sourceName, false))
                    {
                        sourceFilter = sourceName;
                    }
                }
            }

            foreach (PerformanceCounterCollection c in collections)
            {
                this.OutputPerformanceCollectionDetail(monitor, c, averageInterval, thresholdMilliseconds, sourceFilter);
            }
        }

        /// <summary>Handles the trigger sub command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleTriggerSubCommand(IMonitor monitor, ArgumentParser args)
        {
            if (args.TryGet(1, "mode", out string mode, false))
            {
                switch (mode)
                {
                    case "list":
                        this.OutputAlertTriggers(monitor);
                        break;
                    case "collection":
                        if (args.TryGet(2, "name", out string collectionName))
                        {
                            if (args.TryGetDecimal(3, "threshold", out decimal threshold))
                            {
                                if (args.TryGet(4, "source", out string source, false))
                                {
                                    this.ConfigureAlertTrigger(monitor, collectionName, source, threshold);
                                }
                                else
                                {
                                    this.ConfigureAlertTrigger(monitor, collectionName, null, threshold);
                                }
                            }
                        }
                        break;
                    case "pause":
                        SCore.PerformanceCounterManager.PauseAlerts = true;
                        monitor.Log($"Alerts are now paused.", LogLevel.Info);
                        break;
                    case "resume":
                        SCore.PerformanceCounterManager.PauseAlerts = false;
                        monitor.Log($"Alerts are now resumed.", LogLevel.Info);
                        break;
                    case "clear":
                        this.ClearAlertTriggers(monitor);
                        break;
                    default:
                        this.LogUsageError(monitor, $"Unknown mode {mode}. See 'pc help trigger' for usage.");
                        break;
                }

            }
            else
            {
                this.OutputAlertTriggers(monitor);
            }
        }

        /// <summary>Sets up an an alert trigger.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="sourceName">The name of the source, or null for all sources.</param>
        /// <param name="threshold">The trigger threshold, or 0 to remove.</param>
        private void ConfigureAlertTrigger(IMonitor monitor, string collectionName, string sourceName, decimal threshold)
        {
            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.Name.ToLowerInvariant().Equals(collectionName.ToLowerInvariant()))
                {
                    if (sourceName == null)
                    {
                        if (threshold != 0)
                        {
                            collection.EnableAlerts = true;
                            collection.AlertThresholdMilliseconds = (double) threshold;
                            monitor.Log($"Set up alert triggering for '{collectionName}' with '{this.FormatMilliseconds((double?) threshold)}'", LogLevel.Info);
                        }
                        else
                        {
                            collection.EnableAlerts = false;
                            monitor.Log($"Cleared alert triggering for '{collection}'.");
                        }

                        return;
                    }
                    else
                    {
                        foreach (var performanceCounter in collection.PerformanceCounters)
                        {
                            if (performanceCounter.Value.Source.ToLowerInvariant().Equals(sourceName.ToLowerInvariant()))
                            {
                                if (threshold != 0)
                                {
                                    performanceCounter.Value.EnableAlerts = true;
                                    performanceCounter.Value.AlertThresholdMilliseconds = (double) threshold;
                                    monitor.Log($"Set up alert triggering for '{sourceName}' in collection '{collectionName}' with '{this.FormatMilliseconds((double?) threshold)}", LogLevel.Info);
                                }
                                else
                                {
                                    performanceCounter.Value.EnableAlerts = false;
                                }

                                return;
                            }
                        }

                        monitor.Log($"Could not find the source '{sourceName}' in collection '{collectionName}'", LogLevel.Warn);
                        return;
                    }
                }
            }

            monitor.Log($"Could not find the collection '{collectionName}'", LogLevel.Warn);
        }


        /// <summary>Clears alert triggering for all collections.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        private void ClearAlertTriggers(IMonitor monitor)
        {
            int clearedTriggers = 0;
            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.EnableAlerts)
                {
                    collection.EnableAlerts = false;
                    clearedTriggers++;
                }

                foreach (var performanceCounter in collection.PerformanceCounters)
                {
                    if (performanceCounter.Value.EnableAlerts)
                    {
                        performanceCounter.Value.EnableAlerts = false;
                        clearedTriggers++;
                    }
                }

            }

            monitor.Log($"Cleared {clearedTriggers} alert triggers.", LogLevel.Info);
        }

        /// <summary>Lists all configured alert triggers.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        private void OutputAlertTriggers(IMonitor monitor)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Configured triggers:");
            sb.AppendLine();
            var collectionTriggers = new List<(string collectionName, double threshold)>();
            var sourceTriggers = new List<(string collectionName, string sourceName, double threshold)>();

            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.EnableAlerts)
                {
                    collectionTriggers.Add((collection.Name, collection.AlertThresholdMilliseconds));
                }

                sourceTriggers.AddRange(from performanceCounter in
                    collection.PerformanceCounters where performanceCounter.Value.EnableAlerts
                    select (collection.Name, performanceCounter.Value.Source, performanceCounter.Value.AlertThresholdMilliseconds));
            }

            if (collectionTriggers.Count > 0)
            {
                sb.AppendLine("Collection Triggers:");
                sb.AppendLine();
                sb.AppendLine(this.GetTableString(
                    data: collectionTriggers,
                    header: new[] {"Collection", "Threshold"},
                    getRow: item => new[]
                    {
                        item.collectionName,
                        this.FormatMilliseconds(item.threshold)
                    },
                    true
                ));

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("No collection triggers.");
            }

            if (sourceTriggers.Count > 0)
            {
                sb.AppendLine("Source Triggers:");
                sb.AppendLine();
                sb.AppendLine(this.GetTableString(
                    data: sourceTriggers,
                    header: new[] {"Collection", "Source", "Threshold"},
                    getRow: item => new[]
                    {
                        item.collectionName,
                        item.sourceName,
                        this.FormatMilliseconds(item.threshold)
                    },
                    true
                ));

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("No source triggers.");
            }

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        /// <summary>Handles the reset sub command.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="args">The command arguments.</param>
        private void HandleResetSubCommand(IMonitor monitor, ArgumentParser args)
        {
            if (args.TryGet(1, "type", out string type, false, new []{"category", "source"}))
            {
                args.TryGet(2, "name", out string name);

                switch (type)
                {
                    case "category":
                        SCore.PerformanceCounterManager.ResetCollection(name);
                        monitor.Log($"All performance counters for category {name} are now cleared.", LogLevel.Info);
                        break;
                    case "source":
                        SCore.PerformanceCounterManager.ResetSource(name);
                        monitor.Log($"All performance counters for source {name} are now cleared.", LogLevel.Info);
                        break;
                }
            }
            else
            {
                SCore.PerformanceCounterManager.Reset();
                monitor.Log("All performance counters are now cleared.", LogLevel.Info);
            }
        }


        /// <summary>Outputs the details for a collection.</summary>
        /// <param name="monitor">Writes messages to the console and log file.</param>
        /// <param name="collection">The collection.</param>
        /// <param name="averageInterval">The interval over which to calculate the averages.</param>
        /// <param name="thresholdMilliseconds">The threshold.</param>
        /// <param name="sourceFilter">The source filter.</param>
        private void OutputPerformanceCollectionDetail(IMonitor monitor, PerformanceCounterCollection collection,
            TimeSpan averageInterval, double? thresholdMilliseconds, string sourceFilter = null)
        {
            StringBuilder sb = new StringBuilder($"Performance Counter for {collection.Name}:\n\n");

            List<KeyValuePair<string, PerformanceCounter>> data = collection.PerformanceCounters.ToList();

            if (sourceFilter != null)
            {
                data = collection.PerformanceCounters.Where(p =>
                    p.Value.Source.ToLowerInvariant().Contains(sourceFilter.ToLowerInvariant())).ToList();
            }

            if (thresholdMilliseconds != null)
            {
                data = data.Where(p => p.Value.GetAverage(averageInterval) >= thresholdMilliseconds).ToList();
            }

            if (data.Any())
            {
                sb.AppendLine(this.GetTableString(
                    data: data,
                    header: new[] {"Mod", $"Avg Execution Time (last {(int) averageInterval.TotalSeconds}s)", "Last Execution Time", "Peak Execution Time"},
                    getRow: item => new[]
                    {
                        item.Key,
                        this.FormatMilliseconds(item.Value.GetAverage(averageInterval), thresholdMilliseconds),
                        this.FormatMilliseconds(item.Value.GetLastEntry()?.ElapsedMilliseconds),
                        this.FormatMilliseconds(item.Value.GetPeak()?.ElapsedMilliseconds)
                    },
                    true
                ));
            }
            else
            {
                sb.Clear();
                sb.AppendLine($"Performance Counter for {collection.Name}: none.");
            }

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        /// <summary>Parses a command string and returns the associated command.</summary>
        /// <param name="commandString">The command string</param>
        /// <returns>The parsed command.</returns>
        private SubCommand ParseCommandString(string commandString)
        {
            foreach (var i in this.SubCommandNames.Where(i =>
                i.Value.Any(str => str.Equals(commandString, StringComparison.InvariantCultureIgnoreCase))))
            {
                return i.Key;
            }

            return SubCommand.None;
        }


        /// <summary>Formats the given milliseconds value into a string format. Optionally
        /// allows a threshold to return "-" if the value is less than the threshold.</summary>
        /// <param name="milliseconds">The milliseconds to format. Returns "-" if null</param>
        /// <param name="thresholdMilliseconds">The threshold. Any value below this is returned as "-".</param>
        /// <returns>The formatted milliseconds.</returns>
        private string FormatMilliseconds(double? milliseconds, double? thresholdMilliseconds = null)
        {
            if (milliseconds == null || (thresholdMilliseconds != null && milliseconds < thresholdMilliseconds))
            {
                return "-";
            }

            return ((double) milliseconds).ToString("F2");
        }

        /// <summary>Shows detailed help for a specific sub command.</summary>
        /// <param name="monitor">The output monitor</param>
        /// <param name="subCommand">The sub command</param>
        private void OutputHelp(IMonitor monitor, SubCommand subCommand)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            switch (subCommand)
            {
                case SubCommand.Concepts:
                    sb.AppendLine("A performance counter is a metric which measures execution time. Each performance");
                    sb.AppendLine("counter consists of:");
                    sb.AppendLine();
                    sb.AppendLine(" - A source, which typically is a mod or the game itself.");
                    sb.AppendLine(" - A ring buffer which stores the data points (execution time and time when it was executed)");
                    sb.AppendLine();
                    sb.AppendLine("A set of performance counters is organized in a collection to group various areas.");
                    sb.AppendLine("Per default, collections for all game events [1] are created.");
                    sb.AppendLine();
                    sb.AppendLine("Example:");
                    sb.AppendLine();
                    sb.AppendLine("The performance counter collection named 'Display.Rendered' contains one performance");
                    sb.AppendLine("counters when the game executes the 'Display.Rendered' event, and one additional");
                    sb.AppendLine("performance counter for each mod which handles the 'Display.Rendered' event.");
                    sb.AppendLine();
                    sb.AppendLine("[1] https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events");
                    break;
                case SubCommand.Detail:
                    sb.AppendLine("Usage: pc detail <collection> <source>");
                    sb.AppendLine("       pc detail <collection> <threshold>");
                    sb.AppendLine();
                    sb.AppendLine("Displays details for a specific collection.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <collection>  Required. The full or partial name of the collection to display.");
                    sb.AppendLine("  <source>      Optional. The full or partial name of the source.");
                    sb.AppendLine("  <threshold>   Optional. The threshold in milliseconds. Any average execution time below that");
                    sb.AppendLine("                threshold is not reported.");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine("pc detail Display.Rendering                              Displays all performance counters for the 'Display.Rendering' collection");
                    sb.AppendLine("pc detail Display.Rendering Pathoschild.ChestsAnywhere   Displays the 'Display.Rendering' performance counter for 'Pathoschild.ChestsAnywhere'");
                    sb.AppendLine("pc detail Display.Rendering 5                            Displays the 'Display.Rendering' performance counters exceeding an average of 5ms");
                    break;
                case SubCommand.Summary:
                    sb.AppendLine("Usage: pc summary <mode|name> <threshold>");
                    sb.AppendLine();
                    sb.AppendLine("Displays the performance counter summary.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <mode>      Optional. Defaults to 'important' if omitted. Specifies one of these modes:");
                    sb.AppendLine("              - all        Displays performance counters from all collections");
                    sb.AppendLine("              - important  Displays only important performance counter collections");
                    sb.AppendLine();
                    sb.AppendLine("  <name>      Optional. Only shows performance counter collections matching the given name");
                    sb.AppendLine("  <threshold> Optional. Hides the actual execution time if it is below this threshold");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine("pc summary all                Shows all events");
                    sb.AppendLine("pc summary all 5              Shows all events");
                    sb.AppendLine("pc summary Display.Rendering  Shows only the 'Display.Rendering' collection");
                    break;
                case SubCommand.Trigger:
                    sb.AppendLine("Usage: pc trigger <mode>");
                    sb.AppendLine("Usage: pc trigger collection <collectionName> <threshold>");
                    sb.AppendLine("Usage: pc trigger collection <collectionName> <threshold> <sourceName>");
                    sb.AppendLine();
                    sb.AppendLine("Manages alert triggers.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <mode>           Optional. Specifies if a specific source or a specific collection should be triggered.");
                    sb.AppendLine("                   - list        Lists current triggers");
                    sb.AppendLine("                   - collection  Sets up a trigger for a collection");
                    sb.AppendLine("                   - clear       Clears all trigger entries");
                    sb.AppendLine("                   - pause       Pauses triggering of alerts");
                    sb.AppendLine("                   - resume      Resumes triggering of alerts");
                    sb.AppendLine("                   Defaults to 'list' if not specified.");
                    sb.AppendLine();
                    sb.AppendLine("  <collectionName> Required if the mode 'collection' is specified.");
                    sb.AppendLine("                   Specifies the name of the collection to be triggered. Must be an exact match.");
                    sb.AppendLine();
                    sb.AppendLine("  <sourceName>     Optional. Specifies the name of a specific source. Must be an exact match.");
                    sb.AppendLine();
                    sb.AppendLine("  <threshold>      Required if the mode 'collection' is specified.");
                    sb.AppendLine("                   Specifies the threshold in milliseconds (fractions allowed).");
                    sb.AppendLine("                   Specify '0' to remove the threshold.");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine();
                    sb.AppendLine("pc trigger collection Display.Rendering 10");
                    sb.AppendLine("  Sets up an alert trigger which writes on the console if the execution time of all performance counters in");
                    sb.AppendLine("  the 'Display.Rendering' collection exceed 10 milliseconds.");
                    sb.AppendLine();
                    sb.AppendLine("pc trigger collection Display.Rendering 5 Pathoschild.ChestsAnywhere");
                    sb.AppendLine("  Sets up an alert trigger to write on the console if the execution time of Pathoschild.ChestsAnywhere in");
                    sb.AppendLine("  the 'Display.Rendering' collection exceed 5 milliseconds.");
                    sb.AppendLine();
                    sb.AppendLine("pc trigger collection Display.Rendering 0");
                    sb.AppendLine("  Removes the threshold previously defined from the collection. Note that source-specific thresholds are left intact.");
                    sb.AppendLine();
                    sb.AppendLine("pc trigger clear");
                    sb.AppendLine("  Clears all previously setup alert triggers.");
                    break;
                case SubCommand.Reset:
                    sb.AppendLine("Usage: pc reset <type> <name>");
                    sb.AppendLine();
                    sb.AppendLine("Resets performance counters.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <type>  Optional. Specifies if a collection or source should be reset.");
                    sb.AppendLine("          If omitted, all performance counters are reset.");
                    sb.AppendLine();
                    sb.AppendLine("          - source     Clears performance counters for a specific source");
                    sb.AppendLine("          - collection Clears performance counters for a specific collection");
                    sb.AppendLine();
                    sb.AppendLine("  <name>  Required if a <type> is given. Specifies the name of either the collection");
                    sb.AppendLine("          or the source. The name must be an exact match.");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine("pc reset                                    Resets all performance counters");
                    sb.AppendLine("pc reset source Pathoschild.ChestsAnywhere  Resets all performance for the source named Pathoschild.ChestsAnywhere");
                    sb.AppendLine("pc reset collection Display.Rendering       Resets all performance for the collection named Display.Rendering");
                    break;
            }

            sb.AppendLine();
            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        /// <summary>Get the command description.</summary>
        private static string GetDescription()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Displays and configures performance counters.");
            sb.AppendLine();
            sb.AppendLine("A performance counter records the invocation time of in-game events being");
            sb.AppendLine("processed by mods or the game itself. See 'concepts' for a detailed explanation.");
            sb.AppendLine();
            sb.AppendLine("Usage: pc <command> <action>");
            sb.AppendLine();
            sb.AppendLine("Commands:");
            sb.AppendLine();
            sb.AppendLine("  summary|sum|s   Displays a summary of important or all collections");
            sb.AppendLine("  detail|d        Shows performance counter information for a given collection");
            sb.AppendLine("  reset|r         Resets the performance counters");
            sb.AppendLine("  trigger         Configures alert triggers");
            sb.AppendLine("  examples        Displays various examples");
            sb.AppendLine("  concepts        Displays an explanation of the performance counter concepts");
            sb.AppendLine("  help            Displays verbose help for the available commands");
            sb.AppendLine();
            sb.AppendLine("To get help for a specific command, use 'pc help <command>', for example:");
            sb.AppendLine("pc help summary");
            sb.AppendLine();
            sb.AppendLine("Defaults to summary if no command is given.");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
