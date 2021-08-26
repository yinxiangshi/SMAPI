using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Symbols
{
    /// <summary>Provides assembly symbol readers for Mono.Cecil.</summary>
    internal class SymbolReaderProvider : ISymbolReaderProvider
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying symbol reader provider.</summary>
        private readonly ISymbolReaderProvider BaseProvider = new DefaultSymbolReaderProvider();

        /// <summary>The symbol data loaded by absolute assembly path.</summary>
        private readonly Dictionary<string, Stream> SymbolsByAssemblyPath = new Dictionary<string, Stream>(StringComparer.OrdinalIgnoreCase);


        /*********
        ** Public methods
        *********/
        /// <summary>Add the symbol file for a given assembly name, if it's not already registered.</summary>
        /// <param name="fileName">The assembly file name.</param>
        /// <param name="symbolStream">The raw file stream for the symbols.</param>
        public void AddSymbolData(string fileName, Stream symbolStream)
        {
            this.SymbolsByAssemblyPath.Add(fileName, symbolStream);
        }

        /// <summary>Get a symbol reader for a given module and assembly name.</summary>
        /// <param name="module">The loaded assembly module.</param>
        /// <param name="fileName">The assembly file name.</param>
        public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
        {
            return this.SymbolsByAssemblyPath.TryGetValue(module.Name, out Stream symbolData)
                ? new SymbolReader(module, symbolData)
                : this.BaseProvider.GetSymbolReader(module, fileName);
        }

        /// <summary>Get a symbol reader for a given module and symbol stream.</summary>
        /// <param name="module">The loaded assembly module.</param>
        /// <param name="symbolStream">The loaded symbol file stream.</param>
        public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
        {
            return this.SymbolsByAssemblyPath.TryGetValue(module.Name, out Stream symbolData)
                ? new SymbolReader(module, symbolData)
                : this.BaseProvider.GetSymbolReader(module, symbolStream);
        }
    }
}
