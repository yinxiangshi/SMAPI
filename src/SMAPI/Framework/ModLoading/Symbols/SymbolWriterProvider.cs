using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace StardewModdingAPI.Framework.ModLoading.Symbols
{
    internal class SymbolWriterProvider : ISymbolWriterProvider
    {
        private readonly ISymbolWriterProvider BaseProvider = new DefaultSymbolWriterProvider();

        public ISymbolWriter GetSymbolWriter( ModuleDefinition module, string fileName )
        {
            return this.BaseProvider.GetSymbolWriter( module, fileName );
        }

        public ISymbolWriter GetSymbolWriter( ModuleDefinition module, Stream symbolStream )
        {
            // Not implemented in default native pdb writer, so fallback to portable
            return new PortablePdbWriterProvider().GetSymbolWriter( module, symbolStream );
        }
    }
}
