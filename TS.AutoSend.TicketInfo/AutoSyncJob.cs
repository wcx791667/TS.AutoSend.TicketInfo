using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS.AutoSend.Business;

namespace TS.AutoSend.TicketInfo
{
    public class AutoSyncJob : IJob
    {
        private SellTkDal _service;
        public SellTkDal Service
        {
            get { return _service ?? (_service = new SellTkDal()); }
        }
        public void Execute(IJobExecutionContext context)
        {
            Service.SendTickets();
        }
    }
}
