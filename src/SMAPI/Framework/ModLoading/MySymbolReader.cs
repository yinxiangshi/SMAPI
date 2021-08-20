using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

namespace StardewModdingAPI.Framework.ModLoading
{
    internal class MySymbolReader : ISymbolReader
    {
        private ModuleDefinition Module;
        private Stream Stream;
        private ISymbolReader Using;

        public MySymbolReader( ModuleDefinition module, Stream stream )
        {
            this.Module = module;
            this.Stream = stream;
            this.Using = new NativePdbReaderProvider().GetSymbolReader( module, stream );
        }
        
        public void Dispose()
        {
            this.Using.Dispose();
        }

        public ISymbolWriterProvider GetWriterProvider()
        {
            return new PortablePdbWriterProvider();
        }

        public bool ProcessDebugHeader( ImageDebugHeader header )
        {
            try
            {
                return this.Using.ProcessDebugHeader( header );
            }
            catch (Exception e)
            {
                this.Using.Dispose();
                this.Using = new PortablePdbReaderProvider().GetSymbolReader( this.Module, this.Stream );
                return this.Using.ProcessDebugHeader( header );
            }
        }

        public MethodDebugInformation Read( MethodDefinition method )
        {
            return Using.Read( method );
        }
    }
}
