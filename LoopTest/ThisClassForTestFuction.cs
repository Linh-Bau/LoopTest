using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using AutoIt;

namespace LoopTest
{

    public partial class Form1
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //Tìm cửa sổ của cái button ấy
        //Get text
        //WindowsForms10.Window.8.app.0.34f5582_r9_ad1
        //AutoTestSystem V1.22.6.17

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd1, IntPtr hWnd2, string lpsz1, string lpsz2);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int Param, string s);

        [DllImport("user32.dll")]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName,int max_count);

        const int WM_SETTEXT = 0x000c;
        const int WM_KEYDOWN = 0x0100;
        const int VK_RETURN = 0x0d;



        //###############################################################################################################

        public static void SetTexttAndEnter(IntPtr hWnd, string text)
        {
            SendMessage(hWnd, WM_SETTEXT, 0, text);
            SendMessage(hWnd, WM_KEYDOWN, VK_RETURN, null);
        }

        static string GetClassName(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder(256);
            int nRet = GetClassName(hwnd, sb, sb.Capacity);
            if (nRet != 0)
            {
                return sb.ToString();
            }
            else
            {
                return null;
            }
        }
        public static string GetTextWindow(IntPtr hwnd)
        {
            StringBuilder sb = new StringBuilder();
            int windw_txt_length = GetWindowTextLength(hwnd);
            GetWindowText(hwnd, sb, windw_txt_length+1);
            return sb.ToString();
        }

        static IntPtr FindWindowByIndex(IntPtr hWndParent, int index)
        {
            if (index == 0)
                return hWndParent;
            else
            {
                int ct = 0;
                IntPtr result = IntPtr.Zero;
                do
                {
                    result = FindWindowEx(hWndParent, result, null, null);
                    if (result != IntPtr.Zero)
                        ++ct;
                }
                while (ct < index && result != IntPtr.Zero);
                return result;
            }
        }

        public static ElementTree GetAllElement(IntPtr hWndParent)
        {
            ElementBranch base_branch = new ElementBranch(hWndParent, GetTextWindow(hWndParent),GetClassName(hWndParent));
            ElementLeaf elementLeaf = new ElementLeaf(hWndParent, GetTextWindow(hWndParent), GetClassName(hWndParent));
            bool isElementLeaf = true;
            IntPtr result = IntPtr.Zero;
            do
            {
                result = FindWindowEx(hWndParent, result, null, null);
                //this is a branch
                if (result != IntPtr.Zero)
                {
                    isElementLeaf = false;
                    var branch = GetAllElement(result);
                    base_branch.AddElements(branch);
                    result = branch.GetHwnd;
                }
            }
            while (result != IntPtr.Zero);
            if(isElementLeaf)
            {
                return elementLeaf;
            }
            else
            return base_branch;
        }

        public enum TEST_STATUS
        {
            NONE,
            STANDBY,
            PASS,
            FAIL,
            TESTING
        }
        public static TEST_STATUS GetTestResultFromTestProgram(ElementTree elementTree)
        {
            //var parent_hwmd = FindWindow("WindowsForms10.Window.8.app.0.34f5582_r9_ad1", "AutoTestSystem V1.22.6.17");
            
            //////Debug.WriteLine("get tree ok!, find Standby");
            while(true)
            {
                if (elementTree.findCaption("Standby",elementTree)!=null)
                {
                    set_text_lb(currentForm.lb_Status, "Standby");
                    return TEST_STATUS.STANDBY;
                }
                else if(elementTree.findCaption("PASS",elementTree)!=null)
                {
                    ////Debug.WriteLine("Test Pass");
                    set_text_lb(currentForm.lb_Status, "PASS");
                    return TEST_STATUS.PASS;
                }
                else if(elementTree.findCaption("FAIL", elementTree)!=null)
                {
                    ////Debug.WriteLine("Test FAIL");
                    set_text_lb(currentForm.lb_Status, "Fail");
                    return TEST_STATUS.FAIL;
                }
                else if (elementTree.findCaption("Testing", elementTree)!=null)
                {
                    //System.Threading.Thread.Sleep(1000);
                    ////Debug.WriteLine("Testing");
                    set_text_lb(currentForm.lb_Status, "Testing");
                    return TEST_STATUS.TESTING;
                }
                else
                {
                    //System.Threading.Thread.Sleep(1000);
                    ////Debug.WriteLine("None");
                    return TEST_STATUS.NONE;
                }
            }
        }
    }

    public abstract class ElementTree
    {
        protected IntPtr hwnd;
        public IntPtr GetHwnd { get { return hwnd; } }
        protected string caption;
        public string GetCaption { get { return caption; } }
        protected string className;
        public string GetClassName { get { return className; } }
        //inhan
        public void ShowAllCaption(ElementTree elementTree)
        { 
            foreach (ElementTree element in elementTree.GetElementTree)
            {
                if (element is ElementLeaf)
                {
                    Form1.WriteDebugLog(string.Format("{0},{1},{2}", element.GetHwnd.ToString("X8"), element.GetCaption, element.GetClassName));
                }
                else
                {
                    //Debug.WriteLine(String.Format("Type: Branch, HWND: {0}, caption: {1}", element.GetHwnd.ToString("X8"), element.GetCaption));
                    ShowAllCaption(element);
                }
            }
        }

        public ElementTree findCaption(string caption,ElementTree elementTree)
        {
            foreach(ElementTree element in elementTree.GetElementTree)
            {                
                if(element.GetCaption==caption)
                {
                    return element;
                }
                else
                {
                    var c_rs=findCaption(caption,element);
                    if(c_rs==null)
                    {
                        continue;
                    }
                    else if(c_rs.GetCaption==caption)
                    {
                        return c_rs;
                    }
                }
            }
            return null;
        }

        public ElementTree findElementByClassContains(string classname,ElementTree elementTree)
        {
            ElementTree element_rs = null;
            foreach (ElementTree element in elementTree.GetElementTree)
            {
                if (element is ElementLeaf)
                {
                    ////Debug.WriteLine(String.Format("Type: Left, HWND: {0}, caption: {1}", element.GetHwnd.ToString("X8"), element.GetCaption));
                    if (element.GetClassName.Contains(classname)) return element;
                }
                else
                {
                    ////Debug.WriteLine(String.Format("Type: Branch, HWND: {0}, caption: {1}", element.GetHwnd.ToString("X8"), element.GetCaption));
                    var e_rs=findElementByClassContains(classname, element);
                    if (e_rs != null) return e_rs;
                }
            }
            return element_rs;
        }
        public ElementTree(IntPtr hwnd,string caption,string classname)
        {
            this.elementTree = new List<ElementTree>();
            this.hwnd = hwnd;   
            this.caption = caption;
            this.className = classname;
        }
        protected List<ElementTree> elementTree;
        public List<ElementTree> GetElementTree { get { return elementTree; } }
        public abstract void AddElements(ElementTree tree);
    }
    class ElementLeaf : ElementTree
    {
        public ElementLeaf(IntPtr hwnd, string caption, string classname) : base(hwnd, caption,classname)
        {
        }

        public override void AddElements(ElementTree tree)
        {
            throw new NotSupportedException();
        }
    }
    class ElementBranch : ElementTree
    {
        public ElementBranch(IntPtr hwnd, string caption, string classname) : base(hwnd, caption,classname)
        {
        }

        public override void AddElements(ElementTree tree)
        {
           elementTree.Add(tree);
        }
    }
}
