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
            Console.WriteLine(KeyHoaDon);
            return true;
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBox1.Text += value;
        }
    }
}
