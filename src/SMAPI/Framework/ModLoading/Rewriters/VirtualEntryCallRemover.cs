using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites virtual calls to the <see cref="Mod.Entry"/> method.</summary>
    internal class VirtualEntryCallRemover : IInstructionHandler
    {
        /*********
        ** Properties
        *********/
        /// <summary>The type containing the method.</summary>
        private readonly Type ToType;

        /// <summary>The name of the method.</summary>
        private readonly string MethodName;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public VirtualEntryCallRemover()
        {
            this.ToType = typeof(Mod);
            this.MethodName = nameof(Mod.Entry);
            this.NounPhrase = $"{this.ToType.Name}::{this.MethodName}";
        }

        /// <summary>Perform the predefined logic for a method if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="method">The method definition containing the instruction.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public InstructionHandleResult Handle(ModuleDefinition module, MethodDefinition method, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            return InstructionHandleResult.None;
        }

        /// <summary>Perform the predefined logic for an instruction if applicable.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="cil">The CIL processor.</param>
        /// <param name="instruction">The instruction to handle.</param>
        /// <param name="assemblyMap">Metadata for mapping assemblies to the current platform.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        public InstructionHandleResult Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction, PlatformAssemblyMap assemblyMap, bool platformChanged)
        {
            if (!this.IsMatch(instruction))
                return InstructionHandleResult.None;

            // get instructions comprising method call
            int index = cil.Body.Instructions.IndexOf(instruction);
            Instruction loadArg0 = cil.Body.Instructions[index - 2];
            Instruction loadArg1 = cil.Body.Instructions[index - 1];
            if (loadArg0.OpCode != OpCodes.Ldarg_0)
                throw new InvalidOperationException($"Unexpected instruction sequence while removing virtual {this.ToType.Name}.{this.MethodName} call: found {loadArg0.OpCode.Name} instead of {OpCodes.Ldarg_0.Name}");
            if (loadArg1.OpCode != OpCodes.Ldarg_1)
                throw new InvalidOperationException($"Unexpected instruction sequence while removing virtual {this.ToType.Name}.{this.MethodName} call: found {loadArg1.OpCode.Name} instead of {OpCodes.Ldarg_1.Name}");

            // remove method call
            cil.Remove(loadArg0);
            cil.Remove(loadArg1);
            cil.Remove(instruction);
            return InstructionHandleResult.Rewritten;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        protected bool IsMatch(Instruction instruction)
        {
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            return
                methodRef != null
                && methodRef.DeclaringType.FullName == this.ToType.FullName
                && methodRef.Name == this.MethodName;
        }
    }
}
