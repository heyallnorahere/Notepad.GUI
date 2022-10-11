using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace Notepad.GUI.Bindings
{
    [Flags]
    internal enum AllocationProtect : uint
    {
        PAGE_EXECUTE = 0x00000010,
        PAGE_EXECUTE_READ = 0x00000020,
        PAGE_EXECUTE_READWRITE = 0x00000040,
        PAGE_EXECUTE_WRITECOPY = 0x00000080,
        PAGE_NOACCESS = 0x00000001,
        PAGE_READONLY = 0x00000002,
        PAGE_READWRITE = 0x00000004,
        PAGE_WRITECOPY = 0x00000008,
        PAGE_GUARD = 0x00000100,
        PAGE_NOCACHE = 0x00000200,
        PAGE_WRITECOMBINE = 0x00000400
    }

    internal enum AllocationState : uint
    {
        MEM_COMMIT = 0x01000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x02000,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public AllocationProtect AllocationProtect;
        public IntPtr RegionSize;
        public AllocationState State;
        public AllocationProtect Protect;
        public uint Type;
    }

    internal enum GetWindowType : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;
    }

    [SupportedOSPlatform("windows")]
    internal static class WindowsBindings
    {
        private const string Kernel32 = "kernel32.dll";
        private const string User32 = "user32.dll";

        #region Memory functions

        [DllImport(Kernel32)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        #endregion
        #region Window functions

        [DllImport(User32)]
        public static extern IntPtr GetTopWindow(IntPtr hwnd);

        [DllImport(User32, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetClassNameW")]
        public static extern nint GetClassName(IntPtr hwnd, StringBuilder lpClassName, nint nMaxCount);

        [DllImport(User32, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FindWindowExW")]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr hwndChildAfter, string className, string? windowTitle);

        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hwnd, GetWindowType uCmd);

        [DllImport(User32, SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, nint x, nint y, nint nWidth, nint nHeight, bool bRepaint);

        [DllImport(User32, SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport(User32, EntryPoint = "PostMessageW")]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wParam, UIntPtr lParam);

        #endregion
    }
}
