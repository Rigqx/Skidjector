using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Injector
{
    public partial class Injector : Form
    {
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CAPTION = 0x2;

        public Injector()
        {
            InitializeComponent();

            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            siticoneRoundedComboBox1.Items.Clear();
            foreach (Process p in PC)
            {
                siticoneRoundedComboBox1.Items.Add(p.ProcessName);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private static string DLLP { get; set; }
        private void button_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = @"C:\";
                ofd.Title = "Locate DLL";
                ofd.DefaultExt = "dll";
                ofd.Filter = "DLL Files (*.dll)|*.dll";
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.ShowDialog();

                siticoneTextBox1.Text = ofd.FileName;

                DLLP = ofd.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void siticoneTextBox1_TextChanged(object sender, EventArgs e)
        {
            DLLP = siticoneTextBox1.Text;
        }
        private void siticoneRoundedButton1_Click(object sender, EventArgs e)
        {
            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            siticoneRoundedComboBox1.Items.Clear();
            foreach (Process p in PC)
            {
                siticoneRoundedComboBox1.Items.Add(p.ProcessName);
            }
        }

        /*
         * Injection Method
         */
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern int CloseHandle(IntPtr intPtr);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]

        static extern IntPtr WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        public static bool SI(uint P, string DLLP)
        {
            IntPtr hndProc = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, P);
            if (hndProc == IntPtr.Zero) { return false; }

            IntPtr lpAddress = VirtualAllocEx(hndProc, (IntPtr)null, (IntPtr)DLLP.Length, (0x1000 | 0x2000), 0x40);
            if (lpAddress == IntPtr.Zero) { return false; }

            byte[] bytes = Encoding.ASCII.GetBytes(DLLP);

            if (WriteProcessMemory(hndProc, lpAddress, bytes, (uint)bytes.Length, 0).Equals(0))
            { 
                return false; 
            }

            CloseHandle(hndProc);
            return true;
        }

        public static int Inject(string PN, string DLLP)
        {
            // 1 = File doesn't exist
            // 2 = doesn't exist
            if (!File.Exists(DLLP)) { return 1; }

            uint _procId = 0;
            Process[] _procs = Process.GetProcesses();
            for (int i = 0; i < _procs.Length; i++)
            {
                if(_procs[i].ProcessName == PN)
                {
                    _procId = (uint)_procs[i].Id;
                }
            }

            if(_procId == 0) { return 2; }
            if(!SI(_procId, DLLP))
            {
                return 3;
            }

            return 4;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int Result = Inject(siticoneRoundedComboBox1.Text, DLLP);

            if(Result == 1)
            {
                MessageBox.Show("The file doesn't exist..!");
                label3.Text = "Status: Offline!";
            }
            else if(Result == 2)
            {
                MessageBox.Show("The process doesn't exist..!");
                label3.Text = "Status: Offline!";
            }
            else if(Result == 3)
            {
                MessageBox.Show("The injection failed..!");
                label3.Text = "Status: Injection failed!";
            }
            else if(Result == 4)
            {
                MessageBox.Show("Injection succeded!");
                label3.Text = "Status: Injection succeded!";
            }
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
