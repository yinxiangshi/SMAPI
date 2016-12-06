using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using StardewModdingAPI.AssemblyRewriters;

namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Rewrites type references.</summary>
    internal class AssemblyTypeRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>Metadata for mapping assemblies to the current <see cref="Platform"/>.</summary>
        private readonly PlatformAssemblyMap AssemblyMap;

        /// <summary>A type => assembly lookup for types which should be rewritten.</summary>
        private readonly IDictionary<string, Assembly> TypeAssemblies;

        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current <see cref="Platform"/>.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public AssemblyTypeRewriter(PlatformAssemblyMap assemblyMap, IMonitor monitor)
        {
            // save config
            this.AssemblyMap = assemblyMap;
            this.Monitor = monitor;

            // collect type => assembly lookup
            this.TypeAssemblies = new Dictionary<string, Assembly>();
            foreach (Assembly assembly in assemblyMap.Targets)
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

        /// <summary>Rewrite the types referenced by an assembly.</summary>
        /// <param name="assembly">The assembly to rewrite.</param>
        /// <returns>Returns whether the assembly was modified.</returns>
        public bool RewriteAssembly(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.Modules.Single(); // technically an assembly can have multiple modules, but none of the build tools (including MSBuild) support it; simplify by assuming one module

            // remove old assembly references
            bool shouldRewrite = false;
            for (int i = 0; i < module.AssemblyReferences.Count; i++)
            {
                if (this.AssemblyMap.RemoveNames.Any(name => module.AssemblyReferences[i].Name == name))
                {
                    this.Monitor.Log($"removing reference to {module.AssemblyReferences[i]}", LogLevel.Trace);
                    shouldRewrite = true;
                    module.AssemblyReferences.RemoveAt(i);
                    i--;
                }
            }
            if (!shouldRewrite)
                return false;

            // add target assembly references
            foreach (AssemblyNameReference target in this.AssemblyMap.TargetReferences.Values)
            {
                this.Monitor.Log($"  adding reference to {target}", LogLevel.Trace);
                module.AssemblyReferences.Add(target);
            }

            // rewrite type scopes to use target assemblies
            IEnumerable<TypeReference> typeReferences = module.GetTypeReferences().OrderBy(p => p.FullName);
            string lastTypeLogged = null;
            foreach (TypeReference type in typeReferences)
            {
                this.ChangeTypeScope(type, shouldLog: type.FullName != lastTypeLogged);
                lastTypeLogged = type.FullName;
            }

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
                            this.Monitor.Log($"rewriting method reference {methodRef.DeclaringType.FullName}.{methodRef.Name}", LogLevel.Trace);
                            rewriter.Rewrite(module, cil, op, methodRef, this.AssemblyMap);
                        }
                    }
                }
                method.Body.OptimizeMacros();
            }
            return true;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the correct reference to use for compatibility with the current platform.</summary>
        /// <param name="type">The type reference to rewrite.</param>
        /// <param name="shouldLog">Whether to log a message.</param>
        private void ChangeTypeScope(TypeReference type, bool shouldLog)
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
            if (shouldLog)
                this.Monitor.Log($"redirecting {type.FullName} from {type.Scope.Name} to {assemblyRef.Name}", LogLevel.Trace);
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
