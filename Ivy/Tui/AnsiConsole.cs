using System.Runtime.InteropServices;

namespace Ivy.Tui
{
    public static class AnsiConsole
    {
        private static bool _ansiEnabled = false;
        private static readonly object _lock = new object();

        public static void Write(AnsiTable ansiTable)
        {
            if (ansiTable == null)
                throw new ArgumentNullException(nameof(ansiTable));

            EnsureAnsiEnabled();

            var output = ansiTable.Render();
            Console.Write(output);
        }

        private static void EnsureAnsiEnabled()
        {
            if (!_ansiEnabled)
            {
                lock (_lock)
                {
                    if (!_ansiEnabled)
                    {
                        EnableAnsiSupport();
                        _ansiEnabled = true;
                    }
                }
            }
        }

        private static void EnableAnsiSupport()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                EnableWindowsAnsiSupport();
            }
        }

        private static void EnableWindowsAnsiSupport()
        {
            try
            {
                var handle = GetStdHandle(-11);
                if (handle != IntPtr.Zero && handle != new IntPtr(-1))
                {
                    if (GetConsoleMode(handle, out uint mode))
                    {
                        mode |= 0x0004;
                        SetConsoleMode(handle, mode);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
    }
}