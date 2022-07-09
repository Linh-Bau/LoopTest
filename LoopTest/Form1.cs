using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoopTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Thread LoopTestThread;
        RunningState tester;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("please open by adminstrator access!");
            currentForm = this;
            LoopTestThread = new Thread(loopTestThread);
            LoopTestThread.Start();
            if(Config.getKeyValue("debug").ToUpper()=="TRUE")
            {
                Form1.debugForm = new DebugForm();
                debugForm.Show();
            }
        }

        private void BTN_START_Click(object sender, EventArgs e)
        {
            int loop_test_times;
            bool tryCovert=int.TryParse(textBox1.Text, out loop_test_times);
            if(tryCovert)
            {
                looptest_info = new looptest_info(loop_test_times);
                textBox1.Text = loop_test_times.ToString();
                tester.ManualChangeState();
            }
            else
            {
                MessageBox.Show("Input a right one!");
            }
            
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            
        }


        private void BTN_STOP_Click(object sender, EventArgs e)
        { 
        }

        private void loopTestThread()
        {
            tester = new WaitStartButtonClick();
            while (true)
            {               
                tester.Run();
                tester.WaitChangeState();
                if (tester.isStateChange)
                {
                    tester.UpdateForm();//update form previous state
                    tester = tester.GetCurrentTestState;
                }
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                BTN_START_Click(sender, e);
            }
        }

        DateTime test_time = new DateTime();
        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan total_time = DateTime.Now - test_time;
            lb_Total_Time.Text = String.Format("{0}:{1}:{2}",total_time.Hours, total_time.Minutes, total_time.Seconds);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //richTextBox1.SelectionStart = richTextBox1.Text.Length;
            //// scroll it automatically
            //richTextBox1.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                LoopTestThread.Abort();
            }
            catch
            {
               
            }
        }
  
        private void button1_Click(object sender, EventArgs even)
        {
            var testProcess = findProcess(Config.getKeyValue("test_program_name"));
            ElementTree elementTree = Form1.GetAllElement(testProcess.MainWindowHandle);
            elementTree.ShowAllCaption(elementTree);
            var e= elementTree.findElementByClassContains("ErrorCode", elementTree);
            //Form1.WriteDebugLog(String.Format("{0},{1},{2}", e.GetHwnd.ToString("X8"), e.GetCaption, e.GetClassName));
        }
        private Process findProcess(string pname)
        {
            var p = Process.GetProcessesByName(pname);
            if (p == null) throw new Exception();
            return p[0];
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void BTN_STOP_Click_1(object sender, EventArgs e)
        {
            try
            {
                if(MessageBox.Show("Are you sure?","Warring",MessageBoxButtons.YesNo)==DialogResult.Yes)
                {
                    LoopTestThread.Abort();
                    System.Threading.Thread.Sleep(3000);
                }
            }
            catch
            {

            }
            finally
            {
                LoopTestThread = new Thread(loopTestThread);
                LoopTestThread.Start();
            }
        }
    }
}
