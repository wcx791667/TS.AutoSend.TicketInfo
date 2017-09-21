using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TS.AutoSend.Business
{
    public class SecurityHelper
    {
        public static string _KEY = "44446666";  //密钥  

        /// <summary>  
        /// 加密  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static string Encode(string data)
        {

            byte[] byKey = System.Text.ASCIIEncoding.UTF8.GetBytes(_KEY);
            byte[] byIV = System.Text.ASCIIEncoding.UTF8.GetBytes(_KEY);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

            int i = cryptoProvider.KeySize;
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);

            StreamWriter sw = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>  
        /// 解密  
        /// </summary>  
        /// <param name="data"></param>  
        /// <returns></returns>  
        public static string Decode(string data)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[data.Length / 2];
            for (int x = 0; x < data.Length / 2; x++)
            {
                int i = (Convert.ToInt32(data.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = ASCIIEncoding.ASCII.GetBytes(_KEY);　//建立加密对象的密钥和偏移量，此值重要，不能修改
            des.IV = ASCIIEncoding.ASCII.GetBytes(_KEY);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();　//建立StringBuild对象，CreateDecrypt使用的是流对象，必须把解密后的文本变成流对象
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }
        /// <summary>
        /// 创建加密签名
        /// </summary>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        //public static string CreateSignature(string userinfo)
        //{
        //    var token = Guid.NewGuid().ToString("N").Substring(0, 8);
        //    return Encode(userinfo, token);
        //}

        /// <summary>
        /// 获取当前随机码
        /// </summary>
        /// <returns></returns>
        //public static string GetCode()
        //{
        //    Random rd = new Random();
        //    int shu = rd.Next(97, 122);
        //    char c = (char)shu;
        //    Random rdnumber = new Random();
        //    var val = rdnumber.Next(0, 99999);
        //    return c + val.ToString().PadLeft(5, '0');
        //}
    }
}
