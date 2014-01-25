using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetworkClipboard
{
    public partial class Form1 : Form
    {
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        private bool allowShow = false;
        private NotifyIcon icon;
        public Form1()
        {
            InitializeComponent();
            icon = new NotifyIcon();
            icon.Icon = Properties.Resources.ClipboardIcon;
            icon.Visible = true;
            icon.Text = "Network Clipboard";
            icon.Click += delegate
            {
                allowShow = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };
            Icon = Properties.Resources.ClipboardIcon;
            Hide();
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowShow ? value : false);
        }

        protected override void OnClosed(EventArgs e)
        {
            icon.Dispose();
            Program.state.running = false;
            Program.evt.Set();
            base.OnClosed(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CLIPBOARDUPDATE:
                    clipboardData.Text = "";
                    break;
            }
            base.WndProc(ref m);
        }
        public void SetText(String text)
        {
            clipboardData.Text = text;
            clipboardData.Select(0, 0);
            NotifyUpdate(text);
        }
        public void NotifyUpdate(String text)
        {
            icon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            icon.BalloonTipTitle = "Clipboard Updated";
            icon.BalloonTipText = text;
            icon.ShowBalloonTip(1000);
        }


    }
}
