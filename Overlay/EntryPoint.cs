using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using Overlay.Hooking;

using EasyHook;

namespace Overlay {
    public class EntryPoint : IEntryPoint {

        private IDirectX _hook;
        private OverlayInterface _interface = null;
        private IpcServerChannel _serverChannel = null;
        private ClientOverlayInterfaceProxy _proxy = new ClientOverlayInterfaceProxy();

        private ManualResetEvent _runWait;
        private Task _checkConnectionTask;
        private long _stopCheck = 0;


        public EntryPoint(RemoteHooking.IContext InContext, string InChannelName) {
            _interface = RemoteHooking.IpcConnectClient<OverlayInterface>(InChannelName);
            _interface.Ping();

            IDictionary properties = new Hashtable();
            properties["name"] = InChannelName;
            properties["portName"] = InChannelName + Guid.NewGuid().ToString("N");

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            _serverChannel = new IpcServerChannel(properties, provider);
            ChannelServices.RegisterChannel(_serverChannel, false);
        }


        public void Run(RemoteHooking.IContext InContext, string InChannelName) {
            AppDomain domain = AppDomain.CurrentDomain;
            domain.AssemblyResolve += (sender, args) => {
                return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
            };

            Log.Write("Injected into process");


            _runWait = new ManualResetEvent(false);
            _runWait.Reset();
            try {
                if (!InstallHooks())
                    return;

                _interface.Disconnected += _proxy.DisconnectedProxyHandler;
                _proxy.Disconnected += () => {
                    _runWait.Set();
                };

                CheckConnection();
                _runWait.WaitOne();
                StopCheckingConnection();
                UninstallHooks();
            } catch {
                
            } finally {
                ChannelServices.UnregisterChannel(_serverChannel);
                Thread.Sleep(1000);
            }

        }


        private bool InstallHooks() {
            try {
                IntPtr d3d11 = IntPtr.Zero;

                int delay = 100;
                int retry = 0;

                while (d3d11 == IntPtr.Zero) {
                    retry++;
                    d3d11 = NativeMethods.GetModuleHandle("d3d11.dll");
                    if (retry * delay > 5000) {
                        return false;
                    }
                }

                _hook = new DirectX11();
                _hook.Install(RemoteHooking.GetCurrentProcessId());

                Log.Write("Installed DirectX hook");
                return true;
            } catch(Exception ex) {
                Log.Write("Failed to install DirectX hooks: {0}", ex.Message);
                return false; 
            }
        }

        private void UninstallHooks() {
            if (_hook != null) {
                _hook.Dispose();
                _hook = null;
            }
        }

        private void CheckConnection() {
            _checkConnectionTask = new Task(() => {
                try {
                    while (Interlocked.Read(ref _stopCheck) == 0) {
                        Thread.Sleep(1000);
                        _interface.Ping();
                    }
                } catch {
                    _runWait.Set();
                }
            });

            _checkConnectionTask.Start();
        }

        private void StopCheckingConnection() {
            Interlocked.Increment(ref _stopCheck);
        }

    }
}
