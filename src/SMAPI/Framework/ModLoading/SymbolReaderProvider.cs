using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace StardewModdingAPI.Framework.ModLoading
{
    internal class SymbolReaderProvider : ISymbolReaderProvider
    {
        private readonly ISymbolReaderProvider BaseProvider = new DefaultSymbolReaderProvider();

        private readonly Dictionary<string, Stream> SymbolMapping = new Dictionary<string, Stream>();

        public void AddSymbolMapping( string dllName, Stream symbolStream )
        {
            this.SymbolMapping.Add( dllName, symbolStream );
        }

        public ISymbolReader GetSymbolReader( ModuleDefinition module, string fileName )
        {
            if ( this.SymbolMapping.ContainsKey( module.Name ) )
                return new NativePdbReaderProvider().GetSymbolReader( module, this.SymbolMapping[ module.Name ] );
            
            return this.BaseProvider.GetSymbolReader( module, fileName );
        }

        public ISymbolReader GetSymbolReader( ModuleDefinition module, Stream symbolStream )
        {
            if ( this.SymbolMapping.ContainsKey( module.Name ) )
                return new PortablePdbReaderProvider().GetSymbolReader( module, this.SymbolMapping[ module.Name ] );

            return this.BaseProvider.GetSymbolReader( module, symbolStream );
        }
    }
}
