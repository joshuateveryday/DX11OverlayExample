using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SharpDX.DXGI;

namespace Overlay.Hooking {

    internal static class DXGI {
        public enum DXGISwapChainVTbl : short {
            // IUnknown
            QueryInterface = 0,
            AddRef = 1,
            Release = 2,

            // IDXGIObject
            SetPrivateData = 3,
            SetPrivateDataInterface = 4,
            GetPrivateData = 5,
            GetParent = 6,

            // IDXGIDeviceSubObject
            GetDevice = 7,

            // IDXGISwapChain
            Present = 8,
            GetBuffer = 9,
            SetFullscreenState = 10,
            GetFullscreenState = 11,
            GetDesc = 12,
            ResizeBuffers = 13,
            ResizeTarget = 14,
            GetContainingOutput = 15,
            GetFrameStatistics = 16,
            GetLastPresentCount = 17,
        }


        public const int DXGI_SWAPCHAIN_METHOD_COUNT = 18;

        public static SwapChainDescription CreateSwapChainDescription(IntPtr windowHandle) {
            return new SwapChainDescription {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = windowHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
        }
    }
    
}
