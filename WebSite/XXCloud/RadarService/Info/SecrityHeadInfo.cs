using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using RadarService.COMObject;

namespace RadarService.Info
{
    public static class SecrityHeadInfo
    {
        public static bool 是否需要授权校验 = false;

        static List<string> 授权机头 = new List<string>();
        public static string 店密码 = "FFFFFFFFFFFF";
        public static string 店ID = "";
        public static DateTime 到期日期;
        public static int 错误获取日期次数 = 0;
        public static int 客户类别 = 0; //0 普通客户 1 好酷专用 2 娃娃家族专用

        public static void Init()
        {
            if (File.Exists("HeadList.wxd"))
            {
                string[] data = DecryptDES(File.ReadAllText("HeadList.wxd"), "wicky123").Replace("\r", "").Split('\n');
                foreach (string d in data)
                {
                    string[] sub = d.Split('=');
                    switch (sub[0])
                    {
                        case "StorePassword":
                            店密码 = sub[1];
                            break;
                        case "HeadMCUID":
                            授权机头.Add(sub[1].ToUpper());
                            break;
                        case "UserID":
                            店ID = sub[1];
                            break;
                        case "powerDueDate":
                            DateTime.TryParse(sub[1], out 到期日期);
                            break;
                        case "MemberType":
                            客户类别 = Convert.ToInt32(sub[1]);
                            break;
                        case "shopId":
                            HKApi.ShopID = sub[1];
                            break;
                        case "StoreSecret":
                            HKApi.StoreSecret = sub[1];
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DecryptDES(string pToDecrypt, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[pToDecrypt.Length / 2];
            for (int x = 0; x < pToDecrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(pToDecrypt.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();

            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="pToEncrypt"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string EncryptDES(string pToEncrypt, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.Default.GetBytes(pToEncrypt);
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();
        }

        public static bool CheckHead(string MCUID)
        {
            return 授权机头.Contains(MCUID);
        }


        public static void CheckAuthor(DateTime d)
        {
            if (d >= 到期日期)
            {
                PubLib.frmUser.lblWarning.Text = "授权到期，串口关闭";
                TerminalDataProcess.Close();
                FrmAuthor frm = new FrmAuthor(true);
                frm.ShowDialog();
            }
        }

        /// <summary>
        /// 更新授权文件
        /// </summary>
        /// <param name="newDate"></param>
        public static void UpdateAuthorFile(string newDate)
        {
            if (File.Exists("HeadList.wxd"))
            {
                string context = "";

                string[] data = DecryptDES(File.ReadAllText("HeadList.wxd"), "wicky123").Replace("\r", "").Split('\n');
                foreach (string d in data)
                {
                    string[] sub = d.Split('=');
                    if (sub[0] == "powerDueDate")
                        context += string.Format("powerDueDate={0}\r\n", newDate);
                    else
                        context += d + "\r\n";
                }

                StreamWriter sw = new StreamWriter("HeadList.wxd");
                string encode = EncryptDES(context, "wicky123");
                sw.Write(encode);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
