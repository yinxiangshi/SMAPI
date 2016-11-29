using System.Reflection;

namespace StardewModdingAPI.AssemblyRewriters
{
    /// <summary>Metadata for mapping assemblies to the current <see cref="Platform"/>.</summary>
    public class PlatformAssemblyMap
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The target game platform.</summary>
        public readonly Platform TargetPlatform;

        /// <summary>The short assembly names to remove as assembly reference, and replace with the <see cref="Targets"/>. These should be short names (like "Stardew Valley").</summary>
        public readonly string[] RemoveNames;

        /// <summary>The assembly filenames to target. Equivalent types should be rewritten to use these assemblies.</summary>
        public readonly Assembly[] Targets;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="targetPlatform">The target game platform.</param>
        /// <param name="removeAssemblyNames">The assembly short names to remove (like <c>Stardew Valley</c>).</param>
        /// <param name="targetAssemblies">The assemblies to target.</param>
        public PlatformAssemblyMap(Platform targetPlatform, string[] removeAssemblyNames, Assembly[] targetAssemblies)
        {
            this.TargetPlatform = targetPlatform;
            this.RemoveNames = removeAssemblyNames;
            this.Targets = targetAssemblies;
        }
    }
}