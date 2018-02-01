using SocketCommunicate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestPrintServer
{
    public partial class Form1 : Form, IServerProccessing
    {
        AsynchronousSocketListener printServer = null;
        public Form1()
        {
            InitializeComponent();
            printServer = new AsynchronousSocketListener(11560, this, "");
            printServer.Start();
        }

        public bool ActionProcessingData(string KeyHoaDon)
        {
            AppendTextbox(KeyHoaDon + "\r\n");
            return true;
        }

        private void AppendTextbox(string value)
        {
            if (this.textBox1.InvokeRequired)
            {
                this.textBox1.BeginInvoke((MethodInvoker)delegate () { this.textBox1.Text += value; });
            }
            else
            {
                this.textBox1.Text += value;
            }
        }
    }
}
