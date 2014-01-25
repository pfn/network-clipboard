using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkClipboard
{
    class State
    {
        public volatile bool running = true;
    }
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public static ManualResetEvent evt = new ManualResetEvent(false);
        public static State state = new State();
        private static Form1 form;
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            AddClipboardFormatListener(form.Handle);

            var thread = new Thread(new ThreadStart(NetworkListener));
            thread.Start();
            Application.Run(form);
        }

        static void NetworkListener()
        {
            //form.BeginInvoke(new MethodInvoker(Show));
            //Show();
            var listener = new TcpListener(IPAddress.Parse("0.0.0.0"), 4573);
            listener.Start();
            while (state.running)
            {
                evt.Reset();
                listener.BeginAcceptSocket(new AsyncCallback(ConnectSocket), listener);
                evt.WaitOne();
            }
        }

        delegate void ShowInvoker(String text);
        static void ConnectSocket(IAsyncResult iar)
        {
            var listener = (TcpListener)iar.AsyncState;
            var socket = listener.EndAcceptSocket(iar);

            byte[] buf = new byte[32768];
            int read = -1;
            var builder = new StringBuilder();
            while (read != 0)
            {
                read = socket.Receive(buf);
                var data = Encoding.UTF8.GetChars(buf, 0, read);
                builder.Append(data);
            }
            form.BeginInvoke(new ShowInvoker(Show), builder.ToString());
            socket.Close();
            socket.Dispose();
            evt.Set();
        }
        static void Show(String text)
        {
            text = text.IndexOf("\r\n") == -1 ? text.Replace("\n", "\r\n") : text;
            RemoveClipboardFormatListener(form.Handle);
            Clipboard.SetText(text);
            form.SetText(text);
            AddClipboardFormatListener(form.Handle);
        }
    }
}
