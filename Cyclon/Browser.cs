using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cyclon
{
    partial class Browser : Form
    {
        Form1 form;
        int ip;
        public Browser(Form1 form, int ip)
        {
            InitializeComponent();
            this.form = form;
            Listen();
        }
        bool listen = true;
        List<Message> messages = new List<Message>();
        public async void Listen()
        {
            for(; listen;)
            {
                foreach(var msg in messages)
                {

                }
                await Task.Delay(500);
            }
        }
    }
}
