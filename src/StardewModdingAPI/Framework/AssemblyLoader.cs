using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using StardewModdingAPI.AssemblyRewriters;

namespace StardewModdingAPI.Framework
{
    /// <summary>Preprocesses and loads mod assemblies.</summary>
    internal class AssemblyLoader
    {
        /*********
        ** Properties
        *********/
        /// <summary>Metadata for mapping assemblies to the current platform.</summary>
        private readonly PlatformAssemblyMap AssemblyMap;

        /// <summary>A type => assembly lookup for types which should be rewritten.</summary>
        private readonly IDictionary<string, Assembly> TypeAssemblies;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="targetPlatform">The current game platform.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public AssemblyLoader(Platform targetPlatform, IMonitor monitor)
        {
            this.Monitor = monitor;
            this.AssemblyMap = Constants.GetAssemblyMap(targetPlatform);

            // generate type => assembly lookup for types which should be rewritten
            this.TypeAssemblies = new Dictionary<string, Assembly>();
            foreach (Assembly assembly in this.AssemblyMap.Targets)
            {
                ModuleDefinition module = this.AssemblyMap.TargetModules[assembly];
                foreach (TypeDefinition type in module.GetTypes())
                {
                    if (!type.IsPublic)
                        continue; // no need to rewrite
                    if (type.Namespace.Contains("<"))
                        continue; // ignore assembly metadata
                    this.TypeAssemblies[type.FullName] = assembly;
                }
            }
        }

        /// <summary>Preprocess and load an assembly.</summary>
        /// <param name="assemblyPath">The assembly file path.</param>
        /// <returns>Returns the rewrite metadata for the preprocessed assembly.</returns>
        public Assembly Load(string assemblyPath)
        {
            // get referenced local assemblies
            AssemblyParseResult[] assemblies;
            {
                AssemblyDefinitionResolver resolver = new AssemblyDefinitionResolver();
                assemblies = this.GetReferencedLocalAssemblies(new FileInfo(assemblyPath), new HashSet<string>(), resolver).ToArray();
                if (!assemblies.Any())
                    throw new InvalidOperationException($"Could not load '{assemblyPath}' because it doesn't exist.");
                resolver.Add(assemblies.Select(p => p.Definition).ToArray());
            }

            // rewrite & load assemblies in leaf-to-root order
            Assembly lastAssembly = null;
            foreach (AssemblyParseResult assembly in assemblies)
            {
                this.Monitor.Log($"Loading {assembly.File.Name}...", LogLevel.Trace);
                bool changed = this.RewriteAssembly(assembly.Definition);
                if (changed)
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        assembly.Definition.Write(outStream);
                        byte[] bytes = outStream.ToArray();
                        lastAssembly = Assembly.Load(bytes);
                    }
                }
                else
                    lastAssembly = Assembly.UnsafeLoadFrom(assembly.File.FullName);
            }

