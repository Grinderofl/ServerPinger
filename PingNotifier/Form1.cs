using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PingNotifier.Properties;

namespace PingNotifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void Loaded(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            listBox1.Items.Clear();
            var items = new string[Settings.Default.Targets.Count];
            Settings.Default.Targets.CopyTo(items, 0);
            listBox1.Items.AddRange(items);
        }

        private void AddClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                Settings.Default.Targets.Add(textBox1.Text);
                Settings.Default.Save();
            }
            textBox1.Text = "";
            LoadSettings();
        }

        private void RemoveClicked(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Settings.Default.Targets.Remove(listBox1.SelectedItem.ToString());
                Settings.Default.Save();
            }
            LoadSettings();
        }

        private void MinimizeClicked(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void NotifyClicked(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Ticked(object sender, EventArgs e)
        {
            if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void Resized(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.ShowBalloonTip(500, "Minimized",
                    "Ping notifier has been minimized and will continue in the background.", ToolTipIcon.Info);
                Hide();
            }
        }

        private void Worked(object sender, DoWorkEventArgs e)
        {
            foreach (var entry in listBox1.Items)
            {
                var uri = new Uri(entry.ToString());
                var pingResult = PingHost(uri.DnsSafeHost, uri.Port);
                var progressUpdate = new Tuple<string, bool>(uri.AbsoluteUri, pingResult);
                backgroundWorker1.ReportProgress(0, progressUpdate);

                if (!pingResult)
                    notifyIcon1.ShowBalloonTip(4000, uri.Host, "Failed to ping " + uri.AbsoluteUri, ToolTipIcon.Error);
            }
            
        }

        private static bool PingHost(string hostUrl, int port)
        {
            try
            {
                new TcpClient(hostUrl, port);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ProgressReported(object sender, ProgressChangedEventArgs e)
        {
            var userState = e.UserState as Tuple<string, bool>;
            textBox2.Text = string.Format("Pinging {0} : Result = {1}" + Environment.NewLine, userState.Item1, userState.Item2) + textBox2.Text;
        }

        private void TextBoxUpdated(object sender, EventArgs e)
        {
            if (textBox2.Lines.Length > 100)
                textBox2.Lines = textBox2.Lines.Take(100).ToArray();
        }
    }
}
