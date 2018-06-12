using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Business.XCGameMana;
using XCCloudService.CacheService;
using XCCloudService.Model.CustomModel.XCCloud;
using XCCloudService.Model.WeiXin;
using XCCloudService.Model.XCCloud;
using XCCloudService.OrderPayCallback.Common;

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
    }
}