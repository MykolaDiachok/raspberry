    using System.Runtime.InteropServices;
    using raspberry;
    namespace raspberry;
  public class Ws281XController : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ws2811_channel_t
        {
            public int gpionum;        // GPIO pin number
            public int invert;         // Invert signal
            public int count;          // Number of LEDs
            public int brightness;     // Brightness (0-255)
            public int strip_type;     // LED strip type (e.g., WS2812_STRIP)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ws2811_t
        {
            public uint freq;          // Frequency in Hz
            public int dmanum;         // DMA channel
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public ws2811_channel_t[] channels; // Channels for LEDs
        }

        private ws2811_t _controller;

        [DllImport("libws2811", EntryPoint = "ws2811_init")]
        private static extern int ws2811_init(ref ws2811_t ws2811);

        [DllImport("libws2811", EntryPoint = "ws2811_render")]
        private static extern int ws2811_render(ref ws2811_t ws2811);

        [DllImport("libws2811", EntryPoint = "ws2811_fini")]
        private static extern void ws2811_fini(ref ws2811_t ws2811);

        [DllImport("libws2811", EntryPoint = "ws2811_get_return_t_str")]
        private static extern IntPtr ws2811_get_return_t_str(int state);

        public int Brightness
        {
            get => _controller.channels[0].brightness;
            set
            {
                if (value < 0 || value > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Brightness must be between 0 and 255.");
                }
                _controller.channels[0].brightness = value;
            }
        }
        
        public Ws281XController(int gpioPin, int ledCount)
        {
            _controller = new ws2811_t
            {
                freq = 800000,
                dmanum = 10,
                channels = new[]
                {
                    new ws2811_channel_t
                    {
                        gpionum = gpioPin,
                        invert = 0,
                        count = ledCount,
                        brightness = 255,
                        strip_type = 0x180810 // WS2812_STRIP
                    },
                    new ws2811_channel_t()
                }
            };

            var initResult = ws2811_init(ref _controller);
            if (initResult != 0)
            {
                var errorMsg = Marshal.PtrToStringAnsi(ws2811_get_return_t_str(initResult));
                throw new InvalidOperationException($"Failed to initialize WS281x: {errorMsg}");
            }
        }

        public void SetPixelColor(int index, RgbColor color)
        {
            if (index < 0 || index >= _controller.channels[0].count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid LED index.");
            }

            uint colorValue = color.ToUint();
            IntPtr ledsPtr = _controller.channels[0].gpionum;

            Marshal.WriteInt32(ledsPtr, index * sizeof(uint), (int)colorValue);
        }
        
        public void SetPixelColor(int index, uint color)
        {
            if (index < 0 || index >= _controller.channels[0].count)
                throw new ArgumentOutOfRangeException(nameof(index));

            unsafe
            {
                uint* leds = (uint*)_controller.channels[0].gpionum;
                leds[index] = color;
            }
        }

        public void Render()
        {
            var renderResult = ws2811_render(ref _controller);
            if (renderResult != 0)
            {
                var errorMsg = Marshal.PtrToStringAnsi(ws2811_get_return_t_str(renderResult));
                throw new InvalidOperationException($"Render failed: {errorMsg}");
            }
        }
        
        public void Show()
        {
            var renderResult = ws2811_render(ref _controller);
            if (renderResult != 0)
            {
                var errorMsg = Marshal.PtrToStringAnsi(ws2811_get_return_t_str(renderResult));
                throw new InvalidOperationException($"Render failed: {errorMsg}");
            }
        }

        public void Dispose()
        {
            ws2811_fini(ref _controller);
        }
    }