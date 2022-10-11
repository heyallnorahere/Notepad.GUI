using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

// windows api bindings
using Notepad.GUI.Bindings;
using static Notepad.GUI.Bindings.WindowsBindings;

namespace Notepad.GUI.Implementations
{
    /// <summary>
    /// Implementation is based off of http://kylehalladay.com/blog/2020/05/20/Rendering-With-Notepad.html
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class WindowsClient : NotepadClient
    {
        private static IntPtr FindBytePatternInProcessMemory(Process process, byte[] pattern)
        {
            var handle = process.Handle;
            IntPtr basePtr = IntPtr.Zero;

            while (VirtualQueryEx(handle, basePtr, out MEMORY_BASIC_INFORMATION memInfo, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) != 0)
            {
                if (memInfo.State == AllocationState.MEM_COMMIT && memInfo.Protect == AllocationProtect.PAGE_READWRITE)
                {
                    var localCopyContents = new byte[(int)memInfo.RegionSize];
                    if (ReadProcessMemory(handle, memInfo.BaseAddress, localCopyContents, localCopyContents.Length, out IntPtr bytesRead))
                    {
                        int offset = Utilities.FindPattern(localCopyContents, pattern);
                        if (offset >= 0)
                        {
                            return memInfo.BaseAddress + offset;
                        }
                    }
                }

                basePtr = (IntPtr)((ulong)memInfo.BaseAddress + (ulong)memInfo.RegionSize);
            }

            return IntPtr.Zero;
        }

        private static IntPtr GetWindowForProcessAndClassName(uint pid, string className)
        {
            var currentWindow = GetTopWindow(IntPtr.Zero);
            var buffer = new StringBuilder();

            while (currentWindow != IntPtr.Zero)
            {
                GetWindowThreadProcessId(currentWindow, out uint currentPid);
                if (currentPid == pid)
                {
                    GetClassName(currentWindow, buffer, 256); // arbitrary limit
                    if (buffer.ToString().ToLower() == className.ToLower())
                    {
                        return currentWindow;
                    }

                    var childWindow = FindWindowEx(currentWindow, IntPtr.Zero, className, null);
                    if (childWindow != IntPtr.Zero)
                    {
                        return childWindow;
                    }
                }

                currentWindow = GetWindow(currentWindow, GetWindowType.GW_HWNDNEXT);
            }

            return IntPtr.Zero;
        }

        public WindowsClient()
        {
            mEncoding = Encoding.Unicode;
            mBytesPerChar = -1;
            mBufferAddress = IntPtr.Zero;
        }

        public override void Write(string data, int offset)
        {
            if (mBytesPerChar < 0 || mBufferAddress == IntPtr.Zero)
            {
                throw new Exception("The client has not been initialized yet!");
            }

            int realOffset = offset * mBytesPerChar;
            int totalByteCount = BufferSize.X * BufferSize.Y * mBytesPerChar;

            var bytes = mEncoding.GetBytes(data);
            if (realOffset < 0 || realOffset + bytes.Length > totalByteCount)
            {
                throw new IndexOutOfRangeException();
            }

            var baseAddress = mBufferAddress + realOffset;
        }

        protected override Process CreateProcess()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    UseShellExecute = true
                }
            };

            process.Start();
            return process;
        }

        protected override Process? FindProcess()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName == "notepad")
                {
                    return process;
                }
            }

            return null;
        }

        private unsafe IntPtr ReadValue(byte[] bytes)
        {
            if (bytes.Length == 1)
            {
                return (IntPtr)bytes[0];
            }
            else
            {
                fixed (byte* buffer = bytes)
                {
                    var ptr = (IntPtr)buffer;
                    return bytes.Length switch
                    {
                        2 => (IntPtr)Marshal.ReadInt16(ptr),
                        4 => (IntPtr)Marshal.ReadInt32(ptr),
                        8 => (IntPtr)Marshal.ReadInt64(ptr),
                        _ => throw new ArgumentException("Invalid integer size!")
                    };
                }
            }
        }

        protected override void Connect()
        {
            var editWindow = GetWindowForProcessAndClassName((uint)Process.Id, "Edit");
            var parentWindow = GetWindow(editWindow, GetWindowType.GW_OWNER);

            var windowInfo = new WINDOWINFO
            {
                cbSize = (uint)Marshal.SizeOf<WINDOWINFO>()
            };

            // hard-coded window size
            GetWindowInfo(parentWindow, ref windowInfo);
            MoveWindow(parentWindow, windowInfo.rcWindow.Left, windowInfo.rcWindow.Top, 1365, 768, true);

            var bufferSize = BufferSize;
            int charCount = bufferSize.X * bufferSize.Y;

            string characterString = string.Empty;
            var rng = new Random();

            Logger?.Report("Submitting random key presses...");
            for (int i = 0; i < charCount; i++)
            {
                int value = rng.Next('A', 'Z');
                characterString += (char)value;

                // https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-char
                PostMessage(editWindow, 0x0102, (IntPtr)value, UIntPtr.Zero);
            }

            // messages take a bit to process
            Logger?.Report("Waiting for messages to process...");
            Thread.Sleep(5000);

            var bytes = mEncoding.GetBytes(characterString);
            mBytesPerChar = bytes.Length / charCount;

            mBufferAddress = FindBytePatternInProcessMemory(Process, bytes);
            if (mBufferAddress == IntPtr.Zero)
            {
                throw new Exception("Could not find the text buffer in memory!");
            }
            else
            {
                Logger?.Report("Found buffer!");
            }
        }

        // static buffer size
        public override Vector2 BufferSize => (131, 30);

        private readonly Encoding mEncoding;
        private int mBytesPerChar;
        private IntPtr mBufferAddress;
    }
}
