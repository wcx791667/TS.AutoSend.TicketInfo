using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using TS.AutoSend.Business;

namespace TS.AutoSend.TicketInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ServiceRunner>();

                x.SetDescription("定时同步车票信息");
                x.SetDisplayName("车票信息同步");
                x.SetServiceName("车票信息同步");

                x.EnablePauseAndContinue();
            });

            Console.ReadKey();

        }
    }
}
