using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using TS.AutoSend.Entity;
using Newtonsoft.Json;
using System.Net.Http;
using com.Helper;
using log4net;
using System.Reflection;
using System.Net;
using System.IO;
using System.Text;

namespace TS.AutoSend.Business
{
    public class SellTkDal
    {
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Database db = DatabaseFactory.CreateDatabase("1");

        public List<TicketInfo> GetTicketInfo(string carryStaId)
        {
            DataTable dt;
             List<TicketInfo> list = new List<TicketInfo>();
            try
            {
                string sql = string.Format(@"select  * from  SpTicket_View  where datediff(minute,售票时间,getdate())<=5");
                dt = db.ExecuteDataSet(CommandType.Text, sql).Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    TicketInfo ticket = new TicketInfo();

                    ticket.xm = dr["姓名"].ToString().Trim().Replace("\n","").Replace("\r","");
                    ticket.idcardtype = "11";
                    ticket.idcard = dr["身份证"].ToString().Trim();
                    string sexFlag = ticket.idcard.Length == 15 ? ticket.idcard.Substring(14, 1) : ticket.idcard.Substring(16, 1);
                    if (int.Parse(sexFlag) % 2 == 0)//判断身份证最后一位（15位身份证）或第17位（18位身份证）：奇数为男性，偶数为女性
                    {
                        ticket.sex = "女";
                    }
                    else
                    {
                        ticket.sex = "男";
                    }
                    ticket.people = "";
                    ticket.birthday = ticket.idcard.Substring(6, 8);
                    ticket.tel = "";
                    ticket.address = "";
                    ticket.dept = "";
                    ticket.vpstartdate = "19000314122000";
                    ticket.vpenddate = "19000314122000";
                    ticket.photo = "";
                    ticket.carnum = dr["车牌号"].ToString().Trim();
                    ticket.startstation = dr["所属站"].ToString().Trim();
                    ticket.endstation = dr["站点名"].ToString().Trim();
                    //"buydate":"20170909 16:52:33:5233"
                    //2017 03 14 12 20 00
                    string drvDate = Convert.ToDateTime(dr["发车日期"].ToString()).ToString("yyyy-MM-dd").Replace("-", "");
                    string schId = dr["车次"].ToString().Trim();
                    ticket.departdate = GetTickeDrvTime(schId, drvDate);
                    ticket.buydate = Convert.ToDateTime(dr["售票时间"].ToString()).ToString("yyyy-MM-dd HH:mm:ss").Replace("-", "").Replace(" ", "").Replace(":", ""); ;
                    list.Add(ticket);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public string GetTickeDrvTime(string schId,string drvDate )
        {
            DataTable dt;
            try
            {
                string result = "";
                string drvTime = "";
                string sql = "select  top 1 convert(varchar(5),a.发车时间,108) drvTime  from  " + "tempdb.dbo.cc"+ drvDate+"  as a  where  a.车次=" + "'" + schId + "'";
                dt = db.ExecuteDataSet(CommandType.Text, sql).Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    drvTime = dr["drvTime"].ToString().Trim();
                }
                result = drvDate + drvTime.Trim().Replace(":", "")+"00";
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public static string Post(string url, byte[] data)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            req.ContentLength = data.Length;
            Stream requestStream = req.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8);
            string backstr = sr.ReadToEnd();
            sr.Close();
            res.Close();
            return backstr;
        }


        public void SendTickets()
        {
            _logger.Info($"同步车票信息开始：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            HttpClient _httpClient = new HttpClient();
            string ApiUrl = "http://124.112.209.58:8089/scpt/smgp/add";
            //第一步获取token
            _httpClient.BaseAddress = new Uri(ApiUrl);

         
            var ticketList = GetTicketInfo("");
            int ticketCount = ticketList.Count;
            if (ticketCount > 0)
            {
                try
                {
                    string psgStrlist = JsonConvert.SerializeObject(ticketList);
                    psgStrlist = psgStrlist.Remove(0, 1).TrimEnd(']');

                    //string result2 = Encrpt.encrypt(psgStrlist, key);
                    //var x22 = Encrpt.decrypt(result2, key);

                    //_logger.Info($"车票信息:" + psgStrlist);

                    string postdata = SecurityHelper.Encode(psgStrlist);
                    string postString = "systemcode=XCJTZWGLXT&systemname=宣城敬亭站务管理系统&key=0&qybm=N003418250001&info=" + postdata;
                    byte[] gopost = Encoding.UTF8.GetBytes(postString);
                    string backResult = Post(ApiUrl, gopost);

                    if (backResult.Contains("数据已成功录入"))
                    {
                        _logger.Info($"同步车票信息完成：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}同步车票数量:{ticketCount}");
                    }
                    else
                    {
                        _logger.Info($"同步车票信息失败,接口返回信息：{backResult}");
                    }

                   

                }
                catch (Exception ex)
                {
                    _logger.Error($"车次同步异常：{ex.Message}");
                    //throw ex;
                }
            }
            else
            {
                _logger.Info($"五分钟内没有车票信息");
            }

        }



    }
}
