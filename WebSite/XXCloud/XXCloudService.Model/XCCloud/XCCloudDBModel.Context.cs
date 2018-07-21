﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace XCCloudService.Model.XCCloud
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class XCCloudDBEntities : DbContext
    {
        public XCCloudDBEntities()
            : base("name=XCCloudDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Base_ChainRule> Base_ChainRule { get; set; }
        public virtual DbSet<Base_ChainRule_Store> Base_ChainRule_Store { get; set; }
        public virtual DbSet<Base_DepotInfo> Base_DepotInfo { get; set; }
        public virtual DbSet<Base_DeviceInfo> Base_DeviceInfo { get; set; }
        public virtual DbSet<Base_DeviceInfo_Ext> Base_DeviceInfo_Ext { get; set; }
        public virtual DbSet<Base_EnumParams> Base_EnumParams { get; set; }
        public virtual DbSet<Base_Goodinfo_Price> Base_Goodinfo_Price { get; set; }
        public virtual DbSet<Base_GoodsInfo> Base_GoodsInfo { get; set; }
        public virtual DbSet<Base_MemberInfo> Base_MemberInfo { get; set; }
        public virtual DbSet<Base_MerchAlipay> Base_MerchAlipay { get; set; }
        public virtual DbSet<Base_MerchantInfo> Base_MerchantInfo { get; set; }
        public virtual DbSet<Base_MerchFunction> Base_MerchFunction { get; set; }
        public virtual DbSet<Base_SettleLCPay> Base_SettleLCPay { get; set; }
        public virtual DbSet<Base_SettleOrg> Base_SettleOrg { get; set; }
        public virtual DbSet<Base_SettlePPOS> Base_SettlePPOS { get; set; }
        public virtual DbSet<Base_StoreDogList> Base_StoreDogList { get; set; }
        public virtual DbSet<Base_StoreHKConfig> Base_StoreHKConfig { get; set; }
        public virtual DbSet<Base_StoreInfo> Base_StoreInfo { get; set; }
        public virtual DbSet<Base_StoreWeight> Base_StoreWeight { get; set; }
        public virtual DbSet<Base_StoreWeight_Game> Base_StoreWeight_Game { get; set; }
        public virtual DbSet<Base_UserGrant> Base_UserGrant { get; set; }
        public virtual DbSet<Base_UserGroup> Base_UserGroup { get; set; }
        public virtual DbSet<Base_UserGroup_Grant> Base_UserGroup_Grant { get; set; }
        public virtual DbSet<Base_UserInfo> Base_UserInfo { get; set; }
        public virtual DbSet<Base_WechatFunction> Base_WechatFunction { get; set; }
        public virtual DbSet<Data_BalanceChargeRule> Data_BalanceChargeRule { get; set; }
        public virtual DbSet<Data_BalanceType_StoreList> Data_BalanceType_StoreList { get; set; }
        public virtual DbSet<Data_BillInfo> Data_BillInfo { get; set; }
        public virtual DbSet<Data_Card_Balance> Data_Card_Balance { get; set; }
        public virtual DbSet<Data_Card_Balance_Free> Data_Card_Balance_Free { get; set; }
        public virtual DbSet<Data_Card_Balance_StoreList> Data_Card_Balance_StoreList { get; set; }
        public virtual DbSet<Data_Card_Right> Data_Card_Right { get; set; }
        public virtual DbSet<Data_Card_Right_StoreList> Data_Card_Right_StoreList { get; set; }
        public virtual DbSet<Data_CoinDestory> Data_CoinDestory { get; set; }
        public virtual DbSet<Data_CoinInventory> Data_CoinInventory { get; set; }
        public virtual DbSet<Data_CoinStorage> Data_CoinStorage { get; set; }
        public virtual DbSet<Data_Coupon_StoreList> Data_Coupon_StoreList { get; set; }
        public virtual DbSet<Data_CouponCondition> Data_CouponCondition { get; set; }
        public virtual DbSet<Data_CouponInfo> Data_CouponInfo { get; set; }
        public virtual DbSet<Data_DigitCoinDestroy> Data_DigitCoinDestroy { get; set; }
        public virtual DbSet<Data_DigitCoinFood> Data_DigitCoinFood { get; set; }
        public virtual DbSet<Data_Discount_Detail> Data_Discount_Detail { get; set; }
        public virtual DbSet<Data_Discount_MemberLevel> Data_Discount_MemberLevel { get; set; }
        public virtual DbSet<Data_Discount_RecordStore> Data_Discount_RecordStore { get; set; }
        public virtual DbSet<Data_Discount_StoreList> Data_Discount_StoreList { get; set; }
        public virtual DbSet<Data_DiscountRule> Data_DiscountRule { get; set; }
        public virtual DbSet<Data_Food_Detial> Data_Food_Detial { get; set; }
        public virtual DbSet<Data_Food_Level> Data_Food_Level { get; set; }
        public virtual DbSet<Data_Food_Sale> Data_Food_Sale { get; set; }
        public virtual DbSet<Data_Food_StoreList> Data_Food_StoreList { get; set; }
        public virtual DbSet<Data_Food_WorkStation> Data_Food_WorkStation { get; set; }
        public virtual DbSet<Data_FreeCoinRule> Data_FreeCoinRule { get; set; }
        public virtual DbSet<Data_FreeGiveRule> Data_FreeGiveRule { get; set; }
        public virtual DbSet<Data_FreeGiveRule_Memberlevel> Data_FreeGiveRule_Memberlevel { get; set; }
        public virtual DbSet<Data_Game_StockInfo> Data_Game_StockInfo { get; set; }
        public virtual DbSet<Data_GameAPP_MemberRule> Data_GameAPP_MemberRule { get; set; }
        public virtual DbSet<Data_GameAPP_Rule> Data_GameAPP_Rule { get; set; }
        public virtual DbSet<Data_GameEncourage> Data_GameEncourage { get; set; }
        public virtual DbSet<Data_GameFreeLotteryRule> Data_GameFreeLotteryRule { get; set; }
        public virtual DbSet<Data_GameFreeRule> Data_GameFreeRule { get; set; }
        public virtual DbSet<Data_GameFreeRule_List> Data_GameFreeRule_List { get; set; }
        public virtual DbSet<Data_GameInfo> Data_GameInfo { get; set; }
        public virtual DbSet<Data_GameInfo_Ext> Data_GameInfo_Ext { get; set; }
        public virtual DbSet<Data_GameInfo_Photo> Data_GameInfo_Photo { get; set; }
        public virtual DbSet<Data_GivebackRule> Data_GivebackRule { get; set; }
        public virtual DbSet<Data_GoodExit_Detail> Data_GoodExit_Detail { get; set; }
        public virtual DbSet<Data_GoodExitInfo> Data_GoodExitInfo { get; set; }
        public virtual DbSet<Data_GoodInventory> Data_GoodInventory { get; set; }
        public virtual DbSet<Data_GoodOutOrder> Data_GoodOutOrder { get; set; }
        public virtual DbSet<Data_GoodOutOrder_Detail> Data_GoodOutOrder_Detail { get; set; }
        public virtual DbSet<Data_GoodRequest> Data_GoodRequest { get; set; }
        public virtual DbSet<Data_GoodRequest_List> Data_GoodRequest_List { get; set; }
        public virtual DbSet<Data_GoodsStock> Data_GoodsStock { get; set; }
        public virtual DbSet<Data_GoodStock_Record> Data_GoodStock_Record { get; set; }
        public virtual DbSet<Data_GoodStorage> Data_GoodStorage { get; set; }
        public virtual DbSet<Data_GoodStorage_Detail> Data_GoodStorage_Detail { get; set; }
        public virtual DbSet<Data_GroupArea> Data_GroupArea { get; set; }
        public virtual DbSet<Data_JackpotInfo> Data_JackpotInfo { get; set; }
        public virtual DbSet<Data_Member_Card> Data_Member_Card { get; set; }
        public virtual DbSet<Data_Member_Card_Store> Data_Member_Card_Store { get; set; }
        public virtual DbSet<Data_MemberLevel_Balance> Data_MemberLevel_Balance { get; set; }
        public virtual DbSet<Data_MemberLevel_BalanceCharge> Data_MemberLevel_BalanceCharge { get; set; }
        public virtual DbSet<Data_MemberLevel_Food> Data_MemberLevel_Food { get; set; }
        public virtual DbSet<Data_MemberLevelFree> Data_MemberLevelFree { get; set; }
        public virtual DbSet<Data_MerchAlipay_Shop> Data_MerchAlipay_Shop { get; set; }
        public virtual DbSet<Data_MerchWechatConfig> Data_MerchWechatConfig { get; set; }
        public virtual DbSet<Data_MerchWechatMenu> Data_MerchWechatMenu { get; set; }
        public virtual DbSet<Data_Message> Data_Message { get; set; }
        public virtual DbSet<Data_MessageInfo> Data_MessageInfo { get; set; }
        public virtual DbSet<Data_Parameters> Data_Parameters { get; set; }
        public virtual DbSet<Data_Project_BandPrice> Data_Project_BandPrice { get; set; }
        public virtual DbSet<Data_Project_BindDevice> Data_Project_BindDevice { get; set; }
        public virtual DbSet<Data_Project_TimeInfo> Data_Project_TimeInfo { get; set; }
        public virtual DbSet<Data_ProjectInfo> Data_ProjectInfo { get; set; }
        public virtual DbSet<Data_ProjectTicket> Data_ProjectTicket { get; set; }
        public virtual DbSet<Data_ProjectTicket_Bind> Data_ProjectTicket_Bind { get; set; }
        public virtual DbSet<Data_PushRule> Data_PushRule { get; set; }
        public virtual DbSet<Data_PushRule_GameList> Data_PushRule_GameList { get; set; }
        public virtual DbSet<Data_PushRule_MemberLevelList> Data_PushRule_MemberLevelList { get; set; }
        public virtual DbSet<Data_Reload> Data_Reload { get; set; }
        public virtual DbSet<Data_RuleOverlying_Group> Data_RuleOverlying_Group { get; set; }
        public virtual DbSet<Data_RuleOverlying_List> Data_RuleOverlying_List { get; set; }
        public virtual DbSet<Data_StandardCoinPrice> Data_StandardCoinPrice { get; set; }
        public virtual DbSet<Data_SupplierList> Data_SupplierList { get; set; }
        public virtual DbSet<Data_WorkFlow_Entry> Data_WorkFlow_Entry { get; set; }
        public virtual DbSet<Data_WorkFlow_Node> Data_WorkFlow_Node { get; set; }
        public virtual DbSet<Data_WorkFlowConfig> Data_WorkFlowConfig { get; set; }
        public virtual DbSet<Data_Workstation> Data_Workstation { get; set; }
        public virtual DbSet<Data_WorkStation_GoodList> Data_WorkStation_GoodList { get; set; }
        public virtual DbSet<Dict_Area> Dict_Area { get; set; }
        public virtual DbSet<Dict_BalanceType> Dict_BalanceType { get; set; }
        public virtual DbSet<Dict_FunctionMenu> Dict_FunctionMenu { get; set; }
        public virtual DbSet<Divide_ProjectInfo> Divide_ProjectInfo { get; set; }
        public virtual DbSet<Flw_CheckDate> Flw_CheckDate { get; set; }
        public virtual DbSet<Flw_Digite_Coin> Flw_Digite_Coin { get; set; }
        public virtual DbSet<Flw_Digite_Coin_Detail> Flw_Digite_Coin_Detail { get; set; }
        public virtual DbSet<Flw_DiscountRule> Flw_DiscountRule { get; set; }
        public virtual DbSet<Flw_Food_Exit> Flw_Food_Exit { get; set; }
        public virtual DbSet<Flw_GroupVerity> Flw_GroupVerity { get; set; }
        public virtual DbSet<Flw_Lottery> Flw_Lottery { get; set; }
        public virtual DbSet<Flw_MemberLevelFree> Flw_MemberLevelFree { get; set; }
        public virtual DbSet<Flw_MemberLevelFree_Detail> Flw_MemberLevelFree_Detail { get; set; }
        public virtual DbSet<Flw_Order> Flw_Order { get; set; }
        public virtual DbSet<Flw_Order_Detail> Flw_Order_Detail { get; set; }
        public virtual DbSet<Flw_Order_SerialNumber> Flw_Order_SerialNumber { get; set; }
        public virtual DbSet<Flw_Project_PlayTime> Flw_Project_PlayTime { get; set; }
        public virtual DbSet<Flw_Schedule> Flw_Schedule { get; set; }
        public virtual DbSet<Flw_Ticket_Exit> Flw_Ticket_Exit { get; set; }
        public virtual DbSet<Log_GameAlarm> Log_GameAlarm { get; set; }
        public virtual DbSet<Log_Operation> Log_Operation { get; set; }
        public virtual DbSet<Search_Template> Search_Template { get; set; }
        public virtual DbSet<Search_Template_Detail> Search_Template_Detail { get; set; }
        public virtual DbSet<Store_GameTotal> Store_GameTotal { get; set; }
        public virtual DbSet<Store_HeadTotal> Store_HeadTotal { get; set; }
        public virtual DbSet<XC_HolidayList> XC_HolidayList { get; set; }
        public virtual DbSet<XC_WorkInfo> XC_WorkInfo { get; set; }
        public virtual DbSet<Data_DigitCoin> Data_DigitCoin { get; set; }
        public virtual DbSet<Data_LotteryInventory> Data_LotteryInventory { get; set; }
        public virtual DbSet<Data_LotteryStorage> Data_LotteryStorage { get; set; }
        public virtual DbSet<Flw_Game_Free> Flw_Game_Free { get; set; }
        public virtual DbSet<Flw_ProjectTicket_Entry> Flw_ProjectTicket_Entry { get; set; }
        public virtual DbSet<Store_CheckDate> Store_CheckDate { get; set; }
        public virtual DbSet<Data_MemberLevel_BookRule> Data_MemberLevel_BookRule { get; set; }
        public virtual DbSet<Data_MemberLevel_GamePoint> Data_MemberLevel_GamePoint { get; set; }
        public virtual DbSet<Data_MemberLevel> Data_MemberLevel { get; set; }
        public virtual DbSet<BUF_UserAnalysis> BUF_UserAnalysis { get; set; }
        public virtual DbSet<Data_Card_BalanceCharge> Data_Card_BalanceCharge { get; set; }
        public virtual DbSet<Data_Discount_RecordMember> Data_Discount_RecordMember { get; set; }
        public virtual DbSet<Flw_Game_WinPrize> Flw_Game_WinPrize { get; set; }
        public virtual DbSet<Data_Jackpot_Level> Data_Jackpot_Level { get; set; }
        public virtual DbSet<Data_Jackpot_Matrix> Data_Jackpot_Matrix { get; set; }
        public virtual DbSet<Flw_ProjectTicket_Bind> Flw_ProjectTicket_Bind { get; set; }
        public virtual DbSet<Flw_Project_TicketInfo> Flw_Project_TicketInfo { get; set; }
        public virtual DbSet<Flw_MemberCard_Exit> Flw_MemberCard_Exit { get; set; }
        public virtual DbSet<Dict_System> Dict_System { get; set; }
        public virtual DbSet<Data_FoodInfo> Data_FoodInfo { get; set; }
        public virtual DbSet<Flw_Project_TicketUse> Flw_Project_TicketUse { get; set; }
        public virtual DbSet<Flw_Project_TicketDeviceLog> Flw_Project_TicketDeviceLog { get; set; }
        public virtual DbSet<Flw_MemberData> Flw_MemberData { get; set; }
        public virtual DbSet<Flw_Food_SaleDetail> Flw_Food_SaleDetail { get; set; }
        public virtual DbSet<Flw_Food_Sale_Pay> Flw_Food_Sale_Pay { get; set; }
        public virtual DbSet<Flw_Game_Watch> Flw_Game_Watch { get; set; }
        public virtual DbSet<Flw_Jackpot> Flw_Jackpot { get; set; }
        public virtual DbSet<Flw_Schedule_UserInfo> Flw_Schedule_UserInfo { get; set; }
        public virtual DbSet<Flw_Transfer> Flw_Transfer { get; set; }
        public virtual DbSet<Flw_CouponUse> Flw_CouponUse { get; set; }
        public virtual DbSet<Flw_DeviceData> Flw_DeviceData { get; set; }
        public virtual DbSet<Flw_Food_Exit_Pay> Flw_Food_Exit_Pay { get; set; }
        public virtual DbSet<Flw_Food_ExitDetail> Flw_Food_ExitDetail { get; set; }
        public virtual DbSet<Flw_Food_Sale> Flw_Food_Sale { get; set; }
        public virtual DbSet<Flw_Giveback> Flw_Giveback { get; set; }
        public virtual DbSet<Data_CouponList> Data_CouponList { get; set; }
        public virtual DbSet<Flw_MemberCard_BalanceCharge> Flw_MemberCard_BalanceCharge { get; set; }
        public virtual DbSet<Flw_MemberCard_Change> Flw_MemberCard_Change { get; set; }
        public virtual DbSet<Flw_MemberCard_LevelChange> Flw_MemberCard_LevelChange { get; set; }
        public virtual DbSet<Flw_MemberCard_Renew> Flw_MemberCard_Renew { get; set; }
        public virtual DbSet<Flw_MemberInfo_Change> Flw_MemberInfo_Change { get; set; }
        public virtual DbSet<Flw_MemberCard_Free> Flw_MemberCard_Free { get; set; }
    }
}
