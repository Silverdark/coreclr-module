using System;
using System.Runtime.Loader;

namespace AltV.Net
{
    // TODO: Rename, maybe remove
    public class WrapperContext
    {
        public IntPtr ServerPointer { get; set; }
        public IntPtr ResourcePointer { get; set; }
        public AssemblyLoadContext AssemblyLoadContext { get; set; }
    }
}