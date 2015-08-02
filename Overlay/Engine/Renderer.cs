using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D11;

namespace Overlay.Engine {
    public class Renderer : DisposeBase {

        
        private bool _initialized = false;
        private bool _initializing = false;

        private Device _device;
        private DeviceContext _deviceContext;
        private Texture2D _renderTarget;
        private RenderTargetView _renderTargetView;
        private DXSpriteRenderer _spriteEngine;
        private Dictionary<string, DXFont> _fontCache = new Dictionary<string, DXFont>();

        public bool DeferredContext {
            get { return _deviceContext.TypeInfo == DeviceContextType.Deferred; }
        }

        public Renderer() {

        }

        public bool Init(SharpDX.DXGI.SwapChain swapChain) {
            Debug.Assert(swapChain != null);

            return Init(swapChain.GetDevice<Device>(), swapChain.GetBackBuffer<Texture2D>(0));
        }

        public bool Init(Device device, Texture2D renderTarget) {
            Debug.Assert(!_initializing);

            if (_initializing)
                return false;

            _initializing = true;
            try {


                _device = device;
                _renderTarget = renderTarget;
                try {
                    // TODO: determine if any benefit to using deferred context here
                    _deviceContext = new DeviceContext(_device);
                } catch (SharpDXException) {
                    _deviceContext = _device.ImmediateContext;
                }

                _renderTargetView = new RenderTargetView(_device, _renderTarget);

                if (DeferredContext) {
                    _deviceContext.Rasterizer.SetViewports(new ViewportF(0, 0, _renderTarget.Description.Width, _renderTarget.Description.Height, 0, 1));
                    _deviceContext.OutputMerger.SetTargets(_renderTargetView);
                }

                _spriteEngine = new DXSpriteRenderer(_device, _deviceContext);
                if (!_spriteEngine.Initialize())
                    return false;


                // Initialise any resources required for overlay elements
                // IntialiseElementResources();

                _initialized = true;
                return true;
            } catch (Exception ex) {
                Log.Write("Failed to initialize Renderer: {0}", ex.Message);
                return false;
            } finally {
                _initializing = false;
            }

        }


        private void BeginScene() {
            if (!DeferredContext) {
                _deviceContext.Rasterizer.SetViewports(new ViewportF(0, 0, _renderTarget.Description.Width, _renderTarget.Description.Height, 0, 1));
                _deviceContext.OutputMerger.SetTargets(_renderTargetView);
            }
        }

        private void EndScene() {
            if (DeferredContext) {
                var commands = _deviceContext.FinishCommandList(true);
                _device.ImmediateContext.ExecuteCommandList(commands, true);
                commands.Dispose();
            }
        }


        public void Render() {
            Debug.Assert(_initialized);

            BeginScene();

            var baseFont = new System.Drawing.Font("Arial", 14.0f);
            var font = GetFont(baseFont);
            var color = System.Drawing.Color.LimeGreen;

            _spriteEngine.DrawString(10, 10, "Hello, World", color.R, color.G, color.B, color.A, font);

            EndScene();
        }


        private DXFont GetFont(System.Drawing.Font font) {
            DXFont result = null;

            string fontKey = string.Format("{0}{1}{2}", font.Name, font.Size, font.Style);
            if (!_fontCache.TryGetValue(fontKey, out result)) {
                result = new DXFont(_device, _deviceContext);
                result.Initialize(font.Name, font.Size, font.Style, true);
                _fontCache[fontKey] = result;
            }

            return result;
        }

        protected override void Dispose(bool disposing) {
            if (true) {
                SafeDispose(_device);
                _device = null;
            }
        }

        private void SafeDispose(DisposeBase disposableObj) {
            if (disposableObj != null)
                disposableObj.Dispose();
        }

    }
}
