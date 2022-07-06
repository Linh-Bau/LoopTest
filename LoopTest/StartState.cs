using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoopTest
{
    public class StartState
    {
        private int test_time_loop = 0;
        public StartState(int test_times_loop)
        {
            if (test_times_loop > 0)
                this.test_time_loop = test_times_loop;
            else
                throw new IndexOutOfRangeException();
        }

        public void Run()
        {
            RunningState tester = null;
            while (true)
            {
                tester.UpdateForm();//update form previous state
                tester.Run();
                tester.WaitChangeState();
                if (tester is LoopFinish)
                {
                    break;
                }
                if (tester.isStateChange)
                {
                    tester = tester.GetCurrentTestState;
                }
            }
        }
    }

    public abstract class RunningState
    {
        protected static Process testProcess;
        protected bool isReceivedStateChange = false;
        public bool isStateChange = false;
        protected static RunningState current_state;
        protected static IntPtr Error_label_hwnd = IntPtr.Zero;

        protected bool SentAndWaitCommand(string sentCommand, string WaitFor, int timeout)
        {
            Form1.WriteDebugLog(string.Format("sentCommand: {0}, WaitFor: {1}, timeout: {2}", sentCommand, WaitFor, timeout));
            using (SerialPort port = new SerialPort("COM1", 115200))
            {
                try
                {
                    string dataReceived = "";
                    port.Open();
                    port.WriteLine(sentCommand);
                    Form1.WriteDebugLog(String.Format("port:{0} send: {1}", port.PortName, sentCommand));
                    while (timeout > 0)
                    {
                        sleep(1000);
                        dataReceived += port.ReadExisting();
                        if (dataReceived.Contains(WaitFor))
                        {
                            Form1.WriteDebugLog((String.Format("port:{0} received: {1} PASS!", port.PortName, WaitFor)));
                            return true;
                        }
                        else
                        {
                            timeout--;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Form1.WriteDebugLog(ex.ToString());
                    return false;
                }
            }
        }
        protected bool SentCloseFixtureCommand()
        {
            string command = Config.getKeyValue("end_command");
            string waitfot = Config.getKeyValue("wait_end_command_respond");
            int waitaftersuccess = int.Parse(Config.getKeyValue("wait_fixture_out"));
            int timeout = int.Parse(Config.getKeyValue("command_timeout")) / 1000;
            if (!SentAndWaitCommand(command, waitfot, timeout))
                return false;
            else
                sleep(waitaftersuccess);
            return true;

        }

        protected bool SentStartCommand()
        {
            string commad = Config.getKeyValue("start_test_command");
            string waitfor = Config.getKeyValue("wait_start_command_respond");
            int waitaftersuccess = int.Parse(Config.getKeyValue("wait_close_fixture"));
            int timeout = int.Parse(Config.getKeyValue("command_timeout")) / 1000;
            if (!SentAndWaitCommand(commad, waitfor, timeout))
                return false;
            else
                sleep(waitaftersuccess);
            return true;
        }
        protected bool SentUsbCommand()
        {
            string commad = Config.getKeyValue("usb_power_command");
            string waitfor = Config.getKeyValue("wait_end_command_respond");
            int waitaftersuccess = int.Parse(Config.getKeyValue("wait_open_usb"));
            int timeout = int.Parse(Config.getKeyValue("command_timeout")) / 1000;
            if (!SentAndWaitCommand(commad, waitfor, timeout))
                return false;
            else
                sleep(waitaftersuccess);
            return true;
        }


        public RunningState GetCurrentTestState { get { return current_state; } }
        public abstract void UpdateForm();
        public abstract void UpdateState();
        public abstract void Run();

        protected abstract bool waiTestProgramFinishedTest();
        public virtual void WaitChangeState()
        {
            while (true)
            {
                if (waiTestProgramFinishedTest())
                {
                    break;
                }
                else
                {
                    sleep(1000);
                }
            }
            UpdateState();
        }

        public virtual void ManualChangeState()
        {

        }

        protected void sleep(int milisecon)
        {
            System.Threading.Thread.Sleep(milisecon);
        }

        protected void UpdateStatusPannel()
        {

        }

        protected void UpdateConfigPannel()
        {

        }
    }

    public class WaitStartButtonClick : RunningState
    {

        public WaitStartButtonClick()
        {
            //close all test program of instance
            Form1.UpdateForm_ConfigPanel(Form1.Config_Panel_Mode.UNLOCK);
            string exe_name = Config.getKeyValue("test_program_name");
            var exe_running = Process.GetProcessesByName(exe_name);
            foreach (var exe in exe_running)
            {
                exe.Kill();
            }
            Form1.WriteDebugLog("Kill all test program of instance success!");
        }
        public override void Run()
        {
            //Debug.WriteLine(this.GetType().Name+" Run");       
        }

        public override void UpdateForm()
        {
            //Debug.WriteLine(this.GetType().Name + " UpdateForm");
        }

        public override void UpdateState()
        {

        }

        protected override bool waiTestProgramFinishedTest()
        {
            return true;
        }
        public override void ManualChangeState()
        {
            //Debug.WriteLine(this.GetType().Name + " ManualTestChange");
            current_state = new Start();
            isStateChange = true;
        }

    }
    public class Start : RunningState
    {
        public override void Run()
        {
            //Debug.WriteLine(this.GetType().Name + " Run");
        }

        public override void UpdateForm()
        {
            Form1.UpdateForm_ConfigPanel(Form1.Config_Panel_Mode.LOCK);
        }

        public override void UpdateState()
        {
            isStateChange = true;
            current_state = new Running();
        }

        protected override bool waiTestProgramFinishedTest()
        {
            sleep(100);
            return true;
        }
    }

    public class Running : RunningState
    {
        private bool ProcessExists(int iProcessID)
        {
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (p.Id == iProcessID)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private Process findProcess(string pname)
        {
            var p = Process.GetProcessesByName(pname);
            if (p == null) throw new Exception();
            return p[0];
        }

        
        public override void Run()
        {
            if (testProcess == null)
            {

            }
            
            else if (ProcessExists(testProcess.Id))
            {
                testProcess.Kill();
                Debug.WriteLine("Testprocess exist, close test program and sent push the fixture out command");
                sleep(2000);
            }

            if (!SentCloseFixtureCommand())
            {
                MessageBox.Show("Fail when sent command to fixture!");
                Run();
            }
            else if (!SentStartCommand())
            {
                MessageBox.Show("Fail when sent command to fixture!");
                Run();
            }
            else if (!SentUsbCommand())
            {
                MessageBox.Show("Fail when sent command to fixture!");
                Run();
            }
            Form1.WriteDebugLog("Open test program");
            string test_program_path = Config.getKeyValue("test_program_path");
            Process.Start(System.IO.Directory.GetCurrentDirectory() + "\\cmd.bat");
            sleep(3000);
            testProcess = findProcess(Config.getKeyValue("test_program_name"));

            //post the sn to textbox
            ElementTree elementTree = Form1.GetAllElement(testProcess.MainWindowHandle);
            ElementTree element = elementTree.findCaption("ErrorCode",elementTree);
            Form1.WriteDebugLog(string.Format("error label info-{0},{1},{2}", element.GetHwnd.ToString("X8"), element.GetCaption, element.GetClassName));
            
            Error_label_hwnd = element.GetHwnd;
            ElementTree e = elementTree.findElementByClassContains("EDIT", elementTree);
            Form1.SetTexttAndEnter(e.GetHwnd, Form1.GetSN());
            Debug.WriteLine("Sent text success!");
        }

        public override void UpdateForm()
        {

        }

        public override void UpdateState()
        {
            isStateChange = true;
            current_state = new UnitFinish();
        }

        protected override bool waiTestProgramFinishedTest()
        {
            ElementTree elementTree = Form1.GetAllElement(testProcess.MainWindowHandle);
            if (Form1.GetTestResultFromTestProgram(elementTree) == Form1.TEST_STATUS.TESTING)
            {
                return true;
            }
            return false;
        }
    }
    public class UnitFinish : RunningState
    {
        public override void Run()
        {

        }

        public override void UpdateForm()
        {

        }

        public override void UpdateState()
        {
            isStateChange = true;
            if (Form1.isLoopTestFinished)
            {
                current_state = new LoopFinish();
            }
            else current_state = new Running();
        }

        protected override bool waiTestProgramFinishedTest()
        {
            //Debug.WriteLine("isTestFinished: " + Form1.isLoopTestFinished);
            if (!Form1.isLoopTestFinished)
            {
                ElementTree elementTree = Form1.GetAllElement(testProcess.MainWindowHandle);
                if (Form1.GetTestResultFromTestProgram(elementTree) == Form1.TEST_STATUS.PASS)
                {
                    Form1.WriteDebugLog("Test PASS!");
                    Form1.UpdateForm_StatusPannel(Form1.StatusPanel_Mode.TEST_PASS);
                    return true;
                }
                else if (Form1.GetTestResultFromTestProgram(elementTree) == Form1.TEST_STATUS.FAIL)
                {
                    string errorCode= Form1.GetTextWindow(Error_label_hwnd);
                    Form1.WriteDebugLog("Test FAIL!, get error code is:"+errorCode);
                    Form1.UpdateForm_StatusPannel(Form1.StatusPanel_Mode.TEST_FAIL,errorCode);
                    return true;
                }
                else
                {
                    Form1.WriteDebugLog("Testing");
                    return false;
                }
            }
            return true;
        }
    }
    public class LoopFinish : RunningState
    {

        public override void Run()
        {
            SentCloseFixtureCommand();
        }

        public override void UpdateForm()
        {

        }

        public override void UpdateState()
        {
            isStateChange = true;
            current_state = new WaitStartButtonClick();
        }

        protected override bool waiTestProgramFinishedTest()
        {
            sleep(100);
            return true;
        }
    }

    public static class Config
    {
        static string path = System.IO.Directory.GetCurrentDirectory() + "\\config.ini";

        static string data = System.IO.File.ReadAllText(path);
        public static string getKeyValue(string key)
        {
            //like <key=value>
            string pattern = string.Format("<(?<key>.*?{0}.*?)=(?<value>.*?)>", key);
            var result = Regex.Match(data, pattern);
            string key_value = result.Groups["value"].Value.Trim();
            return key_value;
        }

    }
}
