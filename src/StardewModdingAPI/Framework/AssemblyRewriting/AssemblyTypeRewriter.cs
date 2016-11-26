using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CallSite = Mono.Cecil.CallSite;

namespace StardewModdingAPI.Framework.AssemblyRewriting
{
    /// <summary>Rewrites type references.</summary>
    internal class AssemblyTypeRewriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The assemblies to target. Equivalent types will be rewritten to use these assemblies.</summary>
        private readonly Assembly[] TargetAssemblies;

        /// <summary>>The short assembly names to remove as assembly reference, and replace with the <see cref="TargetAssemblies"/>.</summary>
        private readonly string[] RemoveAssemblyNames;

        /// <summary>A type => assembly lookup for types which should be rewritten.</summary>
        private readonly IDictionary<string, Assembly> TypeAssemblies;

        /// <summary>An assembly => reference cache.</summary>
        private readonly IDictionary<Assembly, AssemblyNameReference> AssemblyNameReferences;

        /// <summary>An assembly => module cache.</summary>
        private readonly IDictionary<Assembly, ModuleDefinition> AssemblyModules;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="targetAssemblies">The assembly filenames to target. Equivalent types will be rewritten to use these assemblies.</param>
        /// <param name="removeAssemblyNames">The short assembly names to remove as assembly reference, and replace with the <paramref name="targetAssemblies"/>.</param>
        public AssemblyTypeRewriter(Assembly[] targetAssemblies, string[] removeAssemblyNames)
        {
            // save config
            this.TargetAssemblies = targetAssemblies;
            this.RemoveAssemblyNames = removeAssemblyNames;

            // cache assembly metadata
            this.AssemblyNameReferences = targetAssemblies.ToDictionary(assembly => assembly, assembly => AssemblyNameReference.Parse(assembly.FullName));
            this.AssemblyModules = targetAssemblies.ToDictionary(assembly => assembly, assembly => ModuleDefinition.ReadModule(assembly.Modules.Single().FullyQualifiedName)); // technically an assembly can contain multiple modules, but none of the build tools (including MSBuild itself) support it

            // collect type => assembly lookup
            this.TypeAssemblies = new Dictionary<string, Assembly>();
            foreach (Assembly assembly in targetAssemblies)
            {
                ModuleDefinition module = this.AssemblyModules[assembly];
                foreach (TypeDefinition type in module.GetTypes())
                {
                    if (!type.IsPublic)
                        continue; // no need to rewrite
                    if (type.Namespace.Contains("<"))
                        continue; // ignore C++ stuff
                    this.TypeAssemblies[type.FullName] = assembly;
                }
            }
        }

