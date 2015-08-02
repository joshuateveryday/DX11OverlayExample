using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using EasyHook;

namespace Overlay.Hooking {
    public abstract class DXBase : IDirectX {
        public int ProcessId { get; set; }
        

        private List<LocalHook> _activeHooks = new List<LocalHook>();
        protected List<LocalHook> ActiveHooks {
            get { return _activeHooks; }
        }


        public DXBase() {

        }

        ~DXBase() {
            Dispose(false);
        }


        public virtual void Install(int processId) {
            ProcessId = processId;
        }
        public abstract void Uninstall();


        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                try {
                    foreach (LocalHook hook in ActiveHooks) {
                        hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
                    }

                    Thread.Sleep(100);

                    foreach (LocalHook hook in ActiveHooks) {
                        hook.Dispose();
                    }

                    ActiveHooks.Clear();
                } catch { }
            }
        }

        public void Dispose() {
            Dispose(true);
        }

       
    }
}
