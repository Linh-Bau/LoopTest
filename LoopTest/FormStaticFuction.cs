using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoopTest
{
    public partial class Form1
    {
        //##########################################
        public static Form1 currentForm;
        static looptest_info looptest_info;
        static DebugForm debugForm;

        public static bool isLoopTestFinished { get { return looptest_info.IsLoopTestFinished; } }  

        public static string GetSN()
        {
            string sn = "";
            if(currentForm.txb_SN.InvokeRequired)
            {
                currentForm.txb_SN.Invoke((Action)(() => { sn = currentForm.txb_SN.Text; }));
                return sn;
            }
            else
            {
                sn=currentForm.txb_SN.Text;
                return sn;
            }
        }
        public enum Config_Panel_Mode
        {
            LOCK,
            UNLOCK
        }

        public static void controlInvoke(Control ctr,Action action)
        {
            if (ctr.InvokeRequired)
            {
                ctr.Invoke(action);
            }
            else action();
        }

        public static void updateFailCount(List<Err> listErr)
        {
            if(currentForm.richTextBox1.InvokeRequired)
            {
                currentForm.richTextBox1.Invoke((Action)(() =>
                {
                    currentForm.richTextBox1.Clear();
                    currentForm.richTextBox1.AppendText("List Error: ");
                    currentForm.richTextBox1.AppendText(Environment.NewLine);
                    foreach(Err err in listErr)
                    {
                        currentForm.richTextBox1.AppendText(String.Format("Error Name: {0}, times: {1}",err.GetName,err.GetFailTimes.ToString()));
                        currentForm.richTextBox1.AppendText(Environment.NewLine);
                    }
                }));
            }
            else
            {
                currentForm.richTextBox1.Clear();
                currentForm.richTextBox1.AppendText("List Error: ");
                currentForm.richTextBox1.AppendText(Environment.NewLine);
                foreach (Err err in listErr)
                {
                    currentForm.richTextBox1.AppendText(String.Format("Error Name: {0}, times: {1}", err.GetName, err.GetFailTimes.ToString()));
                    currentForm.richTextBox1.AppendText(Environment.NewLine);
                }
            }
        }

        public static void UpdateForm_ConfigPanel(Config_Panel_Mode mode)
        {
            controlInvoke(currentForm.pnl_Config, (Action)(() => { currentForm.pnl_Config.Enabled = mode == Config_Panel_Mode.UNLOCK; }));
            controlInvoke(currentForm.BTN_START,
                (Action)(() =>
                {
                    if(mode == Config_Panel_Mode.LOCK)
                    {
                        timer_start();
                        currentForm.BTN_START.Text = "Running";
                        currentForm.BTN_START.BackColor = System.Drawing.Color.Yellow;
                        currentForm.BTN_START.Enabled = false;
                    }
                    else
                    {
                        timer_stop();
                        currentForm.BTN_START.Text = "Start";
                        currentForm.BTN_START.BackColor = System.Drawing.Color.Gray;
                        currentForm.BTN_START.Enabled = true;
                    }
                }));
        }
        public enum StatusPanel_Mode
        {
            RESET,
            TEST_PASS,
            TEST_FAIL,
        }
        public static void UpdateForm_StatusPannel(StatusPanel_Mode mode,string errorCode=null)
        {
            switch (mode)
            {
                case StatusPanel_Mode.RESET:
                    looptest_info.reset();
                    break;

                case StatusPanel_Mode.TEST_PASS:
                    looptest_info.Test_pass();
                    break;

                case StatusPanel_Mode.TEST_FAIL:
                    if(errorCode != null)
                    {
                        looptest_info.Test_fail(errorCode);
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                    updateFailCount(looptest_info.GetErrList);
                    break;

                default:
                    throw new NotSupportedException();
            }
            set_text_lb_with_test_info();
        }
        private static void Uf_statusPanel_Reset()
        {
            set_text_lb(currentForm.lb_Test_total, "0");
            set_text_lb(currentForm.lb_Test_Pass, "0");
            set_text_lb(currentForm.lb_Test_Fail, "0");
        }
        private static void Uf_statusPanel_()
        {
            set_text_lb(currentForm.lb_Test_total, "0");
            set_text_lb(currentForm.lb_Test_Pass, "0");
            set_text_lb(currentForm.lb_Test_Fail, "0");
        }

        private static void set_text_lb(Label label,string text)
        {
            controlInvoke(label,(Action)(() => { label.Text = text; }));    
        }

        private static void set_text_lb_with_test_info()
        {
            set_text_lb(currentForm.lb_Test_total, looptest_info.GetTestTotal.ToString());
            set_text_lb(currentForm.lb_Test_Pass, looptest_info.GetTestPass.ToString());
            set_text_lb(currentForm.lb_Test_Fail, looptest_info.GetTestFail.ToString());
        }

        public static void WriteDebugLog(string text)
        {
            if(debugForm==null)
            {
                return;
            }
            else
            {
                debugForm.WriteLog(text);
            }
        }

        public static void timer_start()
        {
            currentForm.test_time = DateTime.Now;
            currentForm.timer1.Enabled = true;
            currentForm.timer1.Start();
        }
        public static void timer_stop()
        {
            currentForm.timer1.Stop();
        }

    }

    public class looptest_info
    {
        private int test_total = 0;
        
        public int GetTestTotal { get { return test_total; } }
        private int test_pass = 0;
        public int GetTestPass { get { return test_pass; } }
        private int test_fail = 0;
        public int GetTestFail { get { return test_fail; } }
        private int loop_test_times = 0;
        public int GetLoopTestTimes { get { return loop_test_times; } }
        public bool IsLoopTestFinished { get { return loop_test_times <= test_total; } }

        private ErrList errList;
        public List<Err> GetErrList { get { return errList.GetErrs; } }


        public looptest_info(int loop_test_time)
        {
            loop_test_times = loop_test_time;
            errList = new ErrList();
        }


        public void reset()
        {
            test_total = 0;
            test_pass = 0;
            loop_test_times = 0;
            loop_test_times = 0;
        }
        public void Test_pass()
        {
            test_total++;
            test_pass++;
            //Debug.WriteLine(String.Format("Total: {0}, test: {1}", GetLoopTestTimes, GetTestTotal));
        }
        public void Test_fail(string errorcode)
        {
            test_total++;
            test_fail++;
            errList.Add(errorcode);
        }
    }
    public class ErrList
    {
        private List<Err> err_list;
        public List<Err> GetErrs { get { return err_list; } }
        public ErrList()
        {
            err_list = new List<Err>();
        }
        public void Add(string errcode)
        {
            foreach(Err err in err_list)
            {
                if(err.GetName==errcode)
                {
                    err.Add();
                    return;
                }
            }
            err_list.Add(new Err(errcode));
        }
    }
    public class Err
    {
        private string errcode;
        public string GetName { get { return errcode; } }
        private int errcode_count=1;

        public int GetFailTimes { get { return errcode_count; } }
        public Err(string errcode)
        {
            this.errcode = errcode;
        }
        public void Add()
        {
            errcode_count++;
        }
    }
}