        /// <summary>Rewrite the types referenced by an assembly.</summary>
        /// <param name="assembly">The assembly to rewrite.</param>
        public void RewriteAssembly(AssemblyDefinition assembly)
        {
            foreach (ModuleDefinition module in assembly.Modules)
            {
                // rewrite assembly references
                bool shouldRewriteTypes = false;
                for (int i = 0; i < module.AssemblyReferences.Count; i++)
                {
                    bool shouldRemove = this.RemoveAssemblyNames.Any(name => module.AssemblyReferences[i].Name == name) || this.TargetAssemblies.Any(a => module.AssemblyReferences[i].Name == a.GetName().Name);
                    if (shouldRemove)
                    {
                        shouldRewriteTypes = true;
                        module.AssemblyReferences.RemoveAt(i);
                        i--;
                    }
                }
                foreach (AssemblyNameReference target in this.AssemblyNameReferences.Values)
                {
                    module.AssemblyReferences.Add(target);
                    shouldRewriteTypes = true;
                }

                // rewrite references
                if (shouldRewriteTypes)
                {
                    // rewrite types
                    foreach (TypeDefinition type in module.GetTypes())
                        this.RewriteReferences(type, module);

                    // rewrite type references
                    TypeReference[] refs = (TypeReference[])module.GetTypeReferences();
                    for (int i = 0; i < refs.Length; ++i)
                        refs[i] = this.GetTypeReference(refs[i], module);
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Rewrite the references for a code object.</summary>
        /// <param name="type">The type to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private void RewriteReferences(TypeDefinition type, ModuleDefinition module)
        {
            // rewrite base type
            type.BaseType = this.GetTypeReference(type.BaseType, module);

            // rewrite interfaces
            for (int i = 0; i < type.Interfaces.Count; i++)
                type.Interfaces[i] = this.GetTypeReference(type.Interfaces[i], module);

            // rewrite events
            foreach (EventDefinition @event in type.Events)
            {
                this.RewriteReferences(@event.AddMethod, module);
                this.RewriteReferences(@event.RemoveMethod, module);
                this.RewriteReferences(@event.InvokeMethod, module);
            }

            // rewrite properties
            foreach (PropertyDefinition property in type.Properties)
            {
                this.RewriteReferences(property.GetMethod, module);
                this.RewriteReferences(property.SetMethod, module);
            }

            // rewrite methods
            foreach (MethodDefinition method in type.Methods)
                this.RewriteReferences(method, module);

            // rewrite fields
            foreach (FieldDefinition field in type.Fields)
                this.RewriteReferences(field, module);

            // rewrite nested types
            foreach (TypeDefinition nestedType in type.NestedTypes)
                this.RewriteReferences(nestedType, module);

            // rewrite generic parameters
            foreach (GenericParameter parameter in type.GenericParameters)
                this.RewriteReferences(parameter, module);

            module.Import(type);
        }

        /// <summary>Rewrite the references for a code object.</summary>
        /// <param name="method">The method to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private void RewriteReferences(MethodReference method, ModuleDefinition module)
        {
            // parameter types
            if (method.HasParameters)
            {
                foreach (ParameterDefinition parameter in method.Parameters)
                    parameter.ParameterType = this.GetTypeReference(parameter.ParameterType, module);
            }

            // return type
            method.MethodReturnType.ReturnType = this.GetTypeReference(method.MethodReturnType.ReturnType, module);

            module.Import(method);
        }

        /// <summary>Rewrite the references for a code object.</summary>
        /// <param name="method">The method to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private void RewriteReferences(MethodDefinition method, ModuleDefinition module)
        {
            if (method == null)
                return;

            this.RewriteReferences((MethodReference)method, module);

            // overrides
            foreach (MethodReference @override in method.Overrides)
                this.RewriteReferences(@override, module);

            // body
            if (method.HasBody)
            {
                // this
                if (method.Body.ThisParameter != null)
                    method.Body.ThisParameter.ParameterType = this.GetTypeReference(method.Body.ThisParameter.ParameterType, module);

                // variables
                if (method.Body.HasVariables)
                {
                    foreach (VariableDefinition variable in method.Body.Variables)
                        variable.VariableType = this.GetTypeReference(variable.VariableType, module);
                }

                // instructions
                foreach (Instruction instruction in method.Body.Instructions)
                {
                    object operand = instruction.Operand;

                    // type
                    {
                        TypeReference type = operand as TypeReference;
                        if (type != null)
                        {
                            instruction.Operand = this.GetTypeReference(type, module);
                            continue;
                        }
                    }

                    // method
                    {
                        MethodReference methodRef = operand as MethodReference;
                        if (methodRef != null)
                        {
                            this.RewriteReferences(methodRef, module);
                            continue;
                        }
                    }

                    // field
                    {
                        FieldReference field = operand as FieldReference;
                        if (field != null)
                        {
                            this.RewriteReferences(field, module);
                            continue;
                        }
                    }

                    // variable
                    {
                        VariableDefinition variable = operand as VariableDefinition;
                        if (variable != null)
                        {
                            variable.VariableType = this.GetTypeReference(variable.VariableType, module);
                            continue;
                        }
                    }

                    // parameter
                    {
                        ParameterDefinition parameter = operand as ParameterDefinition;
                        if (parameter != null)
                        {
                            parameter.ParameterType = this.GetTypeReference(parameter.ParameterType, module);
                            continue;
                        }
                    }

                    // call site
                    {
                        CallSite call = operand as CallSite;
                        if (call != null)
                        {
                            foreach (ParameterDefinition parameter in call.Parameters)
                                parameter.ParameterType = this.GetTypeReference(parameter.ParameterType, module);
                            call.ReturnType = this.GetTypeReference(call.ReturnType, module);
                        }
                    }
                }
            }

            module.Import(method);
        }

        /// <summary>Rewrite the references for a code object.</summary>
        /// <param name="parameter">The generic parameter to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private void RewriteReferences(GenericParameter parameter, ModuleDefinition module)
        {
            // constraints
            for (int i = 0; i < parameter.Constraints.Count; i++)
                parameter.Constraints[i] = this.GetTypeReference(parameter.Constraints[i], module);

            // generic parameters
            foreach (GenericParameter genericParam in parameter.GenericParameters)
                this.RewriteReferences(genericParam, module);
        }

        /// <summary>Rewrite the references for a code object.</summary>
        /// <param name="field">The field to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private void RewriteReferences(FieldReference field, ModuleDefinition module)
        {
            field.DeclaringType = this.GetTypeReference(field.DeclaringType, module);
            field.FieldType = this.GetTypeReference(field.FieldType, module);
            module.Import(field);
        }

        /// <summary>Get the correct reference to use for compatibility with the current platform.</summary>
        /// <param name="type">The type reference to rewrite.</param>
        /// <param name="module">The module being rewritten.</param>
        private TypeReference GetTypeReference(TypeReference type, ModuleDefinition module)
        {
            // check skip conditions
            if (type == null)
                return null;
            if (type.FullName.StartsWith("System."))
                return type;

            // get assembly
            Assembly assembly;
            if (!this.TypeAssemblies.TryGetValue(type.FullName, out assembly))
                return type;

            // replace type
            AssemblyNameReference newAssembly = this.AssemblyNameReferences[assembly];
            ModuleDefinition newModule = this.AssemblyModules[assembly];
            type = new TypeReference(type.Namespace, type.Name, newModule, newAssembly);

            return module.Import(type);
        }
    }
}
