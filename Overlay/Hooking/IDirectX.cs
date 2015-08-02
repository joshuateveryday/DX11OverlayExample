using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using EasyHook;

namespace Overlay.Hooking {

    public static class VTable {

        public static IntPtr[] LookupAddresses(IntPtr start, int count) {
            return LookupAddresses(start, 0, count);
        }

        public static IntPtr[] LookupAddresses(IntPtr start, int index, int count) {
            List<IntPtr> results = new List<IntPtr>();

            IntPtr tablePtr = Marshal.ReadIntPtr(start);

            for (int i = index; i < (index + count); i++) {
                results.Add(Marshal.ReadIntPtr(tablePtr, i * IntPtr.Size));
            }

            return results.ToArray();
        }

    }

    interface IDirectX : IDisposable {
        int ProcessId { get; set; }

        void Install(int processId);
        void Uninstall();
    }


}