            // last assembly loaded is the root
            return lastAssembly;
        }

        /// <summary>Resolve an assembly by its name.</summary>
        /// <param name="name">The assembly name.</param>
        /// <remarks>
        /// This implementation returns the first loaded assembly which matches the short form of
        /// the assembly name, to resolve assembly resolution issues when rewriting
        /// assemblies (especially with Mono). Since this is meant to be called on <see cref="AppDomain.AssemblyResolve"/>,
        /// the implicit assumption is that loading the exact assembly failed.
        /// </remarks>
        public Assembly ResolveAssembly(string name)
        {
            string shortName = name.Split(new[] { ',' }, 2).First(); // get simple name (without version and culture)
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(p => p.GetName().Name == shortName);
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Assembly parsing
        ****/
        /// <summary>Get a list of referenced local assemblies starting from the mod assembly, ordered from leaf to root.</summary>
        /// <param name="file">The assembly file to load.</param>
        /// <param name="visitedAssemblyPaths">The assembly paths that should be skipped.</param>
        /// <returns>Returns the rewrite metadata for the preprocessed assembly.</returns>
        private IEnumerable<AssemblyParseResult> GetReferencedLocalAssemblies(FileInfo file, HashSet<string> visitedAssemblyPaths, IAssemblyResolver assemblyResolver)
        {
            // validate
            if (file.Directory == null)
                throw new InvalidOperationException($"Could not get directory from file path '{file.FullName}'.");
            if (visitedAssemblyPaths.Contains(file.FullName))
                yield break; // already visited
            if (!file.Exists)
                yield break; // not a local assembly
            visitedAssemblyPaths.Add(file.FullName);

            // read assembly
            byte[] assemblyBytes = File.ReadAllBytes(file.FullName);
            AssemblyDefinition assembly;
            using (Stream readStream = new MemoryStream(assemblyBytes))
                assembly = AssemblyDefinition.ReadAssembly(readStream, new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = assemblyResolver });

            // yield referenced assemblies
            foreach (AssemblyNameReference dependency in assembly.MainModule.AssemblyReferences)
            {
                FileInfo dependencyFile = new FileInfo(Path.Combine(file.Directory.FullName, $"{dependency.Name}.dll"));
                foreach (AssemblyParseResult result in this.GetReferencedLocalAssemblies(dependencyFile, visitedAssemblyPaths, assemblyResolver))
                    yield return result;
            }

            // yield assembly
            yield return new AssemblyParseResult(file, assembly);
        }

        /****
        ** Assembly rewriting
        ****/
        /// <summary>Rewrite the types referenced by an assembly.</summary>
        /// <param name="assembly">The assembly to rewrite.</param>
        /// <returns>Returns whether the assembly was modified.</returns>
        private bool RewriteAssembly(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.Modules.Single(); // technically an assembly can have multiple modules, but none of the build tools (including MSBuild) support it; simplify by assuming one module

            // remove old assembly references
            bool shouldRewrite = false;
            for (int i = 0; i < module.AssemblyReferences.Count; i++)
            {
                if (this.AssemblyMap.RemoveNames.Any(name => module.AssemblyReferences[i].Name == name))
                {
                    shouldRewrite = true;
                    module.AssemblyReferences.RemoveAt(i);
                    i--;
                }
            }
            if (!shouldRewrite)
                return false;

            // add target assembly references
            foreach (AssemblyNameReference target in this.AssemblyMap.TargetReferences.Values)
                module.AssemblyReferences.Add(target);

            // rewrite type scopes to use target assemblies
            IEnumerable<TypeReference> typeReferences = module.GetTypeReferences().OrderBy(p => p.FullName);
            foreach (TypeReference type in typeReferences)
                this.ChangeTypeScope(type);

            // rewrite incompatible methods
            IMethodRewriter[] methodRewriters = Constants.GetMethodRewriters().ToArray();
            foreach (MethodDefinition method in this.GetMethods(module))
            {
                // skip methods with no rewritable method
                bool hasMethodToRewrite = method.Body.Instructions.Any(op => (op.OpCode == OpCodes.Call || op.OpCode == OpCodes.Callvirt) && methodRewriters.Any(rewriter => rewriter.ShouldRewrite((MethodReference)op.Operand)));
                if (!hasMethodToRewrite)
                    continue;

                // rewrite method references
                method.Body.SimplifyMacros();
                ILProcessor cil = method.Body.GetILProcessor();
                Instruction[] instructions = cil.Body.Instructions.ToArray();
                foreach (Instruction op in instructions)
                {
                    if (op.OpCode == OpCodes.Call || op.OpCode == OpCodes.Callvirt)
                    {
                        IMethodRewriter rewriter = methodRewriters.FirstOrDefault(p => p.ShouldRewrite((MethodReference)op.Operand));
                        if (rewriter != null)
                        {
                            MethodReference methodRef = (MethodReference)op.Operand;
                            rewriter.Rewrite(module, cil, op, methodRef, this.AssemblyMap);
                        }
                    }
                }
                method.Body.OptimizeMacros();
            }
            return true;
        }

        /// <summary>Get the correct reference to use for compatibility with the current platform.</summary>
        /// <param name="type">The type reference to rewrite.</param>
        private void ChangeTypeScope(TypeReference type)
        {
            // check skip conditions
            if (type == null || type.FullName.StartsWith("System."))
                return;

            // get assembly
            Assembly assembly;
            if (!this.TypeAssemblies.TryGetValue(type.FullName, out assembly))
                return;

            // replace scope
            AssemblyNameReference assemblyRef = this.AssemblyMap.TargetReferences[assembly];
            type.Scope = assemblyRef;
        }

        /// <summary>Get all methods in a module.</summary>
        /// <param name="module">The module to search.</param>
        private IEnumerable<MethodDefinition> GetMethods(ModuleDefinition module)
        {
            return (
                from type in module.GetTypes()
                where type.HasMethods
                from method in type.Methods
                where method.HasBody
                select method
            );
        }
    }
}
