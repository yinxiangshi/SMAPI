using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.PerformanceCounter;

namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Other
{
    internal class PerformanceCounterCommand : TrainerCommand
    {
        private readonly Dictionary<Command, string[]> CommandNames = new Dictionary<Command, string[]>()
        {
            {Command.Summary, new[] {"summary", "sum", "s"}},
            {Command.Detail, new[] {"detail", "d"}},
            {Command.Reset, new[] {"reset", "r"}},
            {Command.Monitor, new[] {"monitor"}},
            {Command.Examples, new[] {"examples"}},
            {Command.Concepts, new[] {"concepts"}},
            {Command.Help, new[] {"help"}},
        };

        private enum Command
        {
            Summary,
            Detail,
            Reset,
            Monitor,
            Examples,
            Help,
            Concepts,
            None
        }

        public PerformanceCounterCommand() : base("pc", PerformanceCounterCommand.GetDescription())
        {
        }

        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {
            if (args.TryGet(0, "command", out string subCommandString, false))
            {
                Command subCommand = this.ParseCommandString(subCommandString);

                switch (subCommand)
                {
                    case Command.Summary:
                        this.DisplayPerformanceCounterSummary(monitor, args);
                        break;
                    case Command.Detail:
                        this.DisplayPerformanceCounterDetail(monitor, args);
                        break;
                    case Command.Reset:
                        this.ResetCounter(monitor, args);
                        break;
                    case Command.Monitor:
                        this.HandleMonitor(monitor, args);
                        break;
                    case Command.Examples:
                        break;
                    case Command.Concepts:
                        this.ShowHelp(monitor, Command.Concepts);
                        break;
                    case Command.Help:
                        args.TryGet(1, "command", out string commandString, true);

                        var helpCommand = this.ParseCommandString(commandString);
                        this.ShowHelp(monitor, helpCommand);
                        break;
                    default:
                        this.LogUsageError(monitor, $"Unknown command {subCommandString}");
                        break;
                }
            }
            else
            {
                this.DisplayPerformanceCounterSummary(monitor, args);
            }
        }

        private Command ParseCommandString(string command)
        {
            foreach (var i in this.CommandNames.Where(i => i.Value.Any(str => str.Equals(command, StringComparison.InvariantCultureIgnoreCase))))
            {
                return i.Key;
            }

            return Command.None;
        }

        private void HandleMonitor(IMonitor monitor, ArgumentParser args)
        {
            if (args.TryGet(1, "mode", out string mode, false))
            {
                switch (mode)
                {
                    case "list":
                        this.ListMonitors(monitor);
                        break;
                    case "collection":
                        args.TryGet(2, "name", out string collectionName);
                        decimal threshold = 0;
                        if (args.IsDecimal(3) && args.TryGetDecimal(3, "threshold", out threshold, false))
                        {
                            this.SetCollectionMonitor(monitor, collectionName, null, (double)threshold);
                        } else if (args.TryGet(3, "source", out string source))
                        {
                            if (args.TryGetDecimal(4, "threshold", out threshold))
                            {
                                this.SetCollectionMonitor(monitor, collectionName, source, (double) threshold);
                            }
                        }
                        break;
                    case "clear":
                        this.ClearMonitors(monitor);
                        break;
                    default:
                        monitor.Log($"Unknown mode {mode}. See 'pc help monitor' for usage.");
                        break;
                }

            }
            else
            {
                this.ListMonitors(monitor);
            }
        }

        private void SetCollectionMonitor(IMonitor monitor, string collectionName, string sourceName, double threshold)
        {
            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.Name.ToLowerInvariant().Equals(collectionName.ToLowerInvariant()))
                {
                    if (sourceName == null)
                    {
                        collection.EnableAlerts = true;
                        collection.AlertThresholdMilliseconds = threshold;
                        monitor.Log($"Set up monitor for '{collectionName}' with '{this.FormatMilliseconds(threshold)}'", LogLevel.Info);
                        return;
                    }
                    else
                    {
                        foreach (var performanceCounter in collection.PerformanceCounters)
                        {
                            if (performanceCounter.Value.Source.ToLowerInvariant().Equals(sourceName.ToLowerInvariant()))
                            {
                                performanceCounter.Value.EnableAlerts = true;
                                performanceCounter.Value.AlertThresholdMilliseconds = threshold;
                                monitor.Log($"Set up monitor for '{sourceName}' in collection '{collectionName}' with '{this.FormatMilliseconds(threshold)}", LogLevel.Info);
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


        private void ClearMonitors(IMonitor monitor)
        {
            int clearedCounters = 0;
            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.EnableAlerts)
                {
                    collection.EnableAlerts = false;
                    clearedCounters++;
                }

                foreach (var performanceCounter in collection.PerformanceCounters)
                {
                    if (performanceCounter.Value.EnableAlerts)
                    {
                        performanceCounter.Value.EnableAlerts = false;
                        clearedCounters++;
                    }
                }

            }

            monitor.Log($"Cleared {clearedCounters} counters.", LogLevel.Info);
        }

        private void ListMonitors(IMonitor monitor)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            var collectionMonitors = new List<(string collectionName, double threshold)>();
            var sourceMonitors = new List<(string collectionName, string sourceName, double threshold)>();

            foreach (PerformanceCounterCollection collection in SCore.PerformanceCounterManager.PerformanceCounterCollections)
            {
                if (collection.EnableAlerts)
                {
                    collectionMonitors.Add((collection.Name, collection.AlertThresholdMilliseconds));
                }

                sourceMonitors.AddRange(from performanceCounter in
                    collection.PerformanceCounters where performanceCounter.Value.EnableAlerts
                    select (collection.Name, performanceCounter.Value.Source, MonitorThresholdMilliseconds: performanceCounter.Value.AlertThresholdMilliseconds));
            }

            if (collectionMonitors.Count > 0)
            {
                sb.AppendLine("Collection Monitors:");
                sb.AppendLine();
                sb.AppendLine(this.GetTableString(
                    data: collectionMonitors,
                    header: new[] {"Collection", "Threshold"},
                    getRow: item => new[]
                    {
                        item.collectionName,
                        this.FormatMilliseconds(item.threshold)
                    }
                ));

                sb.AppendLine();


            }

            if (sourceMonitors.Count > 0)
            {
                sb.AppendLine("Source Monitors:");
                sb.AppendLine();
                sb.AppendLine(this.GetTableString(
                    data: sourceMonitors,
                    header: new[] {"Collection", "Source", "Threshold"},
                    getRow: item => new[]
                    {
                        item.collectionName,
                        item.sourceName,
                        this.FormatMilliseconds(item.threshold)
                    }
                ));

                sb.AppendLine();
            }

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        private void ShowHelp(IMonitor monitor, Command command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            switch (command)
            {
                case Command.Concepts:
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
                case Command.Detail:
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
                case Command.Summary:
                    sb.AppendLine("Usage: pc summary <mode|name>");
                    sb.AppendLine();
                    sb.AppendLine("Displays the performance counter summary.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <mode>  Optional. Defaults to 'important' if omitted. Specifies one of these modes:");
                    sb.AppendLine("          - all        Displays performance counters from all collections");
                    sb.AppendLine("          - important  Displays only important performance counter collections");
                    sb.AppendLine();
                    sb.AppendLine("  <name>  Optional. Only shows performance counter collections matching the given name");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine("pc summary all                Shows all events");
                    sb.AppendLine("pc summary Display.Rendering  Shows only the 'Display.Rendering' collection");
                    break;
                case Command.Monitor:
                    sb.AppendLine("Usage: pc monitor <mode> <collectionName> <threshold>");
                    sb.AppendLine("Usage: pc monitor <mode> <collectionName> <sourceName> <threshold>");
                    sb.AppendLine();
                    sb.AppendLine("Manages monitoring settings.");
                    sb.AppendLine();
                    sb.AppendLine("Arguments:");
                    sb.AppendLine("  <mode>           Optional. Specifies if a specific source or a specific collection should be monitored.");
                    sb.AppendLine("                   - list        Lists current monitoring settings");
                    sb.AppendLine("                   - collection  Sets up a monitor for a collection");
                    sb.AppendLine("                   - clear       Clears all monitoring entries");
                    sb.AppendLine("                   Defaults to 'list' if not specified.");
                    sb.AppendLine();
                    sb.AppendLine("  <collectionName> Required if the mode 'collection' is specified.");
                    sb.AppendLine("                   Specifies the name of the collection to be monitored. Must be an exact match.");
                    sb.AppendLine();
                    sb.AppendLine("  <sourceName>     Optional. Specifies the name of a specific source. Must be an exact match.");
                    sb.AppendLine();
                    sb.AppendLine("  <threshold>      Required if the mode 'collection' is specified.");
                    sb.AppendLine("                   Specifies the threshold in milliseconds (fractions allowed).");
                    sb.AppendLine("                   Can also be 'remove' to remove the threshold.");
                    sb.AppendLine();
                    sb.AppendLine("Examples:");
                    sb.AppendLine();
                    sb.AppendLine("pc monitor collection Display.Rendering 10");
                    sb.AppendLine("  Sets up monitoring to write an alert on the console if the execution time of all performance counters in");
                    sb.AppendLine("  the 'Display.Rendering' collection exceed 10 milliseconds.");
                    sb.AppendLine();
                    sb.AppendLine("pc monitor collection Display.Rendering Pathoschild.ChestsAnywhere 5");
                    sb.AppendLine("  Sets up monitoring to write an alert on the console if the execution time of Pathoschild.ChestsAnywhere in");
                    sb.AppendLine("  the 'Display.Rendering' collection exceed 5 milliseconds.");
                    sb.AppendLine();
                    sb.AppendLine("pc monitor collection Display.Rendering remove");
                    sb.AppendLine("  Removes the threshold previously defined from the collection. Note that source-specific thresholds are left intact.");
                    sb.AppendLine();
                    sb.AppendLine("pc monitor clear");
                    sb.AppendLine("  Clears all previously setup monitors.");
                    break;
                case Command.Reset:
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

        private void ResetCounter(IMonitor monitor, ArgumentParser args)
        {
            if (args.TryGet(1, "type", out string type, false))
            {
                args.TryGet(2, "name", out string name);

                switch (type)
                {
                    case "category":
                        SCore.PerformanceCounterManager.ResetCollection(name);
                        monitor.Log($"All performance counters for category {name} are now cleared.", LogLevel.Info);
                        break;
                    case "mod":
                        SCore.PerformanceCounterManager.ResetSource(name);
                        monitor.Log($"All performance counters for mod {name} are now cleared.", LogLevel.Info);
                        break;
                }
            }
            else
            {
                SCore.PerformanceCounterManager.Reset();
                monitor.Log("All performance counters are now cleared.", LogLevel.Info);
            }
        }

        private void DisplayPerformanceCounterSummary(IMonitor monitor, ArgumentParser args)
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

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Summary:");
            sb.AppendLine(this.GetTableString(
                data: data,
                header: new[] {"Collection", "Avg Calls/s", "Avg Execution Time (Game)", "Avg Execution Time (Mods)", "Avg Execution Time (Game+Mods)"},
                getRow: item => new[]
                {
                    item.Name,
                    item.GetAverageCallsPerSecond().ToString(),
                    this.FormatMilliseconds(item.GetGameAverageExecutionTime()),
                    this.FormatMilliseconds(item.GetModsAverageExecutionTime()),
                    this.FormatMilliseconds(item.GetAverageExecutionTime())
                }
            ));

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        private void DisplayPerformanceCounterDetail(IMonitor monitor, ArgumentParser args)
        {
            List<PerformanceCounterCollection> collections = new List<PerformanceCounterCollection>();
            TimeSpan averageInterval = TimeSpan.FromSeconds(60);
            double? thresholdMilliseconds = null;
            string sourceFilter = null;

            if (args.TryGet(1, "collection", out string collectionName))
            {
                collections.AddRange(SCore.PerformanceCounterManager.PerformanceCounterCollections.Where(collection => collection.Name.ToLowerInvariant().Contains(collectionName.ToLowerInvariant())));

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

            foreach (var c in collections)
            {
                this.DisplayPerformanceCollectionDetail(monitor, c, averageInterval, thresholdMilliseconds, sourceFilter);
            }
        }

        private void DisplayPerformanceCollectionDetail(IMonitor monitor, PerformanceCounterCollection collection,
            TimeSpan averageInterval, double? thresholdMilliseconds, string sourceFilter = null)
        {
            StringBuilder sb = new StringBuilder($"Performance Counter for {collection.Name}:\n\n");

            IEnumerable<KeyValuePair<string, PerformanceCounter>> data = collection.PerformanceCounters;

            if (sourceFilter != null)
            {
                data = collection.PerformanceCounters.Where(p =>
                    p.Value.Source.ToLowerInvariant().Contains(sourceFilter.ToLowerInvariant()));
            }

            if (thresholdMilliseconds != null)
            {
                data = data.Where(p => p.Value.GetAverage(averageInterval) >= thresholdMilliseconds);
            }

            sb.AppendLine(this.GetTableString(
                data: data,
                header: new[] {"Mod", $"Avg Execution Time (last {(int) averageInterval.TotalSeconds}s)", "Last Execution Time", "Peak Execution Time"},
                getRow: item => new[]
                {
                    item.Key,
                    this.FormatMilliseconds(item.Value.GetAverage(averageInterval), thresholdMilliseconds),
                    this.FormatMilliseconds(item.Value.GetLastEntry()?.ElapsedMilliseconds),
                    this.FormatMilliseconds(item.Value.GetPeak()?.ElapsedMilliseconds)
                }
            ));

            monitor.Log(sb.ToString(), LogLevel.Info);
        }

        private string FormatMilliseconds(double? milliseconds, double? thresholdMilliseconds = null)
        {
            if (milliseconds == null || (thresholdMilliseconds != null && milliseconds < thresholdMilliseconds))
            {
                return "-";
            }

            return ((double) milliseconds).ToString("F2");
        }

        /// <summary>Get the command description.</summary>
        private static string GetDescription()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Displays and configured performance counters.");
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
            sb.AppendLine("  monitor         Configures monitoring settings");
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
