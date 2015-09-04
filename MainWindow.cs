using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace DynDNSUpdater
{
    public partial class MainWindow : Form
    {
        private BackgroundUpdater updater;
        private bool errorNotified;

        public MainWindow()
        {
            InitializeComponent();

            errorNotified = false;

            updater = new BackgroundUpdater();
            trayIcon.Icon = Properties.Resources.NormalIcon;
            trayIcon.ContextMenu = new ContextMenu();
            trayIcon.ContextMenu.MenuItems.Add(0, new MenuItem("Restaurer", new EventHandler(MenuRestore)));
            trayIcon.ContextMenu.MenuItems.Add(1, new MenuItem("Quitter", new EventHandler(MenuExit)));

            serviceUrl.Text = Properties.Settings.Default.ServiceURL;
            domain.Text = Properties.Settings.Default.Domain;
            username.Text = Properties.Settings.Default.Username;
            password.Text = FromBase64(Properties.Settings.Default.Password);
            updateInterval.Value = Properties.Settings.Default.UpdateInterval;

            updater.ServiceUrl = Properties.Settings.Default.ServiceURL;
            updater.Domain = Properties.Settings.Default.Domain;
            updater.Username = Properties.Settings.Default.Username;
            updater.Password = FromBase64(Properties.Settings.Default.Password);
            updater.UpdateInterval = (int)Properties.Settings.Default.UpdateInterval;

            updater.ErrorCallback = new EventHandler(this.ErrorCallback);
            updater.SuccessCallback = new EventHandler(this.SuccessCallback);
        }

        private void ErrorCallback(object sender, EventArgs e)
        {
            if (!errorNotified)
            {
                trayIcon.Icon = Properties.Resources.RedIcon;
                trayIcon.ShowBalloonTip(500, "Impossible d'effectuer la mise à jour", updater.LastErrorMessage, ToolTipIcon.Error);
                errorNotified = true;
            }
        }

        private void SuccessCallback(object sender, EventArgs e)
        {
            errorNotified = false;
            trayIcon.Icon = Properties.Resources.GreenIcon;
        }

        private void MenuRestore(object sender, EventArgs e)
        {
            Show();
            Focus();
        }

        private void MenuExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void validateButton_Click(object sender, EventArgs e)
        {
            updater.ServiceUrl = serviceUrl.Text;
            updater.Domain = domain.Text;
            updater.Username = username.Text;
            updater.Password = password.Text;

            Properties.Settings.Default.ServiceURL = serviceUrl.Text;
            Properties.Settings.Default.Domain = domain.Text;
            Properties.Settings.Default.Username = username.Text;
            Properties.Settings.Default.Password = ToBase64(password.Text);
            Properties.Settings.Default.UpdateInterval = updateInterval.Value;
            Properties.Settings.Default.Save();

            if (!updater.IsRunning())
            {
                updater.Start();
            }

            errorNotified = false;
            Hide();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void updateInterval_ValueChanged(object sender, EventArgs e)
        {
            updater.UpdateInterval = (int)updateInterval.Value;
            Properties.Settings.Default.UpdateInterval = updateInterval.Value;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(domain.Text))
            {
                Hide();
                updater.Start();
            }
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
        }

        private string FromBase64(string input)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(input));
        }

        private string ToBase64(string input)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(input));
        }
    }
}
