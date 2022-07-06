using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoopTest
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        public void WriteLog(string text)
        {
            if(this.richTextBox1.InvokeRequired)
            {
                richTextBox1.Invoke((Action)(() =>
                {
                    richTextBox1.AppendText(text);
                    richTextBox1.AppendText(Environment.NewLine);   
                }));
            }
            else
            {
                richTextBox1.AppendText(text);
                richTextBox1.AppendText(Environment.NewLine);
            }
        }
    }
}
