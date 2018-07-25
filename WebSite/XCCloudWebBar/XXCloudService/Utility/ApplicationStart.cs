using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Business.Common;
using XCCloudWebBar.Business.WeiXin;
using XCCloudWebBar.Business.XCCloudRS232;
using XCCloudWebBar.Business.XCGame;
using XCCloudWebBar.Business.XCGameMana;
using XCCloudWebBar.CacheService;
using XCCloudWebBar.CacheService.WeiXin;
using XCCloudWebBar.Common;
using XCCloudWebBar.Common.Enum;
using XCCloudWebBar.Model.CustomModel.XCCloud;
using XCCloudWebBar.Model.WeiXin.Session;
using XCCloudWebBar.SocketService.TCP.Business;
using XCCloudWebBar.SocketService.UDP;
using XCCloudWebBar.OrderPayCallback.Common;
using XCCloudWebBar.Business.XCCloud;

namespace XCCloudWebBar.Utility
{
    public class ApplicationStart
    {
        public static void Init()
        {
            // 在应用程序启动时运行的代码
            try
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "********************************Application Start********************************");
                //TestInit();
                //TCPSocketInit(); 
                UDPSocketInit(); 
                StoreInit(); //redis
                //StoreDogInit();
                //MibleTokenInit(); //redis
                //MemberTokenInit();//redis
                //RS232MibleTokenInit();//redis
                XCCloudUserInit();//redis
                //XCGameManaDeviceInit();
                XCCloudManaUserInit();//redis
                FilterMobileInit();
                //XinchenPayInit();
                XCCloudStoreInit();
            }
            catch(Exception e)
            {
                LogHelper.SaveLog(e.Message);
            }
        }

        public static void TestInit()
        {
            //TokenDataModel tokenDataModel = new TokenDataModel("S0100022", "778852013145", "lijunjie");
            //XCCloudUserTokenBusiness.SetUserToken("3", (int)RoleType.StoreUser, tokenDataModel);
        }

        public static void XCCloudStoreInit()
        {
            XCCloudStoreBusiness.GetStoreInfo();
        }

        public static void XCGameManaDeviceInit()
        {
            try
            {
                DeviceManaBusiness.Clear();
                DeviceManaBusiness.Init();
                LogHelper.SaveLog(TxtLogType.SystemInit, "ManaDevice Init Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "ManaDevice Init Fail..." + Utils.GetException(ex));
            }
        }

        public static void TCPSocketInit()
        {
            try
            {
                XCCloudWebBar.SocketService.TCP.Server webScocket = new XCCloudWebBar.SocketService.TCP.Server();
                TCPServiceBusiness.Server = webScocket;
                webScocket.Start(XCCloudWebBar.SocketService.TCP.Common.TcpConfig.Port); 
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP Init Sucess");
            }
            catch(Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "TCP Init Fail..." + Utils.GetException(ex));
            }
        }

        public static void StoreInit()
        {
            try
            {
                StoreBusiness.StoreInit();
                LogHelper.SaveLog(TxtLogType.SystemInit, "StoreInit Sucess");
            }
            catch(Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "StoreInit..." + Utils.GetException(ex));
            }     
        }        
     
        public static void StoreDogInit()
        {
            try
            {
                StoreBusiness.StoreDogInit();
                LogHelper.SaveLog(TxtLogType.SystemInit, "XCGameManaStoreDogInit Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "XCGameManaStoreInit..." + Utils.GetException(ex));
            }
        }

        public static void UDPSocketInit()
        {
            try
            {
                XCCloudWebBar.SocketService.UDP.Server.Init(XCCloudWebBar.SocketService.UDP.Common.UDPConfig.Port);
                LogHelper.SaveLog(TxtLogType.SystemInit, "UDP Init Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "UDP Init Fail..." + Utils.GetException(ex));
            }
        }

        public static void MibleTokenInit()
        {
            try
            {
                MobileTokenBusiness.Init();
                LogHelper.SaveLog(TxtLogType.SystemInit, "MibleTokenInit Sucess");
            }
            catch(Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "MibleTokenInit Fail..." + Utils.GetException(ex));
            }
        }

        public static void RS232MibleTokenInit()
        {
            try
            {
                MobileTokenBusiness.SetRS232MobileToken();
                LogHelper.SaveLog(TxtLogType.SystemInit, "RS232MibleTokenInit Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "RS232MibleTokenInit Fail..." + Utils.GetException(ex));
            }
        }

        public static void MemberTokenInit()
        {
            try
            {
                //MemberTokenBusiness.Clear();
                MemberTokenBusiness.Init();
                LogHelper.SaveLog(TxtLogType.SystemInit, "MemberTokenInit Sucess");
            }
            catch(Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "MemberTokenInit Fail..." + Utils.GetException(ex));
            }
        }       

        public static void XCCloudUserInit()
        {
            try
            {
                if (RedisCacheHelper.KeyExists(UserBusiness.userInfoCacheKey))
                {
                    //XCCloudWebBar.Business.XCCloud.UserBusiness.Clear();
                    XCCloudWebBar.Business.XCCloud.UserBusiness.XcUserInit();
                    LogHelper.SaveLog(TxtLogType.SystemInit, "XcUserInit Sucess");
                }
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "XcUserInit..." + Utils.GetException(ex));
            }
        }

        public static void XCCloudManaUserInit()
        {
            try
            {
                //XCManaUserHelperTokenBusiness.Clear();
                XCManaUserHelperTokenBusiness.Init();
                LogHelper.SaveLog(TxtLogType.SystemInit, "XcManaUserInit Sucess");
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "XcManaUserInit..." + Utils.GetException(ex));
            }
        }

        public static void FilterMobileInit()
        {
            try
            {
                FilterMobileBusiness.Clear();
                string mobileStr = System.Configuration.ConfigurationManager.AppSettings["filterMobile"].ToString();
                bool isSMSTest = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["isSMSTest"].ToString());
                string[] mobileArr = mobileStr.Split(',');
                for (int i = 0; i < mobileArr.Length; i++)
                {
                    FilterMobileBusiness.AddMobile(mobileArr[i]);
                }
                FilterMobileBusiness.IsTestSMS = isSMSTest;
            }
            catch(Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "FilterMobileInit..." + Utils.GetException(ex));
            }
        }

        /// <summary>
        /// 莘宸自助机支付数据初始化
        /// </summary>
        public static void XinchenPayInit()
        {
            try
            {
                PayList.Clear();
                PayList.Init();
            }
            catch (Exception ex)
            {
                LogHelper.SaveLog(TxtLogType.SystemInit, "XinchenPayInit..." + Utils.GetException(ex));
            }
        }
    }
}
