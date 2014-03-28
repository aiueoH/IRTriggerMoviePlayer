using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace Furnace
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1(new Model()));
            }
            catch (System.Exception ex)
            {
                string s = "UI執行緒例外" + Environment.NewLine + Environment.NewLine;
                s += "請檢查USB裝置以及config檔是否正確" + Environment.NewLine + Environment.NewLine;
                //s += ex.ToString();
                MessageBox.Show(s);
                Process.GetCurrentProcess().Kill();
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string s = "非UI執行緒例外" + Environment.NewLine + Environment.NewLine;
            if (e.ExceptionObject is Exception)
            {
                s += "請檢查USB裝置以及config檔是否正確" + Environment.NewLine + Environment.NewLine;
                //s += (e.ExceptionObject as Exception).ToString();
            }
            else
                s += "未知的例外";
            MessageBox.Show(s);
            Process.GetCurrentProcess().Kill();
        }
    }
}
