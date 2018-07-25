﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.WeiXin;
using XCCloudService.Model.XCCloud;
using XCCloudService.OrderPayCallback.Common;
using XXCloudService.Utility;

namespace XXCloudService.Test
{
    public partial class redisTest : System.Web.UI.Page
    {
        //protected override void OnInit(EventArgs e)
        //{
        //    this.Button1
        //    base.OnInit(e);
        //}

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string serialNo = RedisCacheHelper.CreateCloudSerialNo("100016360103001");
                Response.Write(serialNo);
            }
            catch
            { 
            }
            
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string conn = Config.EncryptDES("Data Source=192.168.1.119;Initial Catalog=XinchenPay;User Id=sa;Password=xinchen;Connection Timeout=10;", "Xinchen1");
            Response.Write(conn);
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            string token = txtToken.Text.Trim();

            XCCloudUserTokenModel model = XCCloudUserTokenBusiness.GetUserTokenModel(token);
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            MemberTokenModel tokenModel = new MemberTokenModel();
            tokenModel.Token = TextBox1.Text.Trim();
            WechatInfo wechat = new WechatInfo();
            wechat.subscribe = Convert.ToInt32(TextBox2.Text.Trim());
            wechat.headimgurl = TextBox3.Text.Trim();
            wechat.nickname = TextBox4.Text.Trim();
            tokenModel.Info = wechat;

            MemberTokenCache.AddToken(tokenModel.Token, tokenModel);
        }

        protected void Button5_Click(object sender, EventArgs e)
        {
            List<Dict_System> gameTypeList = new List<Dict_System>();
            GetGamingList(515, gameTypeList);

            string str = string.Empty;
            foreach (var item in gameTypeList)
            {
                str += item.ID + "|" + item.DictKey + "<br />";
            }
            Response.Write(str);
        }

        private void GetGamingList(int pid, List<Dict_System> gameTypeList)
        {
            var list = Dict_SystemService.I.GetModels(t => t.PID == pid).ToList();
            foreach (var item in list)
            {
                gameTypeList.Add(item);
                GetGamingList(item.ID, gameTypeList);
            }
        }

        protected void Button6_Click(object sender, EventArgs e)
        {
            try
            {
                Base_MemberInfo member = new Base_MemberInfo();
                member.ID = RedisCacheHelper.CreateCloudSerialNo("0".PadLeft(15, '0'));
                member.WechatOpenID = "oNWocwSC_GFO8n_8mtZ0iV9tL0WI";
                member.UserPassword = Utils.MD5("123456");
                member.UserName = "慕斯";
                member.Photo = "http://thirdwx.qlogo.cn/mmopen/SmBMwo29LaUa5THehbCwRbwnvXUvmCtYPaxImCGEeIGKqJgPGUiaD4JiaNgbBj0VzbJ1nDbePekVUZIuTVVwPtiavPU0xIK1k0P/132";
                member.CreateTime = DateTime.Now;
                member.MemberState = 1;
                bool ret = Base_MemberInfoService.I.Add(member);
                if (!ret)
                {
                    Response.Write("添加成功");
                }
            }
            catch (DbEntityValidationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void Button7_Click(object sender, EventArgs e)
        {
            string fieldKey = "oNWocwSC_GFO8n_8mtZ0iV9tL0WI1";
            MemberTokenModel memberCacheModel = MemberTokenCache.GetModel(fieldKey);
            if (memberCacheModel == null)
            {
                Response.Write("00000");
            }
            else
            {
                Response.Write("11111");
            }
        }

        protected void Button8_Click(object sender, EventArgs e)
        {
            try
            {
                string key = "testWriteExpireKey";
                RedisCacheHelper.StringSet(key, "aadfs");
                for (int i = 0; i < 5; i++)
                {
                    RedisCacheHelper.KeyExpire(key, new TimeSpan(0, 0, 0, CacheExpires.CommonPageQueryDataCacheTime));
                    //Thread.Sleep(10);
                }
            }
            catch(Exception ex)
            {
                Response.Write(ex.Message);
            }
        }

        protected void Button9_Click(object sender, EventArgs e)
        {
            string errMsg = "";
            Flw_Order order = Flw_OrderService.I.GetModels(t => t.ID == txtOrderId.Text.Trim()).FirstOrDefault();
            Data_Member_Card card = Data_Member_CardService.I.GetModels(t => t.ID == order.CardID).FirstOrDefault();
            Base_StoreInfo store = Base_StoreInfoService.I.GetModels(t => t.ID == order.StoreID).FirstOrDefault();
            Flw_GameAPP_Rule_Entry gameRule = Flw_GameAPP_Rule_EntryService.I.GetModels(t => t.ID == txtRuleId.Text.Trim()).FirstOrDefault();
            Base_DeviceInfo device = Base_DeviceInfoService.I.GetModels(t => t.ID == gameRule.DeviceID).FirstOrDefault();
            //请求雷达投币
            if (!IConUtiltiy.RemoteDeviceCoinIn(XCGameManaDeviceStoreType.Store, DevieControlTypeEnum.投币, card.ICCardID, order.ID, device.MCUID, store.Password, "1", gameRule.RuleID.ToString(), "1", out errMsg))
            {
                Response.Write(errMsg);
            }
        }
    }
}