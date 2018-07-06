using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudWebBar.Common.Enum
{
    /// <summary>
    /// 应用系统的Id
    /// </summary>
    public enum SysIdEnum
    { 
        WXSAPP = 0,//微信小程序
        WXHelper = 1,//莘助手
        Radar = 101,//雷达服务程序
        UDP = 102//UDP服务程序
    }


    public enum XCGameManaDeviceStoreType
    { 
        Store = 0,//门店
        Merch =1  //商户
    }

    //设备控制枚举
    public enum DevieControlTypeEnum
    {
        出币 = 1,
        存币 = 2,
        解除报警 = 3,
        远程锁定 = 4,
        远程解锁 = 5,
        投币 = 6,
        退币 = 7
    }

    public enum UDPSocketClientType
    {
        串口通讯服务 = 1,
        数据库服务 = 2,
        地图服务 = 3,
        短信服务 = 4,
        文件服务 = 5,
        后台客户端 = 6,
        吧台客户端 = 7,
        服务中心升级程序 = 8,
        服务中心 = 9,
    }

    /// <summary>
    /// 短信验证码枚举
    /// </summary>
    public enum SMSType
    {
        // 广告类
        Advertisement = 0,

        //验证码类
        VerificationCode = 1
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public enum DeviceState
    {
        Offline = 0,//设备离线
        Normal = 1,// 设备正常
        OutofMoney = 2,//出币中
        Fault = 3,//设备故障
        Locking = 4//设备锁定
    }

    /// <summary>
    /// 充值类型
    /// </summary>
    public enum RechargeType
    {
        Cash = 0, //现金充值
        Coin = 0, //币充值
    }

    /// <summary>
    /// 充值方式
    /// </summary>
    public enum RechargeMode
    {
        OffLine = 0,//线下充值
        OnLine = 1, //线上充值
    }

    public enum TokenType
    { 
        Member = 0,//会员模式
        Mobile = 1,//手机模式
        MemberAndMobile = 2//会员模式与手机模式
    }


    public enum TxtLogType
    {
        [LogFolderAttribute(FolderName = "SystemInit")]
        SystemInit = 0,//系统初始化
        [LogFolderAttribute(FolderName = "UPDService")]
        UPDService = 1,//UDP服务
        [LogFolderAttribute(FolderName = "TCPService")]
        TCPService = 2,//TCP服务
        [LogFolderAttribute(FolderName = "WeiXin")]
        WeiXin = 3,//微信
        [LogFolderAttribute(FolderName = "WeiXinPay")]
        WeiXinPay = 4,//微信支付
        [LogFolderAttribute(FolderName = "Api")]
        Api = 5,
        [LogFolderAttribute(FolderName = "AliPay")]
        AliPay = 6,//支付宝支付
        [LogFolderAttribute(FolderName = "PPosPay")]
        PPosPay = 7, //新大陆支付
        [LogFolderAttribute(FolderName = "LcswPay")]
        LcswPay = 8, //扫呗
        [LogFolderAttribute(FolderName = "DinPay")]
        DinPay = 9, //智付
        [LogFolderAttribute(FolderName = "WorkFlow")]
        WorkFlow = 10, //工作流
        [LogFolderAttribute(FolderName = "WorkFlow")]
        LogDBExcepton = 11, //工作流
        [LogFolderAttribute(FolderName = "Redis")]
        Redis = 12 //Redis
    }
    
    public enum TxtLogContentType
    {
        [LogFolderAttribute(FolderName = "Common")]
        Common = 0,//通用数据记录
        [LogFolderAttribute(FolderName = "Exception")]
        Exception = 1,//异常
        [LogFolderAttribute(FolderName = "Debug")]
        Debug = 2, //支付支付使用
        Record = 3//记录特定日志数据
    }

    public enum TxtLogFileType
    {
        Day = 0,//按日期生成文件，批量记录
        Time = 1//按时间生成文件，批量记录
    }

    /// <summary>
    /// tcpsocket消息类型
    /// </summary>
    public enum TCPMessageType
    {
        会员客户端注册 = 1,
        推送消息 = 2,
        雷达操作回复 = 3,
        业务授权应答 = 4,
        会员客户端注册应答 = 101
    }

    /// <summary>
    /// tcp应答消息类型
    /// </summary>
    public enum TCPAnswerMessageType
    {
        出币 = 1,
        存币 = 2
    }

    /// <summary>
    /// socket客户端类型
    /// </summary>
    public enum SocketClientType
    {
        UnknownClient = 0,//未知客户端
        WebClient = 1,//web客户端
        Client = 2//接口访问的客户端,通过接口请求
    }

    public enum SAppMessageType
    {
        MemberFoodSaleNotify = 0,//会员购买套餐
        MemberCoinsOperationNotify = 1
    }

    public enum SAppMessageParamsModelType
    {
        Cache = 0,//缓存
        RealTime = 1//实时参数 
    }

    public enum WeiXinMesageType
    {
        BuySuccessNotify = 0,//购买成功通知
        MemberRechargeNotify = 1,//会员充值通知
        UserRegisterRemind = 2, //新用户注册审批提醒
        OrderPaySuccess = 3, //订单支付成功
        OrderFailSuccess = 4,//订单支付失败
        MerchNewPassword = 5,//商户登录密码
        MerchResetPassword = 6,//商户重置密码
        StoreRegisterRemind = 7,//开店申请通知
        XcUserNewPassword = 8,//莘宸用户登录密码
        XcUserResetPassword = 9,//莘宸用户重置密码
        XCGameGetCoinSuccess = 10,//莘宸用户提币成功
        XCCloudOrderAuditRequest = 11,//订单审核
        DoSchedule = 12,//交班通知
        PhoneVerifyCode = 13//手机验证码
    }

    public enum OrderType
    { 
        WeiXin = 0,
        Ali = 1
    }

    //设备绑定状态枚举
    public enum DeviceBoundStateEnum : int
    {
        /// <summary>
        /// 已绑定
        /// </summary>
        [Description("已绑定")]
        Bound = 1,

        /// <summary>
        /// 未绑定
        /// </summary>
        [Description("未绑定")]
        NotBound = 0
    }

    //设备类型枚举
    public enum DeviceTypeEnum : int
    {
        /// <summary>
        /// 控制器
        /// </summary>
        [Description("控制器")]
        Router = 0,

        /// <summary>
        /// 提(售)币机
        /// </summary>
        [Description("售币机")]
        SlotMachines = 1,

        /// <summary>
        /// 存币机
        /// </summary>
        [Description("存币机")]
        DepositMachine = 2,

        /// <summary>
        /// 出票器
        /// </summary>
        [Description("出票器")]
        Clerk = 3,

        /// <summary>
        /// 卡头
        /// </summary>
        [Description("卡头")]
        Terminal = 4
    }

    //分组类型枚举
    //0 娃娃机
    //1 压分机
    //2 推土机
    //3 剪刀机
    //4 彩票机
    //5 枪战机
    //6 VR设备
    //7 鱼机
    public enum GroupTypeEnum : int
    {
        /// <summary>
        /// 娃娃机
        /// </summary>
        [Description("娃娃机")]
        娃娃机 = 0,

        /// <summary>
        /// 压分机
        /// </summary>
        [Description("压分机")]
        压分机 = 1,

        /// <summary>
        /// 推土机
        /// </summary>
        [Description("推土机")]
        推土机 = 2,

        /// <summary>
        /// 剪刀机
        /// </summary>
        [Description("剪刀机")]
        剪刀机 = 3,

        /// <summary>
        /// 彩票机
        /// </summary>
        [Description("彩票机")]
        彩票机 = 4,

        /// <summary>
        /// 枪战机
        /// </summary>
        [Description("枪战机")]
        枪战机 = 5,

        /// <summary>
        /// VR设备
        /// </summary>
        [Description("VR设备")]
        VR设备 = 6,

        /// <summary>
        /// 鱼机
        /// </summary>
        [Description("鱼机")]
        鱼机 = 7
    }

    //分组类型枚举
    //0 未激活
    //1 启用
    //2 停用
    //3 在线
    //4 离线
    //5 报警
    //6 锁定
    //7 工作中
    public enum DeviceStatusEnum : int
    {
        /// <summary>
        /// 未激活
        /// </summary>
        [Description("未激活")]
        未激活 = 0,

        /// <summary>
        /// 启用
        /// </summary>
        [Description("启用")]
        启用 = 1,

        /// <summary>
        /// 停用
        /// </summary>
        [Description("停用")]
        停用 = 2,

        /// <summary>
        /// 在线
        /// </summary>
        [Description("在线")]
        在线 = 3,

        /// <summary>
        /// 离线
        /// </summary>
        [Description("离线")]
        离线 = 4,

        /// <summary>
        /// 报警
        /// </summary>
        [Description("报警")]
        报警 = 5,

        /// <summary>
        /// 锁定
        /// </summary>
        [Description("锁定")]
        锁定 = 6,

        /// <summary>
        /// 工作中
        /// </summary>
        [Description("工作中")]
        工作中 = 7
    }    

    public enum SettleType
    {
        None = 0,//不采用第三方结算
        Org = 1,//微信支付官方通道
        PPOS = 2,//新大陆
        LKL = 3 //拉卡拉
    }

    public enum StoreState
    {
        Invalid = 0,//无效
        Valid = 1,//有效
        Suspend = 2,//暂停
        Cancel = 3, //注销
        Open=4 //开业
    }

    public enum WorkType
    {
        UserCheck = 0,//用户审核
        StoreCheck = 1//门店审核        
    }

    public enum WorkState
    {
        Pending = 0, //待审核
        Pass = 1, //审核通过
        Reject = 2 //审核拒绝
    }

    public enum RoleType
    {
        XcUser = 0,     //莘宸普通员工
        XcAdmin = 1,    //莘宸管理员
        StoreUser = 2,  //门店员工
        MerchUser = 3   //商户专员
    }

    public enum MerchType
    {
        Normal = 1, //普通商户
        Heavy = 2, //大客户
        Agent = 3 //代理商
    }

    public enum UserType
    {
        Xc = 0,       //莘宸用户
        Normal = 1,  //普通商户
        Heavy = 2,  //大客户
        Agent = 3, //代理商
        Store = 4, //门店用户
        StoreBoss = 5 //门店老板
    }

    public enum CreateType
    {
        Xc = 1,   //莘宸管理创建，用户编号为莘宸员工编号
        Agent = 2 //代理商创建，用户编号为代理商商户号
    }

    public enum MerchState
    {
        Stop = 0,   //停用
        Normal = 1 //正常
    }

    public enum MerchTag
    {
        Game = 0,    //游乐行业
        Lottery = 1 //博彩行业
    }

    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    { 
        //0登录1注销2换班3加记录4改记录5删记录6数据备份7查询8清记录9短信查账10短信清账11网站登录12网站注销13网站查账14网站改密码15网站清账16网站兑币17打印18打开
    }

    public enum AlertStatus
    {
        Active = 0,  //活动
        Confirm = 1, //确认
        Resolved = 2 //解决
    }

    public enum DigitStatus
    {
        Unused = 0,//未使用
        Inuse = 1,//使用中
        Cancel = 2//作废
    }

    public enum UserStatus
    {
        None = 0,//未审核
        Pass = 1,//已审核,在职
        Leave = 2,//离职
        Lock = 3,//锁定
        Vacation = 4//休假
    }    

    public enum FoodDetailType
    {
        Coin = 0,//会员币种
        Good = 1,//礼品
        Ticket = 2,//门票
        Digit = 3,//数字币
        Coupon = 4//优惠券
    }

    public enum OperateType
    {
        CoinOrTicket = 0,//实物币票
        Card = 1 //存入卡
    }

    public enum RechargeManner
    {
        Manual = 0,//手动选择实物币或充值到卡
        Coin = 1,//只允许实物币
        Card = 2//只允许充值到卡
    }

    public enum FoodState
    {
        Invalid = 0,//无效（下架）
        Valid = 1//有效（上架）
    }

    public enum GoodType
    {
        Good = 0,//销售商品
        Gift = 1,//兑换礼品
        BGYP = 2,//办公用品
        ZDSB = 3,//终端设备
        XTHC = 4,//系统耗材
        Coin = 5,//代币
        Digit = 6,//数字币
        Card = 7,//会员卡
        JP = 8,//奖品
        Other = 9//其他
    }

    public enum FeeType
    {
        Count = 0,//计次
        Time = 1,//计时
        Day = 2,//日票
        Month = 3,//月票
        Year = 4//年票
    }

    //混合套餐中使用
    public enum WeightType
    {
        Percent = 0,//比例
        Money = 1//金额
    }
    
    //连锁门店权重配置中使用
    public enum ChainStoreWeightType
    {
        Whole = 0,//按全场
        Game = 1//按游戏机
    }
    
    /// <summary>
    /// 余额类别映射类型
    /// </summary>
    public enum HKType
    {
        [Description("不绑定")]
        NoBound = 0, //不绑定
        [Description("代币")]
        Coin = 1, //代币
        [Description("彩票")]
        Lottery = 2, //彩票
        [Description("积分")]
        Points = 3, //积分
        [Description("储值金")]
        Money = 4, //储值金
    }

    /// <summary>
    /// 叠加规则类别
    /// </summary>
    public enum RuleType
    {
        [Description("满减规则")]
        Discount = 0, //满减规则
        [Description("优惠券")]
        Coupon = 1, //券规则
    }

    /// <summary>
    /// 赠送规则间隔类别
    /// </summary>
    public enum SpanType
    {
        [Description("小时")]
        Hour = 0,   //小时
        [Description("天")]
        Day = 1,    //天
        [Description("周")]
        Week = 2,   //周
        [Description("月")]
        Month = 3,  //月
        [Description("季")]
        Season = 4, //季
        [Description("年")]
        Year = 5,   //年
        [Description("整个有效期（仅限自然周期）")]
        Whole = 6,  //整个有效期（仅限自然周期）
    }

    /// <summary>
    /// 游乐项目绑定设备工作方式
    /// </summary>
    public enum ProjectBindDeviceWorkType
    {
        [Description("入口")]
        Entry = 0,   
        [Description("出口")]
        Exit = 1,   
        [Description("自动")]
        Auto = 2,   
    }

    public enum ProjectInfoChargeType
    {
        [Description("按次")]
        Count = 0,
        [Description("计时")]
        Time = 1 
    }

    /// <summary>
    /// 计时项目扣费方式
    /// </summary>
    public enum ProjectTimeChargeType
    {
        [Description("进闸扣除基础费用")]
        Base = 0,
        [Description("进闸扣除封顶费用")]
        Top = 1,
        [Description("进闸验证微信票码")]
        Weixin = 2,   
    }

    /// <summary>
    /// 游乐计时项目计费方式
    /// </summary>
    public enum CycleType
    {
        [Description("按周期扣除")]
        Cycle = 0,
        [Description("出闸时扣除")]
        Out = 1 
    }

    /// <summary>
    /// 小数位舍弃方式
    /// </summary>
    public enum AddingType
    {
        [Description("全部舍弃")]
        AllOmit = 0, //全部舍弃 只取整数部分
        [Description("全部保留")]
        AllAdd = 1, //全部保留 有任何小数都进位
        [Description("四舍五入")]
        Round = 2 //四舍五入
    }

    /// <summary>
    /// 门票类别
    /// </summary>
    public enum TicketType
    {
        [Description("计次票")]
        Count = 0,        
        [Description("团体票")]
        Group = 1, 
        [Description("期限票")]
        Period = 2
    }

    /// <summary>
    /// 分摊方式
    /// </summary>
    public enum DivideType
    {
        [Description("不分摊")]
        NoDivide = 0,
        [Description("一次性分摊")]
        Once = 1,
        [Description("按次分摊")]
        Count = 2,
        [Description("按天分摊")]
        Day = 3
    }

    /// <summary>
    /// 业务类型
    /// </summary>
    public enum BusinessType
    {
        [Description("门票")]
        Ticket = 0,
        [Description("限时任玩")]
        TimePlay = 1,
        [Description("机台打包")]
        StationPack = 2
    }

    /// <summary>
    /// 生效方式
    /// </summary>
    public enum EffactType
    {
        [Description("按时长")]
        Period = 0,
        [Description("按日期")]
        Date = 1
    }

    /// <summary>
    /// 退票方式
    /// </summary>
    public enum ExitTicketType
    {
        [Description("按金额")]
        Money = 0,  
        [Description("按比例")]
        Percent = 1        
    }

    /// <summary>
    /// 代理渠道类别
    /// </summary>
    public enum ProxyType
    {
        [Description("无代理")]
        None = 0,
        [Description("广州好酷")]
        HaoKu = 1    
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public enum DeviceType : int
    {
        /// <summary>
        /// 卡头
        /// </summary>
        [Description("卡头")]
        卡头 = 0,

        /// <summary>
        /// 碎票机
        /// </summary>
        [Description("碎票机")]
        碎票机 = 1,

        /// <summary>
        /// 存币机
        /// </summary>
        [Description("存币机")]
        存币机 = 2,

        /// <summary>
        /// 提币机
        /// </summary>
        [Description("提币机")]
        提币机 = 3,

        /// <summary>
        /// 售币机
        /// </summary>
        [Description("售币机")]
        售币机 = 4,

        /// <summary>
        /// 投币机
        /// </summary>
        [Description("投币机")]
        投币机 = 5,

        /// <summary>
        /// 存币机
        /// </summary>
        [Description("自助机")]
        自助机 = 6,

        /// <summary>
        /// 闸机
        /// </summary>
        [Description("闸机")]
        闸机 = 7,

        /// <summary>
        /// 路由器
        /// </summary>
        [Description("路由器")]
        路由器 = 8
    }
    /// <summary>
    /// 设备在线状态
    /// </summary>
    public enum DeviceStatus
    {
        停用 = 0,
        正常 = 1,
        锁定 = 2
    }
    
    /// <summary>
    /// 设备通讯方式
    /// </summary>
    public enum CmdType
    {
        RS232 = 0,    //串口
        RS485 = 1,    //工业总线
        Wireless = 2, //店内无线
        TCPIP = 3    //以太网
    }

    public enum SignType
    {
        In = 0,//签入
        Out = 1,//签出
        Once = 2//单次
    }

    public enum JoinType
    {
        Ticket = 0,//门票
        Time = 1 //计时
    }

    /// <summary>
    /// 班次状态
    /// </summary>
    public enum ScheduleState
    {
        [Description("未开班")]
        Stopped = 0,
        [Description("进行中")]
        Starting = 1,
        [Description("已交班")]
        Submitted = 2,
        [Description("已审核")]
        Checked = 3
    }

    /// <summary>
    /// 班次个数
    /// </summary>
    public enum ScheduleCount
    {
        [Description("A班")]
        One = 0,
        [Description("AB班")]
        Two = 1,
        [Description("ABC班")]
        Three = 2
    }
    
    /// <summary>
    /// 优惠券类别
    /// </summary>
    public enum CouponType
    {
        [Description("代金券")]
        Cash = 0,//代金
        [Description("折扣券")]
        Discount = 1,//折扣
        [Description("兑换券")]
        Charge = 2//兑换
    }

    /// <summary>
    /// 兑换方式
    /// </summary>
    public enum ChargeType
    {
        [Description("礼品")]
        Good = 0,//礼品
        [Description("门票")]
        Project = 1,//门票
        [Description("代币")]
        Coin = 2//代币
    }

    /// <summary>
    /// 派发方式
    /// </summary>
    public enum SendType
    {
        [Description("消费赠券")]
        Consume = 0,     //消费赠券
        [Description("定向派发")]
        Orient = 1,      //定向派发
        [Description("抽奖赠券")]
        Jackpot = 2,     //抽奖赠券
        [Description("街边派送")]
        Delivery = 3     //街边派送
    }

    /// <summary>
    /// 实物券标记
    /// </summary>
    public enum CouponFlag
    {
        [Description("电子优惠券")]
        Digit = 0,//电子优惠券
        [Description("实物优惠券")]
        Entry = 1 //实物优惠券
    }

    /// <summary>
    /// 券状态
    /// </summary>
    public enum CouponState
    {
        [Description("未分配")]
        NotAssigned = 0,  //未分配 创建初始状态
        [Description("未激活")]
        NotActivated = 1, //未激活 调拨门店
        [Description("已激活")]
        Activated = 2,    //已激活 门店派发
        [Description("已使用")]
        Applied = 3       //已使用 用户核销       
    }

    /// <summary>
    /// 优惠时段，时段类型
    /// </summary>
    public enum TimeType
    {
        [Description("自定义")]
        Custom = 0,//自定义
        [Description("工作日")]
        Workday = 1,//工作日
        [Description("周末")]
        Weekend = 2,//周末
        [Description("节假日")]
        Holiday = 3//节假日

    }

    
    /// <summary>
    /// 派发周期
    /// </summary>
    public enum SendCycle
    {
        [Description("每天")]
        EveryDay = 0,    //每天
        [Description("每周")]
        EveryWeek = 1,   //每周
        [Description("每月")]
        EveryMonth = 2,  //每月
        [Description("每年")]
        EveryYear = 3    //每年
    }

    /// <summary>
    /// 满减优惠频率
    /// </summary>
    public enum FreqType
    {
        [Description("天")]
        Day = 0,    //每天
        [Description("周")]
        Week = 1,   //每周
        [Description("月")]
        Month = 2,  //每月
        [Description("季")]
        Season = 3,  //每季
        [Description("年")]
        Year = 4    //每年
    }

    /// <summary>
    /// 条件类型
    /// </summary>
    public enum ConditionType
    {
        Manual = 0,        //手动派发
        Auto = 1,          //自动派发
    }


    /// <summary>
    /// 条件ID
    /// </summary>
    public enum ConditionID
    {
        Activability = 0,    //活跃能力
        Consumability = 1,   //消费能力
        Bunkoability = 2,    //输赢能力
        Birthday = 3,        //生日
        MemberLevel = 4,     //会员级别
        MemberBalance = 5    //会员余额
    }

    /// <summary>
    /// 礼品调拨类型
    /// </summary>
    public enum RequestType
    {
        [Description("门店向门店申请")]
        RequestStore = 0,   //门店间申请
        [Description("门店向总部申请")]
        RequestMerch = 1,   //门店向总部申请
        [Description("总部向门店派货")]
        MerchSend = 2,      //总部配货
        [Description("总部向门店申请")]
        MerchRequest = 3    //总部申请
    }

    /// <summary>
    /// 数据异动类别
    /// </summary>
    public enum SourceType
    {
        [Description("入库单")]
        GoodStorage = 0,    
        [Description("调拨单")]
        GoodRequest = 1,    
        [Description("安装记录")]
        GoodReload = 2,    
        [Description("出库单")]
        GoodOut = 3,       
        [Description("退货单")]
        GoodExit = 4        
    }

    /// <summary>
    /// 退货来源类别
    /// </summary>
    public enum GoodExitSourceType
    {
        [Description("入库单")]
        GoodStorage = 0,    
        [Description("调拨单")]
        GoodRequest = 1,   
    }

    /// <summary>
    /// 出库类别
    /// </summary>
    public enum GoodOutOrderType
    {
        [Description("废品出库")]
        Discard = 0,        //废品出库
        [Description("转仓出库")]
        Transfer = 1,       //转仓出库
        [Description("入库退货")]
        Exit = 2,           //入库退货
        [Description("入库退货单")]
        ExitOrder = 3            //入库退货单
    }

    /// <summary>
    /// 出入库状态
    /// </summary>
    public enum GoodOutInState
    {
        [Description("未审核")]
        Pending = 0,        //未审核
        [Description("已完成")]
        Done = 1,           //已完成
        [Description("已撤销")]
        Cancel = 2,         //已撤销
    }

    //出入库标志
    public enum StockFlag
    {
        [Description("入库")]
        In = 0,             //入库
        [Description("出库")]
        Out = 1             //出库
    }

    /// <summary>
    /// 工作流节点类别
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 开始
        /// </summary>
        [Description("开始")]
        Start = 0,
        /// <summary>
        /// 过程
        /// </summary>
        [Description("过程")]
        Process = 1,
        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        End = 2
    }
    /// <summary>
    /// 工作流事件
    /// </summary>
    public enum Trigger : int
    {
        /// <summary>
        /// 调拨申请
        /// </summary>
        [Description("调拨申请")]
        Request = 1,
        /// <summary>
        /// 调拨申请审核
        /// </summary>
        [Description("调拨申请审核")]
        RequestVerify = 2,
        /// <summary>
        /// 调拨出库
        /// </summary>
        [Description("调拨出库")]
        SendDeal = 3,
        /// <summary>
        /// 调拨出库审核
        /// </summary>
        [Description("调拨出库审核")]
        SendDealVerify = 4,
        /// <summary>
        /// 调拨入库
        /// </summary>
        [Description("调拨入库")]
        RequestDeal = 5,
        /// <summary>
        /// 调拨入库审核
        /// </summary>
        [Description("调拨入库审核")]
        RequestDealVerify = 6,
        /// <summary>
        /// 撤销
        /// </summary>
        [Description("撤销")]
        Cancel = 7,
        /// <summary>
        /// 流程关闭
        /// </summary>
        [Description("流程关闭")]
        Close = 8,
        /// <summary>
        /// 调拨退货
        /// </summary>
        [Description("调拨退货")]
        RequestExit = 9,
        /// <summary>
        /// 删除
        /// </summary>
        [Description("删除")]
        Delete = 10,
    }
    /// <summary>
    /// 工作流状态
    /// </summary>
    public enum State
    {
        /// <summary>
        /// 开始
        /// </summary>
        [Description("开始")]
        Open = 0,
        /// <summary>
        /// 申请
        /// </summary>
        [Description("已申请")]
        Requested = 1,
        /// <summary>
        /// 申请审核已通过
        /// </summary>
        [Description("审核通过")]
        RequestVerifiedPass = 2,
        /// <summary>
        /// 申请审核已拒绝
        /// </summary>
        [Description("审核拒绝")]
        RequestVerifiedRefuse = 3,
        /// <summary>
        /// 调拨已出库
        /// </summary>
        [Description("已出库")]
        SendDealed = 4,
        /// <summary>
        /// 调拨出库审核已通过
        /// </summary>
        [Description("出库审核通过")]
        SendDealVerifiedPass = 5,
        /// <summary>
        /// 调拨出库审核已拒绝
        /// </summary>
        [Description("出库审核拒绝")]
        SendDealVerifiedRefuse = 6,
        /// <summary>
        /// 调拨已入库
        /// </summary>
        [Description("已入库")]
        RequestDealed = 7,        
        /// <summary>
        /// 调拨入库审核已通过
        /// </summary>
        [Description("入库审核通过")]
        RequestDealVerifiedPass = 8,
        /// <summary>
        /// 调拨入库审核已拒绝
        /// </summary>
        [Description("入库审核拒绝")]
        RequestDealVerifiedRefuse = 9,
        /// <summary>
        /// 流程结束
        /// </summary>
        [Description("结束")]
        Closed = 10,
        /// <summary>
        /// 调拨已退货
        /// </summary>
        [Description("已退货")]
        RequestExited = 11,
        /// <summary>
        /// 调拨申请单已删除
        /// </summary>
        [Description("已删除")]
        RequestDeleted = 12,
        /// <summary>
        /// 总部派货已撤销
        /// </summary>
        [Description("已撤销")]
        Canceled = 13
    }

    /// <summary>
    /// 库存类别
    /// </summary>
    public enum StockType
    {      
        [Description("仓库")]
        Depot = 0,
        
        [Description("吧台")]
        WorkStation = 1,
        
        [Description("自助机")]
        SelfService = 2,
        
        [Description("游戏机")]
        GameInfo = 3,
    }

    //工作流工作类别
    public enum WorkflowType
    {
        GoodRequest = 0     //调拨申请
    }

    //工作流工作事件类别
    public enum WorkflowEventType
    {
        GoodRequest = 0     //产品调拨对应表格 Data_GoodRequest
    }    

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PaymentChannel
    {
        /// <summary>
        /// 微信
        /// </summary>
        [Description("010")]
        WXPAY = 1, 

        /// <summary>
        /// 支付宝
        /// </summary>
        [Description("020")]
        ALIPAY = 2
    }

    /// <summary>
    /// 结算类型
    /// 0 不采用三方结算
    /// 1 微信支付宝官方通道
    /// 2 新大陆
    /// 3 拉卡拉
    /// 4 扫呗
    /// </summary>
    public enum SelttleType
    {
        /// <summary>
        /// 不使用第三方支付
        /// </summary>
        NotThird = 0,
        /// <summary>
        /// 微信支付宝官方通道
        /// </summary>
        AliWxPay = 1,
        /// <summary>
        /// 新大陆
        /// </summary>
        StarPos = 2,
        Lakala = 3,
        /// <summary>
        /// 扫呗
        /// </summary>
        LcswPay = 4,
        /// <summary>
        /// 智付
        /// </summary>
        DinPay = 5
    }

    public enum OrderState
    {
        /// <summary>
        /// 未结算
        /// </summary>
        Unsettled = 0,

        /// <summary>
        /// 未付款
        /// </summary>
        Unpaid = 1,

        /// <summary>
        /// 已付款
        /// </summary>
        AlreadyPaid = 2,

        /// <summary>
        /// 异常支付警报
        /// </summary>
        Alarm = 3
    }


    public enum AuditOrderType
    {
        FoodSale = 0
    }

    /// <summary>
    /// 操作渠道
    /// </summary>
    public enum MemberDataChannelType
    {
        [Description("吧台")]
        吧台 = 0,
        [Description("自助设备")]
        自助设备 = 1,
        [Description("移动终端")]
        移动终端 = 2,
        [Description("莘宸云")]
        莘宸云 = 3,
        [Description("美团")]
        美团 = 4,
        [Description("大众点评")]
        大众点评 = 5,
        [Description("口碑")]
        口碑 = 6
    }

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum MemberDataOperationType
    {
        [Description("售币")]
        售币 = 0,
        [Description("存币")]
        存币 = 1,
        [Description("提币")]
        提币 = 2,
        [Description("投币")]
        投币 = 3,
        [Description("碎票")]
        碎票 = 4,
        [Description("退币")]
        退币 = 5,
        [Description("消费赠送")]
        消费赠送 = 6,
        [Description("生日赠送")]
        生日赠送 = 7,
        [Description("输赢赠送")]
        输赢赠送 = 8,
        [Description("余额兑换转入")]
        余额兑换转入 = 9,
        [Description("余额兑换转出")]
        余额兑换转出 = 10,
        [Description("过户入")]
        过户入 = 11,
        [Description("过户出")]
        过户出 = 12,
        [Description("退储值金")]
        退储值金 = 13,
        [Description("退货返回")]
        退货返回 = 14,
        [Description("余额支付")]
        余额支付 = 15,
        [Description("礼品回购")]
        礼品回购 = 16
    }
}
