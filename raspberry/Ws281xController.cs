    using System.Runtime.InteropServices;
    using raspberry;

    public class Ws281XController
    {
        private const string GPIO_BASE_PATH = "/dev/mem"; // Path to memory-mapped I/O
        private const int GPIO_BASE_ADDRESS = 0x3F200000; // Base address for GPIO
        private const int BLOCK_SIZE = 4096; // Memory block size

        private IntPtr _gpioMemory; // Pointer to mapped GPIO memory
        private readonly int _ledCount;
        private readonly int _pin;
        public int Brightness { get; set; }

        public Ws281XController(int pin, int ledCount)
        {
            _pin = pin;
            _ledCount = ledCount;
            Brightness = 255;
        }

        public bool Initialize()
        {
            int fileDescriptor = open(GPIO_BASE_PATH, 2); // Open /dev/mem
            if (fileDescriptor < 0)
            {
                Console.WriteLine("Unable to open /dev/mem. Are you running as root?");
                return false;
            }

            _gpioMemory = mmap(IntPtr.Zero, BLOCK_SIZE, 3 /* PROT_READ | PROT_WRITE */, 1 /* MAP_SHARED */, fileDescriptor, GPIO_BASE_ADDRESS);
            if (_gpioMemory == IntPtr.Zero || _gpioMemory == new IntPtr(-1))
            {
                Console.WriteLine("Memory mapping failed.");
                close(fileDescriptor);
                return false;
            }

            close(fileDescriptor);
            SetupPinAsOutput();
            return true;
        }

        public void SetColor(uint color)
        {
            for (int i = 0; i < _ledCount; i++)
            {
                SendBitPattern(color);
            }
            Show();
        }

        public void SetPixelColor(int id, int r, int g, int b)
        {
            if (id < 0 || id >= _ledCount) return;

            uint color = (uint)((r << 16) | (g << 8) | b);
            SendBitPattern(color);
            Show();
        }

        public void SetPixelColor(int id, RgbColor color)
        {
            SetPixelColor(id, color.R, color.G, color.B);
        }

        public void SetBrightness(int brightness)
        {
            Brightness = Math.Clamp(brightness, 0, 255);
            uint adjustedColor = (uint)(Brightness << 16 | Brightness << 8 | Brightness);
            SetColor(adjustedColor);
        }

        public void Shutdown()
        {
            if (_gpioMemory != IntPtr.Zero && _gpioMemory != new IntPtr(-1))
            {
                munmap(_gpioMemory, BLOCK_SIZE);
            }
        }

        private void SetupPinAsOutput()
        {
            int pinRegister = _pin / 10;
            int pinBit = (_pin % 10) * 3;
            IntPtr gpioRegister = _gpioMemory + pinRegister * 4;

            uint currentValue = (uint)Marshal.ReadInt32(gpioRegister);
            currentValue &= ~(7u << pinBit); // Clear the bits for the pin
            currentValue |= (1u << pinBit); // Set the pin to output
            Marshal.WriteInt32(gpioRegister, (int)currentValue);
        }

        private void SendBitPattern(uint color)
        {
            for (int i = 23; i >= 0; i--)
            {
                if ((color & (1 << i)) != 0)
                {
                    SendHighBit();
                }
                else
                {
                    SendLowBit();
                }
            }
        }

        private void SendHighBit()
        {
            WriteGpio(1);
            WaitNanoSeconds(900); // High time
            WriteGpio(0);
            WaitNanoSeconds(350); // Low time
        }

        private void SendLowBit()
        {
            WriteGpio(1);
            WaitNanoSeconds(350); // High time
            WriteGpio(0);
            WaitNanoSeconds(900); // Low time
        }

        private void WriteGpio(int value)
        {
            int setOrClearRegister = value == 1 ? 7 : 10; // GPIO Set or Clear
            IntPtr gpioRegister = _gpioMemory + setOrClearRegister * 4;

            Marshal.WriteInt32(gpioRegister, 1 << _pin);
        }

        private void WaitNanoSeconds(int nanoseconds)
        {
            var start = DateTime.UtcNow.Ticks;
            long ticksToWait = nanoseconds * 10 / 1000;

            while (DateTime.UtcNow.Ticks - start < ticksToWait)
            {
                // Busy-wait loop for timing
            }
        }

        public void Show()
        {
            // Placeholder method for rendering updates to LEDs.
            Console.WriteLine("LEDs updated.");
        }

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern IntPtr mmap(IntPtr addr, int length, int prot, int flags, int fd, int offset);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern int munmap(IntPtr addr, int length);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern int open(string pathname, int flags);

        [DllImport("libc.so.6", SetLastError = true)]
        private static extern int close(int fd);
    }
