
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace CpuTop5
{
    public class ProcessInfo : IComparable
    {
        public int pId;
        public string processName;
        public int handleCount;
        public double cpuUsage;
        public long mem;
        public TimeSpan prevCpuTime;
        public TimeSpan curCpuTime;

        public ProcessInfo()
        {
            this.pId = 0;
            this.processName = "NULL";
            this.handleCount = 0;
            this.cpuUsage = 0.0F;
            this.mem = 0;
            this.prevCpuTime = TimeSpan.Zero;
            this.curCpuTime = TimeSpan.Zero;
        }


        public int CompareTo(object obj)
        {
            if (obj is ProcessInfo)
            {
                return cpuUsage.CompareTo(((ProcessInfo)obj).cpuUsage);
            }
            return 1;
        }
    }

    class Cpu
    {

        static int t = 60000;
        public static void fun(string logName)
        {
            StreamWriter file;
            if (!File.Exists(logName))
            {
                file = File.CreateText(logName);
            }
            else
            {
                file = File.AppendText(logName);
            }
            //file.WriteLine("Top,Time,ProcessId,Name,Cpu(%),Memory(KB),HandleCount");

            //收集所有进程信息
            Process[] allProcess = Process.GetProcesses();
            ProcessInfo[] allProcessInfo = new ProcessInfo[allProcess.Length];

            for (int i = 0; i < allProcess.Length; i++)
            {
                allProcessInfo[i] = new ProcessInfo();
            }

            //保存进程的id,名字,第一次时间记录
            for (int i = 0; i < allProcess.Length; i++)
            {
                if (allProcess[i].Id == 0)
                {
                    continue;
                }
                // Console.WriteLine("id={0}", allProcess[i].Id);
                try
                {
                    allProcessInfo[i].pId = allProcess[i].Id;
                    allProcessInfo[i].processName = allProcess[i].ProcessName;
                    allProcessInfo[i].prevCpuTime = allProcess[i].TotalProcessorTime;
                }
                catch (Exception x)
                {
                    continue;
                }

            }

            //等待时间
            Thread.Sleep(t);
            //保存进程的句柄数,内存,第二次时间记录,计算cpu
            for (int i = 0; i < allProcess.Length; i++)
            {
                if (allProcessInfo[i].pId == 0)
                {
                    continue;
                }
                try
                {
                    allProcessInfo[i].handleCount = allProcess[i].HandleCount;
                    allProcessInfo[i].mem = allProcess[i].PrivateMemorySize64 / 1024;
                    allProcessInfo[i].curCpuTime = allProcess[i].TotalProcessorTime;
                    //Console.WriteLine("pt={0},ct={1},pt-ct={2}",allProcessInfo[i].prevCpuTime,allProcessInfo[i].curCpuTime
                    var usage = (allProcessInfo[i].curCpuTime - allProcessInfo[i].prevCpuTime).TotalMilliseconds / t / Environment.ProcessorCount * 100;
                    allProcessInfo[i].cpuUsage = usage;

                    allProcessInfo[i].prevCpuTime = allProcessInfo[i].curCpuTime;
                }
                catch (Exception x)
                {
                    continue;
                }

            }

            //排序
            Array.Sort(allProcessInfo);
            Array.Reverse(allProcessInfo);
            for (int i = 0; i < allProcessInfo.Length && i < 5; i++)
            {
                file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", i + 1, DateTime.Now, allProcessInfo[i].pId, allProcessInfo[i].processName, allProcessInfo[i].cpuUsage, allProcessInfo[i].mem, allProcessInfo[i].handleCount));
                file.Flush();

            }
            file.Close();

        }

        public static void test()
        {
            Process p = Process.GetProcessById(4);

        }

        static void Main(string[] args)
        {
            string logName = @"CPU消耗前5进程信息数据.csv";
            StreamWriter file;
            try
            {
                if (!File.Exists(logName))
                {
                    file = File.CreateText(logName);
                }
                else
                {
                    file = File.AppendText(logName);
                }
                file.WriteLine("Top,Time,ProcessId,Name,Cpu(%),Memory(KB),HandleCount");
                file.Close();
            }
            catch (Exception x)
            {
                Console.WriteLine("打开日志文件失败！");
                return;
            }
            Console.WriteLine("正在记录进程信息...\n\n信息记录在《CPU消耗前5进程信息数据.csv》中...\n\n按Ctrl+C结束程序\n");
            while (true)
            {
                fun(logName);

                //test();
            }


        }




    }



}
