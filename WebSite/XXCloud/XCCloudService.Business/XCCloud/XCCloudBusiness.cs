 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.Container;
using XCCloudService.BLL.IBLL.XCCloud;

namespace XCCloudService.Business.XCCloud
{  
    
    public partial class Base_ChainRuleBusiness
    {
		public static IBase_ChainRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_ChainRuleService>();
            }
        }

        public static IBase_ChainRuleService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_ChainRuleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_ChainRule_StoreBusiness
    {
		public static IBase_ChainRule_StoreService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_ChainRule_StoreService>();
            }
        }

        public static IBase_ChainRule_StoreService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_ChainRule_StoreService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_DepotInfoBusiness
    {
		public static IBase_DepotInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_DepotInfoService>();
            }
        }

        public static IBase_DepotInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_DepotInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_DeviceInfoBusiness
    {
		public static IBase_DeviceInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_DeviceInfoService>();
            }
        }

        public static IBase_DeviceInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_DeviceInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_EnumParamsBusiness
    {
		public static IBase_EnumParamsService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_EnumParamsService>();
            }
        }

        public static IBase_EnumParamsService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_EnumParamsService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_GoodsInfoBusiness
    {
		public static IBase_GoodsInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_GoodsInfoService>();
            }
        }

        public static IBase_GoodsInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_GoodsInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_MemberInfoBusiness
    {
		public static IBase_MemberInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_MemberInfoService>();
            }
        }

        public static IBase_MemberInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_MemberInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_MerchAlipayBusiness
    {
		public static IBase_MerchAlipayService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchAlipayService>();
            }
        }

        public static IBase_MerchAlipayService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchAlipayService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_MerchantInfoBusiness
    {
		public static IBase_MerchantInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>();
            }
        }

        public static IBase_MerchantInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchantInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_MerchFunctionBusiness
    {
		public static IBase_MerchFunctionService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchFunctionService>();
            }
        }

        public static IBase_MerchFunctionService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_MerchFunctionService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_SettleLCPayBusiness
    {
		public static IBase_SettleLCPayService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettleLCPayService>();
            }
        }

        public static IBase_SettleLCPayService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettleLCPayService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_SettleOrgBusiness
    {
		public static IBase_SettleOrgService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettleOrgService>();
            }
        }

        public static IBase_SettleOrgService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettleOrgService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_SettlePPOSBusiness
    {
		public static IBase_SettlePPOSService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettlePPOSService>();
            }
        }

        public static IBase_SettlePPOSService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_SettlePPOSService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_StorageInfoBusiness
    {
		public static IBase_StorageInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_StorageInfoService>();
            }
        }

        public static IBase_StorageInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_StorageInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_StoreDogListBusiness
    {
		public static IBase_StoreDogListService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreDogListService>();
            }
        }

        public static IBase_StoreDogListService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreDogListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_StoreInfoBusiness
    {
		public static IBase_StoreInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreInfoService>();
            }
        }

        public static IBase_StoreInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_StoreWeightBusiness
    {
		public static IBase_StoreWeightService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreWeightService>();
            }
        }

        public static IBase_StoreWeightService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreWeightService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_StoreWeight_GameBusiness
    {
		public static IBase_StoreWeight_GameService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreWeight_GameService>();
            }
        }

        public static IBase_StoreWeight_GameService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_StoreWeight_GameService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_UserGrantBusiness
    {
		public static IBase_UserGrantService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGrantService>();
            }
        }

        public static IBase_UserGrantService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGrantService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_UserGroupBusiness
    {
		public static IBase_UserGroupService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroupService>();
            }
        }

        public static IBase_UserGroupService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroupService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_UserGroup_GrantBusiness
    {
		public static IBase_UserGroup_GrantService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>();
            }
        }

        public static IBase_UserGroup_GrantService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserGroup_GrantService>(resolveNew: true);
            }
        }
    }
    
    public partial class Base_UserInfoBusiness
    {
		public static IBase_UserInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserInfoService>();
            }
        }

        public static IBase_UserInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IBase_UserInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_BalanceChargeRuleBusiness
    {
		public static IData_BalanceChargeRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_BalanceChargeRuleService>();
            }
        }

        public static IData_BalanceChargeRuleService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_BalanceChargeRuleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_BalanceType_StoreListBusiness
    {
		public static IData_BalanceType_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_BalanceType_StoreListService>();
            }
        }

        public static IData_BalanceType_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_BalanceType_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_BillInfoBusiness
    {
		public static IData_BillInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_BillInfoService>();
            }
        }

        public static IData_BillInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_BillInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Card_BalanceBusiness
    {
		public static IData_Card_BalanceService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_BalanceService>();
            }
        }

        public static IData_Card_BalanceService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_BalanceService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Card_Balance_StoreListBusiness
    {
		public static IData_Card_Balance_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_Balance_StoreListService>();
            }
        }

        public static IData_Card_Balance_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_Balance_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Card_RightBusiness
    {
		public static IData_Card_RightService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_RightService>();
            }
        }

        public static IData_Card_RightService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_RightService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Card_Right_StoreListBusiness
    {
		public static IData_Card_Right_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_Right_StoreListService>();
            }
        }

        public static IData_Card_Right_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Card_Right_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_CoinDestoryBusiness
    {
		public static IData_CoinDestoryService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinDestoryService>();
            }
        }

        public static IData_CoinDestoryService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinDestoryService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_CoinInventoryBusiness
    {
		public static IData_CoinInventoryService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinInventoryService>();
            }
        }

        public static IData_CoinInventoryService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinInventoryService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_CoinStorageBusiness
    {
		public static IData_CoinStorageService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinStorageService>();
            }
        }

        public static IData_CoinStorageService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_CoinStorageService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Coupon_StoreListBusiness
    {
		public static IData_Coupon_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>();
            }
        }

        public static IData_Coupon_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Coupon_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_CouponInfoBusiness
    {
		public static IData_CouponInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponInfoService>();
            }
        }

        public static IData_CouponInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_CouponListBusiness
    {
		public static IData_CouponListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponListService>();
            }
        }

        public static IData_CouponListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_CouponListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_DigitCoinBusiness
    {
		public static IData_DigitCoinService I
        {
            get
            {
                return BLLContainer.Resolve<IData_DigitCoinService>();
            }
        }

        public static IData_DigitCoinService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_DigitCoinService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_DigitCoinDestroyBusiness
    {
		public static IData_DigitCoinDestroyService I
        {
            get
            {
                return BLLContainer.Resolve<IData_DigitCoinDestroyService>();
            }
        }

        public static IData_DigitCoinDestroyService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_DigitCoinDestroyService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Food_DetialBusiness
    {
		public static IData_Food_DetialService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_DetialService>();
            }
        }

        public static IData_Food_DetialService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_DetialService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Food_LevelBusiness
    {
		public static IData_Food_LevelService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_LevelService>();
            }
        }

        public static IData_Food_LevelService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_LevelService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Food_SaleBusiness
    {
		public static IData_Food_SaleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_SaleService>();
            }
        }

        public static IData_Food_SaleService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_SaleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Food_StoreListBusiness
    {
		public static IData_Food_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>();
            }
        }

        public static IData_Food_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Food_WorkStationBusiness
    {
		public static IData_Food_WorkStationService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_WorkStationService>();
            }
        }

        public static IData_Food_WorkStationService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Food_WorkStationService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_FoodInfoBusiness
    {
		public static IData_FoodInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_FoodInfoService>();
            }
        }

        public static IData_FoodInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_FoodInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Game_StockInfoBusiness
    {
		public static IData_Game_StockInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Game_StockInfoService>();
            }
        }

        public static IData_Game_StockInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Game_StockInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GameFreeRuleBusiness
    {
		public static IData_GameFreeRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameFreeRuleService>();
            }
        }

        public static IData_GameFreeRuleService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GameFreeRuleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GameFreeRule_ListBusiness
    {
		public static IData_GameFreeRule_ListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameFreeRule_ListService>();
            }
        }

        public static IData_GameFreeRule_ListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GameFreeRule_ListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GameInfoBusiness
    {
		public static IData_GameInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfoService>();
            }
        }

        public static IData_GameInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GameInfo_ExtBusiness
    {
		public static IData_GameInfo_ExtService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfo_ExtService>();
            }
        }

        public static IData_GameInfo_ExtService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfo_ExtService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GameInfo_PhotoBusiness
    {
		public static IData_GameInfo_PhotoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfo_PhotoService>();
            }
        }

        public static IData_GameInfo_PhotoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GameInfo_PhotoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GivebackRuleBusiness
    {
		public static IData_GivebackRuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GivebackRuleService>();
            }
        }

        public static IData_GivebackRuleService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GivebackRuleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GoodInventoryBusiness
    {
		public static IData_GoodInventoryService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>();
            }
        }

        public static IData_GoodInventoryService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodInventoryService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GoodsStockBusiness
    {
		public static IData_GoodsStockService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodsStockService>();
            }
        }

        public static IData_GoodsStockService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodsStockService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GoodStock_RecordBusiness
    {
		public static IData_GoodStock_RecordService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStock_RecordService>();
            }
        }

        public static IData_GoodStock_RecordService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStock_RecordService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GoodStorageBusiness
    {
		public static IData_GoodStorageService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStorageService>();
            }
        }

        public static IData_GoodStorageService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStorageService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_GoodStorage_DetailBusiness
    {
		public static IData_GoodStorage_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStorage_DetailService>();
            }
        }

        public static IData_GoodStorage_DetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_GoodStorage_DetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Jackpot_LevelBusiness
    {
		public static IData_Jackpot_LevelService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Jackpot_LevelService>();
            }
        }

        public static IData_Jackpot_LevelService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Jackpot_LevelService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Jackpot_MatrixBusiness
    {
		public static IData_Jackpot_MatrixService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Jackpot_MatrixService>();
            }
        }

        public static IData_Jackpot_MatrixService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Jackpot_MatrixService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_JackpotInfoBusiness
    {
		public static IData_JackpotInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_JackpotInfoService>();
            }
        }

        public static IData_JackpotInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_JackpotInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_LotteryInventoryBusiness
    {
		public static IData_LotteryInventoryService I
        {
            get
            {
                return BLLContainer.Resolve<IData_LotteryInventoryService>();
            }
        }

        public static IData_LotteryInventoryService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_LotteryInventoryService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_LotteryStorageBusiness
    {
		public static IData_LotteryStorageService I
        {
            get
            {
                return BLLContainer.Resolve<IData_LotteryStorageService>();
            }
        }

        public static IData_LotteryStorageService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_LotteryStorageService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Member_CardBusiness
    {
		public static IData_Member_CardService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Member_CardService>();
            }
        }

        public static IData_Member_CardService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Member_CardService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Member_Card_StoreBusiness
    {
		public static IData_Member_Card_StoreService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Member_Card_StoreService>();
            }
        }

        public static IData_Member_Card_StoreService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Member_Card_StoreService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_MemberLevelBusiness
    {
		public static IData_MemberLevelService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevelService>();
            }
        }

        public static IData_MemberLevelService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevelService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_MemberLevel_FoodBusiness
    {
		public static IData_MemberLevel_FoodService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevel_FoodService>();
            }
        }

        public static IData_MemberLevel_FoodService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevel_FoodService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_MemberLevelFreeBusiness
    {
		public static IData_MemberLevelFreeService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevelFreeService>();
            }
        }

        public static IData_MemberLevelFreeService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_MemberLevelFreeService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_MerchAlipay_ShopBusiness
    {
		public static IData_MerchAlipay_ShopService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MerchAlipay_ShopService>();
            }
        }

        public static IData_MerchAlipay_ShopService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_MerchAlipay_ShopService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_MessageBusiness
    {
		public static IData_MessageService I
        {
            get
            {
                return BLLContainer.Resolve<IData_MessageService>();
            }
        }

        public static IData_MessageService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_MessageService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ParametersBusiness
    {
		public static IData_ParametersService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ParametersService>();
            }
        }

        public static IData_ParametersService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ParametersService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Project_BindGameBusiness
    {
		public static IData_Project_BindGameService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_BindGameService>();
            }
        }

        public static IData_Project_BindGameService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_BindGameService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Project_DeviceBusiness
    {
		public static IData_Project_DeviceService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_DeviceService>();
            }
        }

        public static IData_Project_DeviceService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_DeviceService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Project_StoreListBusiness
    {
		public static IData_Project_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_StoreListService>();
            }
        }

        public static IData_Project_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Project_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ProjectInfoBusiness
    {
		public static IData_ProjectInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectInfoService>();
            }
        }

        public static IData_ProjectInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ProjectTime_BandPriceBusiness
    {
		public static IData_ProjectTime_BandPriceService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTime_BandPriceService>();
            }
        }

        public static IData_ProjectTime_BandPriceService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTime_BandPriceService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ProjectTime_StoreListBusiness
    {
		public static IData_ProjectTime_StoreListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTime_StoreListService>();
            }
        }

        public static IData_ProjectTime_StoreListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTime_StoreListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ProjectTimeInfoBusiness
    {
		public static IData_ProjectTimeInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTimeInfoService>();
            }
        }

        public static IData_ProjectTimeInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ProjectTimeInfoService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Push_RuleBusiness
    {
		public static IData_Push_RuleService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Push_RuleService>();
            }
        }

        public static IData_Push_RuleService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Push_RuleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_ReloadBusiness
    {
		public static IData_ReloadService I
        {
            get
            {
                return BLLContainer.Resolve<IData_ReloadService>();
            }
        }

        public static IData_ReloadService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_ReloadService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_Storage_RecordBusiness
    {
		public static IData_Storage_RecordService I
        {
            get
            {
                return BLLContainer.Resolve<IData_Storage_RecordService>();
            }
        }

        public static IData_Storage_RecordService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_Storage_RecordService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_SupplierListBusiness
    {
		public static IData_SupplierListService I
        {
            get
            {
                return BLLContainer.Resolve<IData_SupplierListService>();
            }
        }

        public static IData_SupplierListService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_SupplierListService>(resolveNew: true);
            }
        }
    }
    
    public partial class Data_WorkstationBusiness
    {
		public static IData_WorkstationService I
        {
            get
            {
                return BLLContainer.Resolve<IData_WorkstationService>();
            }
        }

        public static IData_WorkstationService NI
        {
            get
            {
                return BLLContainer.Resolve<IData_WorkstationService>(resolveNew: true);
            }
        }
    }
    
    public partial class Dict_AreaBusiness
    {
		public static IDict_AreaService I
        {
            get
            {
                return BLLContainer.Resolve<IDict_AreaService>();
            }
        }

        public static IDict_AreaService NI
        {
            get
            {
                return BLLContainer.Resolve<IDict_AreaService>(resolveNew: true);
            }
        }
    }
    
    public partial class Dict_BalanceTypeBusiness
    {
		public static IDict_BalanceTypeService I
        {
            get
            {
                return BLLContainer.Resolve<IDict_BalanceTypeService>();
            }
        }

        public static IDict_BalanceTypeService NI
        {
            get
            {
                return BLLContainer.Resolve<IDict_BalanceTypeService>(resolveNew: true);
            }
        }
    }
    
    public partial class Dict_FunctionMenuBusiness
    {
		public static IDict_FunctionMenuService I
        {
            get
            {
                return BLLContainer.Resolve<IDict_FunctionMenuService>();
            }
        }

        public static IDict_FunctionMenuService NI
        {
            get
            {
                return BLLContainer.Resolve<IDict_FunctionMenuService>(resolveNew: true);
            }
        }
    }
    
    public partial class Dict_SystemBusiness
    {
		public static IDict_SystemService I
        {
            get
            {
                return BLLContainer.Resolve<IDict_SystemService>();
            }
        }

        public static IDict_SystemService NI
        {
            get
            {
                return BLLContainer.Resolve<IDict_SystemService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_485_CoinBusiness
    {
		public static IFlw_485_CoinService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_485_CoinService>();
            }
        }

        public static IFlw_485_CoinService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_485_CoinService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_485_SaveCoinBusiness
    {
		public static IFlw_485_SaveCoinService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_485_SaveCoinService>();
            }
        }

        public static IFlw_485_SaveCoinService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_485_SaveCoinService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_CheckDateBusiness
    {
		public static IFlw_CheckDateService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_CheckDateService>();
            }
        }

        public static IFlw_CheckDateService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_CheckDateService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Coin_ExitBusiness
    {
		public static IFlw_Coin_ExitService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Coin_ExitService>();
            }
        }

        public static IFlw_Coin_ExitService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Coin_ExitService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Coin_SaleBusiness
    {
		public static IFlw_Coin_SaleService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Coin_SaleService>();
            }
        }

        public static IFlw_Coin_SaleService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Coin_SaleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_CouponUseBusiness
    {
		public static IFlw_CouponUseService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_CouponUseService>();
            }
        }

        public static IFlw_CouponUseService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_CouponUseService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Digite_CoinBusiness
    {
		public static IFlw_Digite_CoinService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_CoinService>();
            }
        }

        public static IFlw_Digite_CoinService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_CoinService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Digite_Coin_DetailBusiness
    {
		public static IFlw_Digite_Coin_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_Coin_DetailService>();
            }
        }

        public static IFlw_Digite_Coin_DetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Digite_Coin_DetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Food_ExitBusiness
    {
		public static IFlw_Food_ExitService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_ExitService>();
            }
        }

        public static IFlw_Food_ExitService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_ExitService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Food_ExitDetailBusiness
    {
		public static IFlw_Food_ExitDetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_ExitDetailService>();
            }
        }

        public static IFlw_Food_ExitDetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_ExitDetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Food_SaleBusiness
    {
		public static IFlw_Food_SaleService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleService>();
            }
        }

        public static IFlw_Food_SaleService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Food_SaleDetailBusiness
    {
		public static IFlw_Food_SaleDetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleDetailService>();
            }
        }

        public static IFlw_Food_SaleDetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Food_SaleDetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Game_FreeBusiness
    {
		public static IFlw_Game_FreeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_FreeService>();
            }
        }

        public static IFlw_Game_FreeService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_FreeService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Game_WatchBusiness
    {
		public static IFlw_Game_WatchService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>();
            }
        }

        public static IFlw_Game_WatchService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WatchService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Game_WinPrizeBusiness
    {
		public static IFlw_Game_WinPrizeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WinPrizeService>();
            }
        }

        public static IFlw_Game_WinPrizeService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Game_WinPrizeService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_GivebackBusiness
    {
		public static IFlw_GivebackService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GivebackService>();
            }
        }

        public static IFlw_GivebackService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GivebackService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Good_DetailBusiness
    {
		public static IFlw_Good_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Good_DetailService>();
            }
        }

        public static IFlw_Good_DetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Good_DetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_GoodsBusiness
    {
		public static IFlw_GoodsService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GoodsService>();
            }
        }

        public static IFlw_GoodsService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GoodsService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_GroupVerityBusiness
    {
		public static IFlw_GroupVerityService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GroupVerityService>();
            }
        }

        public static IFlw_GroupVerityService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_GroupVerityService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_JackpotBusiness
    {
		public static IFlw_JackpotService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_JackpotService>();
            }
        }

        public static IFlw_JackpotService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_JackpotService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_LotteryBusiness
    {
		public static IFlw_LotteryService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_LotteryService>();
            }
        }

        public static IFlw_LotteryService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_LotteryService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_OrderBusiness
    {
		public static IFlw_OrderService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_OrderService>();
            }
        }

        public static IFlw_OrderService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_OrderService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Order_DetailBusiness
    {
		public static IFlw_Order_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>();
            }
        }

        public static IFlw_Order_DetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_DetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Order_SerialNumberBusiness
    {
		public static IFlw_Order_SerialNumberService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>();
            }
        }

        public static IFlw_Order_SerialNumberService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Order_SerialNumberService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Project_BuyDetailBusiness
    {
		public static IFlw_Project_BuyDetailService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_BuyDetailService>();
            }
        }

        public static IFlw_Project_BuyDetailService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_BuyDetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Project_Play_TimeBusiness
    {
		public static IFlw_Project_Play_TimeService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_Play_TimeService>();
            }
        }

        public static IFlw_Project_Play_TimeService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Project_Play_TimeService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_ScheduleBusiness
    {
		public static IFlw_ScheduleService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_ScheduleService>();
            }
        }

        public static IFlw_ScheduleService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_ScheduleService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_Ticket_ExitBusiness
    {
		public static IFlw_Ticket_ExitService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Ticket_ExitService>();
            }
        }

        public static IFlw_Ticket_ExitService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_Ticket_ExitService>(resolveNew: true);
            }
        }
    }
    
    public partial class Flw_TransferBusiness
    {
		public static IFlw_TransferService I
        {
            get
            {
                return BLLContainer.Resolve<IFlw_TransferService>();
            }
        }

        public static IFlw_TransferService NI
        {
            get
            {
                return BLLContainer.Resolve<IFlw_TransferService>(resolveNew: true);
            }
        }
    }
    
    public partial class Log_GameAlarmBusiness
    {
		public static ILog_GameAlarmService I
        {
            get
            {
                return BLLContainer.Resolve<ILog_GameAlarmService>();
            }
        }

        public static ILog_GameAlarmService NI
        {
            get
            {
                return BLLContainer.Resolve<ILog_GameAlarmService>(resolveNew: true);
            }
        }
    }
    
    public partial class Log_OperationBusiness
    {
		public static ILog_OperationService I
        {
            get
            {
                return BLLContainer.Resolve<ILog_OperationService>();
            }
        }

        public static ILog_OperationService NI
        {
            get
            {
                return BLLContainer.Resolve<ILog_OperationService>(resolveNew: true);
            }
        }
    }
    
    public partial class Search_TemplateBusiness
    {
		public static ISearch_TemplateService I
        {
            get
            {
                return BLLContainer.Resolve<ISearch_TemplateService>();
            }
        }

        public static ISearch_TemplateService NI
        {
            get
            {
                return BLLContainer.Resolve<ISearch_TemplateService>(resolveNew: true);
            }
        }
    }
    
    public partial class Search_Template_DetailBusiness
    {
		public static ISearch_Template_DetailService I
        {
            get
            {
                return BLLContainer.Resolve<ISearch_Template_DetailService>();
            }
        }

        public static ISearch_Template_DetailService NI
        {
            get
            {
                return BLLContainer.Resolve<ISearch_Template_DetailService>(resolveNew: true);
            }
        }
    }
    
    public partial class Store_CheckDateBusiness
    {
		public static IStore_CheckDateService I
        {
            get
            {
                return BLLContainer.Resolve<IStore_CheckDateService>();
            }
        }

        public static IStore_CheckDateService NI
        {
            get
            {
                return BLLContainer.Resolve<IStore_CheckDateService>(resolveNew: true);
            }
        }
    }
    
    public partial class Store_GameTotalBusiness
    {
		public static IStore_GameTotalService I
        {
            get
            {
                return BLLContainer.Resolve<IStore_GameTotalService>();
            }
        }

        public static IStore_GameTotalService NI
        {
            get
            {
                return BLLContainer.Resolve<IStore_GameTotalService>(resolveNew: true);
            }
        }
    }
    
    public partial class Store_HeadTotalBusiness
    {
		public static IStore_HeadTotalService I
        {
            get
            {
                return BLLContainer.Resolve<IStore_HeadTotalService>();
            }
        }

        public static IStore_HeadTotalService NI
        {
            get
            {
                return BLLContainer.Resolve<IStore_HeadTotalService>(resolveNew: true);
            }
        }
    }
    
    public partial class XC_WorkInfoBusiness
    {
		public static IXC_WorkInfoService I
        {
            get
            {
                return BLLContainer.Resolve<IXC_WorkInfoService>();
            }
        }

        public static IXC_WorkInfoService NI
        {
            get
            {
                return BLLContainer.Resolve<IXC_WorkInfoService>(resolveNew: true);
            }
        }
    }
}