using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Liangddyy.UnityKitModule.Common
{
    public class ShellUtil
    {
        /// <summary>
        ///     使用powerShell，换成bash ?
        ///     cmd.exe不可用
        ///     可使用多条命令
        /// </summary>
        /// <param name="command"></param>
        public static bool RunPowershellCommand(string command)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell";
                process.StartInfo.Arguments = command;

                process.StartInfo.CreateNoWindow = true; // 是否要查看powershell窗口执行过程
                process.StartInfo.ErrorDialog = true; // 该值指示不能启动进程时是否向用户显示错误对话框  
                process.StartInfo.UseShellExecute = false;
                // 默认设置
                //            process.StartInfo.RedirectStandardError = true;  
                //process.StartInfo.RedirectStandardInput = true;  
                //process.StartInfo.RedirectStandardOutput = true;  

                try
                {
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 运行cmd命令
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string RunCmdCommand(string command)
        {
            //例Process
            using (Process p = new Process())
            {
                p.StartInfo.FileName = "cmd.exe"; //确定程序名
                p.StartInfo.Arguments = "/c " + command; //确定程式命令行
                p.StartInfo.UseShellExecute = false; //Shell的使用
                p.StartInfo.RedirectStandardInput = true; //重定向输入
                p.StartInfo.RedirectStandardOutput = true; //重定向输出
                p.StartInfo.RedirectStandardError = true; //重定向输出错误
                p.StartInfo.CreateNoWindow = false; //设置置不显示示窗口
                p.Start();
                return p.StandardOutput.ReadToEnd();
            }
        }
    }
}