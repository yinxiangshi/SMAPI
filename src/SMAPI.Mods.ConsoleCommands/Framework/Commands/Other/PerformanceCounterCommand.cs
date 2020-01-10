namespace StardewModdingAPI.Mods.ConsoleCommands.Framework.Commands.Other
{
    internal class PerformanceCounterCommand: TrainerCommand
    {
        public PerformanceCounterCommand(string name, string description) : base("performance_counters", "Displays performance counters")
        {
        }

        public override void Handle(IMonitor monitor, string command, ArgumentParser args)
        {

        }
    }
}
