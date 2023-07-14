using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll")]
    static extern bool Thread32First(IntPtr hSnapshot, ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll")]
    static extern bool Thread32Next(IntPtr hSnapshot, ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll")]
    static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    static extern int ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    static extern int CloseHandle(IntPtr hObject);

    [Flags]
    enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VirtualMemoryOperation = 0x00000008,
        VirtualMemoryRead = 0x00000010,
        VirtualMemoryWrite = 0x00000020,
        DuplicateHandle = 0x00000040,
        CreateProcess = 0x000000080,
        SetQuota = 0x00000100,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        SuspendResume = 0x00000800,
        QueryLimitedInformation = 0x00001000,
        Synchronize = 0x00100000
    }

    [Flags]
    enum ThreadAccess : int
    {
        SUSPEND_RESUME = 0x0002
    }

    const uint TH32CS_SNAPTHREAD = 0x00000004;

    [StructLayout(LayoutKind.Sequential)]
    struct THREADENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ThreadID;
        public uint th32OwnerProcessID;
        public uint tpBasePri;
        public uint tpDeltaPri;
        public uint dwFlags;
    }

    static void SuspendProcess(int processId)
    {
        IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processId);
        if (hProcess != IntPtr.Zero)
        {
            IntPtr hThreadSnap = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
            if (hThreadSnap != IntPtr.Zero)
            {
                THREADENTRY32 threadEntry = new THREADENTRY32();
                threadEntry.dwSize = (uint)Marshal.SizeOf(typeof(THREADENTRY32));

                if (Thread32First(hThreadSnap, ref threadEntry))
                {
                    do
                    {
                        if (threadEntry.th32OwnerProcessID == (uint)processId)
                        {
                            IntPtr hThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, threadEntry.th32ThreadID);
                            if (hThread != IntPtr.Zero)
                            {
                                SuspendThread(hThread);
                                CloseHandle(hThread);
                            }
                        }
                    } while (Thread32Next(hThreadSnap, ref threadEntry));
                }
                CloseHandle(hThreadSnap);
            }
            CloseHandle(hProcess);
        }
    }

    static void ResumeProcess(int processId)
    {
        IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, processId);
        if (hProcess != IntPtr.Zero)
        {
            IntPtr hThreadSnap = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);
            if (hThreadSnap != IntPtr.Zero)
            {
                THREADENTRY32 threadEntry = new THREADENTRY32();
                threadEntry.dwSize = (uint)Marshal.SizeOf(typeof(THREADENTRY32));

                if (Thread32First(hThreadSnap, ref threadEntry))
                {
                    do
                    {
                        if (threadEntry.th32OwnerProcessID == (uint)processId)
                        {
                            IntPtr hThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, threadEntry.th32ThreadID);
                            if (hThread != IntPtr.Zero)
                            {
                                ResumeThread(hThread);
                                CloseHandle(hThread);
                            }
                        }
                    } while (Thread32Next(hThreadSnap, ref threadEntry));
                }
                CloseHandle(hThreadSnap);
            }
            CloseHandle(hProcess);
        }
    }

    static void Main()
    {
        Console.WriteLine("Ingrese el ID del proceso a suspender:");
        int processId = int.Parse(Console.ReadLine());

        bool continuar = true;

        while (continuar)
        {
            Console.WriteLine("Seleccione una opción:");
            Console.WriteLine("1. Suspender el proceso");
            Console.WriteLine("2. Reanudar el proceso");
            Console.WriteLine("0. Salir");
            Console.Write("Opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    SuspendProcess(processId);
                    Console.WriteLine("Proceso suspendido exitosamente.");
                    break;
                case "2":
                    ResumeProcess(processId);
                    Console.WriteLine("Proceso reanudado exitosamente.");
                    break;
                case "0":
                    continuar = false;
                    break;
                default:
                    Console.WriteLine("Opción inválida. Intente nuevamente.");
                    break;
            }

            Console.WriteLine();
        }

        Console.WriteLine("Presione cualquier tecla para salir.");
        Console.ReadKey();
    }
}
