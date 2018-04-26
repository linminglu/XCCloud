 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
using XCCloudService.BLL.Container;

namespace XCCloudService.BLL.XCCloud
{  
    
    public partial class Base_ChainRuleService:BaseService<Base_ChainRule>,IBase_ChainRuleService
    {
		public override void SetDal()
        {

        }

        public Base_ChainRuleService()
            : this(false)
        {

        }

        public Base_ChainRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_ChainRuleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_ChainRule_StoreService:BaseService<Base_ChainRule_Store>,IBase_ChainRule_StoreService
    {
		public override void SetDal()
        {

        }

        public Base_ChainRule_StoreService()
            : this(false)
        {

        }

        public Base_ChainRule_StoreService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_ChainRule_StoreDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_DepotInfoService:BaseService<Base_DepotInfo>,IBase_DepotInfoService
    {
		public override void SetDal()
        {

        }

        public Base_DepotInfoService()
            : this(false)
        {

        }

        public Base_DepotInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_DepotInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_DeviceInfoService:BaseService<Base_DeviceInfo>,IBase_DeviceInfoService
    {
		public override void SetDal()
        {

        }

        public Base_DeviceInfoService()
            : this(false)
        {

        }

        public Base_DeviceInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_DeviceInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_EnumParamsService:BaseService<Base_EnumParams>,IBase_EnumParamsService
    {
		public override void SetDal()
        {

        }

        public Base_EnumParamsService()
            : this(false)
        {

        }

        public Base_EnumParamsService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_EnumParamsDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_GoodsInfoService:BaseService<Base_GoodsInfo>,IBase_GoodsInfoService
    {
		public override void SetDal()
        {

        }

        public Base_GoodsInfoService()
            : this(false)
        {

        }

        public Base_GoodsInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_GoodsInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_MemberInfoService:BaseService<Base_MemberInfo>,IBase_MemberInfoService
    {
		public override void SetDal()
        {

        }

        public Base_MemberInfoService()
            : this(false)
        {

        }

        public Base_MemberInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MemberInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_MerchAlipayService:BaseService<Base_MerchAlipay>,IBase_MerchAlipayService
    {
		public override void SetDal()
        {

        }

        public Base_MerchAlipayService()
            : this(false)
        {

        }

        public Base_MerchAlipayService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MerchAlipayDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_MerchantInfoService:BaseService<Base_MerchantInfo>,IBase_MerchantInfoService
    {
		public override void SetDal()
        {

        }

        public Base_MerchantInfoService()
            : this(false)
        {

        }

        public Base_MerchantInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MerchantInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_MerchFunctionService:BaseService<Base_MerchFunction>,IBase_MerchFunctionService
    {
		public override void SetDal()
        {

        }

        public Base_MerchFunctionService()
            : this(false)
        {

        }

        public Base_MerchFunctionService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_MerchFunctionDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_SettleLCPayService:BaseService<Base_SettleLCPay>,IBase_SettleLCPayService
    {
		public override void SetDal()
        {

        }

        public Base_SettleLCPayService()
            : this(false)
        {

        }

        public Base_SettleLCPayService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_SettleLCPayDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_SettleOrgService:BaseService<Base_SettleOrg>,IBase_SettleOrgService
    {
		public override void SetDal()
        {

        }

        public Base_SettleOrgService()
            : this(false)
        {

        }

        public Base_SettleOrgService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_SettleOrgDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_SettlePPOSService:BaseService<Base_SettlePPOS>,IBase_SettlePPOSService
    {
		public override void SetDal()
        {

        }

        public Base_SettlePPOSService()
            : this(false)
        {

        }

        public Base_SettlePPOSService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_SettlePPOSDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_StorageInfoService:BaseService<Base_StorageInfo>,IBase_StorageInfoService
    {
		public override void SetDal()
        {

        }

        public Base_StorageInfoService()
            : this(false)
        {

        }

        public Base_StorageInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_StorageInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_StoreDogListService:BaseService<Base_StoreDogList>,IBase_StoreDogListService
    {
		public override void SetDal()
        {

        }

        public Base_StoreDogListService()
            : this(false)
        {

        }

        public Base_StoreDogListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_StoreDogListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_StoreInfoService:BaseService<Base_StoreInfo>,IBase_StoreInfoService
    {
		public override void SetDal()
        {

        }

        public Base_StoreInfoService()
            : this(false)
        {

        }

        public Base_StoreInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_StoreInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_StoreWeightService:BaseService<Base_StoreWeight>,IBase_StoreWeightService
    {
		public override void SetDal()
        {

        }

        public Base_StoreWeightService()
            : this(false)
        {

        }

        public Base_StoreWeightService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_StoreWeightDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_StoreWeight_GameService:BaseService<Base_StoreWeight_Game>,IBase_StoreWeight_GameService
    {
		public override void SetDal()
        {

        }

        public Base_StoreWeight_GameService()
            : this(false)
        {

        }

        public Base_StoreWeight_GameService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_StoreWeight_GameDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_UserGrantService:BaseService<Base_UserGrant>,IBase_UserGrantService
    {
		public override void SetDal()
        {

        }

        public Base_UserGrantService()
            : this(false)
        {

        }

        public Base_UserGrantService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserGrantDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_UserGroupService:BaseService<Base_UserGroup>,IBase_UserGroupService
    {
		public override void SetDal()
        {

        }

        public Base_UserGroupService()
            : this(false)
        {

        }

        public Base_UserGroupService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserGroupDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_UserGroup_GrantService:BaseService<Base_UserGroup_Grant>,IBase_UserGroup_GrantService
    {
		public override void SetDal()
        {

        }

        public Base_UserGroup_GrantService()
            : this(false)
        {

        }

        public Base_UserGroup_GrantService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserGroup_GrantDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Base_UserInfoService:BaseService<Base_UserInfo>,IBase_UserInfoService
    {
		public override void SetDal()
        {

        }

        public Base_UserInfoService()
            : this(false)
        {

        }

        public Base_UserInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IBase_UserInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_BalanceChargeRuleService:BaseService<Data_BalanceChargeRule>,IData_BalanceChargeRuleService
    {
		public override void SetDal()
        {

        }

        public Data_BalanceChargeRuleService()
            : this(false)
        {

        }

        public Data_BalanceChargeRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_BalanceChargeRuleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_BalanceType_StoreListService:BaseService<Data_BalanceType_StoreList>,IData_BalanceType_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_BalanceType_StoreListService()
            : this(false)
        {

        }

        public Data_BalanceType_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_BalanceType_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_BillInfoService:BaseService<Data_BillInfo>,IData_BillInfoService
    {
		public override void SetDal()
        {

        }

        public Data_BillInfoService()
            : this(false)
        {

        }

        public Data_BillInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_BillInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Card_BalanceService:BaseService<Data_Card_Balance>,IData_Card_BalanceService
    {
		public override void SetDal()
        {

        }

        public Data_Card_BalanceService()
            : this(false)
        {

        }

        public Data_Card_BalanceService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Card_BalanceDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Card_Balance_StoreListService:BaseService<Data_Card_Balance_StoreList>,IData_Card_Balance_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_Card_Balance_StoreListService()
            : this(false)
        {

        }

        public Data_Card_Balance_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Card_Balance_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Card_RightService:BaseService<Data_Card_Right>,IData_Card_RightService
    {
		public override void SetDal()
        {

        }

        public Data_Card_RightService()
            : this(false)
        {

        }

        public Data_Card_RightService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Card_RightDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Card_Right_StoreListService:BaseService<Data_Card_Right_StoreList>,IData_Card_Right_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_Card_Right_StoreListService()
            : this(false)
        {

        }

        public Data_Card_Right_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Card_Right_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_CoinDestoryService:BaseService<Data_CoinDestory>,IData_CoinDestoryService
    {
		public override void SetDal()
        {

        }

        public Data_CoinDestoryService()
            : this(false)
        {

        }

        public Data_CoinDestoryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CoinDestoryDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_CoinInventoryService:BaseService<Data_CoinInventory>,IData_CoinInventoryService
    {
		public override void SetDal()
        {

        }

        public Data_CoinInventoryService()
            : this(false)
        {

        }

        public Data_CoinInventoryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CoinInventoryDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_CoinStorageService:BaseService<Data_CoinStorage>,IData_CoinStorageService
    {
		public override void SetDal()
        {

        }

        public Data_CoinStorageService()
            : this(false)
        {

        }

        public Data_CoinStorageService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CoinStorageDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Coupon_StoreListService:BaseService<Data_Coupon_StoreList>,IData_Coupon_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_Coupon_StoreListService()
            : this(false)
        {

        }

        public Data_Coupon_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Coupon_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_CouponInfoService:BaseService<Data_CouponInfo>,IData_CouponInfoService
    {
		public override void SetDal()
        {

        }

        public Data_CouponInfoService()
            : this(false)
        {

        }

        public Data_CouponInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CouponInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_CouponListService:BaseService<Data_CouponList>,IData_CouponListService
    {
		public override void SetDal()
        {

        }

        public Data_CouponListService()
            : this(false)
        {

        }

        public Data_CouponListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_CouponListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_DigitCoinService:BaseService<Data_DigitCoin>,IData_DigitCoinService
    {
		public override void SetDal()
        {

        }

        public Data_DigitCoinService()
            : this(false)
        {

        }

        public Data_DigitCoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_DigitCoinDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_DigitCoinDestroyService:BaseService<Data_DigitCoinDestroy>,IData_DigitCoinDestroyService
    {
		public override void SetDal()
        {

        }

        public Data_DigitCoinDestroyService()
            : this(false)
        {

        }

        public Data_DigitCoinDestroyService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_DigitCoinDestroyDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Food_DetialService:BaseService<Data_Food_Detial>,IData_Food_DetialService
    {
		public override void SetDal()
        {

        }

        public Data_Food_DetialService()
            : this(false)
        {

        }

        public Data_Food_DetialService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_DetialDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Food_LevelService:BaseService<Data_Food_Level>,IData_Food_LevelService
    {
		public override void SetDal()
        {

        }

        public Data_Food_LevelService()
            : this(false)
        {

        }

        public Data_Food_LevelService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_LevelDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Food_SaleService:BaseService<Data_Food_Sale>,IData_Food_SaleService
    {
		public override void SetDal()
        {

        }

        public Data_Food_SaleService()
            : this(false)
        {

        }

        public Data_Food_SaleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_SaleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Food_StoreListService:BaseService<Data_Food_StoreList>,IData_Food_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_Food_StoreListService()
            : this(false)
        {

        }

        public Data_Food_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Food_WorkStationService:BaseService<Data_Food_WorkStation>,IData_Food_WorkStationService
    {
		public override void SetDal()
        {

        }

        public Data_Food_WorkStationService()
            : this(false)
        {

        }

        public Data_Food_WorkStationService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Food_WorkStationDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_FoodInfoService:BaseService<Data_FoodInfo>,IData_FoodInfoService
    {
		public override void SetDal()
        {

        }

        public Data_FoodInfoService()
            : this(false)
        {

        }

        public Data_FoodInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_FoodInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Game_StockInfoService:BaseService<Data_Game_StockInfo>,IData_Game_StockInfoService
    {
		public override void SetDal()
        {

        }

        public Data_Game_StockInfoService()
            : this(false)
        {

        }

        public Data_Game_StockInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Game_StockInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GameFreeRuleService:BaseService<Data_GameFreeRule>,IData_GameFreeRuleService
    {
		public override void SetDal()
        {

        }

        public Data_GameFreeRuleService()
            : this(false)
        {

        }

        public Data_GameFreeRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameFreeRuleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GameFreeRule_ListService:BaseService<Data_GameFreeRule_List>,IData_GameFreeRule_ListService
    {
		public override void SetDal()
        {

        }

        public Data_GameFreeRule_ListService()
            : this(false)
        {

        }

        public Data_GameFreeRule_ListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameFreeRule_ListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GameInfoService:BaseService<Data_GameInfo>,IData_GameInfoService
    {
		public override void SetDal()
        {

        }

        public Data_GameInfoService()
            : this(false)
        {

        }

        public Data_GameInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GameInfo_ExtService:BaseService<Data_GameInfo_Ext>,IData_GameInfo_ExtService
    {
		public override void SetDal()
        {

        }

        public Data_GameInfo_ExtService()
            : this(false)
        {

        }

        public Data_GameInfo_ExtService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameInfo_ExtDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GameInfo_PhotoService:BaseService<Data_GameInfo_Photo>,IData_GameInfo_PhotoService
    {
		public override void SetDal()
        {

        }

        public Data_GameInfo_PhotoService()
            : this(false)
        {

        }

        public Data_GameInfo_PhotoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GameInfo_PhotoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GivebackRuleService:BaseService<Data_GivebackRule>,IData_GivebackRuleService
    {
		public override void SetDal()
        {

        }

        public Data_GivebackRuleService()
            : this(false)
        {

        }

        public Data_GivebackRuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GivebackRuleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GoodInventoryService:BaseService<Data_GoodInventory>,IData_GoodInventoryService
    {
		public override void SetDal()
        {

        }

        public Data_GoodInventoryService()
            : this(false)
        {

        }

        public Data_GoodInventoryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodInventoryDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GoodsStockService:BaseService<Data_GoodsStock>,IData_GoodsStockService
    {
		public override void SetDal()
        {

        }

        public Data_GoodsStockService()
            : this(false)
        {

        }

        public Data_GoodsStockService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodsStockDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GoodStock_RecordService:BaseService<Data_GoodStock_Record>,IData_GoodStock_RecordService
    {
		public override void SetDal()
        {

        }

        public Data_GoodStock_RecordService()
            : this(false)
        {

        }

        public Data_GoodStock_RecordService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodStock_RecordDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GoodStorageService:BaseService<Data_GoodStorage>,IData_GoodStorageService
    {
		public override void SetDal()
        {

        }

        public Data_GoodStorageService()
            : this(false)
        {

        }

        public Data_GoodStorageService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodStorageDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_GoodStorage_DetailService:BaseService<Data_GoodStorage_Detail>,IData_GoodStorage_DetailService
    {
		public override void SetDal()
        {

        }

        public Data_GoodStorage_DetailService()
            : this(false)
        {

        }

        public Data_GoodStorage_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_GoodStorage_DetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Jackpot_LevelService:BaseService<Data_Jackpot_Level>,IData_Jackpot_LevelService
    {
		public override void SetDal()
        {

        }

        public Data_Jackpot_LevelService()
            : this(false)
        {

        }

        public Data_Jackpot_LevelService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Jackpot_LevelDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Jackpot_MatrixService:BaseService<Data_Jackpot_Matrix>,IData_Jackpot_MatrixService
    {
		public override void SetDal()
        {

        }

        public Data_Jackpot_MatrixService()
            : this(false)
        {

        }

        public Data_Jackpot_MatrixService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Jackpot_MatrixDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_JackpotInfoService:BaseService<Data_JackpotInfo>,IData_JackpotInfoService
    {
		public override void SetDal()
        {

        }

        public Data_JackpotInfoService()
            : this(false)
        {

        }

        public Data_JackpotInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_JackpotInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_LotteryInventoryService:BaseService<Data_LotteryInventory>,IData_LotteryInventoryService
    {
		public override void SetDal()
        {

        }

        public Data_LotteryInventoryService()
            : this(false)
        {

        }

        public Data_LotteryInventoryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_LotteryInventoryDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_LotteryStorageService:BaseService<Data_LotteryStorage>,IData_LotteryStorageService
    {
		public override void SetDal()
        {

        }

        public Data_LotteryStorageService()
            : this(false)
        {

        }

        public Data_LotteryStorageService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_LotteryStorageDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Member_CardService:BaseService<Data_Member_Card>,IData_Member_CardService
    {
		public override void SetDal()
        {

        }

        public Data_Member_CardService()
            : this(false)
        {

        }

        public Data_Member_CardService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Member_CardDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Member_Card_StoreService:BaseService<Data_Member_Card_Store>,IData_Member_Card_StoreService
    {
		public override void SetDal()
        {

        }

        public Data_Member_Card_StoreService()
            : this(false)
        {

        }

        public Data_Member_Card_StoreService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Member_Card_StoreDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_MemberLevelService:BaseService<Data_MemberLevel>,IData_MemberLevelService
    {
		public override void SetDal()
        {

        }

        public Data_MemberLevelService()
            : this(false)
        {

        }

        public Data_MemberLevelService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevelDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_MemberLevel_FoodService:BaseService<Data_MemberLevel_Food>,IData_MemberLevel_FoodService
    {
		public override void SetDal()
        {

        }

        public Data_MemberLevel_FoodService()
            : this(false)
        {

        }

        public Data_MemberLevel_FoodService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevel_FoodDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_MemberLevelFreeService:BaseService<Data_MemberLevelFree>,IData_MemberLevelFreeService
    {
		public override void SetDal()
        {

        }

        public Data_MemberLevelFreeService()
            : this(false)
        {

        }

        public Data_MemberLevelFreeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MemberLevelFreeDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_MerchAlipay_ShopService:BaseService<Data_MerchAlipay_Shop>,IData_MerchAlipay_ShopService
    {
		public override void SetDal()
        {

        }

        public Data_MerchAlipay_ShopService()
            : this(false)
        {

        }

        public Data_MerchAlipay_ShopService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MerchAlipay_ShopDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_MessageService:BaseService<Data_Message>,IData_MessageService
    {
		public override void SetDal()
        {

        }

        public Data_MessageService()
            : this(false)
        {

        }

        public Data_MessageService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_MessageDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ParametersService:BaseService<Data_Parameters>,IData_ParametersService
    {
		public override void SetDal()
        {

        }

        public Data_ParametersService()
            : this(false)
        {

        }

        public Data_ParametersService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ParametersDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Project_BindGameService:BaseService<Data_Project_BindGame>,IData_Project_BindGameService
    {
		public override void SetDal()
        {

        }

        public Data_Project_BindGameService()
            : this(false)
        {

        }

        public Data_Project_BindGameService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Project_BindGameDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Project_DeviceService:BaseService<Data_Project_Device>,IData_Project_DeviceService
    {
		public override void SetDal()
        {

        }

        public Data_Project_DeviceService()
            : this(false)
        {

        }

        public Data_Project_DeviceService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Project_DeviceDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Project_StoreListService:BaseService<Data_Project_StoreList>,IData_Project_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_Project_StoreListService()
            : this(false)
        {

        }

        public Data_Project_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Project_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ProjectInfoService:BaseService<Data_ProjectInfo>,IData_ProjectInfoService
    {
		public override void SetDal()
        {

        }

        public Data_ProjectInfoService()
            : this(false)
        {

        }

        public Data_ProjectInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ProjectTime_BandPriceService:BaseService<Data_ProjectTime_BandPrice>,IData_ProjectTime_BandPriceService
    {
		public override void SetDal()
        {

        }

        public Data_ProjectTime_BandPriceService()
            : this(false)
        {

        }

        public Data_ProjectTime_BandPriceService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectTime_BandPriceDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ProjectTime_StoreListService:BaseService<Data_ProjectTime_StoreList>,IData_ProjectTime_StoreListService
    {
		public override void SetDal()
        {

        }

        public Data_ProjectTime_StoreListService()
            : this(false)
        {

        }

        public Data_ProjectTime_StoreListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectTime_StoreListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ProjectTimeInfoService:BaseService<Data_ProjectTimeInfo>,IData_ProjectTimeInfoService
    {
		public override void SetDal()
        {

        }

        public Data_ProjectTimeInfoService()
            : this(false)
        {

        }

        public Data_ProjectTimeInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ProjectTimeInfoDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Push_RuleService:BaseService<Data_Push_Rule>,IData_Push_RuleService
    {
		public override void SetDal()
        {

        }

        public Data_Push_RuleService()
            : this(false)
        {

        }

        public Data_Push_RuleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Push_RuleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_ReloadService:BaseService<Data_Reload>,IData_ReloadService
    {
		public override void SetDal()
        {

        }

        public Data_ReloadService()
            : this(false)
        {

        }

        public Data_ReloadService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_ReloadDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_Storage_RecordService:BaseService<Data_Storage_Record>,IData_Storage_RecordService
    {
		public override void SetDal()
        {

        }

        public Data_Storage_RecordService()
            : this(false)
        {

        }

        public Data_Storage_RecordService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_Storage_RecordDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_SupplierListService:BaseService<Data_SupplierList>,IData_SupplierListService
    {
		public override void SetDal()
        {

        }

        public Data_SupplierListService()
            : this(false)
        {

        }

        public Data_SupplierListService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_SupplierListDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Data_WorkstationService:BaseService<Data_Workstation>,IData_WorkstationService
    {
		public override void SetDal()
        {

        }

        public Data_WorkstationService()
            : this(false)
        {

        }

        public Data_WorkstationService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IData_WorkstationDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Dict_AreaService:BaseService<Dict_Area>,IDict_AreaService
    {
		public override void SetDal()
        {

        }

        public Dict_AreaService()
            : this(false)
        {

        }

        public Dict_AreaService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_AreaDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Dict_BalanceTypeService:BaseService<Dict_BalanceType>,IDict_BalanceTypeService
    {
		public override void SetDal()
        {

        }

        public Dict_BalanceTypeService()
            : this(false)
        {

        }

        public Dict_BalanceTypeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_BalanceTypeDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Dict_FunctionMenuService:BaseService<Dict_FunctionMenu>,IDict_FunctionMenuService
    {
		public override void SetDal()
        {

        }

        public Dict_FunctionMenuService()
            : this(false)
        {

        }

        public Dict_FunctionMenuService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_FunctionMenuDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Dict_SystemService:BaseService<Dict_System>,IDict_SystemService
    {
		public override void SetDal()
        {

        }

        public Dict_SystemService()
            : this(false)
        {

        }

        public Dict_SystemService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IDict_SystemDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_485_CoinService:BaseService<Flw_485_Coin>,IFlw_485_CoinService
    {
		public override void SetDal()
        {

        }

        public Flw_485_CoinService()
            : this(false)
        {

        }

        public Flw_485_CoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_485_CoinDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_485_SaveCoinService:BaseService<Flw_485_SaveCoin>,IFlw_485_SaveCoinService
    {
		public override void SetDal()
        {

        }

        public Flw_485_SaveCoinService()
            : this(false)
        {

        }

        public Flw_485_SaveCoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_485_SaveCoinDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_CheckDateService:BaseService<Flw_CheckDate>,IFlw_CheckDateService
    {
		public override void SetDal()
        {

        }

        public Flw_CheckDateService()
            : this(false)
        {

        }

        public Flw_CheckDateService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_CheckDateDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Coin_ExitService:BaseService<Flw_Coin_Exit>,IFlw_Coin_ExitService
    {
		public override void SetDal()
        {

        }

        public Flw_Coin_ExitService()
            : this(false)
        {

        }

        public Flw_Coin_ExitService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Coin_ExitDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Coin_SaleService:BaseService<Flw_Coin_Sale>,IFlw_Coin_SaleService
    {
		public override void SetDal()
        {

        }

        public Flw_Coin_SaleService()
            : this(false)
        {

        }

        public Flw_Coin_SaleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Coin_SaleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_CouponUseService:BaseService<Flw_CouponUse>,IFlw_CouponUseService
    {
		public override void SetDal()
        {

        }

        public Flw_CouponUseService()
            : this(false)
        {

        }

        public Flw_CouponUseService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_CouponUseDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Digite_CoinService:BaseService<Flw_Digite_Coin>,IFlw_Digite_CoinService
    {
		public override void SetDal()
        {

        }

        public Flw_Digite_CoinService()
            : this(false)
        {

        }

        public Flw_Digite_CoinService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Digite_CoinDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Digite_Coin_DetailService:BaseService<Flw_Digite_Coin_Detail>,IFlw_Digite_Coin_DetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Digite_Coin_DetailService()
            : this(false)
        {

        }

        public Flw_Digite_Coin_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Digite_Coin_DetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Food_ExitService:BaseService<Flw_Food_Exit>,IFlw_Food_ExitService
    {
		public override void SetDal()
        {

        }

        public Flw_Food_ExitService()
            : this(false)
        {

        }

        public Flw_Food_ExitService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Food_ExitDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Food_ExitDetailService:BaseService<Flw_Food_ExitDetail>,IFlw_Food_ExitDetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Food_ExitDetailService()
            : this(false)
        {

        }

        public Flw_Food_ExitDetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Food_ExitDetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Food_SaleService:BaseService<Flw_Food_Sale>,IFlw_Food_SaleService
    {
		public override void SetDal()
        {

        }

        public Flw_Food_SaleService()
            : this(false)
        {

        }

        public Flw_Food_SaleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Food_SaleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Food_SaleDetailService:BaseService<Flw_Food_SaleDetail>,IFlw_Food_SaleDetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Food_SaleDetailService()
            : this(false)
        {

        }

        public Flw_Food_SaleDetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Food_SaleDetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Game_FreeService:BaseService<Flw_Game_Free>,IFlw_Game_FreeService
    {
		public override void SetDal()
        {

        }

        public Flw_Game_FreeService()
            : this(false)
        {

        }

        public Flw_Game_FreeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Game_FreeDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Game_WatchService:BaseService<Flw_Game_Watch>,IFlw_Game_WatchService
    {
		public override void SetDal()
        {

        }

        public Flw_Game_WatchService()
            : this(false)
        {

        }

        public Flw_Game_WatchService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Game_WatchDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Game_WinPrizeService:BaseService<Flw_Game_WinPrize>,IFlw_Game_WinPrizeService
    {
		public override void SetDal()
        {

        }

        public Flw_Game_WinPrizeService()
            : this(false)
        {

        }

        public Flw_Game_WinPrizeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Game_WinPrizeDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_GivebackService:BaseService<Flw_Giveback>,IFlw_GivebackService
    {
		public override void SetDal()
        {

        }

        public Flw_GivebackService()
            : this(false)
        {

        }

        public Flw_GivebackService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_GivebackDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Good_DetailService:BaseService<Flw_Good_Detail>,IFlw_Good_DetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Good_DetailService()
            : this(false)
        {

        }

        public Flw_Good_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Good_DetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_GoodsService:BaseService<Flw_Goods>,IFlw_GoodsService
    {
		public override void SetDal()
        {

        }

        public Flw_GoodsService()
            : this(false)
        {

        }

        public Flw_GoodsService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_GoodsDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_GroupVerityService:BaseService<Flw_GroupVerity>,IFlw_GroupVerityService
    {
		public override void SetDal()
        {

        }

        public Flw_GroupVerityService()
            : this(false)
        {

        }

        public Flw_GroupVerityService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_GroupVerityDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_JackpotService:BaseService<Flw_Jackpot>,IFlw_JackpotService
    {
		public override void SetDal()
        {

        }

        public Flw_JackpotService()
            : this(false)
        {

        }

        public Flw_JackpotService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_JackpotDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_LotteryService:BaseService<Flw_Lottery>,IFlw_LotteryService
    {
		public override void SetDal()
        {

        }

        public Flw_LotteryService()
            : this(false)
        {

        }

        public Flw_LotteryService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_LotteryDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_OrderService:BaseService<Flw_Order>,IFlw_OrderService
    {
		public override void SetDal()
        {

        }

        public Flw_OrderService()
            : this(false)
        {

        }

        public Flw_OrderService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_OrderDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Order_DetailService:BaseService<Flw_Order_Detail>,IFlw_Order_DetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Order_DetailService()
            : this(false)
        {

        }

        public Flw_Order_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Order_DetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Order_SerialNumberService:BaseService<Flw_Order_SerialNumber>,IFlw_Order_SerialNumberService
    {
		public override void SetDal()
        {

        }

        public Flw_Order_SerialNumberService()
            : this(false)
        {

        }

        public Flw_Order_SerialNumberService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Order_SerialNumberDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Project_BuyDetailService:BaseService<Flw_Project_BuyDetail>,IFlw_Project_BuyDetailService
    {
		public override void SetDal()
        {

        }

        public Flw_Project_BuyDetailService()
            : this(false)
        {

        }

        public Flw_Project_BuyDetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Project_BuyDetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Project_Play_TimeService:BaseService<Flw_Project_Play_Time>,IFlw_Project_Play_TimeService
    {
		public override void SetDal()
        {

        }

        public Flw_Project_Play_TimeService()
            : this(false)
        {

        }

        public Flw_Project_Play_TimeService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Project_Play_TimeDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_ScheduleService:BaseService<Flw_Schedule>,IFlw_ScheduleService
    {
		public override void SetDal()
        {

        }

        public Flw_ScheduleService()
            : this(false)
        {

        }

        public Flw_ScheduleService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_ScheduleDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_Ticket_ExitService:BaseService<Flw_Ticket_Exit>,IFlw_Ticket_ExitService
    {
		public override void SetDal()
        {

        }

        public Flw_Ticket_ExitService()
            : this(false)
        {

        }

        public Flw_Ticket_ExitService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_Ticket_ExitDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Flw_TransferService:BaseService<Flw_Transfer>,IFlw_TransferService
    {
		public override void SetDal()
        {

        }

        public Flw_TransferService()
            : this(false)
        {

        }

        public Flw_TransferService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IFlw_TransferDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Log_GameAlarmService:BaseService<Log_GameAlarm>,ILog_GameAlarmService
    {
		public override void SetDal()
        {

        }

        public Log_GameAlarmService()
            : this(false)
        {

        }

        public Log_GameAlarmService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ILog_GameAlarmDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Log_OperationService:BaseService<Log_Operation>,ILog_OperationService
    {
		public override void SetDal()
        {

        }

        public Log_OperationService()
            : this(false)
        {

        }

        public Log_OperationService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ILog_OperationDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Search_TemplateService:BaseService<Search_Template>,ISearch_TemplateService
    {
		public override void SetDal()
        {

        }

        public Search_TemplateService()
            : this(false)
        {

        }

        public Search_TemplateService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ISearch_TemplateDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Search_Template_DetailService:BaseService<Search_Template_Detail>,ISearch_Template_DetailService
    {
		public override void SetDal()
        {

        }

        public Search_Template_DetailService()
            : this(false)
        {

        }

        public Search_Template_DetailService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ISearch_Template_DetailDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Store_CheckDateService:BaseService<Store_CheckDate>,IStore_CheckDateService
    {
		public override void SetDal()
        {

        }

        public Store_CheckDateService()
            : this(false)
        {

        }

        public Store_CheckDateService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IStore_CheckDateDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Store_GameTotalService:BaseService<Store_GameTotal>,IStore_GameTotalService
    {
		public override void SetDal()
        {

        }

        public Store_GameTotalService()
            : this(false)
        {

        }

        public Store_GameTotalService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IStore_GameTotalDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class Store_HeadTotalService:BaseService<Store_HeadTotal>,IStore_HeadTotalService
    {
		public override void SetDal()
        {

        }

        public Store_HeadTotalService()
            : this(false)
        {

        }

        public Store_HeadTotalService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IStore_HeadTotalDAL>(resolveNew: resolveNew);
        }
    }
    
    public partial class XC_WorkInfoService:BaseService<XC_WorkInfo>,IXC_WorkInfoService
    {
		public override void SetDal()
        {

        }

        public XC_WorkInfoService()
            : this(false)
        {

        }

        public XC_WorkInfoService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<IXC_WorkInfoDAL>(resolveNew: resolveNew);
        }
    }
}