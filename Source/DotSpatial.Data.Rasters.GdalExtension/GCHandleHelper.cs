using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Data.Rasters.GdalExtension
{
    public static class GCHandleHelper
    {
        public static IntPtr GetIntPtr(object obj)
        {
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            IntPtr bufferPtr = handle.AddrOfPinnedObject();
            if (handle.IsAllocated) { handle.Free(); }
            return bufferPtr;
        }
    }
}
