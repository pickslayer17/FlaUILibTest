using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class ProcessKillJob
{
    private static IntPtr _job;

    public static void AttachCurrentProcessToKillJob()
    {
        _job = CreateJobObject(IntPtr.Zero, null);

        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = { LimitFlags = KILL_ON_JOB_CLOSE }
        };
        int len = Marshal.SizeOf(info);
        IntPtr p = Marshal.AllocHGlobal(len);
        try
        {
            Marshal.StructureToPtr(info, p, false);
            SetInformationJobObject(_job, ExtendedLimitInfoClass, p, (uint)len);
        }
        finally
        {
            Marshal.FreeHGlobal(p);
        }

        IntPtr self = OpenProcess(PROCESS_SET_QUOTA | PROCESS_TERMINATE, false, Process.GetCurrentProcess().Id);
        try
        {
            AssignProcessToJobObject(_job, self);
        }
        finally
        {
            CloseHandle(self);
        }
    }

    const uint KILL_ON_JOB_CLOSE = 0x2000;
    const int ExtendedLimitInfoClass = 9;
    const uint PROCESS_SET_QUOTA = 0x0100;
    const uint PROCESS_TERMINATE = 0x0001;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr CreateJobObject(IntPtr a, string n);
    [DllImport("kernel32.dll")]
    static extern bool SetInformationJobObject(IntPtr j, int c, IntPtr i, uint l);
    [DllImport("kernel32.dll")]
    static extern bool AssignProcessToJobObject(IntPtr j, IntPtr p);
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(uint a, bool inh, int pid);
    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr h);

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
        public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }
}