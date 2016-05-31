using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace JerryMouse
{
   public class MemoryManageWrapper
    {

        internal const string libname = "dll\\msvcrt.dll";
        //use memcpy in cSharp
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport(libname, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern 
            IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
    }
}
