using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;

using Overlay;
using EasyHook;

namespace Overlay.Host {
    class Program {

        static string _channelName;
        static IpcServerChannel _server;
        static OverlayInterface _interface;
        static Process _injectedProcess;

        static void Main(string[] args) {
            Process process = LoadProcess("ffxiv_dx11");

            _interface = new OverlayInterface();
            _interface.ProcessId = process.Id;

            _server = RemoteHooking.IpcCreateServer<OverlayInterface>(ref _channelName, WellKnownObjectMode.Singleton, _interface);

            try {
                RemoteHooking.Inject(
                    process.Id,
                    InjectionOptions.Default,
                    typeof(OverlayInterface).Assembly.Location,
                    typeof(OverlayInterface).Assembly.Location,
                    _channelName
                );

                _injectedProcess = process;
            } catch (Exception ex) {
                Console.WriteLine("Injection Failed: {0}", ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.Read();

            _interface.Disconnect();

        }

        static Process LoadProcess(string name) {
            return Process.GetProcessesByName(name).First();
        }
    }
}
