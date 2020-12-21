using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using PreventLockscreen.Properties;

namespace PreventLockscreen
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new cApplicationContext());
        }
    }

    public class cApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        KeyAkt vKeyAkt = new KeyAkt(5);

        RegistryKey RegEdit = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public cApplicationContext()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Resources.LockIcon,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Aktivieren", Enable),
                new MenuItem("Deaktivieren", Disable),
                new MenuItem("Mit Windows Starten", Startup),
                new MenuItem("Schlißen", Exit)
            }),
                Visible = true
            };

            trayIcon.ContextMenu.MenuItems[0].Visible = false;
            StartupCheck();
            vKeyAkt.RunWorkerAsync();
        }

        private void Enable(object sender, EventArgs e)
        {
            trayIcon.ContextMenu.MenuItems[0].Visible = false;
            trayIcon.ContextMenu.MenuItems[1].Visible = true;

            if (!vKeyAkt.IsBusy)
            {
                vKeyAkt.RunWorkerAsync();
            }
        }

        private void Disable(object sender, EventArgs e)
        {
            trayIcon.ContextMenu.MenuItems[0].Visible = true;
            trayIcon.ContextMenu.MenuItems[1].Visible = false;

            vKeyAkt.CancelAsync();
        }

        private void Startup(object sender, EventArgs e)
        {
            if (StartupCheck())
            {
                RegEdit.DeleteValue("PreventLockscreen", false);
                trayIcon.ContextMenu.MenuItems[2].Checked = false;
            }
            else
            {
                RegEdit.SetValue("PreventLockscreen", Application.ExecutablePath);
                trayIcon.ContextMenu.MenuItems[2].Checked = true;
            }
        }
        private bool StartupCheck()
        {
            if (RegEdit.GetValue("PreventLockscreen") == null)
            {
                trayIcon.ContextMenu.MenuItems[2].Checked = false;
                return false;
            }
            else
            {
                trayIcon.ContextMenu.MenuItems[2].Checked = true;
                return true;
            }
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }
    }

    class KeyAkt : BackgroundWorker
    {
        private int vAktMin = 5;

        private DateTime TimerStart;

        public int GetTimer()
        {
            return DateTime.Now.Second - TimerStart.Second;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            WorkerSupportsCancellation = true;

            Console.WriteLine("Start");
            while (true)
            {
                TimerStart = DateTime.Now;
                Thread.Sleep(vAktMin * 60000);
                if (CancellationPending)
                {
                    Console.WriteLine("Stop");
                    return;
                }

                Console.WriteLine("KeySend");
                SendKeys.SendWait("{NUMLOCK}");
                SendKeys.SendWait("{NUMLOCK}");
            }
        }

        public KeyAkt(int pAktMin) : base()
        {
            vAktMin = pAktMin;
        }
    }
}
