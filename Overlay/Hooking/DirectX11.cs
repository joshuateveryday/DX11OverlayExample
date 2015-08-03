using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Overlay.Engine;

using EasyHook;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using DX11Device = SharpDX.Direct3D11.Device;


namespace Overlay.Hooking {
    public class DirectX11 : DXBase {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DXGISwapChain_PresentDelegate(IntPtr swapChainPtr, int syncInterval, PresentFlags dwFlags);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int DXGISwapChain_ResizeTargetDelegate(IntPtr swapChainPtr, ref ModeDescription dwTargetParams);

        const int D3D11_DEVICE_METHOD_COUNT = 43;

        private IntPtr _swapChainPtr = IntPtr.Zero;
        private List<IntPtr> _d3d11Addresses;
        private List<IntPtr> _dxgiAddresses;

        
        private LocalHook SwapChain_PresentHook;
        private LocalHook SwapChain_ResizeTargetHook;

        private Renderer _overlay;

        public DirectX11()
            : base() {

        }


        public override void Install(int processId) {
            base.Install(processId);

            if (_d3d11Addresses == null) {
                _d3d11Addresses = new List<IntPtr>();
                _dxgiAddresses = new List<IntPtr>();


                DX11Device device;
                SwapChain swapChain;
                using (RenderForm form = new RenderForm()) {
                    DX11Device.CreateWithSwapChain(
                        DriverType.Hardware,
                        DeviceCreationFlags.None,
                        DXGI.CreateSwapChainDescription(form.Handle),
                        out device,
                        out swapChain
                    );

                    if (device != null && swapChain != null) {
                        using (device) {
                            _d3d11Addresses.AddRange(VTable.LookupAddresses(device.NativePointer, D3D11_DEVICE_METHOD_COUNT));

                            using (swapChain) {
                                _dxgiAddresses.AddRange(VTable.LookupAddresses(swapChain.NativePointer, DXGI.DXGI_SWAPCHAIN_METHOD_COUNT));
                            }
                        }
                    }
                }
            }

            SwapChain_PresentHook = LocalHook.Create(_dxgiAddresses[(int)DXGI.DXGISwapChainVTbl.Present], new DXGISwapChain_PresentDelegate(Present), this);
            SwapChain_ResizeTargetHook = LocalHook.Create(_dxgiAddresses[(int)DXGI.DXGISwapChainVTbl.ResizeTarget], new DXGISwapChain_ResizeTargetDelegate(ResizeTarget), this);

            SwapChain_PresentHook.ThreadACL.SetExclusiveACL(new Int32[1]);
            SwapChain_ResizeTargetHook.ThreadACL.SetExclusiveACL(new Int32[1]);

            ActiveHooks.Add(SwapChain_PresentHook);
            ActiveHooks.Add(SwapChain_ResizeTargetHook);
        }

        public override void Uninstall() {
            try {
                if (SwapChain_PresentHook != null) {
                    SwapChain_PresentHook.Dispose();
                    SwapChain_PresentHook = null;
                }

                if (SwapChain_ResizeTargetHook != null) {
                    SwapChain_ResizeTargetHook.Dispose();
                    SwapChain_ResizeTargetHook = null;
                }
            } catch { }
        }


        int Present(IntPtr swapChainPtr, int syncInterval, PresentFlags dwFlags) {
            SwapChain swapChain = (SwapChain)swapChainPtr;

            try {
                if (swapChainPtr != swapChain.NativePointer || _overlay == null) {
                    if (_overlay != null)
                        _overlay.Dispose();

                    _overlay = new Renderer();
                    _overlay.Init(swapChain);

                    _swapChainPtr = swapChain.NativePointer;
                } else if (_overlay != null) {
                    _overlay.Render();
                }
            } catch(Exception ex) {
                Log.Write("Failed present: {0}", ex.Message);
            }
            
            swapChain.Present(syncInterval, dwFlags);
            return Result.Ok.Code;
        }

        int ResizeTarget(IntPtr swapChainPtr, ref ModeDescription dwTargetParams) {
            SwapChain swapChain = (SwapChain)swapChainPtr;
            {
                
                if (_overlay != null) {
                    _overlay.Dispose();
                    _overlay = null;
                }
                swapChain.ResizeTarget(ref dwTargetParams);
                return Result.Ok.Code;
            }
        }

    }

    enum D3D11_VTABLE : short {
        // IUnknown
        QueryInterface = 0,
        AddRef = 1,
        Release = 2,

        // ID3D11Device
        CreateBuffer = 3,
        CreateTexture1D = 4,
        CreateTexture2D = 5,
        CreateTexture3D = 6,
        CreateShaderResourceView = 7,
        CreateUnorderedAccessView = 8,
        CreateRenderTargetView = 9,
        CreateDepthStencilView = 10,
        CreateInputLayout = 11,
        CreateVertexShader = 12,
        CreateGeometryShader = 13,
        CreateGeometryShaderWithStreamOutput = 14,
        CreatePixelShader = 15,
        CreateHullShader = 16,
        CreateDomainShader = 17,
        CreateComputeShader = 18,
        CreateClassLinkage = 19,
        CreateBlendState = 20,
        CreateDepthStencilState = 21,
        CreateRasterizerState = 22,
        CreateSamplerState = 23,
        CreateQuery = 24,
        CreatePredicate = 25,
        CreateCounter = 26,
        CreateDeferredContext = 27,
        OpenSharedResource = 28,
        CheckFormatSupport = 29,
        CheckMultisampleQualityLevels = 30,
        CheckCounterInfo = 31,
        CheckCounter = 32,
        CheckFeatureSupport = 33,
        GetPrivateData = 34,
        SetPrivateData = 35,
        SetPrivateDataInterface = 36,
        GetFeatureLevel = 37,
        GetCreationFlags = 38,
        GetDeviceRemovedReason = 39,
        GetImmediateContext = 40,
        SetExceptionMode = 41,
        GetExceptionMode = 42
    }
}
