using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Overlay {

    [Serializable]
    public delegate void DisconnectedEvent();


    [Serializable]
    public class OverlayInterface : MarshalByRefObject {

        public int ProcessId { get; set; }

        public event DisconnectedEvent Disconnected;

        public void Ping() {

        }

        public void Disconnect() {
            SafeInvokeDisconnected();
        }


        public void SafeInvokeDisconnected() {
            if (Disconnected == null)
                return;

            DisconnectedEvent listener = null;
            Delegate[] delegates = Disconnected.GetInvocationList();

            foreach (Delegate del in delegates) {
                try {
                    listener = (DisconnectedEvent)del;
                    listener.Invoke();
                } catch {
                    Disconnected -= listener;
                }
            }
        }
    }


    public class ClientOverlayInterfaceProxy : MarshalByRefObject {

        public event DisconnectedEvent Disconnected;

        public override object InitializeLifetimeService() {
            return null;
        }

        public void DisconnectedProxyHandler() {
            if (Disconnected != null) 
                Disconnected();
        }
    }
}
