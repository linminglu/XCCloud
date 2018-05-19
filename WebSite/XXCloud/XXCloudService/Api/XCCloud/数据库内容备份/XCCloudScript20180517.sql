USE [XCCloud]
GO
/****** Object:  StoredProcedure [dbo].[GetOrderDateFormatStr]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetOrderDateFormatStr](@time datetime,@str char(17) output )
as
declare @year varchar(4) = datepart(Year,@time)
declare @month varchar(2) = datepart(Month,@time)
declare @day varchar(2) = datepart(Day,@time)
declare @hour varchar(2) = datepart(Day,@time)
declare @minute varchar(2) = datepart(MI,@time)
declare @second varchar(2) = datepart(SS,@time)
declare @msecond varchar(3) = datepart(MS,@time)
if LEN(@month) < 2 set @month = '0' + @month
if LEN(@day) < 2 set @day = '0' + @day
if LEN(@hour) < 2 set @hour = '0' + @hour
if LEN(@minute) < 2 set @minute = '0' + @minute
if LEN(@second) < 2 set @second = '0' + @second
if LEN(@msecond) < 2 set @msecond = '00' + @msecond
if LEN(@msecond) < 3 set @msecond = '0' + @msecond
set @str = cast(@year + @month + @day + @hour + @minute + @second + @msecond as varchar)
GO
/****** Object:  UserDefinedTableType [dbo].[IdListType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[IdListType] AS TABLE(
	[FoodId] [int] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[GetWeekDate]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[GetWeekDate](@Week int,@WeekStr varchar(20)) RETURNS varchar(50) 
 as
   begin
	   declare @Len int
	   declare @Index int = 0
	   declare @WeekValue char(1) = ''
	   declare @WeekDayStr varchar(50) = ''
	   declare @CurrentWeekValue int = 0
	   --计算当前日期是周几
	   select @CurrentWeekValue = datepart(weekday,getdate()) - 1 
	   if @CurrentWeekValue = 0 
	     set @CurrentWeekValue = 7
	   
	   if @Week = 0
	     begin
		   set @Len = LEN(@WeekStr)
		   while @Index < @Len
			 begin
				select @WeekValue = substring(@WeekStr,(@Index + 1),1)
				if @WeekValue <> '|'
				  begin
					if @WeekDayStr <> '' set @WeekDayStr = @WeekDayStr + ','
					set @WeekDayStr = @WeekDayStr + '周' + CAST(@WeekValue as varchar)
				  end
				set @Index = @Index + 1
			 end	
	     end
	   else if @Week = 1
	     begin
	       return '周1,周2,周3,周4,周5'
	     end  
	   else if @Week = 2
	     begin
	       return '周6,周7'
	     end
	   else if @Week = 3
	     begin
	       return '节假日'
	     end
	
	   return @WeekDayStr
   end
GO
/****** Object:  UserDefinedFunction [dbo].[GetWeek]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[GetWeek]() RETURNS INT 
 as
   begin
	   declare @CurrentWeekValue int = 0
	   --计算当前日期是周几
	   select @CurrentWeekValue = datepart(weekday,getdate()) - 1
	   if @CurrentWeekValue = 0 
	     set @CurrentWeekValue = 7 	
	   return @CurrentWeekValue	
   end
GO
/****** Object:  StoredProcedure [dbo].[GetSplitTable]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetSplitTable](@Str varchar(200))
as
  create table #TmpSplitTable(Id varchar(20))
  declare @TmpStr varchar(20) = ''
  declare @Index int = 1 
  declare @Len int = LEN(@Str)
  while @Index <= @Len
    begin	
		if SUBSTRING(@Str,@Index,1) = ','
		  begin
			insert #TmpSplitTable(Id)
			values(@TmpStr)
			set @TmpStr = ''
		  end
		else
		  begin
		    set @TmpStr = @TmpStr + SUBSTRING(@Str,@Index,1)
		    print '@TmpStr:' +@TmpStr
		  end 
		set @Index = @Index + 1	
    end
  if @TmpStr <> ''
    begin
      insert #TmpSplitTable(Id)
	  values(@TmpStr)
    end    
  select * from #TmpSplitTable
GO
/****** Object:  UserDefinedTableType [dbo].[NoArrayType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[NoArrayType] AS TABLE(
	[StartNo] [int] NULL,
	[EndNo] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[MemberIDsType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[MemberIDsType] AS TABLE(
	[MemberID] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[MemberBalanceListType2]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[MemberBalanceListType2] AS TABLE(
	[BalanceIndex] [int] NULL,
	[Balance] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[MemberBalanceListType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[MemberBalanceListType] AS TABLE(
	[BalanceIndex] [int] NULL,
	[BalanceName] [varchar](50) NULL,
	[Balance] [decimal](18, 2) NULL
)
GO
/****** Object:  Table [dbo].[Log_Operation]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Log_Operation](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](10) NULL,
	[Realtime] [datetime] NULL,
	[ScheduleID] [int] NULL,
	[LogType] [int] NULL,
	[OperName] [varchar](100) NULL,
	[WorkStation] [varchar](20) NULL,
	[UserID] [int] NULL,
	[AuthorID] [int] NULL,
	[Content] [varchar](500) NULL,
 CONSTRAINT [PK_Log_Operation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Realtime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'Realtime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0登录1注销2换班3加记录4改记录5删记录6数据备份7查询8清记录9短信查账10短信清账11网站登录12网站注销13网站查账14网站改密码15网站清账16网站兑币17打印18打开' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'LogType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OperName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'OperName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Content' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation', @level2type=N'COLUMN',@level2name=N'Content'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统操作日志表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_Operation'
GO
/****** Object:  Table [dbo].[Log_GameAlarm]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Log_GameAlarm](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[ICCardID] [int] NULL,
	[Segment] [varchar](4) NULL,
	[HeadAddress] [varchar](2) NULL,
	[AlertType] [int] NULL,
	[HappenTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[State] [int] NULL,
	[LockGame] [int] NULL,
	[LockMember] [int] NULL,
	[AlertContent] [varchar](500) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'ICCardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Segment' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'Segment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HeadAddress' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'HeadAddress'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AlertType' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'AlertType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HappenTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'HappenTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'State' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LockGame' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'LockGame'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LockMember' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'LockMember'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AlertContent' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm', @level2type=N'COLUMN',@level2name=N'AlertContent'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'游戏机报警表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Log_GameAlarm'
GO
/****** Object:  UserDefinedTableType [dbo].[RegisterMemberType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[RegisterMemberType] AS TABLE(
	[Mobile] [varchar](20) NULL,
	[WeChat] [varchar](64) NULL,
	[QQ] [varchar](64) NULL,
	[IMME] [varchar](64) NULL,
	[CardShape] [int] NULL,
	[MemberName] [varchar](50) NULL,
	[MemberPassword] [varchar](20) NULL,
	[Birthday] [varchar](16) NULL,
	[Gender] [varchar](1) NULL,
	[IdentityCard] [varchar](50) NULL,
	[EMail] [varchar](50) NULL,
	[LeftHandCode] [varchar](5000) NULL,
	[RightHandCode] [varchar](5000) NULL,
	[Photo] [varchar](100) NULL,
	[RepeatCode] [int] NULL,
	[ICCardId] [varchar](20) NULL,
	[Deposit] [int] NULL,
	[ICCardUID] [bigint] NULL,
	[Note] [varchar](200) NULL
)
GO
/****** Object:  StoredProcedure [dbo].[QueryDiscountRule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[QueryDiscountRule](
@MerchID varchar(15),@SqlWhere varchar(MAX),@Result int out)
as	
	declare @sql nvarchar(max)
	SET @sql = ''
	SET @sql = @sql + 
	'select a.ID, a.RuleName, a.ShareCount, a.RuleLevel, a.Note, '+        
    '(case when isnull(a.StartDate,'''')='''' then '''' else convert(varchar,a.StartDate,23) end) as StartDate, (case when isnull(a.EndDate,'''')='''' then '''' else convert(varchar,a.EndDate,23) end) as EndDate '+
    ' from Data_DiscountRule a'+    
    ' where a.State=1 and a.MerchID=' + @MerchID + @SqlWhere
	exec (@sql)
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[QueryCouponInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[QueryCouponInfo](
@MerchID varchar(15),@SqlWhere varchar(MAX),@Result int out)
as	
	declare @sql nvarchar(max)
	SET @sql = ''
	SET @sql = @sql + 
	'select a.ID, a.CouponName, a.EntryCouponFlag, a.CouponType, b.DictKey as CouponTypeStr, a.PublishCount, (isnull(c.UseCount, 0) + isnull(d.UseCount, 0)) as UseCount, '+
    'isnull(f.NotAssignedCount, 0) as NotAssignedCount, isnull(g.NotActivatedCount, 0) as NotActivatedCount, isnull(h.ActivatedCount, 0) as ActivatedCount, '+
    'a.AuthorFlag, a.AllowOverOther, a.OpUserID, j.LogName as OpUserName, a.Context, isnull(i.IsLock, 0) as IsLock, '+
    '(case when isnull(a.StartDate,'''')='''' then '''' else convert(varchar,a.StartDate,23) end) as StartDate, (case when isnull(a.EndDate,'''')='''' then '''' else convert(varchar,a.EndDate,23) end) as EndDate, '+
    '(case when isnull(a.CreateTime,'''')='''' then '''' else convert(varchar,a.CreateTime,23) end) as CreateTime '+
    ' from Data_CouponInfo a'+
    ' left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey=''优惠券类别'' and a.PID=0) b on convert(varchar, a.CouponType)=b.DictValue '+
    ' left join (select a.ID as CouponID, count(c.ID) as UseCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID inner join Flw_CouponUse c on b.ID=c.CouponCode group by a.ID) c on a.ID=c.CouponID ' +
    ' left join (select a.ID as CouponID, count(b.ID) as UseCount from Data_CouponInfo a inner join Flw_CouponUse b on a.ID=b.CouponID group by a.ID) d on a.ID=d.CouponID '+
    ' left join (select a.ID as CouponID, count(b.ID) as NotAssignedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=0 group by a.ID) f on a.ID=f.CouponID ' +
    ' left join (select a.ID as CouponID, count(b.ID) as NotActivatedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=1 group by a.ID) g on a.ID=g.CouponID ' +
    ' left join (select a.ID as CouponID, count(b.ID) as ActivatedCount from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID where isnull(b.State, 0)=2 group by a.ID) h on a.ID=h.CouponID ' +
    ' left join (select a.ID as CouponID, min(isnull(b.IsLock,0)) as IsLock from Data_CouponInfo a inner join Data_CouponList b on a.ID=b.CouponID group by a.ID) i on a.ID=i.CouponID ' +
    ' left join Base_UserInfo j on a.OpUserID=j.UserID ' +
    ' where a.MerchID=' + @MerchID + @SqlWhere
	exec (@sql)
	set @Result = 1
GO
/****** Object:  Table [dbo].[Search_Template_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Search_Template_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TempID] [int] NULL,
	[FieldName] [varchar](50) NULL,
	[Title] [varchar](50) NULL,
	[Datatype] [varchar](50) NULL,
	[Width] [int] NULL,
	[ShowColume] [int] NULL,
	[ShowSearch] [int] NULL,
	[DictID] [int] NULL,
 CONSTRAINT [PK_SEARCH_TEMPLATE_DETAIL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0则无默认值
   非0则查询字典表中PID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Search_Template_Detail', @level2type=N'COLUMN',@level2name=N'DictID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'查询模板内容' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Search_Template_Detail'
GO
/****** Object:  Table [dbo].[Search_Template]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Search_Template](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[PageName] [varchar](50) NULL,
	[ProcessName] [varchar](50) NULL,
 CONSTRAINT [PK_SEARCH_TEMPLATE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'当用户编号为0时则为公共查询模板' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Search_Template', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'查询模板，查询喜好' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Search_Template'
GO
/****** Object:  Table [dbo].[t_MPOrder]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[t_MPOrder](
	[OrderNumber] [varchar](200) NULL,
	[StoreId] [varchar](200) NULL,
	[CreateTime] [varchar](50) NULL,
	[MerchID] [varchar](20) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Store_HeadTotal]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Store_HeadTotal](
	[ID] [int] NOT NULL,
	[CheckDate] [date] NULL,
	[HeadID] [varchar](10) NULL,
	[CoinFromCard] [int] NULL,
	[CoinFromDigit] [int] NULL,
	[CoinFromReal] [int] NULL,
	[CoinFromFree] [int] NULL,
	[OutFromCard] [int] NULL,
	[OutFromPrint] [int] NULL,
	[OutFromReal] [int] NULL,
 CONSTRAINT [PK_STORE_HEADTOTAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'卡头营业总结数据存储' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Store_HeadTotal'
GO
/****** Object:  Table [dbo].[Store_GameTotal]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Store_GameTotal](
	[ID] [int] NOT NULL,
	[CheckDate] [date] NULL,
	[GameiD] [varchar](10) NULL,
	[CoinFromCard] [int] NULL,
	[CoinFromDigit] [int] NULL,
	[CoinFromReal] [int] NULL,
	[CoinFromFree] [int] NULL,
	[OutFromCard] [int] NULL,
	[OutFromPrint] [int] NULL,
	[OutFromReal] [int] NULL,
 CONSTRAINT [PK_STORE_GAMETOTAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'游戏机营业总结数据存储' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Store_GameTotal'
GO
/****** Object:  Table [dbo].[Store_CheckDate]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Store_CheckDate](
	[ID] [int] NOT NULL,
	[CheckDate] [date] NULL,
	[StoreID] [varchar](10) NULL,
	[PostDate] [datetime] NULL,
	[Cash] [numeric](11, 2) NULL,
	[AliPay] [numeric](11, 2) NULL,
	[Wechat] [numeric](11, 2) NULL,
	[BankPOS] [numeric](11, 2) NULL,
	[CardNumberCount] [int] NULL,
	[CardCoins] [int] NULL,
	[CardDepositCount] [numeric](11, 2) NULL,
	[ICPushCoins] [int] NULL,
	[DigiteCoins] [int] NULL,
	[OutCoins] [int] NULL,
	[LotteryCount] [int] NULL,
	[LotteryCoins] [int] NULL,
	[ChargeCoins] [int] NULL,
	[NoChargeCoins] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'营业日期结账存储表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Store_CheckDate'
GO
/****** Object:  Table [dbo].[XC_WorkInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[XC_WorkInfo](
	[WorkID] [int] IDENTITY(1,1) NOT NULL,
	[WorkType] [int] NULL,
	[SenderID] [int] NULL,
	[SenderTime] [datetime] NULL,
	[WorkState] [int] NULL,
	[WorkBody] [varchar](500) NULL,
	[AuditorID] [int] NULL,
	[AuditTime] [datetime] NULL,
	[AuditBody] [varchar](500) NULL,
 CONSTRAINT [PK_XC_WORKINFO] PRIMARY KEY CLUSTERED 
(
	[WorkID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'见字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'XC_WorkInfo', @level2type=N'COLUMN',@level2name=N'SenderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'莘宸平台工单管理信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'XC_WorkInfo'
GO
/****** Object:  StoredProcedure [dbo].[GetFoodPoint]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodPoint](@StoreId varchar(20),@FoodId int,@Point int output)
as
  set @Point = 0
	--declare @FoodType int = 0
	--declare @ContainID int = 0
	--declare @ContainCount int = 0
	--declare @TmpPoint int = 0
	--set @Point = 0
	--declare cur cursor for
	--select b.FoodType,b.ContainID,ContainCount from Data_FoodInfo a inner join Data_Food_Detial b on a.FoodID = b.FoodID
	--where a.FoodID = @FoodId and b.Status = 1
	--open cur  
	--fetch next from cur into @FoodType,@ContainID,@ContainCount
	--WHILE @@FETCH_STATUS = 0
	--  begin
	--	--0 代币;1 餐饮;2 礼品;3 门票	
	--	if @FoodType = 0
	--	  begin
	--		exec GetCoinSalePoint @StoreID,@ContainCount,@TmpPoint output
	--		set @Point = @Point + @TmpPoint
	--	  end
	--	else if @FoodType = 1
	--	  begin
	--	    set @TmpPoint = 0
	--		select @TmpPoint = Points from Base_GoodsInfo where ID = @ContainID
	--		set @Point = @Point + @TmpPoint * @ContainCount
	--	  end
	--	else if @FoodType = 2
	--	  begin
	--	    set @TmpPoint = 0
	--		select @TmpPoint = Points from Base_GoodsInfo where ID = @ContainID
	--		set @Point = @Point + @TmpPoint * @ContainCount
	--	  end
	--	fetch next from cur into @FoodType,@ContainID,@ContainCount
	--  end
	--close cur  
	--deallocate cur
GO
/****** Object:  UserDefinedTableType [dbo].[FoodDetailType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[FoodDetailType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodCount] [int] NULL,
	[PayType] [int] NULL,
	[PayNum] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FoodDetailLevelQueryType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[FoodDetailLevelQueryType] AS TABLE(
	[FoodId] [int] NULL
)
GO
/****** Object:  Table [dbo].[Flw_Transfer]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Transfer](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ExitFlwID] [int] NULL,
	[CardIDOut] [int] NULL,
	[CardIDIn] [int] NULL,
	[TransferType] [int] NULL,
	[Coins] [int] NULL,
	[BalanceOut] [int] NULL,
	[BalanceIn] [int] NULL,
	[RealTime] [datetime] NULL,
	[UserID] [int] NULL,
	[WorkStation] [varchar](255) NULL,
	[ScheduleID] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 退币
   1 兑币
   2 兑票
   3 其他' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Transfer', @level2type=N'COLUMN',@level2name=N'TransferType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员币过户流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Transfer'
GO
/****** Object:  Table [dbo].[Flw_Ticket_Exit]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Ticket_Exit](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](10) NULL,
	[Segment] [varchar](4) NULL,
	[HeadAddress] [varchar](2) NULL,
	[Barcode] [varchar](14) NULL,
	[Coins] [int] NULL,
	[RealTime] [datetime] NULL,
	[CoinMoney] [numeric](10, 2) NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[AuthorID] [int] NULL,
	[Note] [varchar](100) NULL,
	[WorkStation] [varchar](50) NULL,
	[ChargeTime] [datetime] NULL,
	[State] [int] NULL,
	[PWD] [varchar](2) NULL,
	[isNoAllow] [int] NULL,
	[CardID] [int] NULL,
 CONSTRAINT [PK_FLW_TICKET_EXIT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Segment' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'Segment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HeadAddress' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'HeadAddress'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Barcode' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'Barcode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Note' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ChargeTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'ChargeTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0:未兑
   1:已兑' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PWD' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'PWD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'isNoAllow' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit', @level2type=N'COLUMN',@level2name=N'isNoAllow'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数字币出票器出票流水' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Ticket_Exit'
GO
/****** Object:  Table [dbo].[Flw_Schedule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Schedule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[CheckDateID] [int] NULL,
	[UserID] [int] NULL,
	[OpenTime] [datetime] NULL,
	[ShiftTime] [datetime] NULL,
	[CheckDate] [date] NULL,
	[State] [int] NULL,
	[RealCash] [numeric](18, 2) NULL,
	[RealCredit] [numeric](18, 2) NULL,
	[RealCoin] [int] NULL,
	[WorkStation] [varchar](50) NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_FLW_SCHEDULE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OpenTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Schedule', @level2type=N'COLUMN',@level2name=N'OpenTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ShiftTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Schedule', @level2type=N'COLUMN',@level2name=N'ShiftTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 进行中 1 已交班 2 已审核' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Schedule', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'班次流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Schedule'
GO
/****** Object:  Table [dbo].[Flw_ProjectTicket_Entry]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_ProjectTicket_Entry](
	[ProjectCode] [varchar](32) NULL,
	[ProjectTicketID] [int] IDENTITY(1,1) NOT NULL,
	[TicketName] [varchar](100) NULL,
	[TicketType] [int] NULL,
	[DivideType] [int] NULL,
	[AllowExitTimes] [int] NULL,
	[ActiveBar] [int] NULL,
	[EffactType] [int] NULL,
	[EffactPeriodType] [int] NULL,
	[EffactPeriodValue] [int] NULL,
	[VaildPeriodType] [int] NULL,
	[VaildPeriodValue] [int] NULL,
	[VaildStartDate] [date] NULL,
	[VaildEndDate] [date] NULL,
	[WeekType] [int] NULL,
	[Week] [varchar](20) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[NoStartDate] [date] NULL,
	[NoEndDate] [date] NULL,
	[AccompanyCash] [numeric](10, 2) NULL,
	[BalanceIndex] [int] NULL,
	[BalanceValue] [numeric](10, 2) NULL,
	[AllowExitTicket] [int] NULL,
	[ExitPeriodType] [int] NULL,
	[ExitPeriodValue] [int] NULL,
	[ExitTicketType] [int] NULL,
	[ExitTicketValue] [numeric](10, 6) NULL,
	[AllowRestrict] [int] NULL,
	[RestrictShareCount] [int] NULL,
	[RestrictPeriodType] [int] NULL,
	[RestrictPreiodValue] [int] NULL,
	[RestrctCount] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_ProjectTicket_Bind]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flw_ProjectTicket_Bind](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjcetCode] [int] NULL,
	[ProjcetID] [int] NULL,
	[UseCount] [int] NULL,
	[AllowShareCount] [int] NULL,
	[WeightValue] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Flw_Project_TicketUse]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Project_TicketUse](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[ProjectTicketCode] [varchar](32) NULL,
	[MemberID] [int] NULL,
	[DeviceID] [int] NULL,
	[WorkType] [int] NULL,
	[WorkState] [int] NULL,
	[WorkChannel] [int] NULL,
	[WorkTime] [datetime] NULL,
 CONSTRAINT [PK_FLW_PROJECT_TICKETUSE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_Project_TicketInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Project_TicketInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodSaleID] [int] NULL,
	[CardID] [int] NULL,
	[ProjectID] [int] NULL,
	[ParentID] [varchar](32) NULL,
	[Barcode] [varchar](32) NULL,
	[SaleTime] [datetime] NULL,
	[ActiveTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[State] [int] NULL,
	[BuyCount] [int] NULL,
	[RemainCount] [int] NULL,
	[BuyPrice] [numeric](10, 2) NULL,
	[BalanceIndex] [int] NULL,
	[BalanceCount] [numeric](10, 2) NULL,
	[RemainDividePrice] [numeric](10, 2) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'由主门票分出来的附属门票
   例如 会员购买淘气堡10次，可分裂出一张门票拥有独立门票编号进行使用
   分裂权限可根据会员级别设定
   此功能常见自助机通过卡内余额自助打印门票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Project_TicketInfo', @level2type=N'COLUMN',@level2name=N'ParentID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未使用 
   1 已使用 
   2 被锁定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Project_TicketInfo', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门票购买详情' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Project_TicketInfo'
GO
/****** Object:  Table [dbo].[Flw_Project_PlayTime]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Project_PlayTime](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PID] [int] NULL,
	[StoreID] [varchar](10) NULL,
	[CardID] [int] NULL,
	[ProjectID] [int] NULL,
	[DeviceID] [int] NULL,
	[RecordType] [int] NULL,
	[RecordChannel] [int] NULL,
	[RecordTime] [datetime] NULL,
	[ChargeType] [int] NULL,
	[CycleType] [int] NULL,
	[LockMember] [int] NULL,
	[FreeMinute] [int] NULL,
	[BasePrice] [numeric](10, 2) NULL,
	[CycleTimes] [int] NULL,
	[TopPrice] [numeric](10, 2) NULL,
	[OutChargeType] [int] NULL,
	[ChargeBalanceIndex] [int] NULL,
	[ChargeCount] [numeric](10, 2) NULL,
	[Balance] [numeric](10, 2) NULL,
	[Cash] [numeric](10, 2) NULL,
	[PayType] [int] NULL,
	[PayOrderID] [varchar](32) NULL,
	[PayState] [int] NULL,
	[QrCode] [varchar](100) NULL,
 CONSTRAINT [PK_Flw_Project_Play_Time] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 签入扣押金
   1 计费
   2 签出还押金' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Project_PlayTime', @level2type=N'COLUMN',@level2name=N'RecordType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门票收费记录' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Project_PlayTime'
GO
/****** Object:  Table [dbo].[Flw_Order_SerialNumber]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Order_SerialNumber](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreiD] [varchar](15) NULL,
	[CreateDate] [date] NULL,
	[CurNumber] [int] NULL,
 CONSTRAINT [PK_FLW_ORDER_SERIALNUMBER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年月日(8)+门店编号(15)+6位流水号
   其中流水号，每家店每天从1开始' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order_SerialNumber', @level2type=N'COLUMN',@level2name=N'StoreiD'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'订单流水记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order_SerialNumber'
GO
/****** Object:  Table [dbo].[Flw_Order_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Order_Detail](
	[ID] [varchar](32) NOT NULL,
	[OrderFlwID] [varchar](32) NULL,
	[FoodFlwID] [varchar](32) NULL,
	[GoodsCount] [int] NULL,
 CONSTRAINT [PK_FLW_ORDER_DETAIL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年月日(8)+门店编号(15)+6位流水号
   其中流水号，每家店每天从1开始' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order_Detail', @level2type=N'COLUMN',@level2name=N'OrderFlwID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'购物车内容明细流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order_Detail'
GO
/****** Object:  Table [dbo].[Flw_Order]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Order](
	[ID] [varchar](32) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[OrderID] [varchar](32) NULL,
	[FoodCount] [int] NULL,
	[GoodCount] [int] NULL,
	[CardID] [int] NULL,
	[OrderSource] [int] NULL,
	[CreateTime] [datetime] NULL,
	[ModifyTime] [datetime] NULL,
	[PayTime] [datetime] NULL,
	[PayType] [int] NULL,
	[PayCount] [decimal](16, 2) NULL,
	[RealPay] [decimal](16, 2) NULL,
	[FreePay] [decimal](16, 2) NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[WorkStation] [varchar](50) NULL,
	[AuthorID] [int] NULL,
	[OrderStatus] [int] NULL,
	[SettleFlag] [int] NULL,
	[Note] [varchar](500) NULL,
	[PayFee] [decimal](16, 2) NULL,
 CONSTRAINT [PK_FLW_ORDER] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年月日(8)+门店编号(15)+6位流水号
   其中流水号，每家店每天从1开始' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'OrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 代币
   1 餐饮
   2 商品
   3 门票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'GoodCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 现金
   1 微信
   2 支付宝
   3 银联
   4 储值金
   5 代币
   6 彩票
   7 积分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'PayType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果是餐饮则此编号在餐饮表中
   如果是门票则在门票项目表中
   如果是商品则在商品表中' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'PayCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未结算
   1 等待支付
   2 已支付' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'OrderStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未结算
   1 已结算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order', @level2type=N'COLUMN',@level2name=N'SettleFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'购物车订单流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Order'
GO
/****** Object:  Table [dbo].[Flw_Lottery]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Lottery](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WorkType] [int] NULL,
	[GameID] [varchar](20) NULL,
	[HeadID] [varchar](20) NULL,
	[Barcode] [varchar](32) NULL,
	[LotteryCount] [int] NULL,
	[State] [int] NULL,
	[PrintTime] [datetime] NULL,
	[RealTime] [datetime] NULL,
	[CardID] [int] NULL,
	[GoodsFlwID] [int] NULL,
	[ScheduleID] [int] NULL,
	[UserID] [int] NULL,
	[WorkStation] [varchar](255) NULL,
 CONSTRAINT [PK_FLW_LOTTERY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0游戏机出彩票 
   1彩票转条码 
   2彩票充入会员  
   3条码充入会员 
   4条码购买商品 
   5游戏出电子彩票入会员' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Lottery', @level2type=N'COLUMN',@level2name=N'WorkType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0未用1已用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Lottery', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GoodsFlwID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Lottery', @level2type=N'COLUMN',@level2name=N'GoodsFlwID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'彩票流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Lottery'
GO
/****** Object:  Table [dbo].[Flw_Jackpot]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Jackpot](
	[ID] [int] NOT NULL,
	[PID] [int] NULL,
	[StoreID] [varchar](10) NULL,
	[MemberID] [int] NULL,
	[CardID] [int] NULL,
	[MatrixID] [int] NULL,
	[PrizeType] [int] NULL,
	[RealTime] [datetime] NULL,
 CONSTRAINT [PK_FLW_JACKPOT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'根据抽奖类型而定
   消费满额     对应    销售流水表
   ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Jackpot', @level2type=N'COLUMN',@level2name=N'PID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 消费满额
   1 关注
   2 推送
   3 其他' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Jackpot', @level2type=N'COLUMN',@level2name=N'PrizeType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户抽奖记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Jackpot'
GO
/****** Object:  Table [dbo].[Flw_GroupVerity]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_GroupVerity](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[TicketCode] [varchar](64) NULL,
	[GroupID] [varchar](64) NULL,
	[GroupType] [int] NULL,
	[FoodID] [int] NULL,
	[FoodName] [varchar](50) NULL,
	[Coin] [int] NULL,
	[Money] [numeric](10, 2) NULL,
	[VerityTime] [datetime] NULL,
	[VerityType] [int] NULL,
	[WorkStationID] [int] NULL,
	[ScheduleID] [int] NULL,
	[备注] [varchar](200) NULL,
 CONSTRAINT [PK_FLW_GROUPVERITY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_Goods]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Goods](
	[OrderID] [varchar](32) NOT NULL,
	[StoreID] [varchar](10) NULL,
	[CardID] [int] NULL,
	[IsCancel] [int] NULL,
	[GoodsMoney] [numeric](10, 2) NULL,
	[Coins] [int] NULL,
	[Point] [int] NULL,
	[Lottery] [int] NULL,
	[CoinBalance] [int] NULL,
	[PointBalance] [int] NULL,
	[LotteryBalance] [int] NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[RealTime] [datetime] NULL,
	[AuthorID] [int] NULL,
	[WorkStation] [varchar](50) NULL,
 CONSTRAINT [PK_FLW_GOODS] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GoodsID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'OrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N':0现金、1信用卡、2游戏币、3积分、4彩票、5微信、6、支付宝、X退货' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'IsCancel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GoodsMoney' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'GoodsMoney'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Point' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'Point'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Lottery' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'Lottery'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'CoinBalance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商品销售流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Goods'
GO
/****** Object:  Table [dbo].[Flw_Good_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Good_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [varchar](32) NULL,
	[Barcode] [varchar](20) NULL,
	[GoodsName] [varchar](50) NULL,
	[AllPrice] [numeric](6, 1) NULL,
	[Quantity] [int] NULL,
	[AllPoint] [int] NULL,
	[AllCoin] [int] NULL,
	[AllLottery] [int] NULL,
 CONSTRAINT [PK_FLW_GOOD_DETAIL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GoodsID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'OrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Barcode' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'Barcode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GoodsName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'GoodsName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllPrice' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'AllPrice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Quantity' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'Quantity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllPoint' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'AllPoint'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllCoin' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'AllCoin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllLottery' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail', @level2type=N'COLUMN',@level2name=N'AllLottery'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商品销售明细流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Good_Detail'
GO
/****** Object:  Table [dbo].[Flw_Giveback]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Giveback](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](10) NULL,
	[CardID] [int] NULL,
	[PICCardID] [int] NULL,
	[MemberName] [varchar](50) NULL,
	[RealTime] [datetime] NULL,
	[Coins] [int] NULL,
	[AuthorID] [int] NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[WorkStation] [varchar](255) NULL,
	[ExitRealTime] [datetime] NULL,
	[ExitAuthorID] [int] NULL,
	[ExitUserID] [int] NULL,
	[ExitScheduleID] [int] NULL,
	[ExitWorkStation] [varchar](255) NULL,
	[ExitMoney] [numeric](11, 2) NULL,
	[ExitAllCoins] [int] NULL,
	[ExitRealCoins] [int] NULL,
	[ExitBaseCoins] [int] NULL,
	[ExitState] [int] NULL,
	[ExitMin] [int] NULL,
	[ExitBackPrincipal] [varchar](255) NULL,
	[WinMoney] [numeric](11, 2) NULL,
	[MayCoins] [int] NULL,
	[LastTime] [datetime] NULL,
	[Balance] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在返到会员卡模式中与父卡号会一致' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'PICCardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MemberName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'MemberName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitUserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitUserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitWorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitWorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitMoney' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitMoney'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitAllCoins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitAllCoins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitRealCoins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitRealCoins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitBaseCoins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitBaseCoins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitState' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitState'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitMin' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitMin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExitBackPrincipal' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback', @level2type=N'COLUMN',@level2name=N'ExitBackPrincipal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'返还记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Giveback'
GO
/****** Object:  Table [dbo].[Flw_Game_WinPrize]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flw_Game_WinPrize](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameIndex] [int] NULL,
	[GoodID] [int] NULL,
	[MemberID] [int] NULL,
	[WinTime] [datetime] NULL,
	[HeadIndex] [int] NULL,
	[GoodPrice] [numeric](10, 2) NULL,
 CONSTRAINT [PK_FLW_GAME_WINPRIZE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Flw_Game_Watch]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Game_Watch](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameIndex] [int] NULL,
	[HeadIndex] [int] NULL,
	[CreateTime] [datetime] NULL,
	[UserID] [int] NULL,
	[InCoin] [int] NULL,
	[InCoinError] [int] NULL,
	[PrizeCount] [int] NULL,
	[PrizeError] [int] NULL,
	[GoodPrice] [numeric](10, 2) NULL,
	[OutCoin] [int] NULL,
	[OutCoinError] [int] NULL,
	[OutLottery] [int] NULL,
	[OutLotteryError] [int] NULL,
	[MediaUrl1] [varchar](200) NULL,
	[MediaUrl2] [varchar](200) NULL,
	[MediaUrl3] [varchar](200) NULL,
 CONSTRAINT [PK_FLW_GAME_WATCH] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_Game_Free]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Game_Free](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[GameID] [varchar](20) NULL,
	[HeadID] [varchar](10) NULL,
	[CardID] [int] NULL,
	[FreeCoin] [int] NULL,
	[RealTime] [datetime] NULL,
	[RuleID] [int] NULL,
	[Balance] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GameID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'GameID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HeadID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'HeadID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FreeCoin' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'FreeCoin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RuleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free', @level2type=N'COLUMN',@level2name=N'RuleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'游戏机送币流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Game_Free'
GO
/****** Object:  Table [dbo].[Flw_Food_SaleDetail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Food_SaleDetail](
	[ID] [varchar](32) NOT NULL,
	[FlwFoodID] [varchar](32) NULL,
	[SaleType] [int] NULL,
	[ContainID] [int] NULL,
	[ContainCount] [int] NULL,
	[ExpireDay] [datetime] NULL,
	[ValidType] [int] NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_FLW_FOOD_SALEDETIAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 代币
   1 餐饮
   2 商品
   3 门票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_SaleDetail', @level2type=N'COLUMN',@level2name=N'SaleType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果是餐饮则此编号在餐饮表中
   如果是门票则在门票项目表中
   如果是商品则在商品表中' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_SaleDetail', @level2type=N'COLUMN',@level2name=N'ContainID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'该字段只对门票有效
   0 未知
   1 计次票
   2 计时票
   3 迁入迁出票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_SaleDetail', @level2type=N'COLUMN',@level2name=N'ValidType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'套餐销售内容详情' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_SaleDetail'
GO
/****** Object:  Table [dbo].[Flw_Food_Sale_Pay]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Food_Sale_Pay](
	[ID] [varchar](32) NOT NULL,
	[FlwFoodID] [varchar](32) NULL,
	[BalanceIndex] [int] NULL,
	[PayCount] [decimal](18, 2) NULL,
	[Balance] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Table_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_Food_Sale]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Food_Sale](
	[ID] [varchar](32) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[FlowType] [int] NULL,
	[SingleType] [int] NULL,
	[FoodID] [int] NULL,
	[SaleCount] [int] NULL,
	[Point] [int] NULL,
	[PointBalance] [int] NULL,
	[MemberLevelID] [int] NULL,
	[Deposit] [numeric](5, 2) NULL,
	[OpenFee] [numeric](5, 2) NULL,
	[RenewFee] [numeric](5, 2) NULL,
	[ChangeFee] [numeric](5, 2) NULL,
	[TotalMoney] [numeric](5, 2) NULL,
	[Note] [varchar](200) NULL,
	[BuyFoodType] [int] NULL,
 CONSTRAINT [PK_Flw_Food_Sale] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Flw_Food_ExitDetail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flw_Food_ExitDetail](
	[ID] [int] NOT NULL,
	[ExitID] [int] NULL,
	[FoodType] [int] NULL,
	[ContainID] [int] NULL,
	[ContainCount] [int] NULL,
	[ExpireDay] [datetime] NULL,
	[ValidType] [int] NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_FLW_FOOD_EXITDETIAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 代币
   1 餐饮
   2 商品
   3 门票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_ExitDetail', @level2type=N'COLUMN',@level2name=N'FoodType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果是餐饮则此编号在餐饮表中
   如果是门票则在门票项目表中
   如果是商品则在商品表中' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_ExitDetail', @level2type=N'COLUMN',@level2name=N'ContainID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'该字段只对门票有效
   0 未知
   1 计次票
   2 计时票
   3 迁入迁出票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_ExitDetail', @level2type=N'COLUMN',@level2name=N'ValidType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'套餐退货内容详情' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_ExitDetail'
GO
/****** Object:  Table [dbo].[Flw_Food_Exit]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Food_Exit](
	[ExitID] [int] NOT NULL,
	[OrderID] [varchar](32) NULL,
	[StoreID] [varchar](10) NULL,
	[CardID] [int] NULL,
	[Point] [int] NULL,
	[CoinBalance] [int] NULL,
	[PointBalance] [int] NULL,
	[LotteryBalance] [int] NULL,
	[TotalMoney] [numeric](5, 2) NULL,
	[Note] [varchar](200) NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[AuthorID] [int] NULL,
	[RealTime] [datetime] NULL,
	[WorkStation] [varchar](50) NULL,
 CONSTRAINT [PK_FLW_FOOD_EXIT] PRIMARY KEY CLUSTERED 
(
	[ExitID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'与互联网支付通道订单号一致' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'OrderID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Point' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'Point'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'CoinBalance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'TotalMoney' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'TotalMoney'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Note' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'套餐退货流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Food_Exit'
GO
/****** Object:  Table [dbo].[Flw_Digite_Coin_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Digite_Coin_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FlwID] [int] NULL,
	[ICCardID] [varchar](16) NULL,
	[CoinQuantity] [int] NULL,
	[RealTime] [datetime] NULL,
 CONSTRAINT [PK_FLW_DIGITE_COIN_DETIAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin_Detail', @level2type=N'COLUMN',@level2name=N'FlwID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin_Detail', @level2type=N'COLUMN',@level2name=N'ICCardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CoinQuantity' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin_Detail', @level2type=N'COLUMN',@level2name=N'CoinQuantity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin_Detail', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数字币销售子表清单' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin_Detail'
GO
/****** Object:  Table [dbo].[Flw_Digite_Coin]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Digite_Coin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[CreditFee] [numeric](5, 2) NULL,
	[CoinQuantity] [int] NULL,
	[CountNum] [int] NULL,
	[TotalMoney] [numeric](10, 2) NULL,
	[Note] [varchar](200) NULL,
	[PayType] [int] NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[AuthorID] [int] NULL,
	[RealTime] [datetime] NULL,
	[WorkStation] [varchar](20) NULL,
 CONSTRAINT [PK_FLW_DIGITE_COIN] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FoodID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'FoodID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CreditFee' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'CreditFee'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CoinQuantity' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'CoinQuantity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CountNum' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'CountNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'TotalMoney' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'TotalMoney'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Note' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 现金
   1 信用卡
   2 微信
   3 支付宝
   4 银联' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'PayType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数字币销售流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Digite_Coin'
GO
/****** Object:  Table [dbo].[Flw_CouponUse]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_CouponUse](
	[ID] [int] NOT NULL,
	[StoreID] [varchar](15) NULL,
	[OrderFlwID] [int] NULL,
	[CouponID] [int] NULL,
	[CouponCode] [int] NULL,
	[FreeMoney] [numeric](10, 2) NULL,
	[Coins] [int] NULL,
 CONSTRAINT [PK_FLW_COUPONUSE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'年月日(8)+门店编号(15)+6位流水号
   其中流水号，每家店每天从1开始' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_CouponUse', @level2type=N'COLUMN',@level2name=N'OrderFlwID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'优惠券使用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_CouponUse'
GO
/****** Object:  Table [dbo].[Flw_Coin_Sale]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Coin_Sale](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CardID] [int] NULL,
	[Coins] [int] NULL,
	[Balance] [int] NULL,
	[WorkType] [int] NULL,
	[ScheduleID] [int] NULL,
	[RealTime] [datetime] NULL,
	[IsBirthday] [int] NULL,
	[UserID] [int] NULL,
	[AuthorID] [int] NULL,
	[WorkStation] [varchar](50) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'Balance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0售币机加币
   1售币机清币
   2手工实物币送币
   3手工实物币提币
   4手工存币
   5电子币送币
   6电子币提币
   7售币机实物币提币
   8售币机实物币送币' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'WorkType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0不是
   1是
   2表示生日和其他' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'IsBirthday'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'币余额变化流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Sale'
GO
/****** Object:  Table [dbo].[Flw_Coin_Exit]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_Coin_Exit](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FlowType] [int] NULL,
	[CardID] [int] NULL,
	[MemberLevelID] [int] NULL,
	[Coins] [int] NULL,
	[CoinMoney] [numeric](10, 2) NULL,
	[Deposit] [numeric](5, 1) NULL,
	[Balance] [int] NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[RealTime] [datetime] NULL,
	[AuthorID] [int] NULL,
	[Note] [varchar](100) NULL,
	[WorkStation] [varchar](50) NULL,
 CONSTRAINT [PK_FLW_COIN_EXIT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FlowType' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'FlowType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MemberLevelID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'MemberLevelID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Deposit' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'Deposit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'Balance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UserID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ScheduleID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'ScheduleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AuthorID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'AuthorID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Note' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'退卡退钱流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_Coin_Exit'
GO
/****** Object:  Table [dbo].[Flw_CheckDate]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_CheckDate](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[CheckDate] [date] NULL,
	[ScheduleName] [varchar](10) NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_FLW_CHECKDATE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 运营中
   1 已交班
   2 已结算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_CheckDate', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'营业日期班次流水' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_CheckDate'
GO
/****** Object:  Table [dbo].[Flw_485_SaveCoin]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_485_SaveCoin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[Segment] [varchar](4) NULL,
	[HeadAddress] [varchar](2) NULL,
	[CardID] [int] NULL,
	[Coins] [int] NULL,
	[Balance] [int] NULL,
	[RealTime] [datetime] NULL,
 CONSTRAINT [PK_FLW_485_SAVECOIN] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Segment' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'Segment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HeadAddress' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'HeadAddress'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'Balance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'下位机存币器存币数流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_SaveCoin'
GO
/****** Object:  Table [dbo].[Flw_485_Coin]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Flw_485_Coin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[Segment] [varchar](4) NULL,
	[HeadAddress] [varchar](2) NULL,
	[DeviceID] [int] NULL,
	[GameIndexID] [int] NULL,
	[SiteName] [varchar](20) NULL,
	[MemberID] [int] NULL,
	[CardID] [int] NULL,
	[Coins] [int] NULL,
	[CoinType] [int] NULL,
	[BalanceIndex] [int] NULL,
	[Balance] [int] NULL,
	[RealTime] [datetime] NULL,
 CONSTRAINT [PK_FLW_485_COIN] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Segment' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'Segment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'HeadAddress' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'HeadAddress'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ICCardID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'CardID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Coins' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'Coins'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0实物投币
   1刷卡投币
   2实物退币
   3IC卡出币
   4数字币投币' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'CoinType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Balance' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'Balance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RealTime' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin', @level2type=N'COLUMN',@level2name=N'RealTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'下位机控制器投币出币流水表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Flw_485_Coin'
GO
/****** Object:  UserDefinedFunction [dbo].[F_IsWorkDay]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_IsWorkDay]() RETURNS bit 
 as
   begin
       declare @RS bit = 0
	   declare @CurrentWeekValue int = 0
	   --计算当前日期是周几
	   select @CurrentWeekValue = datepart(weekday,getdate()) - 1
	   if @CurrentWeekValue = 0 
	     set @CurrentWeekValue = 7
	   if @CurrentWeekValue >= 1 and @CurrentWeekValue <= 5
	     begin
		   set @RS = 1	
	     end
	   else
	     begin
		   set @RS = 0	
	     end
	   return @RS  
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_IsWeekendDay]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_IsWeekendDay]() RETURNS bit 
 as
   begin
       declare @RS bit = 0
	   declare @CurrentWeekValue int = 0
	   --计算当前日期是周几
	   select @CurrentWeekValue = datepart(weekday,getdate()) - 1
	   if @CurrentWeekValue = 0 
	     set @CurrentWeekValue = 7
	   if @CurrentWeekValue >= 6 and @CurrentWeekValue <= 7
	     begin
		   set @RS = 1	
	     end
	   else
	     begin
		   set @RS = 0	
	     end
	   return @RS  
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_IsLeapYear]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_IsLeapYear](@Year CHAR(4))
RETURNS BIT
AS
BEGIN
    DECLARE @ReturnValue BIT
    DECLARE @iYear INT
    SET @iYear=CONVERT(INT,@Year)
 
    IF (@iYear % 4 = 0 and @iYear % 100 != 0) or (@iYear % 400 = 0)
        SET @ReturnValue = 1
    ELSE
        SET @ReturnValue = 0
    RETURN @ReturnValue
END
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetFlwId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_GetFlwId](@FlwSeedId varchar(29),@Index int )
RETURNS varchar(32)
as
begin
   declare @Str char(3) = ''
   declare @FlwId char(32) = ''
   if @Index < 10
     begin
       set @Str = '00' + CAST(@Index as char(1))
     end
   else if @Index < 100
     begin
       set @Str = '0' + CAST(@Index as char(2))
     end
   else 
       set @Str = CAST(@Index as char(3))       
   return @FlwSeedId + @Str
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetStartDate]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetStartDate](@Freq int) 
returns date
as
  begin
      declare @StartDate date
      --0 天,1 周,2 月,3 季,4 年
	  declare @Year char(4) = cast(datepart(year,GETDATE()) as CHAR(4))
	  declare @CurrentWeekValue int = 0
	  declare @CurrentQuarterValue int = 0
	  declare @CurrentDatetime datetime = getdate()
	  if @Freq = 0
		begin
		  set @StartDate = @CurrentDatetime
		end
	  else if @Freq = 1
		begin
		  select @CurrentWeekValue = datepart(weekday,@CurrentDatetime) - 1
		  if @CurrentWeekValue = 0
			begin
			  set @CurrentWeekValue = 7
			end
		  set @StartDate = DateAdd(dy,0 -@CurrentWeekValue + 1,@CurrentDatetime)
		end
	  else if @Freq = 2
		begin
		  declare @Month int = datepart(MONTH,GETDATE())
		  if @Month = 1
			begin
			  set @StartDate = CAST(@Year + '-01-01' as date)
			end	    
		  else if @Month = 2
			begin
			  set @StartDate = CAST(@Year + '-02-01' as date)
			end
		  else if @Month = 3
			begin
			  set @StartDate = CAST(@Year + '-03-01' as date)
			end  
		  else if @Month = 4
			begin
			  set @StartDate = CAST(@Year + '-04-01' as date)
			end 
		  else if @Month = 5
			begin
			  set @StartDate = CAST(@Year + '-05-01' as date)
			end
		  else if @Month = 6
			begin
			  set @StartDate = CAST(@Year + '-06-01' as date)
			end
		  else if @Month = 7
			begin
			  set @StartDate = CAST(@Year + '-07-01' as date)
			end
		  else if @Month = 8
			begin
			  set @StartDate = CAST(@Year + '-08-01' as date)
			end
		  else if @Month = 9
			begin
			  set @StartDate = CAST(@Year + '-09-01' as date)
			end
		  else if @Month = 10
			begin
			  set @StartDate = CAST(@Year + '-10-01' as date)
			end
		  else if @Month = 11
			begin
			  set @StartDate = CAST(@Year + '-11-01' as date)
			end
		  else if @Month = 12
			begin
			  set @StartDate = CAST(@Year + '-12-01' as date)
			end   
		end
	  else if @Freq = 3
		begin
		  select @CurrentQuarterValue = datepart(QUARTER,@CurrentDatetime)
		  if @CurrentQuarterValue = 1
			begin
			  set @StartDate = CAST(@Year + '-01-01' as date)
			end
		  else if @CurrentQuarterValue = 2
			begin
			  set @StartDate = CAST(@Year + '-04-01' as date)
			end
		  else if @CurrentQuarterValue = 3
			begin
			  set @StartDate = CAST(@Year + '-07-01' as date)
			end
		  else if @CurrentQuarterValue = 4
			begin
			  set @StartDate = CAST(@Year + '-10-01' as date)
			end
		end   
	  else if @Freq = 4
		begin
		  set @StartDate = CAST(@Year + '-01-01' as date)
		end
      return @StartDate
  end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetNewId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create FUNCTION [dbo].[F_GetNewId]()
RETURNS varchar(200)
as
begin
  
  return ''
end
GO
/****** Object:  StoredProcedure [dbo].[GetDiscountPrice]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetDiscountPrice](@FoodPrice decimal(18,2),@CouponDiscount decimal(18,2),@CouponCount int,@FreePrice decimal(18,2) output)
as
  declare @CouponIndex int = 1
  declare @TmpFreePrice decimal(18,6)
  set @TmpFreePrice = @FoodPrice
  while @CouponIndex <= @CouponCount
    begin
      set @TmpFreePrice = @TmpFreePrice * @CouponDiscount
      set @CouponIndex = @CouponIndex + 1
    end
  set @FreePrice = ROUND(@TmpFreePrice,2)
GO
/****** Object:  UserDefinedFunction [dbo].[GetFestival]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create function [dbo].[GetFestival]() RETURNS INT 
 as
   begin
	   return 1	
   end
GO
/****** Object:  Table [dbo].[Data_Workstation]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Workstation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[WorkStation] [varchar](50) NULL,
	[MacAddress] [varchar](50) NULL,
	[DiskID] [varchar](50) NULL,
	[State] [int] NULL,
	[UserID] [int] NULL,
	[UserOnlineState] [int] NULL,
	[ScheduleSender] [int] NULL,
	[DogID] [varchar](32) NULL,
 CONSTRAINT [PK_t_workstation] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [WorkStation_unique] UNIQUE NONCLUSTERED 
(
	[WorkStation] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_WorkFlowConfig]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_WorkFlowConfig](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[WorkName] [varchar](100) NULL,
	[WorkType] [int] NULL,
	[State] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_WORKFLOWCONFIG] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_WorkFlow_Node]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_WorkFlow_Node](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WorkID] [int] NULL,
	[OrderNumber] [int] NULL,
	[NodeType] [int] NULL,
	[UserType] [int] NULL,
	[AuthorFlag] [int] NULL,
	[Timeout] [int] NULL,
 CONSTRAINT [PK_DATA_WORKFLOW_NODE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_WorkFlow_Entry]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_WorkFlow_Entry](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WorkID] [int] NULL,
	[EventID] [int] NULL,
	[EventType] [int] NULL,
	[NodeID] [int] NULL,
	[UserID] [int] NULL,
	[State] [int] NULL,
	[CreateTime] [datetime] NULL,
	[AuthorID] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_WORKFLOW_ENTRY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_SupplierList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_SupplierList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[Supplier] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_SUPPLIERLIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Storage_Record]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Storage_Record](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StorageID] [int] NULL,
	[StorageFlag] [int] NULL,
	[StorageCount] [int] NULL,
	[CreateTime] [datetime] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_Flw_Storage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_StandardCoinPrice]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_StandardCoinPrice](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[BalanceIndex] [int] NULL,
	[CoinCount] [int] NULL,
	[CashPrice] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DATA_STANDARDCOINPRICE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_RuleOverlying_List]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_RuleOverlying_List](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[GroupID] [int] NULL,
	[RuleType] [int] NULL,
	[RuleID] [int] NULL,
 CONSTRAINT [PK_DATA_RULEOVERLYING_LIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_RuleOverlying_Group]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_RuleOverlying_Group](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_RULEOVERLYING_GROUP] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Reload]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Reload](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[GoodID] [int] NULL,
	[DeviceID] [int] NULL,
	[DeviceType] [int] NULL,
	[ReloadCount] [int] NULL,
	[ReloadType] [int] NULL,
	[UserID] [int] NULL,
	[RealTime] [datetime] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_RELOAD] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 售币机
   1 提币机
   2 游戏机
   3 自助售币机
   4 自助发卡机' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Reload', @level2type=N'COLUMN',@level2name=N'DeviceType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 实物币
   1 数字币
   2 会员卡
   3 实物礼品
   4 实物彩票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Reload', @level2type=N'COLUMN',@level2name=N'ReloadType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'币、会员卡、娃娃机、礼品柜安装记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Reload'
GO
/****** Object:  Table [dbo].[Data_PushRule_MemberLevelList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_PushRule_MemberLevelList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[PushRuleID] [int] NULL,
	[MemberLevelID] [int] NULL,
 CONSTRAINT [PK_DATA_PUSHRULE_MEMBERLEVELLI] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_PushRule_GameList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_PushRule_GameList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[PushRuleID] [int] NULL,
	[GameID] [int] NULL,
 CONSTRAINT [PK_DATA_PUSHRULE_GAMELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_PushRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_PushRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[Allow_Out] [int] NULL,
	[Allow_In] [int] NULL,
	[WeekType] [int] NULL,
	[Week] [int] NULL,
	[PushBalanceIndex1] [int] NULL,
	[PushCoin1] [int] NULL,
	[PushBalanceIndex2] [int] NULL,
	[PushCoin2] [int] NULL,
	[Level] [int] NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
 CONSTRAINT [PK_Data_Push_Rule] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_ProjectTimeInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_ProjectTimeInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[ProjectName] [varchar](100) NULL,
	[PayCycle] [int] NULL,
	[DepositType] [int] NULL,
	[Deposit] [int] NULL,
	[BackTime] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_PROJECTTIMEINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_ProjectTime_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_ProjectTime_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectTimeID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_PROJECTTIME_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_ProjectTime_BandPrice]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_ProjectTime_BandPrice](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectTimeID] [int] NULL,
	[MemberLevelID] [int] NULL,
	[BandType] [int] NULL,
	[BandCount] [int] NULL,
	[BalanceType] [int] NULL,
	[Count] [int] NULL,
 CONSTRAINT [PK_DATA_PROJECTTIME_BANDPRICE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_ProjectTicket_Bind]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_ProjectTicket_Bind](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjcetTicketID] [int] NULL,
	[ProjcetID] [int] NULL,
	[UseCount] [int] NULL,
	[AllowShareCount] [int] NULL,
	[WeightValue] [int] NULL,
 CONSTRAINT [PK_DATA_PROJECTTICKET_BIND] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_ProjectTicket]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_ProjectTicket](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[TicketName] [varchar](100) NULL,
	[TicketType] [int] NULL,
	[DivideType] [int] NULL,
	[BusinessType] [int] NULL,
	[AllowExitTimes] [int] NULL,
	[Price] [numeric](10, 2) NULL,
	[Tax] [numeric](10, 6) NULL,
	[GroupStartupCount] [int] NULL,
	[ReadFace] [int] NULL,
	[AllowCreatePoint] [int] NULL,
	[ActiveBar] [int] NULL,
	[SaleAuthor] [int] NULL,
	[EffactType] [int] NULL,
	[EffactPeriodType] [int] NULL,
	[EffactPeriodValue] [int] NULL,
	[VaildPeriodType] [int] NULL,
	[VaildPeriodValue] [int] NULL,
	[VaildStartDate] [date] NULL,
	[VaildEndDate] [date] NULL,
	[WeekType] [int] NULL,
	[Week] [varchar](20) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[NoStartDate] [date] NULL,
	[NoEndDate] [date] NULL,
	[AccompanyCash] [numeric](10, 2) NULL,
	[BalanceIndex] [int] NULL,
	[BalanceValue] [numeric](10, 2) NULL,
	[AllowExitTicket] [int] NULL,
	[ExitPeriodType] [int] NULL,
	[ExitPeriodValue] [int] NULL,
	[ExitTicketType] [int] NULL,
	[ExitTicketValue] [numeric](10, 6) NULL,
	[AllowRestrict] [int] NULL,
	[RestrictShareCount] [int] NULL,
	[RestrictPeriodType] [int] NULL,
	[RestrictPreiodValue] [int] NULL,
	[RestrctCount] [int] NULL,
	[Note] [varchar](500) NULL,
 CONSTRAINT [PK_DATA_PROJECTTICKET] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_ProjectInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_ProjectInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[ProjectName] [varchar](100) NULL,
	[PlayCount] [int] NULL,
	[PlayOnceFlag] [int] NULL,
	[ExpireDays] [int] NULL,
	[AccompanyFlag] [int] NULL,
	[AccompanyCash] [int] NULL,
	[BalanceType] [int] NULL,
	[BalanceCount] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_PROJECTINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 计次
   1 计时' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_ProjectInfo', @level2type=N'COLUMN',@level2name=N'ExpireDays'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'按分钟算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_ProjectInfo', @level2type=N'COLUMN',@level2name=N'AccompanyFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'以代币计算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_ProjectInfo', @level2type=N'COLUMN',@level2name=N'AccompanyCash'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'游乐项目信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_ProjectInfo'
GO
/****** Object:  Table [dbo].[Data_Project_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Project_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_PROJECT_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Project_BindGame]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Project_BindGame](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectID] [int] NULL,
	[GameID] [int] NULL,
 CONSTRAINT [PK_DATA_PROJECT_BINDGAME] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_Parameters]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Parameters](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[System] [varchar](30) NULL,
	[ParameterName] [varchar](50) NULL,
	[IsAllow] [int] NULL,
	[ParameterValue] [varchar](200) NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_PARAMETERS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'System' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'System'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ParameterName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'ParameterName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'IsAllow' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'IsAllow'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ParameterValue' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'ParameterValue'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Note' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户系统参数表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Parameters'
GO
/****** Object:  Table [dbo].[Data_Message]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Message](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Sender] [int] NULL,
	[SenderType] [int] NULL,
	[Receiver] [int] NULL,
	[RecvType] [int] NULL,
	[MsgType] [int] NULL,
	[SendTime] [datetime] NULL,
	[ReadFlag] [int] NULL,
	[ReadTime] [datetime] NULL,
	[MsgText] [varchar](1000) NULL,
 CONSTRAINT [PK_DATA_MESSAGE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消息列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Message'
GO
/****** Object:  Table [dbo].[Data_MerchAlipay_Shop]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_MerchAlipay_Shop](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[ShopID] [varchar](64) NULL,
 CONSTRAINT [PK_DATA_MERCHALIPAY_SHOP] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_MemberLevelFree]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_MemberLevelFree](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelID] [int] NULL,
	[ChargeTotal] [numeric](10, 2) NULL,
	[FreeBalanceType] [int] NULL,
	[FreeCount] [int] NULL,
	[MinSpaceDays] [int] NULL,
	[OnceFreeCount] [int] NULL,
	[ExpireDays] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBERLEVELFREE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_MemberLevel_Food]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_MemberLevel_Food](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberLevelID] [int] NULL,
	[FoodID] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBERLEVEL_FOOD] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_MemberLevel_BalanceCharge]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_MemberLevel_BalanceCharge](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelID] [int] NULL,
	[SourceBalanceIndex] [int] NULL,
	[SourceCount] [int] NULL,
	[TargetBalanceIndex] [int] NULL,
	[TargetCount] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBERLEVEL_BALANCECHA] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_MemberLevel_Balance]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_MemberLevel_Balance](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelID] [int] NULL,
	[BalanceIndex] [int] NULL,
	[ChargeOFF] [int] NULL,
	[MaxSaveCount] [int] NULL,
	[MaxUplife] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBERLEVEL_BALANCE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_MemberLevel]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_MemberLevel](
	[MemberLevelID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelName] [varchar](20) NULL,
	[CoverURL] [varchar](200) NULL,
	[OpenFee] [numeric](10, 2) NULL,
	[Deposit] [numeric](10, 2) NULL,
	[ClearPointDays] [int] NULL,
	[Validday] [int] NULL,
	[NeedAuthor] [int] NULL,
	[MustPhone] [int] NULL,
	[PhoneOnly] [int] NULL,
	[MustIDCard] [int] NULL,
	[UseReadID] [int] NULL,
	[IDCardOnly] [int] NULL,
	[ReadFace] [int] NULL,
	[ReadPlam] [int] NULL,
	[ChangeFee] [numeric](10, 2) NULL,
	[RechargeFee] [numeric](10, 2) NULL,
	[ContinueFee] [numeric](10, 2) NULL,
	[ContinueUsePoint] [int] NULL,
	[ConsumeTotle] [numeric](10, 2) NULL,
	[UpdateUsePoint] [int] NULL,
	[UpdateLevelID] [int] NULL,
	[NonActiveDays] [int] NULL,
	[ReduceLevelID] [int] NULL,
	[FreeRate] [int] NULL,
	[FreeCoin] [int] NULL,
	[FreeType] [int] NULL,
	[FreeNeedWin] [int] NULL,
	[BirthdayFree] [int] NULL,
	[FoodID] [int] NULL,
	[MinCoin] [int] NULL,
	[MaxCoin] [int] NULL,
	[AllowExitCard] [int] NULL,
	[AllowExitMoney] [int] NULL,
	[AllowExitCoinToCard] [int] NULL,
	[LockHead] [int] NULL,
	[AllowCharge] [int] NULL,
	[AllowSaveLottery] [int] NULL,
	[AllowSaveCoin] [int] NULL,
	[AllowGetCoin] [int] NULL,
	[AllowTransfer] [int] NULL,
	[State] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBERLEVEL] PRIMARY KEY CLUSTERED 
(
	[MemberLevelID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Member_Card_Store]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Member_Card_Store](
	[ID] [varchar](32) NOT NULL,
	[CardID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_MEMBER_CARD_STORE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员持卡有效门店列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card_Store'
GO
/****** Object:  Table [dbo].[Data_Member_Card]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Member_Card](
	[ID] [varchar](32) NOT NULL,
	[ICCardID] [varchar](20) NULL,
	[ParentCard] [int] NULL,
	[CardType] [int] NULL,
	[CardShape] [int] NULL,
	[CardLimit] [int] NULL,
	[AllowIn] [int] NULL,
	[AllowOut] [int] NULL,
	[MemberID] [varchar](32) NULL,
	[MemberLevelID] [int] NULL,
	[CreateTime] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[LastStore] [varchar](15) NULL,
	[UpdateTime] [datetime] NULL,
	[Deposit] [int] NULL,
	[RepeatCode] [int] NULL,
	[UID] [bigint] NULL,
	[CardStatus] [int] NULL,
 CONSTRAINT [PK_DATA_MEMBER_CARD] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'附属卡时，此字段显示母卡编号
   本身为母卡时，此字段为0' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card', @level2type=N'COLUMN',@level2name=N'ParentCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 主卡
   1 附属卡' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card', @level2type=N'COLUMN',@level2name=N'CardType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 普通卡
   1 数字手环
   2其他' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card', @level2type=N'COLUMN',@level2name=N'CardShape'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'若该商户离线则此卡无法使用，直到商户上线为止' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card', @level2type=N'COLUMN',@level2name=N'LastStore'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员持卡信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Member_Card'
GO
/****** Object:  Table [dbo].[Data_LotteryStorage]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_LotteryStorage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[Startcode] [varchar](10) NULL,
	[Endcode] [varchar](10) NULL,
	[SingleCount] [int] NULL,
	[StorageCount] [int] NULL,
	[DestroyTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'实物彩票入库记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_LotteryStorage'
GO
/****** Object:  Table [dbo].[Data_LotteryInventory]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_LotteryInventory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[DeviceID] [int] NULL,
	[PredictCount] [int] NULL,
	[Startcode] [varchar](10) NULL,
	[Endcode] [varchar](10) NULL,
	[InventroyCount] [int] NULL,
	[InventoryTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'实物彩票盘点记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_LotteryInventory'
GO
/****** Object:  Table [dbo].[Data_JackpotInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_JackpotInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchInfo] [varchar](15) NULL,
	[ActiveName] [varchar](50) NULL,
	[Threshold] [int] NULL,
	[Concerned] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Note] [varchar](5000) NULL,
 CONSTRAINT [PK_DATA_JACKPOTINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未开奖
   1 已抽取
   2 已过期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_JackpotInfo', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抽奖活动设定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_JackpotInfo'
GO
/****** Object:  Table [dbo].[Data_Jackpot_Matrix]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Jackpot_Matrix](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NULL,
	[ActiveID] [int] NULL,
	[GoodID] [varchar](20) NULL,
	[ActiveFlag] [int] NULL,
 CONSTRAINT [PK_DATA_JACKPOT_MATRIX] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未开奖
   1 已抽取
   2 已过期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Jackpot_Matrix', @level2type=N'COLUMN',@level2name=N'ActiveFlag'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抽奖活动奖池矩阵' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Jackpot_Matrix'
GO
/****** Object:  Table [dbo].[Data_Jackpot_Level]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Jackpot_Level](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ActiveID] [int] NULL,
	[LevelName] [varchar](50) NULL,
	[GoodID] [int] NULL,
	[GoodCount] [int] NULL,
	[Probability] [numeric](10, 6) NULL,
 CONSTRAINT [PK_DATA_JACKPOT_LEVEL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 未开奖
   1 已抽取
   2 已过期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Jackpot_Level', @level2type=N'COLUMN',@level2name=N'Probability'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'抽奖活动等级奖品设定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Jackpot_Level'
GO
/****** Object:  Table [dbo].[Data_GroupArea]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GroupArea](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[AreaName] [varchar](20) NULL,
 CONSTRAINT [PK_DATA_GROUPAREA] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodStorage_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_GoodStorage_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StorageID] [int] NULL,
	[GoodID] [int] NULL,
	[StorageCount] [int] NULL,
	[TotalPrice] [numeric](10, 2) NULL,
	[Price] [numeric](10, 2) NULL,
	[Tax] [numeric](10, 6) NULL,
	[TaxPrice] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DATA_GOODSTORAGE_DETAIL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_GoodStorage]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodStorage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[DepotID] [int] NULL,
	[Payable] [numeric](11, 2) NULL,
	[Payment] [numeric](11, 2) NULL,
	[Discount] [numeric](11, 2) NULL,
	[AuthorFlag] [int] NULL,
	[AuthorID] [int] NULL,
	[Supplier] [varchar](200) NULL,
	[UserID] [int] NULL,
	[RealTime] [datetime] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_GOODSTORAGE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商品入库流水' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GoodStorage'
GO
/****** Object:  Table [dbo].[Data_GoodStock_Record]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodStock_Record](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SourceType] [int] NULL,
	[SourceID] [int] NULL,
	[GoodID] [int] NULL,
	[DepotID] [int] NULL,
	[StockFlag] [int] NULL,
	[StockCount] [int] NULL,
	[CreateTime] [datetime] NULL,
	[Note] [varchar](200) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodsStock]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodsStock](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DepotID] [int] NULL,
	[GoodID] [int] NULL,
	[MinValue] [int] NULL,
	[MaxValue] [int] NULL,
	[InitialValue] [int] NULL,
	[InitialTime] [datetime] NULL,
	[RemainCount] [int] NULL,
	[InitialAvgValue] [numeric](10, 2) NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_GOODSSTOCK] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodRequest_List]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodRequest_List](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RequestID] [int] NULL,
	[GoodID] [int] NULL,
	[RequestCount] [int] NULL,
	[SendCount] [int] NULL,
	[StorageCount] [int] NULL,
	[CostPrice] [numeric](10, 2) NULL,
	[Tax] [numeric](10, 6) NULL,
	[InStoreID] [varchar](15) NULL,
	[InDeportID] [int] NULL,
	[OutStoreID] [varchar](15) NULL,
	[OutDepotID] [int] NULL,
	[SendTime] [datetime] NULL,
	[LogistType] [int] NULL,
	[LogistOrderID] [varchar](32) NULL,
 CONSTRAINT [PK_DATA_GOODREQUEST_LIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodRequest]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodRequest](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RequestCode] [varchar](32) NULL,
	[MerchID] [varchar](15) NULL,
	[CreateStoreID] [varchar](15) NULL,
	[CreateUserID] [int] NULL,
	[CreateTime] [datetime] NULL,
	[RequstType] [int] NULL,
	[RequestReason] [varchar](200) NULL,
	[RequestOutStoreID] [varchar](15) NULL,
	[RequestOutDepotID] [int] NULL,
	[RequestInStoreID] [varchar](15) NULL,
	[RequestInDepotID] [int] NULL,
 CONSTRAINT [PK_DATA_GOODREQUEST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodOutOrder_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodOutOrder_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[OrderID] [int] NULL,
	[GoodID] [int] NULL,
	[OutCount] [int] NULL,
	[WorkStation] [varchar](50) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodOutOrder]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodOutOrder](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[OrderID] [varchar](32) NULL,
	[OrderType] [int] NULL,
	[DepotID] [int] NULL,
	[CreateTime] [datetime] NULL,
	[OPUserID] [int] NULL,
	[AuthorID] [int] NULL,
	[AuthorTime] [datetime] NULL,
	[CancelUserID] [int] NULL,
	[CancelTime] [datetime] NULL,
	[WorkStation] [varchar](50) NULL,
	[State] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodInventory]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodInventory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[Barcode] [varchar](20) NULL,
	[InventoryType] [int] NULL,
	[InventoryIndex] [int] NULL,
	[PredictCount] [int] NULL,
	[InventoryCount] [int] NULL,
	[TotalPrice] [numeric](11, 2) NULL,
	[UserID] [int] NULL,
	[InventoryTime] [datetime] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_GOODINVENTORY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商品盘点记录' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GoodInventory'
GO
/****** Object:  Table [dbo].[Data_GoodExitInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodExitInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExitOrderID] [varchar](32) NULL,
	[StorageOrderID] [varchar](32) NULL,
	[DepotID] [int] NULL,
	[ExitCount] [int] NULL,
	[ExitCost] [numeric](10, 2) NULL,
	[ExitTotal] [numeric](10, 2) NULL,
	[Note] [varchar](200) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GoodExit_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GoodExit_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ExitOrderID] [varchar](32) NULL,
	[GoodID] [int] NULL,
	[ExitCount] [int] NULL,
	[ExitPrice] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DATA_GOODEXIT_DETAIL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GivebackRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GivebackRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelID] [int] NULL,
	[BackMin] [int] NULL,
	[BackMax] [int] NULL,
	[BackScale] [int] NULL,
	[ExitCardMin] [int] NULL,
	[AllowBackPrincipal] [int] NULL,
	[Backtype] [int] NULL,
	[TotalDays] [int] NULL,
	[AllowContainToday] [int] NULL,
 CONSTRAINT [PK_Data_GivebackRule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'back_min' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'BackMin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'back_max' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'BackMax'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'back_ratio' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'BackScale'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'exitcard_min' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'ExitCardMin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'allow_back_principal' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'AllowBackPrincipal'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 营业日期
   1 自然日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule', @level2type=N'COLUMN',@level2name=N'Backtype'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店返还规则设定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GivebackRule'
GO
/****** Object:  Table [dbo].[Data_GameInfo_Photo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GameInfo_Photo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameID] [int] NULL,
	[PhotoURL] [varchar](200) NULL,
	[UploadTime] [datetime] NULL,
 CONSTRAINT [PK_DATA_GAMEINFO_PHOTO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GameInfo_Ext]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GameInfo_Ext](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameID] [int] NULL,
	[GameCode] [varchar](50) NULL,
	[Area] [numeric](10, 6) NULL,
	[ChangeTime] [datetime] NULL,
	[Evaluation] [int] NULL,
	[Price] [int] NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[ValidFlag] [int] NULL,
	[LowLimit] [numeric](10, 2) NULL,
	[HighLimit] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DATA_GAMEINFO_EXT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_GameInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GameInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameID] [varchar](4) NULL,
	[StoreID] [varchar](15) NULL,
	[MerchID] [varchar](15) NULL,
	[AreaID] [int] NULL,
	[GameName] [varchar](50) NULL,
	[GameType] [varchar](50) NULL,
	[State] [varchar](1) NULL,
	[BalanceType] [int] NULL,
	[ReturnCheck] [int] NULL,
	[OutsideAlertCheck] [int] NULL,
	[ICTicketOperation] [int] NULL,
	[NotGiveBack] [int] NULL,
	[LotteryMode] [int] NULL,
	[OnlyExitLottery] [int] NULL,
	[chkCheckGift] [int] NULL,
	[AllowElecPush] [int] NULL,
	[AllowDecuplePush] [int] NULL,
	[GuardConvertCard] [int] NULL,
	[ReadCat] [int] NULL,
	[AllowRealPush] [int] NULL,
	[BanOccupy] [int] NULL,
	[StrongGuardConvertCard] [int] NULL,
	[PushControl] [int] NULL,
	[AllowElecOut] [int] NULL,
	[NowExit] [int] NULL,
	[BOLock] [int] NULL,
	[BOPulse] [int] NULL,
	[AllowRealOut] [int] NULL,
	[BOKeep] [int] NULL,
	[PushReduceFromCard] [int] NULL,
	[PushAddToGame] [int] NULL,
	[PushSpeed] [int] NULL,
	[PushPulse] [int] NULL,
	[PushLevel] [int] NULL,
	[PushStartInterval] [int] NULL,
	[UseSecondPush] [int] NULL,
	[SecondReduceFromCard] [int] NULL,
	[SecondAddToGame] [int] NULL,
	[SecondSpeed] [int] NULL,
	[SecondPulse] [int] NULL,
	[SecondLevel] [int] NULL,
	[SecondStartInterval] [int] NULL,
	[OutSpeed] [int] NULL,
	[OutPulse] [int] NULL,
	[CountLevel] [int] NULL,
	[OutLevel] [int] NULL,
	[OutReduceFromGame] [int] NULL,
	[OutAddToCard] [int] NULL,
	[OnceOutLimit] [int] NULL,
	[OncePureOutLimit] [int] NULL,
	[SSRTimeOut] [int] NULL,
	[ExceptOutTest] [int] NULL,
	[ExceptOutSpeed] [int] NULL,
	[Frequency] [int] NULL,
	[DepotID] [int] NULL,
 CONSTRAINT [PK_DATA_GAMEINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GameID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'GameID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GameName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'GameName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'State' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'遥控器偷分检测' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'ReturnCheck'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutsideAlertCheck' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutsideAlertCheck'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'LotteryMode' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'LotteryMode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OnlyExitLottery' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OnlyExitLottery'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllowElecPush' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'AllowElecPush'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllowDecuplePush' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'AllowDecuplePush'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'GuardConvertCard' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'GuardConvertCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllowRealPush' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'AllowRealPush'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'StrongGuardConvertCard' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'StrongGuardConvertCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushControl' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushControl'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllowElecOut' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'AllowElecOut'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'AllowRealOut' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'AllowRealOut'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushReduceFromCard' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushReduceFromCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushAddToGame' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushAddToGame'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushSpeed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushSpeed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushPulse' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushPulse'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushLevel' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'PushStartInterval' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'PushStartInterval'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'UseSecondPush' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'UseSecondPush'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondReduceFromCard' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondReduceFromCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondAddToGame' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondAddToGame'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondSpeed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondSpeed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondPulse' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondPulse'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondLevel' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'SecondStartInterval' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SecondStartInterval'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutSpeed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutSpeed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutPulse' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutPulse'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'CountLevel' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'CountLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutLevel' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutReduceFromGame' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutReduceFromGame'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OutAddToCard' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OutAddToCard'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OnceOutLimit' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OnceOutLimit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'OncePureOutLimit' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'OncePureOutLimit'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'退币保护盾参数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'SSRTimeOut'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExceptOutTest' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'ExceptOutTest'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ExceptOutSpeed' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'ExceptOutSpeed'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Frequency' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo', @level2type=N'COLUMN',@level2name=N'Frequency'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'游戏机信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_GameInfo'
GO
/****** Object:  Table [dbo].[Data_GameFreeRule_List]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_GameFreeRule_List](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RuleID] [int] NULL,
	[GameIndexID] [int] NULL,
 CONSTRAINT [PK_Data_GameFreeRule_List] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_GameFreeRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_GameFreeRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RuleType] [int] NULL,
	[MemberLevelID] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[NeedCoin] [int] NULL,
	[FreeCoin] [int] NULL,
	[ExitCoin] [int] NULL,
	[State] [int] NULL,
	[CreateTime] [datetime] NULL,
 CONSTRAINT [PK_t_game_free_rule] PRIMARY KEY NONCLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_GameAPP_Rule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_GameAPP_Rule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[GameID] [int] NULL,
	[PayCount] [numeric](10, 2) NULL,
	[PlayCount] [int] NULL,
 CONSTRAINT [PK_DATA_GAMEAPP_RULE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Game_StockInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Game_StockInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GameIndex] [int] NULL,
	[GoodID] [int] NULL,
	[DeportID] [int] NULL,
	[MinValue] [int] NULL,
	[MaxValue] [int] NULL,
	[InitialTime] [datetime] NULL,
	[InitialCount] [int] NULL,
	[RemainCount] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_GAME_STOCKINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_FreeCoinRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_FreeCoinRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[BalanceIndex] [int] NULL,
	[OnceSigleMax] [int] NULL,
	[OnceWarningValue] [int] NULL,
	[DayMax] [int] NULL,
	[DayWarningValue] [int] NULL,
 CONSTRAINT [PK_DATA_FREECOINRULE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_FoodInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_FoodInfo](
	[FoodID] [int] IDENTITY(1,1) NOT NULL,
	[FoodName] [varchar](50) NULL,
	[MerchID] [varchar](15) NULL,
	[Note] [varchar](200) NULL,
	[ImageURL] [varchar](200) NULL,
	[FoodType] [int] NULL,
	[AllowInternet] [int] NULL,
	[MeituanID] [varchar](32) NULL,
	[DianpinID] [varchar](32) NULL,
	[KoubeiID] [varchar](32) NULL,
	[AllowPrint] [int] NULL,
	[FoodState] [int] NULL,
	[ForeAuthorize] [int] NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[ForbidStart] [date] NULL,
	[ForbidEnd] [date] NULL,
	[ClientPrice] [decimal](18, 2) NULL,
	[MemberPrice] [decimal](18, 2) NULL,
	[Tax] [numeric](10, 6) NULL,
	[RenewDays] [int] NULL,
 CONSTRAINT [PK_DATA_FOODINFO] PRIMARY KEY CLUSTERED 
(
	[FoodID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FoodID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'FoodID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FoodName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'FoodName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'RebateNote' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Icon' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'ImageURL'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0售币
   1入会
   2数字币
   3商品
   4门票
   5餐饮
   6混合' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'FoodType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'餐饮、门票等需要二次条码凭证的' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'AllowPrint'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FoodState' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'FoodState'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0否，1是' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo', @level2type=N'COLUMN',@level2name=N'ForeAuthorize'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'在售套餐信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_FoodInfo'
GO
/****** Object:  Table [dbo].[Data_Food_WorkStation]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Food_WorkStation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[WorkStationID] [int] NULL,
 CONSTRAINT [PK_DATA_FOOD_WORKSTATION] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_Food_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Food_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_FOOD_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Food_Sale]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Food_Sale](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[BalanceType] [int] NULL,
	[UseCount] [int] NULL,
 CONSTRAINT [PK_DATA_FOOD_SALE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_Food_Record]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Food_Record](
	[FoodLevelID] [int] NULL,
	[RecordDate] [datetime] NULL,
	[day_sale_count] [int] NULL,
	[member_day_sale_count] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Data_Food_Level]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Food_Level](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[MemberLevelID] [int] NULL,
	[TimeType] [int] NULL,
	[Week] [varchar](20) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[VIPPrice] [numeric](10, 2) NULL,
	[ClientPrice] [numeric](10, 2) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[AllFreqType] [int] NULL,
	[AllCount] [int] NULL,
	[MemberFreqType] [int] NULL,
	[MemberCount] [int] NULL,
	[UpdateLevelID] [int] NULL,
	[PriorityLevel] [int] NULL,
 CONSTRAINT [PK_DATA_FOOD_LEVEL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1~7
   对应周一至周日
   用|隔开
   例如1|2|3|4|5|6|7表示周一至周日有效' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Level', @level2type=N'COLUMN',@level2name=N'Week'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'套餐对应会员级别，按级别独立价格体系' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Level'
GO
/****** Object:  Table [dbo].[Data_Food_Detial]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Food_Detial](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FoodID] [int] NULL,
	[FoodType] [int] NULL,
	[BalanceType] [int] NULL,
	[OperateType] [int] NULL,
	[WeightType] [int] NULL,
	[WeightValue] [decimal](18, 2) NULL,
	[ContainID] [int] NULL,
	[ContainCount] [int] NULL,
	[Days] [int] NULL,
	[ValidType] [int] NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_DATA_FOOD_DETIAL] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 代币
   1 餐饮
   2 礼品品
   3 门票
   4 扩展赠送跟会员级别绑定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Detial', @level2type=N'COLUMN',@level2name=N'FoodType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果是餐饮则此编号在餐饮表中
   如果是门票则在门票项目表中
   如果是商品则在商品表中
   如果是代币则此编号为0' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Detial', @level2type=N'COLUMN',@level2name=N'ContainID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'若为-1则无有效天数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Detial', @level2type=N'COLUMN',@level2name=N'Days'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'该字段只对门票有效
   0 未知
   1 计次票
   2 计时票
   3 迁入迁出票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Detial', @level2type=N'COLUMN',@level2name=N'ValidType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'套餐内容详情' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Food_Detial'
GO
/****** Object:  Table [dbo].[Data_DiscountStore_Record]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DiscountStore_Record](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRuleID] [int] NULL,
	[RecordDate] [date] NULL,
	[MerchId] [varchar](15) NULL,
	[StoreId] [varchar](15) NULL,
	[StoreFreq] [int] NULL,
	[UseCount] [int] NULL,
 CONSTRAINT [PK_Data_DiscountStore_Record] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_DiscountRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DiscountRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[RuleName] [varchar](100) NULL,
	[RuleLevel] [int] NULL,
	[StartDate] [date] NULL,
	[Enddate] [date] NULL,
	[WeekType] [int] NULL,
	[Week] [varchar](20) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[NoStartDate] [date] NULL,
	[NoEndDate] [date] NULL,
	[StoreFreq] [int] NULL,
	[StoreCount] [int] NULL,
	[ShareCount] [int] NULL,
	[MemberFreq] [int] NULL,
	[MemberCount] [int] NULL,
	[AllowGuest] [int] NULL,
	[Note] [varchar](200) NULL,
	[State] [int] NULL,
 CONSTRAINT [PK_Data_DiscountRule] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'MerchID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'满减规则名称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'RuleName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'优先级别' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'RuleLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'开始时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'StartDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'结束时间' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'Enddate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'周方式' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'WeekType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'有效周' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'Week'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'开始时段' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'StartTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'结束时段' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'EndTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'不可用开始日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'NoStartDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'不可用结束日期' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'NoEndDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店频率' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'StoreFreq'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店次数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'StoreCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店次数共享' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'ShareCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员频率' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'MemberFreq'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员次数' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'MemberCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'允许散客' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'AllowGuest'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'说明' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DiscountRule', @level2type=N'COLUMN',@level2name=N'Note'
GO
/****** Object:  Table [dbo].[Data_DiscountMember_Record]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DiscountMember_Record](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRuleID] [int] NULL,
	[RecordDate] [date] NULL,
	[MerchId] [varchar](15) NULL,
	[StoreId] [varchar](15) NULL,
	[MemberId] [varchar](32) NULL,
	[MemberFreq] [int] NULL,
	[UseCount] [int] NULL,
 CONSTRAINT [PK_Data_DiscountMember_Record] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Discount_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Discount_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRuleID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_Data_Discount_StoreList] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'流水号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_StoreList', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'规则编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_StoreList', @level2type=N'COLUMN',@level2name=N'DiscountRuleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_StoreList', @level2type=N'COLUMN',@level2name=N'StoreID'
GO
/****** Object:  Table [dbo].[Data_Discount_MemberLevel]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Discount_MemberLevel](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRuleID] [int] NULL,
	[MerchID] [varchar](15) NULL,
	[MemberLevelID] [int] NULL,
 CONSTRAINT [PK_Data_Discount_MemberLevel] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Discount_Detail]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Discount_Detail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DiscountRuleID] [int] NULL,
	[LimitCount] [decimal](18, 2) NULL,
	[LimitType] [int] NULL,
	[ConsumeCount] [decimal](18, 2) NULL,
	[DiscountCount] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Data_Discount_Detail] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'规则编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_Detail', @level2type=N'COLUMN',@level2name=N'DiscountRuleID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消费额度' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_Detail', @level2type=N'COLUMN',@level2name=N'LimitCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'额度类别' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_Detail', @level2type=N'COLUMN',@level2name=N'LimitType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'消费金额' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_Detail', @level2type=N'COLUMN',@level2name=N'ConsumeCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'优惠金额' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Discount_Detail', @level2type=N'COLUMN',@level2name=N'DiscountCount'
GO
/****** Object:  Table [dbo].[Data_DigitCoinFood]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DigitCoinFood](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[FoodName] [varchar](50) NULL,
	[BalanceIndex] [int] NULL,
	[Coins] [int] NULL,
	[AuthorFlag] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_DIGITCOINFOOD] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_DigitCoinDestroy]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DigitCoinDestroy](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[ICCardID] [varchar](16) NULL,
	[DestroyTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_DIGITCOINDESTROY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'数字币销毁记录' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_DigitCoinDestroy'
GO
/****** Object:  Table [dbo].[Data_DigitCoin]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_DigitCoin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[ICardID] [varchar](16) NULL,
	[UID] [int] NULL,
	[DigitLevelID] [int] NULL,
	[CreateTime] [datetime] NULL,
	[Status] [int] NULL,
	[Balance] [int] NULL,
	[EndDate] [datetime] NULL,
	[SaleDate] [datetime] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_CouponList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CouponList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CouponIndex] [int] NULL,
	[CouponCode] [varchar](32) NULL,
	[CouponID] [int] NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[PublishType] [int] NULL,
	[SendType] [int] NULL,
	[MemberID] [varchar](32) NULL,
	[SendAuthorID] [int] NULL,
	[SendTime] [datetime] NULL,
	[WorkStationID] [int] NULL,
	[State] [int] NULL,
	[IsLock] [int] NULL,
 CONSTRAINT [PK_DATA_COUPONLIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'实体优惠券编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponList', @level2type=N'COLUMN',@level2name=N'CouponCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 人工指定派发
   1 人工不记名派发
   2 抽奖
   3 赠送
   ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponList', @level2type=N'COLUMN',@level2name=N'SendType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 正常
   1 已派发
   2 已使用
   3 已锁定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponList', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'优惠券产生记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponList'
GO
/****** Object:  Table [dbo].[Data_CouponInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CouponInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[CouponLevel] [int] NULL,
	[CouponName] [varchar](50) NULL,
	[CouponType] [int] NULL,
	[EntryCouponFlag] [int] NULL,
	[AuthorFlag] [int] NULL,
	[AllowOverOther] [int] NULL,
	[OverUseCount] [int] NULL,
	[PublishCount] [int] NULL,
	[CouponValue] [numeric](10, 2) NULL,
	[CouponDiscount] [numeric](10, 2) NULL,
	[CouponThreshold] [numeric](10, 2) NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[WeekType] [int] NULL,
	[Week] [varchar](20) NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[NoStartDate] [date] NULL,
	[NoEndDate] [date] NULL,
	[SendType] [int] NULL,
	[OverMoney] [numeric](10, 2) NULL,
	[FreeCouponCount] [int] NULL,
	[JackpotCount] [int] NULL,
	[JackpotID] [int] NULL,
	[ChargeType] [int] NULL,
	[ChargeCount] [int] NULL,
	[BalanceIndex] [int] NULL,
	[GoodID] [int] NULL,
	[ProjectID] [int] NULL,
	[AutoSendCycle] [int] NULL,
	[AutoSendValue] [int] NULL,
	[AutoSendCount] [int] NULL,
	[CreateTime] [datetime] NULL,
	[OpUserID] [int] NULL,
	[Context] [varchar](200) NULL,
 CONSTRAINT [PK_DATA_COUPONINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 通用券
   1 代币券
   2 商品券
   3 餐饮券
   4 其他
   
   使用字典表使用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponInfo', @level2type=N'COLUMN',@level2name=N'CouponType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'优惠券生成信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CouponInfo'
GO
/****** Object:  Table [dbo].[Data_CouponCondition]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CouponCondition](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CouponID] [int] NULL,
	[ConditionType] [int] NULL,
	[ConditionID] [int] NULL,
	[ConnectType] [int] NULL,
	[ConditionValue] [varchar](100) NULL,
 CONSTRAINT [PK_DATA_COUPONCONDITION] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_Coupon_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Coupon_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CouponID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_COUPON_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_CoinStorage]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CoinStorage](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[StorageCount] [int] NULL,
	[DestroyTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_Data_CoinStorage] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代币入库记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CoinStorage'
GO
/****** Object:  Table [dbo].[Data_CoinInventory]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CoinInventory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[PredictCount] [int] NULL,
	[InventoryCount] [int] NULL,
	[InventoryTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_Data_CoinInventory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代币盘点记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CoinInventory'
GO
/****** Object:  Table [dbo].[Data_CoinDestory]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_CoinDestory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[StorageCount] [int] NULL,
	[DestroyTime] [datetime] NULL,
	[UserID] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_Data_CoinDestory] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'代币销毁记录表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_CoinDestory'
GO
/****** Object:  Table [dbo].[Data_Card_Right_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Card_Right_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CardRightID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_CARD_RIGHT_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户权限商户列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Card_Right_StoreList'
GO
/****** Object:  Table [dbo].[Data_Card_Right]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Data_Card_Right](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CardID] [int] NULL,
	[AllowPush] [int] NULL,
	[AllowOut] [int] NULL,
	[AllowExitCoin] [int] NULL,
	[AllowSaleCoin] [int] NULL,
	[AllowSaveCoin] [int] NULL,
	[AllowFreeCoin] [int] NULL,
	[AllowRenew] [int] NULL,
 CONSTRAINT [PK_DATA_CARD_RIGHT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员卡权限表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Card_Right'
GO
/****** Object:  Table [dbo].[Data_Card_Balance_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Card_Balance_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CardBalanceID] [int] NULL,
	[StoreID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_CARD_BALANCE_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户余额商户列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Card_Balance_StoreList'
GO
/****** Object:  Table [dbo].[Data_Card_Balance]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_Card_Balance](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [varchar](32) NULL,
	[BalanceIndex] [int] NULL,
	[Balance] [decimal](10, 2) NULL,
	[UpdateTime] [datetime] NULL,
 CONSTRAINT [PK_DATA_CARD_BALANCE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统字典表中枚举
   0 储值金
   1 代币
   2 积分
   3 彩票' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Card_Balance', @level2type=N'COLUMN',@level2name=N'BalanceIndex'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'卡内余额信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_Card_Balance'
GO
/****** Object:  Table [dbo].[Data_BillInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_BillInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreName] [varchar](100) NULL,
	[StoreID] [varchar](15) NULL,
	[Time] [datetime] NULL,
	[ReleaseTime] [datetime] NULL,
	[PicturePath] [varchar](200) NULL,
	[Title] [varchar](100) NULL,
	[PagePath] [varchar](max) NULL,
	[Publisher] [varchar](50) NULL,
	[State] [int] NULL,
	[PublishType] [int] NULL,
	[PromotionType] [int] NULL,
 CONSTRAINT [PK_DATA_BILLINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 无效
   1 有效' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_BillInfo', @level2type=N'COLUMN',@level2name=N'State'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'小程序首页活动推送类别
   0 轮播类别
   1 列表类别' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_BillInfo', @level2type=N'COLUMN',@level2name=N'PublishType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 开业促销
   1 限时抢购
   2 预售开团
   3 秒杀' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_BillInfo', @level2type=N'COLUMN',@level2name=N'PromotionType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'促销活动海报信息列表,关联小程序首页' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Data_BillInfo'
GO
/****** Object:  Table [dbo].[Data_BalanceType_StoreList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_BalanceType_StoreList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BalanceIndex] [int] NULL,
	[StroeID] [varchar](15) NULL,
 CONSTRAINT [PK_DATA_BALANCETYPE_STORELIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Data_BalanceChargeRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Data_BalanceChargeRule](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[SourceType] [int] NULL,
	[SourceCount] [numeric](10, 2) NULL,
	[ChargeType] [int] NULL,
	[ChargeCount] [numeric](10, 2) NULL,
	[AlertValue] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DATA_BALANCECHARGERULE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_UserInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_UserInfo](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[AgentID] [int] NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[UserType] [int] NULL,
	[IsAdmin] [int] NULL,
	[LogName] [varchar](50) NULL,
	[LogPassword] [varchar](32) NULL,
	[OpenID] [varchar](32) NULL,
	[RealName] [varchar](50) NULL,
	[Mobile] [varchar](16) NULL,
	[ICCardID] [varchar](16) NULL,
	[CreateTime] [datetime] NULL,
	[Status] [int] NULL,
	[Auditor] [int] NULL,
	[AuditorTime] [datetime] NULL,
	[UserGroupID] [int] NULL,
	[AuthorTempID] [int] NULL,
	[UnionID] [varchar](64) NULL,
	[SwitchMerch] [int] NULL,
	[SwitchStore] [int] NULL,
	[SwitchWorkstation] [int] NULL,
 CONSTRAINT [PK_BASE_USERINFO] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用于判断用户层级，以及用户归属' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo', @level2type=N'COLUMN',@level2name=N'AgentID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果为莘宸管理员则此字段为0' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo', @level2type=N'COLUMN',@level2name=N'StoreID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'莘宸用户
   普通商户
   大客户
   代理商' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo', @level2type=N'COLUMN',@level2name=N'UserType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'需要关注公众号进行操作并绑定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo', @level2type=N'COLUMN',@level2name=N'OpenID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'工作组编号，按工作组来分配权限模板' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo', @level2type=N'COLUMN',@level2name=N'UserGroupID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'操作员信息表，包括所有平台用户' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserInfo'
GO
/****** Object:  Table [dbo].[Base_UserGroup_Grant]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Base_UserGroup_Grant](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GroupID] [int] NULL,
	[FunctionID] [int] NULL,
	[IsAllow] [int] NULL,
 CONSTRAINT [PK_BASE_USERGROUP_GRANT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup_Grant', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'从dict_functionmenu中遍历，继承于base_merchfunction' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup_Grant', @level2type=N'COLUMN',@level2name=N'FunctionID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ParameterName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup_Grant', @level2type=N'COLUMN',@level2name=N'IsAllow'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户工作组权限列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup_Grant'
GO
/****** Object:  Table [dbo].[Base_UserGroup]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_UserGroup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[GroupName] [varchar](50) NULL,
	[Note] [varchar](500) NULL,
 CONSTRAINT [PK_BASE_USERGROUP] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'System' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup', @level2type=N'COLUMN',@level2name=N'GroupName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ParameterName' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup', @level2type=N'COLUMN',@level2name=N'Note'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户功能权限分组表,工作组' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGroup'
GO
/****** Object:  Table [dbo].[Base_UserGrant]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Base_UserGrant](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[GrantID] [int] NULL,
	[GrantEN] [int] NULL,
 CONSTRAINT [PK_BASE_USERGRANT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGrant', @level2type=N'COLUMN',@level2name=N'UserID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'从字典表读取' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGrant', @level2type=N'COLUMN',@level2name=N'GrantID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户授权功能列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_UserGrant'
GO
/****** Object:  Table [dbo].[Base_StoreWeight_Game]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Base_StoreWeight_Game](
	[WeightID] [int] NOT NULL,
	[GameID] [int] NOT NULL,
 CONSTRAINT [PK_Base_StoreWeight_Game] PRIMARY KEY CLUSTERED 
(
	[WeightID] ASC,
	[GameID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店权重游戏机列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreWeight_Game'
GO
/****** Object:  Table [dbo].[Base_StoreWeight]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_StoreWeight](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](20) NULL,
	[BossID] [int] NULL,
	[WeightValue] [int] NULL,
	[WeightType] [int] NULL,
 CONSTRAINT [PK_BASE_STOREWEIGHT] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 按全场
   1 按游戏机' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreWeight', @level2type=N'COLUMN',@level2name=N'WeightType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店权重管理' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreWeight'
GO
/****** Object:  Table [dbo].[Base_StoreInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_StoreInfo](
	[StoreID] [varchar](15) NOT NULL,
	[ParentID] [varchar](15) NULL,
	[MerchID] [varchar](15) NULL,
	[StoreName] [varchar](50) NULL,
	[StoreTag] [int] NULL,
	[Password] [varchar](100) NULL,
	[AuthorExpireDate] [datetime] NULL,
	[AreaCode] [varchar](10) NULL,
	[Address] [varchar](100) NULL,
	[Lng] [numeric](10, 6) NULL,
	[Lat] [numeric](10, 6) NULL,
	[Contacts] [varchar](50) NULL,
	[IDCard] [varchar](20) NULL,
	[IDExpireDate] [date] NULL,
	[Mobile] [varchar](15) NULL,
	[ShopSignPhoto] [varchar](100) NULL,
	[LicencePhoto] [varchar](100) NULL,
	[LicenceID] [varchar](64) NULL,
	[LicenceExpireDate] [date] NULL,
	[BankType] [varchar](32) NULL,
	[BankCode] [varchar](50) NULL,
	[BankAccount] [varchar](50) NULL,
	[SelttleType] [int] NULL,
	[SettleID] [int] NULL,
	[StoreState] [int] NULL,
 CONSTRAINT [PK_Base_StoreInfo] PRIMARY KEY CLUSTERED 
(
	[StoreID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果此字段为0，则为根节点/总店' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'ParentID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'坐标系为BD09百度地图' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'Lng'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'坐标系为BD09百度地图' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'Lat'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'微信公众号下的OPENID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'LicenceID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'最小为0.01元，不足按0.01元算' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'BankAccount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 不采用三方结算
   1 微信支付宝官方通道
   2 新大陆
   3 拉卡拉' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'SelttleType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统字典表中枚举
   0 无效
   1 有效
   2 暂停
   3 注销' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo', @level2type=N'COLUMN',@level2name=N'StoreState'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店档案' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreInfo'
GO
/****** Object:  Table [dbo].[Base_StoreHKConfig]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_StoreHKConfig](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[HKShopID] [varchar](10) NULL,
	[HKStoreSecret] [varchar](16) NULL,
	[HKOrgID] [varchar](16) NULL,
	[HKMerchID] [varchar](16) NULL,
	[HKAppSecret] [varchar](64) NULL,
	[HKAppID] [varchar](64) NULL,
 CONSTRAINT [PK_Base_StoreHKConfig] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'流水号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'MerchID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'门店编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'StoreID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'好酷门店号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKShopID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'好酷店秘钥' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKStoreSecret'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'好酷机构号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKOrgID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'好酷商户号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKMerchID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'好酷应用秘钥' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKAppSecret'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'相关公众号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreHKConfig', @level2type=N'COLUMN',@level2name=N'HKAppID'
GO
/****** Object:  Table [dbo].[Base_StoreDogList]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_StoreDogList](
	[ID] [int] NOT NULL,
	[MerchID] [varchar](15) NULL,
	[DogID] [varchar](128) NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK_BASE_STOREDOGLIST] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户对应加密狗列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_StoreDogList'
GO
/****** Object:  Table [dbo].[Base_StorageInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_StorageInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DepotID] [int] NULL,
	[GoodID] [int] NULL,
	[Initial] [int] NULL,
	[InitialTime] [datetime] NULL,
	[Surplus] [int] NULL,
	[MinValue] [int] NULL,
	[MaxValue] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_Base_StorageInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_SettlePPOS]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_SettlePPOS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchNo] [varchar](100) NULL,
	[TerminalNo] [varchar](100) NULL,
	[Token] [varchar](100) NULL,
	[InstNo] [varchar](100) NULL,
	[SettleFee] [numeric](18, 6) NULL,
 CONSTRAINT [PK_BASE_SETTLEPPOS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'新大陆结算通道配置' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_SettlePPOS'
GO
/****** Object:  Table [dbo].[Base_SettleOrg]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_SettleOrg](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WXPayOpenID] [varchar](32) NULL,
	[WXName] [varchar](50) NULL,
	[AliPay] [varchar](50) NULL,
	[AliPayName] [varchar](50) NULL,
	[SettleFee] [numeric](18, 6) NULL,
	[SettleCycle] [int] NULL,
	[SettleCount] [int] NULL,
 CONSTRAINT [PK_BASE_SETTLEORG] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'官方结算通道配置' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_SettleOrg'
GO
/****** Object:  Table [dbo].[Base_SettleLCPay]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_SettleLCPay](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchNo] [varchar](100) NULL,
	[TerminalNo] [varchar](100) NULL,
	[Token] [varchar](100) NULL,
	[InstNo] [varchar](100) NULL,
	[SettleFee] [numeric](18, 6) NULL,
 CONSTRAINT [PK_BASE_SETTLELCPAY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_MerchFunction]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_MerchFunction](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[FunctionID] [int] NULL,
	[FunctionEN] [int] NULL,
 CONSTRAINT [PK_BASE_MERCHFUNCTION] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'见字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MerchFunction', @level2type=N'COLUMN',@level2name=N'FunctionID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'莘宸商户允许开通功能列表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MerchFunction'
GO
/****** Object:  Table [dbo].[Base_MerchantInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_MerchantInfo](
	[MerchID] [varchar](15) NOT NULL,
	[MerchType] [int] NULL,
	[MerchAccount] [varchar](100) NULL,
	[MerchStatus] [int] NULL,
	[MerchPassword] [varchar](64) NULL,
	[MerchTag] [int] NULL,
	[WxOpenID] [varchar](32) NULL,
	[WxUnionID] [varchar](64) NULL,
	[Mobil] [varchar](15) NULL,
	[MerchName] [varchar](50) NULL,
	[AllowCreateCount] [int] NULL,
	[AllowCreateSub] [int] NULL,
	[CreateType] [int] NULL,
	[CreateUserID] [varchar](15) NULL,
	[Comment] [varchar](500) NULL,
 CONSTRAINT [PK_BASE_MERCHANTINFO] PRIMARY KEY CLUSTERED 
(
	[MerchID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 普通账号，只能开单个账号
   1 大客户账号，可以开多个账号，无结算关系
   2 代理商账号，可以开多个账号，存在结算关系' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MerchantInfo', @level2type=N'COLUMN',@level2name=N'MerchType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1，莘宸管理创建，用户编号为莘宸员工编号
   2，代理商创建，用户编号为代理商商户号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MerchantInfo', @level2type=N'COLUMN',@level2name=N'CreateType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户账号信息' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MerchantInfo'
GO
/****** Object:  Table [dbo].[Base_MerchAlipay]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_MerchAlipay](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[AppID] [varchar](32) NULL,
	[PrivateKey] [varchar](5000) NULL,
	[PublicKey] [varchar](5000) NULL,
	[Fee] [numeric](10, 6) NULL,
 CONSTRAINT [PK_BASE_MERCHALIPAY] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_MemberInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_MemberInfo](
	[ID] [varchar](32) NOT NULL,
	[Wechat] [varchar](64) NULL,
	[QQ] [varchar](64) NULL,
	[IMME] [varchar](64) NULL,
	[UserName] [varchar](50) NULL,
	[UserPassword] [varchar](32) NULL,
	[Gender] [int] NULL,
	[Birthday] [datetime] NULL,
	[IDCard] [varchar](18) NULL,
	[Mobile] [varchar](11) NULL,
	[EMail] [varchar](100) NULL,
	[LeftHandCode] [varchar](5000) NULL,
	[RightHandCode] [varchar](5000) NULL,
	[MemberState] [int] NULL,
	[CreateTime] [datetime] NULL,
	[Photo] [varchar](100) NULL,
	[Note] [text] NULL,
 CONSTRAINT [PK_BASE_MEMBERINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'会员编号' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MemberInfo', @level2type=N'COLUMN',@level2name=N'ID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'绑定微信号OPENID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MemberInfo', @level2type=N'COLUMN',@level2name=N'Wechat'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'用户昵称' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MemberInfo', @level2type=N'COLUMN',@level2name=N'UserName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统字典表中枚举
   0 注销
   1 正常
   2 挂失
   3 锁定' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MemberInfo', @level2type=N'COLUMN',@level2name=N'MemberState'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'客户基础档案' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_MemberInfo'
GO
/****** Object:  Table [dbo].[Base_GoodsInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_GoodsInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[StoreID] [varchar](15) NULL,
	[Barcode] [varchar](20) NULL,
	[MerchID] [varchar](15) NULL,
	[GoodName] [varchar](50) NULL,
	[GoodPhoteURL] [varchar](200) NULL,
	[GoodType] [int] NULL,
	[AllowStorage] [int] NULL,
	[Price] [numeric](10, 2) NULL,
	[Tax] [numeric](10, 6) NULL,
	[ReturnFlag] [int] NULL,
	[AllowCreatePoint] [int] NULL,
	[ReturnTime] [int] NULL,
	[TimeType] [int] NULL,
	[ReturnFee] [numeric](10, 6) NULL,
	[FeeType] [int] NULL,
	[Status] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_BASE_GOODSINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 销售商品       套餐、独立销售中显示
   1 兑换礼品       礼品兑换中显示
   2 办公用品
   3 终端设备
   4 系统耗材
   5 代币
   6 数字币
   7 会员卡
   9 奖品                抽奖活动中使用
   10 其他' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_GoodsInfo', @level2type=N'COLUMN',@level2name=N'GoodType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 停用
   1 正常' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_GoodsInfo', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商品信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_GoodsInfo'
GO
/****** Object:  Table [dbo].[Base_Goodinfo_Price]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Base_Goodinfo_Price](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GoodID] [int] NULL,
	[OperateTypei] [int] NULL,
	[BalanceIndex] [int] NULL,
	[Count] [numeric](10, 2) NULL,
 CONSTRAINT [PK_BASE_GOODINFO_PRICE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Base_EnumParams]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_EnumParams](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[Value] [varchar](50) NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Base_EnumParams] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[Value] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_DeviceInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_DeviceInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[DeviceName] [varchar](50) NULL,
	[SiteName] [varchar](20) NULL,
	[type] [int] NULL,
	[Token] [varchar](32) NULL,
	[GameIndexID] [int] NULL,
	[BindDeviceID] [int] NULL,
	[CmdType] [int] NULL,
	[segment] [varchar](4) NULL,
	[Address] [varchar](2) NULL,
	[MCUID] [varchar](64) NULL,
	[port_name] [varchar](10) NULL,
	[baute_rate] [int] NULL,
	[parity] [int] NULL,
	[IPAddress] [varchar](15) NULL,
	[WorkStation] [varchar](100) NULL,
	[DeviceStatus] [int] NULL,
	[DeviceLock] [int] NULL,
	[create_time] [datetime] NULL,
	[update_time] [datetime] NULL,
	[note] [varchar](255) NULL,
	[AllowPrint] [int] NULL,
	[BarCode] [varchar](100) NULL,
 CONSTRAINT [PK_BASE_DEVICEINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 卡头
   1 碎票机
   2 存币机
   3 提币机
   4 售币机
   5 投币机
   6 自助机
   7 闸机' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'type'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 RS232
   1 RS485
   2 TCP/IP' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'CmdType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'port_name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'port_name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'baute_rate' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'baute_rate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 None
   1 Even
   2 Odd' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'parity'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'WorkStation' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo', @level2type=N'COLUMN',@level2name=N'WorkStation'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'设备信息表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_DeviceInfo'
GO
/****** Object:  Table [dbo].[Base_DepotInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_DepotInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[StoreID] [varchar](15) NULL,
	[DepotName] [varchar](50) NULL,
	[MinusEN] [int] NULL,
 CONSTRAINT [PK_Base_DepotInfo] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Base_ChainRule_Store]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_ChainRule_Store](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RuleGroupID] [int] NULL,
	[StoreID] [varchar](20) NULL,
 CONSTRAINT [PK_BASE_CHAINRULE_STORE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户连锁互通规则设置门店子表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_ChainRule_Store'
GO
/****** Object:  Table [dbo].[Base_ChainRule]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Base_ChainRule](
	[RuleGroupID] [int] IDENTITY(1,1) NOT NULL,
	[GroupName] [varchar](20) NULL,
	[MerchID] [varchar](15) NULL,
	[RuleType] [int] NULL,
 CONSTRAINT [PK_BASE_CHAINRULE] PRIMARY KEY CLUSTERED 
(
	[RuleGroupID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'见字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_ChainRule', @level2type=N'COLUMN',@level2name=N'RuleType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'商户连锁互通规则设置' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Base_ChainRule'
GO
/****** Object:  UserDefinedTableType [dbo].[CouponListType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[CouponListType] AS TABLE(
	[CouponCode] [varchar](32) NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[CheckWeekStr]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[CheckWeekStr](@WeekStr varchar(20)) RETURNS INT 
 as
   begin
	   declare @Len int
	   declare @Index int = 0
	   declare @WeekValue char(1) = ''
	   declare @CurrentWeekValue int = 0
	   --计算当前日期是周几
	   select @CurrentWeekValue = datepart(weekday,getdate()) - 1 
	   if @CurrentWeekValue = 0 
	     set @CurrentWeekValue = 7
	     
	   set @Len = LEN(@WeekStr)
	   while @Index < @Len
		 begin
			select @WeekValue = substring(@WeekStr,(@Index + 1),1)
			if @WeekValue = cast(@CurrentWeekValue as CHAR(1))
			  begin
				return 1
			  end
			set @Index = @Index + 1
		 end
	   return 0	
   end
GO
/****** Object:  UserDefinedTableType [dbo].[CheckFoodDetailType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[CheckFoodDetailType] AS TABLE(
	[Emp_FoodDetailId] [varchar](50) NULL,
	[FoodId] [int] NULL,
	[FoodCount] [int] NULL,
	[PayType] [int] NULL,
	[PayNum] [decimal](18, 2) NULL,
	[Deposit] [decimal](18, 2) NULL,
	[FoodType] [int] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[CheckMobile]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[CheckMobile](@MobileStr varchar(11)) RETURNS INT 
 as
   begin
	 if LEN(@MobileStr) <> 11 
	   begin
		 return 0
	   end
	  
	 if ISNumeric(@MobileStr) <> 1
	   begin
		 return 0
	   end  
	   
	 return 1
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetCouponStatusName]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create FUNCTION [dbo].[F_GetCouponStatusName](@Status int)
RETURNS varchar(50)
as
begin
  declare @StatusName varchar(50) = '未知'
  if @Status = 0 
    set @StatusName = '未分配'
  else if @Status = 1 
    set @StatusName = '未激活'
  else if @Status = 2
    set @StatusName = '已激活'
  else if @Status = 3
    set @StatusName = '已使用'
  return @StatusName
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetCouponLockName]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_GetCouponLockName](@IsLock int)
RETURNS varchar(50)
as
begin
  declare @LockName varchar(50) = '未知'
  if @IsLock = 0 
    set @LockName = '正常'
  else if @IsLock = 1 
    set @LockName = '锁定'
  return @LockName
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetCardStatus]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[F_GetCardStatus](@CardStatus int)
RETURNS varchar(200)
as
begin
  if @CardStatus = 0 
    begin
      return '注销'
    end
  else if @CardStatus = 1
    begin
	  return '正常'	
    end
  else if @CardStatus = 2
    begin
      return '挂失'	
    end
  else if @CardStatus = 3
    begin
	  return '锁定'		
    end   
  return '状态异常'
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_CheckCustomerTypeParams]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_CheckCustomerTypeParams](@CustomerType int,@ICCardId int) RETURNS bit 
 as
   begin
     declare @Result bit = 1
	 if @CustomerType <> 0 and @CustomerType <> 1 and @CustomerType <> 2
	   begin
		 set @Result = 0	
	   end	
	 else
	   begin
		 if @CustomerType = 0 
		   begin
			 if @ICCardId <> 0
			   begin
				 set @Result = 0 
			   end	
		   end
		 else if @CustomerType = 1 
		   begin
			 if @ICCardId = 0 
			   begin
				 set @Result = 0 
			   end     
		   end
		 else if @CustomerType = 2 
		   begin
			 if @ICCardId = 0 
			   begin
				 set @Result = 0 
			   end 
		   end		 
	   end 	 	
	 return @Result
   end
GO
/****** Object:  Table [dbo].[Divide_ProjectInfo]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Divide_ProjectInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProjectCode] [varchar](32) NULL,
	[CheckDate] [date] NULL,
	[DivideType] [int] NULL,
	[DividePrice] [numeric](10, 2) NULL,
	[TicketPrice] [numeric](10, 2) NULL,
 CONSTRAINT [PK_DIVIDE_PROJECTINFO] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Dict_System]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dict_System](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PID] [int] NULL,
	[DictKey] [varchar](50) NULL,
	[DictValue] [varchar](50) NULL,
	[Comment] [varchar](500) NULL,
	[OrderID] [int] NULL,
	[Enabled] [int] NULL,
	[MerchID] [varchar](15) NULL,
	[DictLevel] [int] NULL,
 CONSTRAINT [PK_DICT_SYSTEM] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'0 停用
   1 启用' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dict_System', @level2type=N'COLUMN',@level2name=N'Enabled'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'如果为系统级的，只能有莘宸管理员维护
   如果为商户级的，只能有商户自行维护' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dict_System', @level2type=N'COLUMN',@level2name=N'DictLevel'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'系统字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dict_System'
GO
/****** Object:  Table [dbo].[Dict_FunctionMenu]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dict_FunctionMenu](
	[FunctionID] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NULL,
	[FunctionName] [varchar](50) NULL,
	[OrderID] [int] NULL,
	[Descript] [varchar](500) NULL,
	[PageName] [varchar](100) NULL,
	[ICON] [varchar](100) NULL,
	[MenuType] [int] NULL,
	[UseType] [int] NULL,
 CONSTRAINT [PK_DICT_FUNCTIONMENU] PRIMARY KEY CLUSTERED 
(
	[FunctionID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'功能菜单字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dict_FunctionMenu'
GO
/****** Object:  Table [dbo].[Dict_BalanceType]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dict_BalanceType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MerchID] [varchar](15) NULL,
	[TypeID] [int] NOT NULL,
	[TypeName] [varchar](50) NULL,
	[MappingType] [int] NULL,
	[State] [int] NULL,
	[DecimalNumber] [int] NULL,
	[AddingType] [int] NULL,
	[Note] [varchar](200) NULL,
 CONSTRAINT [PK_DICT_BALANCETYPE] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Dict_Area]    Script Date: 05/17/2018 09:37:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Dict_Area](
	[ID] [int] NOT NULL,
	[PID] [int] NULL,
	[AreaType] [int] NULL,
	[AreaName] [varchar](50) NULL,
	[ShortName] [varchar](20) NULL,
 CONSTRAINT [PK_DICT_AREA] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'行政区域字典表' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Dict_Area'
GO
/****** Object:  StoredProcedure [dbo].[DeleteOrderDetail]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[DeleteOrderDetail](@StoreId varchar(15),@OrderFlwId int)
as 
  create table #TempDetail(FlwOrderDetailId int,FlwFoodId int)
  
  insert #TempDetail(FlwOrderDetailId,FlwFoodId)
  select b.ID as FlwOrderDetailId,c.ID as FlwFoodId from Flw_Order a 
  inner join Flw_Order_Detail b on a.ID = b.OrderFlwID 
  inner join Flw_Food_Sale c on b.FoodFlwID = c.ID 
  where a.ID = @OrderFlwId and a.StoreID = @StoreId and c.StoreID = @StoreId

  delete from Flw_Order_Detail where ID in (select FlwOrderDetailId from #TempDetail)
  delete from Flw_Food_Sale where ID in (select FlwFoodId from #TempDetail)
  delete from Flw_Food_SaleDetail where FlwFoodID in (select FlwFoodId from #TempDetail)
GO
/****** Object:  StoredProcedure [dbo].[CheckEffectiveFood]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckEffectiveFood](@StoreId varchar(15),@FoodDetail [FoodDetailType] readonly,@ErrMsg varchar(200) output)
as
  declare @openCardFoodCount int = 0
  if exists ( select 1 from @FoodDetail a
			  left join 
			  (
				 select a.FoodID
				 from Data_Food_StoreList a inner join Data_FoodInfo b on a.FoodID = b.FoodID
				 where a.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
			  ) b on a.FoodID = b.FoodId
			  where b.FoodID is null )
    begin
      set @ErrMsg = '存在无效套餐'
      return 0
    end
  else
    begin
      return 1
    end
GO
/****** Object:  StoredProcedure [dbo].[AddGuestDiscount]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddGuestDiscount](@MerchId varchar(15),@StoreId varchar(15),@DiscountRuleID int,@StoreFreq int)
as
  if not exists ( select 0 from dbo.Data_DiscountMember_Record where DiscountRuleID = @DiscountRuleID )
    begin
	  insert Data_DiscountStore_Record(DiscountRuleID,RecordDate,MerchId,StoreId,StoreFreq,UseCount)
	  values(@DiscountRuleID,GETDATE(),@MerchId,@StoreId,@StoreFreq,1)
    end    
  else
    begin
       update Data_DiscountStore_Record set UseCount = UseCount + 1 where DiscountRuleID = @DiscountRuleID
    end
GO
/****** Object:  StoredProcedure [dbo].[AddGameWatch]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddGameWatch](
@StoreId varchar(15),
@HeadIndex int,
@UserId int,
@InCoin int,
@PrizeCount int,
@OutCoin int,
@OutLottery int,
@MediaUrl1 varchar(200),
@MediaUrl2 varchar(200),
@MediaUrl3 varchar(200),
@InCoinError int output,
@OutCoinError int output,
@PrizeError int output,
@OutLotteryError int output,
@GoodPrice decimal(18,2) output,
@ErrMsg varchar(200) output)
as
  declare @GameIndex int = 0
  declare @InCoinAll int = 0
  declare @PrizeAll int
  declare @OutCoinAll int = 0
  declare @OutLotteryAll int = 0
  declare @LastAddDateTime datetime = '1900-01-01 00:00:00'
  declare @GameIndexID int = 0
  
  if not exists (select COUNT(0) from Base_DeviceInfo where StoreID = @StoreId and ID = @HeadIndex)
    begin
      set @ErrMsg = '设备无效'
      return 0
    end
  
  --获取当前机头的最后提交时间
  select @LastAddDateTime = CreateTime from Flw_Game_Watch where HeadIndex = @HeadIndex
  --获取游戏机ID
  select @GameIndexID = GameIndexID from Base_DeviceInfo where ID = @HeadIndex
  
  --获取出币总量
  select @OutCoinAll = isnull(SUM(Coins),0) from dbo.Flw_485_Coin where StoreID = @StoreId and DeviceID = @HeadIndex and CoinType in (2,3) and RealTime >= @LastAddDateTime and RealTime <= GETDATE()
  --获取投币总量
  select @InCoinAll = isnull(SUM(Coins),0) from dbo.Flw_485_Coin where StoreID = @StoreId and DeviceID = @HeadIndex and CoinType in (0,1,4) and RealTime >= @LastAddDateTime and RealTime <= GETDATE()
  --中奖总金额
  select @PrizeAll = count(0),@GoodPrice = isnull(SUM(GoodPrice),0) from dbo.Flw_Game_WinPrize where HeadIndex = @HeadIndex and WinTime >= @LastAddDateTime and WinTime <= GETDATE()
  --出票总数
  select @OutLotteryAll = isnull(SUM(LotteryCount),0) from Flw_Lottery where HeadID = @HeadIndex and RealTime >= @LastAddDateTime and RealTime <= GETDATE() 
  
  set @InCoinError = @InCoin - @InCoinAll
  set @OutCoinError = @OutCoin - @OutCoinAll
  set @PrizeError = @PrizeCount - @PrizeAll
  set @OutLotteryError = @OutLottery - @OutLotteryAll

  insert Flw_Game_Watch
  (GameIndex,HeadIndex,CreateTime,UserID,InCoin,InCoinError,
  PrizeCount,PrizeError,GoodPrice,OutCoin,OutCoinError,OutLottery,
  OutLotteryError,MediaUrl1,MediaUrl2,MediaUrl3)
  values
  (@GameIndex,@HeadIndex,GETDATE(),@UserId,@InCoin,@InCoinError,
   @PrizeCount,@PrizeError,@GoodPrice,@OutCoin,@OutCoinError,@OutLottery,
   @OutLotteryError,@MediaUrl1,@MediaUrl2,@MediaUrl3)
   
  return 1
GO
/****** Object:  StoredProcedure [dbo].[CheckMemberLevel]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[CheckMemberLevel](@StoreId varchar(15),@MemberLevelId int,@ErrMsg varchar(200) output)
as
  if exists ( select 0 from Data_MemberLevel a
      where a.MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId) 
      and State = 1 and a.MemberLevelID = @MemberLevelId )
    begin
      return 1
    end
  else 
    begin
      set @ErrMsg = '会员级别Id无效'
      return 0
    end
GO
/****** Object:  StoredProcedure [dbo].[CheckStoreIsInAllGroupRule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckStoreIsInAllGroupRule](@StoreId varchar(15))
as
if exists ( select 0 from 
(select DictValue as RuleTypeKey from dict_system where PID = (select Id from dict_system where DictKey = '规则类别' and DictLevel = 0 and MerchID is null) ) a
left join ( select RuleType from Base_ChainRule_Store a inner join Base_ChainRule b on a.RuleGroupID = b.RuleGroupID where a.StoreID = @StoreId ) b 
on a.RuleTypeKey = b.RuleType where b.RuleType is null )
 return 0
else 
 return 1
GO
/****** Object:  StoredProcedure [dbo].[AddTicket]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddTicket](@TicketId int,@CardId int,@FoodFlwId int,@Days int,@ContainCount int)
as
  declare @NewId varchar(50)
  declare @Project_StoreID VARCHAR(15)
  declare @Project_Name varchar(200)
  declare @Project_Status int
  declare @Project_FeeType int
  declare @Project_FeeCycle int
  declare @Project_FeeDeposit int 
  declare @Project_SignOutEN int
  declare @Project_WhenLock int
  declare @Project_RegretTime int
  declare @Project_Note varchar(200)

  SELECT @NewId = Lower(REPLACE(NEWID(),'-',''))
  select 
  @Project_StoreID=StoreID,@Project_Name=ProjectName,@Project_Status=ProjectStatus,
  @Project_FeeType=FeeType,@Project_FeeCycle=FeeCycle,@Project_FeeDeposit=FeeDeposit,
  @Project_SignOutEN=SignOutEN,@Project_WhenLock=WhenLock,@Project_RegretTime=RegretTime,
  @Project_Note=Note from dbo.Data_ProjectInfo where ID = @TicketId and ProjectStatus = 1
  if @Project_FeeType = 0 
	begin
	  set @Project_FeeType = 0 	
	end
  else 
	begin
	  set @Project_FeeType = 1	
	end
  insert Flw_Project_BuyDetail
  (FoodSaleID,CardID,ProjectID,ParentID,Barcode,SaleTime,State,ProjectType,BuyCount,RemainCount,StartTime,EndTime)
  values(@FoodFlwID,@CardId,@TicketId,0,@NewId,GETDATE(),0,@Project_FeeType,@ContainCount,@ContainCount,GETDATE(),DATEADD(DY,@Days,GETDATE()))
GO
/****** Object:  StoredProcedure [dbo].[GetMemberBaseInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--获取会员信息
CREATE Proc [dbo].[GetMemberBaseInfo](@ICCardID varchar(50),@StoreID varchar(50),@Result int output,@MemberId varchar(32) output,@CardID int output,@ErrMsg varchar(200) output)
as
	--获取卡号的卡ID,
	declare @MerchId int = 0 
	select @CardID = isnull(CardID,0) ,@MemberId = isnull(MemberID,'') from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID
	if @CardID = 0
	   begin
		 set @Result = 0
		 set @ErrMsg = '会员卡不存在'
		 return 
	   end 
   
	--获取门店机构
	select @MerchId = MerchID from dbo.Base_StoreInfo where StoreID = @StoreID
	 
    --会员信息结果集
    select a.ID as CardId,a.ICCardID,a.MemberLevelID,b.UserName as MemberName,b.Gender,convert(char(10),b.Birthday,120) as Birthday,
    b.IDCard,b.Mobile,cast(a.Deposit as decimal(18,2)) as Deposit,b.Note,convert(char(10),a.EndDate,120) as EndDate,a.RepeatCode,b.MemberState,a.CardStatus,
	( select MemberLevelName from Data_MemberLevel where MerchID = @MerchId and MemberLevelID = a.MemberLevelID) as MemberLevelName,@StoreID as StoreId,
	( select StoreName from Base_StoreInfo where StoreID = @StoreID ) as StoreName
	from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID where a.ID = @CardID

    set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[GetMemberBalanceInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberBalanceInfo](@MemberId varchar(32))
as
  select BalanceIndex,TypeName as BanlanceName,Balance from Data_Card_Balance a inner join Dict_BalanceType b on a.BalanceIndex = b.id where MemberID = @MemberId and State = 1
GO
/****** Object:  StoredProcedure [dbo].[GetMember]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[GetMember](@StoreID varchar(50),@ICCardID varchar(50),@Result int output,@ErrMsg varchar(200) output)
as
	--获取卡号的卡ID,
	declare @CardID int = 0
	declare @MemberId int = 0
	declare @MerchId int = 0 
	select @CardID = isnull(CardID,0) ,@MemberId = isnull(MemberID,0) from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID
	if @CardID = 0
	   begin
		 set @Result = 0
		 set @ErrMsg = '会员卡不存在'
		 return 
	   end 
   
	--获取门店机构
	select @MerchId = MerchID from dbo.Base_StoreInfo where StoreID = @StoreID
	 
    --会员信息结果集
    select a.ID as CardId,a.ICCardID,a.MemberLevelID,b.UserName as MemberName,b.Gender,convert(char(10),b.Birthday,120) as Birthday,
    b.IDCard,b.Mobile,cast(a.Deposit as decimal(18,2)) as Deposit,b.Note,convert(char(10),a.EndDate,120) as EndDate,a.RepeatCode,b.MemberState,a.CardStatus,
	( select MemberLevelName from Data_MemberLevel where MerchID = @MerchId and MemberLevelID = a.MemberLevelID) as MemberLevelName,@StoreID as StoreId,
	( select StoreName from Base_StoreInfo where StoreID = @StoreID ) as StoreName
	from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID where a.ID = @CardID
	--会员余额
	select BalanceIndex,TypeName as BalanceName,Balance,HKType from Data_Card_Balance a inner join Dict_BalanceType b on a.BalanceIndex = b.id where MemberID = @MemberId and State = 1
	
    set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[GetGoodsInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[GetGoodsInfo](@StoreId varchar(15))
as
  select Id,GoodName as FoodName,GoodType as Good,Price as FoodPrice,GoodPhoteURL as ImageUrl from Base_GoodsInfo where StoreID = @StoreId
GO
/****** Object:  StoredProcedure [dbo].[GetFoodType]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodType](@MerchId varchar(15))
as
  select DictValue as FoodTypeId,DictKey as FoodTypeName into #FoodType from Dict_System where PID = (select id from dict_system where MerchId = @MerchId and dictKey='套餐类别' and pid = 0 and Enabled = 1) and Enabled = 1  order by FoodTypeId
  
  select '-1' as FoodTypeId,'单品' as FoodTypeName
  union all
  select * from #FoodType

  select DictValue as FoodTypeId,DictKey as FoodTypeName from Dict_System where PID = 5 and Enabled = 1

  select '0' as FoodTypeId,'次票' as FoodTypeName
  union all
  select '1' as FoodTypeId,'期限票' as FoodTypeName
  union all
  select '2' as FoodTypeId,'团体票' as FoodTypeName
GO
/****** Object:  StoredProcedure [dbo].[GetDiscountRuleList]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetDiscountRuleList](@MerchId varchar(15),@StoreId varchar(15),@CustomerType int,@ICCardId int,@FoodPrice decimal(18,2))
as
  declare @StoreFreq int = 0--门店频率
  declare @StoreCount int = 0--门店次数
  declare @ShareCount int = 0--门店次数共享
  declare @MemberFreq int = 0--会员频率
  declare @MemberCount int = 0--会员次数
  declare @CurrentDatetime datetime = getdate()
  declare @CurrentDate date = @CurrentDatetime
  declare @CurrentTime time = @CurrentDatetime
  declare @AllowGuest int = 0
  declare @DiscountRuleName varchar(200) = ''
  declare @UseShareCount int = 0
  declare @UseStoreCount int = 0
  declare @UseMemberCount int = 0
  declare @DiscountRuleID int = 0
  
  select DiscountRuleID into #TmpDiscountRule from 
  (
	select distinct a.DiscountRuleID,MIN(RuleLevel) as RuleLevel 
	from Data_Discount_StoreList a inner join Data_DiscountRule b on a.DiscountRuleID = b.Id
	inner join Data_Discount_Detail c on b.ID = c.DiscountRuleID
	where a.StoreID = @StoreId  
	and DATEDIFF(dy,StartDate,@CurrentDatetime) >= 0 and DATEDIFF(dy,EndDate,@CurrentDatetime) <= 0
	and @CurrentTime >= StartTime and @CurrentTime <= EndTime
	and not (DATEDIFF(dy,NoStartDate,@CurrentDatetime) >= 0 and DATEDIFF(dy,NoEndDate,@CurrentDatetime) <= 0)
	and ( (weektype = 0 and dbo.CheckWeekStr(b.Week) = 1) or (weektype = 1 and dbo.F_IsWorkDay() = 1) or (weektype = 2 and dbo.F_IsWeekendDay() = 1) or (WeekType = 3 and dbo.GetFestival() = 1) )
	and State = 1
	and ( ( LimitType = 0 and @FoodPrice <= LimitCount ) or ( LimitType = 0 and @FoodPrice > LimitCount) ) 
	group by a.DiscountRuleID
	) a
  order by a.DiscountRuleID
  
  declare cur cursor for select DiscountRuleID from #TmpDiscountRule
  open cur
  fetch next from cur into @DiscountRuleID
  while @@FETCH_STATUS = 0
    begin            
      select @StoreFreq = StoreFreq,@StoreCount = StoreCount,@ShareCount = ShareCount,@MemberFreq = MemberFreq,@MemberCount = MemberCount,@AllowGuest = AllowGuest,@DiscountRuleName = RuleName from Data_DiscountRule a where a.ID = @DiscountRuleID
      --获取规则使用记录
      select @UseShareCount = dbo.GetDiscountRecordByMerchId(@MerchId,@DiscountRuleID)
      select @UseStoreCount = dbo.GetDiscountRecordByStoreId(@MerchId,@StoreId,@DiscountRuleID)
      select @UseMemberCount = dbo.GetDiscountRecordByMember(@MerchId,@StoreId,@DiscountRuleID)
      
      --if @ShareCount - @UseShareCount > 0 
      --  begin
		  	
      --  end  			
      fetch next from cur into @DiscountRuleID
    end    
  close cur
  deallocate cur
  

  
  select ID,RuleName,AllowGuest,StoreCount - @UseStoreCount as StoreCount,ShareCount - @UseShareCount as ShareCount,MemberCount - @UseMemberCount as MemberCount from Data_DiscountRule 
  where ID = @DiscountRuleID and StoreCount - @UseStoreCount > 0
GO
/****** Object:  UserDefinedFunction [dbo].[GetDiscountRecordByMemberId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[GetDiscountRecordByMemberId](@MerchId varchar(15),@StoreId varchar(15),@MemberId varchar(32),@DiscountRuleID int)
 returns int
 as
   begin
     declare @UseCount int = 0
	 select @UseCount = UseCount from Data_DiscountMember_Record where MerchId = @MerchId and StoreId = @StoreId and MemberId = @MemberId and DiscountRuleID = @DiscountRuleID and DATEDIFF(YEAR,RecordDate,GETDATE()) = 0
	 return @UseCount
   end
GO
/****** Object:  StoredProcedure [dbo].[GetCommonFoodInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodInfo](@FoodId int,@CustomerType int,@MemberLevelId int)
as
	select a.FoodId,a.FoodName,a.FoodType,AllowInternet,AllowPrint,ForeAuthorize,isnull(b.FoodPrice,a.FoodPrice) as FoodPrice,a.ImageURL
	from 
	(
		select a.FoodId,a.FoodName,a.FoodType,AllowPrint,AllowInternet,ForeAuthorize,
		isnull(case @CustomerType when 0 then a.ClientPrice else a.MemberPrice end,0) AS FoodPrice,a.ImageURL 
		from Data_FoodInfo a where a.FoodId = @FoodId
	) a
	left join 
	(
		select a.FoodId,isnull(case @CustomerType when 0 then ClientPrice else VIPPrice end,0) AS FoodPrice 
		from 
		(
			select a.FoodId,MAX(ID) as MemberLevelId from Data_Food_Level a 
			left join Data_Food_Record b on a.ID = b.FoodLevelID and DATEDIFF(DY,RecordDate,GETDATE()) = 0
			where a.FoodID = @FoodId and DATEDIFF(DY,StartDate,GETDATE()) >= 0 and DATEDIFF(dy,EndDate,GETDATE()) <= 0
			and DATEDIFF(DY,StartTime,cast(GETDATE() as time)) >= 0 and DATEDIFF(dy,EndTime,cast(GETDATE() as time)) <= 0
			and dbo.CheckWeekStr(a.Week) = 1 and a.MemberLevelID = @MemberLevelId
			and isnull((case @CustomerType when 0 then b.day_sale_count else b.member_day_sale_count end),0) - isnull((case @CustomerType when 0 then b.day_sale_count else b.member_day_sale_count end),0) > 0
			group by a.FoodID
		) a
		inner join Data_Food_Level b on a.MemberLevelId = b.ID
	) b
	on a.FoodId = b.FoodId
GO
/****** Object:  StoredProcedure [dbo].[GetCommonFoodBalanceListInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodBalanceListInfo](@FoodDetailLevelQuery [FoodDetailLevelQueryType] readonly)
as
  select a.FoodId,b.BalanceType,TypeName,UseCount from @FoodDetailLevelQuery a inner join Data_Food_Sale b on a.FoodId = b.FoodID
  inner join Dict_BalanceType c on b.BalanceType = c.ID
  where c.State = 1
  order by a.FoodId,b.BalanceType
GO
/****** Object:  StoredProcedure [dbo].[GetCommonFoodBalanceInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodBalanceInfo](@FoodId int)
as
  select a.FoodId,a.BalanceType,TypeName,UseCount from Data_Food_Sale a
  inner join Dict_BalanceType b on a.BalanceType = b.ID
  where a.FoodID = @FoodId and b.State = 1
  order by a.FoodId,a.BalanceType
GO
/****** Object:  StoredProcedure [dbo].[GetCoinSalePoint]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCoinSalePoint](@StoreId varchar(20),@Coins int,@GetPoint int output)
as
declare @ParameterValue int = 0
select @ParameterValue = ParameterValue from Data_Parameters where StoreID = @StoreId and IsAllow = 1 and System = 'txtGivePoint'
if @ParameterValue = 0
  set @GetPoint = 0
else
  set @GetPoint = @Coins / @ParameterValue
GO
/****** Object:  StoredProcedure [dbo].[GetDateQueryFrame]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetDateQueryFrame](@Freq int,@StartDate date output,@EndDate date output)
as
  --0 天,1 周,2 月,3 季,4 年
  declare @Year char(4) = cast(datepart(year,GETDATE()) as CHAR(4))
  declare @CurrentWeekValue int = 0
  declare @CurrentQuarterValue int = 0
  declare @CurrentDatetime datetime = getdate()
  if @Freq = 0
    begin
	  set @StartDate = @CurrentDatetime
	  set @EndDate = @CurrentDatetime
    end
  else if @Freq = 1
    begin
      select @CurrentWeekValue = datepart(weekday,@CurrentDatetime) - 1
      if @CurrentWeekValue = 0
		begin
		  set @CurrentWeekValue = 7
	    end
      set @StartDate = DateAdd(dy,0 -@CurrentWeekValue + 1,@CurrentDatetime)
      set @EndDate = DateAdd(dy,7-@CurrentWeekValue,@CurrentDatetime)
    end
  else if @Freq = 2
    begin
      declare @Month int = datepart(MONTH,GETDATE())
	  if @Month = 1
	    begin
	      set @StartDate = CAST(@Year + '-01-01' as date)
		  set @EndDate = CAST(@Year + '-01-31' as date)
	    end	    
	  else if @Month = 2
	    begin
	      set @StartDate = CAST(@Year + '-02-01' as date)
	      if dbo.F_IsLeapYear(CAST(@Year as CHAR(4))) = 0	        
		    set @EndDate = CAST(@Year + '-02-28' as date)
		  else
		    set @EndDate = CAST(@Year + '-02-29' as date)
	    end
	  else if @Month = 3
	    begin
	      set @StartDate = CAST(@Year + '-03-01' as date)
		  set @EndDate = CAST(@Year + '-03-31' as date)
	    end  
	  else if @Month = 4
	    begin
	      set @StartDate = CAST(@Year + '-04-01' as date)
		  set @EndDate = CAST(@Year + '-04-30' as date)
	    end 
	  else if @Month = 5
	    begin
	      set @StartDate = CAST(@Year + '-05-01' as date)
		  set @EndDate = CAST(@Year + '-05-31' as date)
	    end
	  else if @Month = 6
	    begin
	      set @StartDate = CAST(@Year + '-06-01' as date)
		  set @EndDate = CAST(@Year + '-06-30' as date)
	    end
	  else if @Month = 7
	    begin
	      set @StartDate = CAST(@Year + '-07-01' as date)
		  set @EndDate = CAST(@Year + '-07-31' as date)
	    end
	  else if @Month = 8
	    begin
	      set @StartDate = CAST(@Year + '-08-01' as date)
		  set @EndDate = CAST(@Year + '-08-31' as date)
	    end
	  else if @Month = 9
	    begin
	      set @StartDate = CAST(@Year + '-09-01' as date)
		  set @EndDate = CAST(@Year + '-09-30' as date)
	    end
	  else if @Month = 10
	    begin
	      set @StartDate = CAST(@Year + '-10-01' as date)
		  set @EndDate = CAST(@Year + '-10-31' as date)
	    end
	  else if @Month = 11
	    begin
	      set @StartDate = CAST(@Year + '-11-01' as date)
		  set @EndDate = CAST(@Year + '-11-30' as date)
	    end
	  else if @Month = 12
	    begin
	      set @StartDate = CAST(@Year + '-12-01' as date)
		  set @EndDate = CAST(@Year + '-12-31' as date)
	    end   
    end
  else if @Freq = 3
    begin
	  select @CurrentQuarterValue = datepart(QUARTER,@CurrentDatetime)
	  if @CurrentQuarterValue = 1
	    begin
		  set @StartDate = CAST(@Year + '-01-01' as date)
		  set @EndDate = CAST(@Year + '-03-31' as date)
	    end
	  else if @CurrentQuarterValue = 2
	    begin
	      set @StartDate = CAST(@Year + '-04-01' as date)
		  set @EndDate = CAST(@Year + '-06-30' as date)
	    end
	  else if @CurrentQuarterValue = 3
	    begin
	      set @StartDate = CAST(@Year + '-07-01' as date)
		  set @EndDate = CAST(@Year + '-09-30' as date)
	    end
	  else if @CurrentQuarterValue = 4
	    begin
		  set @StartDate = CAST(@Year + '-10-01' as date)
		  set @EndDate = CAST(@Year + '-12-31' as date)
	    end
    end   
  else if @Freq = 4
    begin
      set @StartDate = CAST(@Year + '-01-01' as date)
      set @EndDate = CAST(@Year + '-12-31' as date)
    end
GO
/****** Object:  StoredProcedure [dbo].[GetCouponDiscountPrice]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[GetCouponDiscountPrice](@StoreId varchar(15),@FoodPrice decimal(18,2),@CouponList [CouponListType] READONLY,@ErrMsg varchar(8000) output)
as
  	create table #TmpCoupon(ID int IDENTITY(1,1),CouponID int,CouponType int,CouponValue decimal(18,2),CouponDiscount decimal(18,2),CouponThreshold decimal(18,2)
	,OverMoney decimal(18,2),CouponDetailCount int,AllowOverOther int,OverUseCount int)
GO
/****** Object:  UserDefinedFunction [dbo].[GetCouponCodeListStr]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[GetCouponCodeListStr](@CouponList [CouponListType] readonly) RETURNS varchar(8000) 
 as
   begin
      declare @CouponCode varchar(32) = ''
      declare @Str varchar(8000) = ''
	  declare b_cur cursor for select CouponCode from @CouponList
	  open b_cur
	  fetch next from b_cur into @CouponCode
	  while @@FETCH_STATUS = 0 
		begin
		  if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'
		  set @Str = @Str + @CouponCode
		  fetch next from b_cur into @CouponCode
		end
	  close b_cur
	  deallocate b_cur
	  return @Str
   end
GO
/****** Object:  UserDefinedFunction [dbo].[GetDiscountFullSubPrice]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[GetDiscountFullSubPrice](@DiscountRuleID int,@FoodPrice decimal(18,2))
 returns int
 as
   begin
     declare @SubPrice decimal(18,2) = 0
     declare @LimitCount decimal(18,2)--0 限额以内并包含;1 限额以外不包含
     declare @LimitType int
     declare @ConsumeCount decimal(18,2)
     declare @DiscountCount decimal(18,2)
     
	 declare cur cursor for select LimitCount,LimitType,ConsumeCount,DiscountCount from Data_Discount_Detail where DiscountRuleID = @DiscountRuleID
	 open cur
	 fetch next from cur into @LimitCount,@LimitType,@ConsumeCount,@DiscountCount
	 while @@fetch_status = 0
	   begin
		 if @LimitType = 0
		   begin
			 if @FoodPrice <= @LimitCount 
			   begin
				 set @SubPrice = @SubPrice + ( cast(@FoodPrice / @ConsumeCount as int) * @DiscountCount )	
			   end
		   end
		 else if @LimitType = 1
		   begin
			 if @FoodPrice > @LimitCount 
			   begin
				 set @SubPrice = @SubPrice + ( cast(@FoodPrice / @ConsumeCount as int) * @DiscountCount )	
			   end	
		   end
		 fetch next from cur into @LimitCount,@LimitType,@ConsumeCount,@DiscountCount
       end
     close cur
     deallocate cur
     return @subPrice
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetTicketDesc]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_GetTicketDesc](@ProjectId int,@ContainCount int,@Days int)
RETURNS varchar(200)
as
begin
  declare @CotainName varchar(200)
  select @CotainName = ProjectName from Data_ProjectInfo where ID = @ProjectId
  return @CotainName
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetMemberId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_GetMemberId](@StoreId varchar(15),@ICCardId int)
RETURNS varchar(32)
as
begin
  declare @MemberId varchar(32)
  select @MemberId = MemberID from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where a.StoreId = @StoreId and b.ICCardID = @ICCardId
  return @MemberId
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetJackpotDesc]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[F_GetJackpotDesc](@JackpotId int,@JackpotCount int)
RETURNS varchar(200)
as
begin
  declare @ActiveName varchar(200)
  declare @Threshold decimal(18,2)
  declare @StartTime datetime
  declare @EndTime datetime
  declare @Note varchar(8000)
  select @ActiveName = ActiveName,@Threshold = Threshold,@StartTime = StartTime,@EndTime = EndTime,@Note = isnull(Note,'') from Data_JackpotInfo where ID = @JackpotId 
  return '满' + cast(@Threshold as varchar) + '元,参与' + @ActiveName + '抽奖活动' + cast(@JackpotCount as varchar) + '次；抽奖时间：' + CONVERT(VARCHAR(16),@StartTime,120) + '至' + CONVERT(VARCHAR(16),@EndTime,120) + ';'
  + '说明：' + @Note
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetGoodDesc]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[F_GetGoodDesc](@GoodId int)
RETURNS varchar(200)
as
begin
  declare @GoodName varchar(200)
  select @GoodName = GoodName from Base_GoodsInfo where ID = @GoodId	 
  return @GoodName
end
GO
/****** Object:  StoredProcedure [dbo].[AddMemberDiscount]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddMemberDiscount](@MerchId varchar(15),@StoreId varchar(15),@MemberId int,@DiscountRuleID int,@StoreFreq int,@MemberFreq int)
as
  if not exists ( select 0 from dbo.Data_DiscountMember_Record where MerchId = @MerchId and StoreId = @StoreId and MemberId = @MemberId and DiscountRuleID = @DiscountRuleID and DATEDIFF(YEAR,RecordDate,GETDATE()) = 0 )
    begin
	  insert Data_DiscountMember_Record(DiscountRuleID,RecordDate,MerchId,StoreId,MemberId,MemberFreq,UseCount)
	  values(@DiscountRuleID,GETDATE(),@MerchId,@StoreId,@MemberId,@MemberFreq,1)
    end
  if not exists ( select 0 from dbo.Data_DiscountStore_Record where MerchId = @MerchId and StoreId = @StoreId and DiscountRuleID = @DiscountRuleID and DATEDIFF(YEAR,RecordDate,GETDATE()) = 0)
    begin
	  insert Data_DiscountStore_Record(DiscountRuleID,RecordDate,MerchId,StoreId,StoreFreq,UseCount)
	  values(@DiscountRuleID,GETDATE(),@MerchId,@StoreId,@StoreFreq,1)
    end    
  else
    begin
       update Data_DiscountMember_Record set UseCount = UseCount + 1 where DiscountRuleID = @DiscountRuleID
       update Data_DiscountStore_Record set UseCount = UseCount + 1 where DiscountRuleID = @DiscountRuleID
    end
GO
/****** Object:  StoredProcedure [dbo].[CreateCouponRecord]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CreateCouponRecord](
@CouponID int,@SendAuthorID int,@SendTime datetime,@PublishType int,
@SendType int,@MerchID varchar(15),@StoreID varchar(15),@IsSingle int,@PublishCount int,
@MemberIDsType [MemberIDsType] readonly,
@Result int output)
as	
	--删除现有记录
	delete from Data_CouponList where CouponID=@CouponID
	
    --插入优惠券记录表
    declare @row int
    declare @tempMemberIDs table (MemberID int NULL)	
	insert @tempMemberIDs select MemberID from @MemberIDsType
    declare @count int = 1
    declare @state int   
    declare @memberId int 
    declare @memberCount int
    select @memberCount=COUNT(MemberID) from @tempMemberIDs
    select @memberId=MemberID from (select top 1 MemberID from @tempMemberIDs order by MemberID) m
    
    if(@PublishType=0)  --电子优惠券
		set @state = 2      --已激活
    else
    begin
		if(@IsSingle=1)     --单门店
			set @state = 1  --未激活
		else
			set @state = 0  --未分配
    end
    
    while(@count<=@PublishCount)
    begin
		if(@memberId>0 and @count<=@memberCount)
		begin
			SET ROWCOUNT 1
			select @memberId=MemberID from @tempMemberIDs			
			SET ROWCOUNT 0			
			delete from @tempMemberIDs where MemberID=@memberId
		end
		insert into Data_CouponList(CouponCode,CouponID,CouponIndex,SendAuthorID,SendTime,PublishType,SendType,MerchID,StoreID,[State],MemberID,IsLock)
		values (Lower(REPLACE(newid(),'-','')),@CouponID,@count,@SendAuthorID,@SendTime,@PublishType,@SendType,@MerchID,@StoreID,@state,@memberId,0)
		select @row=@@ROWCOUNT
		if(@row!=1)
		begin
			set @Result = -1
			return
		end
		
		set @count = @count + 1
    end    
    
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[CheckRoleByOpenId]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckRoleByOpenId](@StoreId varchar(15),@OpenId varchar(50),@RoleItemName varchar(50),@UserId int output,@ErrMsg varchar(200) output)
as 
  declare @RoleId int = 0
  select @UserId = USERId from Base_UserInfo where StoreID = @StoreId and OpenID = @OpenId
  if @UserId = 0
    begin
      set @ErrMsg = '用户不存在'
      return 0
    end
  select @RoleId = ID  from Dict_System where DictKey = '订单审核授权' and Enabled = 1 
  if @RoleId = 0
    begin
      set @ErrMsg = '权限配置不存在'
      return 0
    end
  if not exists (select 0 from Base_UserGrant where UserID = @UserId and GrantID = @RoleId and GrantEN = 1)
    begin
      set @ErrMsg = '用户没有权限'
      return 0
    end    
  return 1
GO
/****** Object:  StoredProcedure [dbo].[CheckOrders]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[CheckOrders](@CheckDate varchar(10), @MerchId varchar(50), @ID int, @Return int output)
as

 begin transaction tran1
 begin try
	if @ID>0
		begin
			update dbo.Flw_CheckDate set [Status]=2 where ID=@ID
			update dbo.Flw_Schedule set [State]=2 where CheckDateID=@ID
		end
	else
		begin
			declare @temp table (ID int)
			insert @temp select c.ID FROM dbo.Base_StoreInfo AS a INNER JOIN
				dbo.Flw_Schedule AS b on a.StoreID = b.StoreID left join
				  dbo.Flw_CheckDate AS c ON b.CheckDateID = c.ID and a.StoreID = c.StoreID and b.CheckDate = c.CheckDate
				  where b.CheckDate=CONVERT(date, @CheckDate) and a.MerchID=@MerchId and
						NOT EXISTS
                          (SELECT     1
                            FROM          dbo.Flw_Schedule
                            WHERE      [State] IN (0, 2) AND CheckDateID = c.ID and c.ID is not null) and
                        NOT EXISTS
						  (SELECT 1
							FROM dbo.Flw_Schedule
							where ID=b.ID and CheckDateID is null)
			update dbo.Flw_CheckDate set [Status]=2 where ID in (select ID from @temp)
			update dbo.Flw_Schedule set [State]=2 where CheckDateID in (select ID from @temp)
		end	
	
	commit transaction tran1
	set @Return = 1
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetProjectDesc]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create FUNCTION [dbo].[F_GetProjectDesc](@ProjectId int)
RETURNS varchar(200)
as
begin
  declare @ProjectName varchar(200)
  select @ProjectName = ProjectName from data_ProjectInfo where ID = @ProjectId 
  return @ProjectName
end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetEndDate]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetEndDate](@Freq int)
returns date
as
  begin
	  declare @EndDate date
	  --0 天,1 周,2 月,3 季,4 年
	  declare @Year char(4) = cast(datepart(year,GETDATE()) as CHAR(4))
	  declare @CurrentWeekValue int = 0
	  declare @CurrentQuarterValue int = 0
	  declare @CurrentDatetime datetime = getdate()
	  if @Freq = 0
		begin
		  set @EndDate = @CurrentDatetime
		end
	  else if @Freq = 1
		begin
		  select @CurrentWeekValue = datepart(weekday,@CurrentDatetime) - 1
		  if @CurrentWeekValue = 0
			begin
			  set @CurrentWeekValue = 7
			end
		  set @EndDate = DateAdd(dy,7-@CurrentWeekValue,@CurrentDatetime)
		end
	  else if @Freq = 2
		begin
		  declare @Month int = datepart(MONTH,GETDATE())
		  if @Month = 1
			begin
			  set @EndDate = CAST(@Year + '-01-31' as date)
			end	    
		  else if @Month = 2
			begin
			  if dbo.F_IsLeapYear(CAST(@Year as CHAR(4))) = 0	        
				set @EndDate = CAST(@Year + '-02-28' as date)
			  else
				set @EndDate = CAST(@Year + '-02-29' as date)
			end
		  else if @Month = 3
			begin
			  set @EndDate = CAST(@Year + '-03-31' as date)
			end  
		  else if @Month = 4
			begin
			  set @EndDate = CAST(@Year + '-04-30' as date)
			end 
		  else if @Month = 5
			begin
			  set @EndDate = CAST(@Year + '-05-31' as date)
			end
		  else if @Month = 6
			begin
			  set @EndDate = CAST(@Year + '-06-30' as date)
			end
		  else if @Month = 7
			begin
			  set @EndDate = CAST(@Year + '-07-31' as date)
			end
		  else if @Month = 8
			begin
			  set @EndDate = CAST(@Year + '-08-31' as date)
			end
		  else if @Month = 9
			begin
			  set @EndDate = CAST(@Year + '-09-30' as date)
			end
		  else if @Month = 10
			begin
			  set @EndDate = CAST(@Year + '-10-31' as date)
			end
		  else if @Month = 11
			begin
			  set @EndDate = CAST(@Year + '-11-30' as date)
			end
		  else if @Month = 12
			begin
			  set @EndDate = CAST(@Year + '-12-31' as date)
			end   
		end
	  else if @Freq = 3
		begin
		  select @CurrentQuarterValue = datepart(QUARTER,@CurrentDatetime)
		  if @CurrentQuarterValue = 1
			begin
			  set @EndDate = CAST(@Year + '-03-31' as date)
			end
		  else if @CurrentQuarterValue = 2
			begin
			  set @EndDate = CAST(@Year + '-06-30' as date)
			end
		  else if @CurrentQuarterValue = 3
			begin
			  set @EndDate = CAST(@Year + '-09-30' as date)
			end
		  else if @CurrentQuarterValue = 4
			begin
			  set @EndDate = CAST(@Year + '-12-31' as date)
			end
		end   
	  else if @Freq = 4
		begin
		  set @EndDate = CAST(@Year + '-12-31' as date)
		end
	  return @EndDate	
  end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetDiscountRecordByStoreId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetDiscountRecordByStoreId](@MerchId varchar(15),@StoreId varchar(15),@DiscountRuleID int,@StartDate date,@EndDate date)
 returns int
 as
   begin
     declare @UseCount int = 0
	 select @UseCount = UseCount from Data_DiscountStore_Record where MerchId = @MerchId and StoreId = @StoreId and DiscountRuleID = @DiscountRuleID 
	 and DATEDIFF(YEAR,@StartDate,RecordDate) >= 0 and DATEDIFF(YEAR,@EndDate,RecordDate) <= 0
	 return @UseCount
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetDiscountRecordByMerchId]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetDiscountRecordByMerchId](@MerchId varchar(15),@DiscountRuleID int,@StartDate date,@EndDate date)
 returns int
 as
   begin
     declare @UseCount int = 0
	 select @UseCount = isnull(sum(UseCount),0) from Data_DiscountStore_Record where MerchId = @MerchId and DiscountRuleID = @DiscountRuleID 
	 and DATEDIFF(YEAR,@StartDate,RecordDate) >= 0 and DATEDIFF(YEAR,@EndDate,RecordDate) <= 0
	 return @UseCount
   end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetDiscountRecordByMember]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetDiscountRecordByMember](@MerchId varchar(15),@StoreId varchar(15),@DiscountRuleID int,@StartDate date,@EndDate date)
 returns int
 as 
   begin 
     declare @UseCount int = 0 
	 select @UseCount = sum(UseCount) from Data_DiscountMember_Record where MerchId = @MerchId and StoreId = @StoreId and DiscountRuleID = @DiscountRuleID 
	 and DATEDIFF(YEAR,@StartDate,RecordDate) >= 0 and DATEDIFF(YEAR,@EndDate,RecordDate) <= 0
	 return @UseCount
   end
GO
/****** Object:  StoredProcedure [dbo].[UpdateMemberBalance]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[UpdateMemberBalance](@StoreId varchar(15),@ICCardId varchar(20),@MemberBalanceList [MemberBalanceListType2] readonly)
as  
  declare @CardBalanceId int = 0
  declare @BalanceType int = 0

  declare cur cursor for
  select a.ID as CardBalanceId,a.BalanceIndex from Data_Card_Balance a inner join Data_Card_Balance_StoreList b on a.ID = b.CardBalanceID 
  where MemberID = (select MemberID from Data_Member_Card a inner join Data_Member_Card_Store b on a.ID = b.CardID 
  where StoreID = @StoreId and ICCardID = @ICCardId)
  order by b.Id,a.BalanceIndex
GO
/****** Object:  StoredProcedure [dbo].[Tmp_CreateFoodInfoPic]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[Tmp_CreateFoodInfoPic]
as
  declare @Id int = 0
  declare @FoodId int = 0
  declare @Url varchar(200) = ''
  declare @Count int = 20
  declare @Index int = 0
  declare @TmpCount int = 0
  create table #TmpFoodInfo(Id int IDENTITY(1,1),FoodId int)
  insert #TmpFoodInfo(FoodId) select FoodId from Data_FoodInfo 
  declare cur cursor for select Id,FoodId from #TmpFoodInfo 
  open cur
  fetch next from cur into @Id,@FoodId
  while @@fetch_status=0
	 begin
	   set @Index = @Index + 1 
	   set @Url = 'http://192.168.1.145/Imgs/' + CAST(@Index as varchar) + '.jpg'
	   update Data_FoodInfo set ImageURL = @Url where FoodID = @FoodId
	   if @Id / @Count > @TmpCount
	     begin
	       set @Index = 0
	       set @TmpCount = @TmpCount + 1
	     end
	   fetch next from cur into @Id,@FoodId
	 end 
  close cur
  deallocate cur
GO
/****** Object:  StoredProcedure [dbo].[SP_RowToCol2]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_RowToCol2](@PID int)
as	
	declare @temp table (ColumnName varchar(50))	
	insert @temp select DictKey as ColumnName from Dict_System with (nolock) where PID=@PID
	
	declare @s varchar(MAX)='select *'
	declare @ColumnName varchar(50)

	WHILE EXISTS(SELECT [ColumnName] FROM @temp)
	BEGIN 

	SET ROWCOUNT 1
	select @ColumnName=t.ColumnName from @temp as t
	set @s = @s + ',(case DictKey when ''' + @ColumnName + ''' then DictValue else null end) ' + @ColumnName	
	SET ROWCOUNT 0
	--删除临时表中的使用完的数据
	DELETE from @temp where [ColumnName] = @ColumnName

	END 		
	
	set @s = @s + ' from Dict_System where PID=' + convert(varchar, @PID)

	exec(@s)
GO
/****** Object:  StoredProcedure [dbo].[SP_RowToCol]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_RowToCol](@PID int)
as
 begin transaction tran1
 begin try	
	declare @temp table (ColumnName varchar(50))	
	declare @DictKey varchar(50)
	DECLARE MyCursor CURSOR FOR select DictKey from Dict_System where PID= @PID	
	--打开一个游标    
	OPEN MyCursor

	--循环一个游标
	FETCH NEXT FROM  MyCursor INTO @DictKey
	WHILE @@FETCH_STATUS =0
    	BEGIN
	
		insert into @temp (ColumnName) values (@DictKey)

	--循环一个游标
        FETCH NEXT FROM  MyCursor INTO @DictKey
    	END    
	--关闭游标
	CLOSE MyCursor
	--释放资源
	DEALLOCATE MyCursor

	declare @ColumnName varchar(50)
	declare @s varchar(MAX)=''
	set @s = 'select *'	
	DECLARE MyCursor CURSOR FOR select ColumnName from @temp	
	--打开一个游标    
	OPEN MyCursor

	--循环一个游标
	FETCH NEXT FROM  MyCursor INTO @ColumnName
	WHILE @@FETCH_STATUS =0
    	BEGIN
	
		set @s = @s + ',(case DictKey when ''' + @ColumnName + ''' then DictValue else null end) ' + @ColumnName

	--循环一个游标
        FETCH NEXT FROM  MyCursor INTO @ColumnName
    	END    
	--关闭游标
	CLOSE MyCursor
	--释放资源
	DEALLOCATE MyCursor
	
	set @s = @s + ' from Dict_System where PID=' + convert(varchar, @PID)

	exec(@s)

COMMIT TRAN 

end try   
begin catch  
	SELECT 'There was an error! ' + ERROR_MESSAGE()   
    ROLLBACK  
end catch
GO
/****** Object:  StoredProcedure [dbo].[SP_RegisterUserFromWx]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_RegisterUserFromWx](@StoreId varchar(15),@MerchId varchar(15),@UserType int,@Mobile varchar(16),@Username varchar(50),@Realname varchar(50),@UserPassword varchar(32),@Message varchar(500) = '',@WXOpenID varchar(100) = '',@UnionID varchar(100) = '',@WorkID int output, @Return int output)
as

 begin transaction tran1
 begin try
	declare @UserId int 
	declare @AuditorId int
	--添加门店用户
	select @AuditorId = UserId from Base_UserInfo where OpenID = (select WXOpenID from Base_MerchantInfo where MerchID = (select MerchID from Base_StoreInfo where StoreID=@StoreId))
	insert into Base_UserInfo(StoreID,MerchID,UserType,LogName,LogPassword,OpenID,UnionID,RealName,Mobile,ICCardID,CreateTime,[Status],Auditor)
	values(@StoreId,@MerchId,@UserType,@Username,@UserPassword,@WXOpenID,@UnionID,@Realname,@Mobile,'',GETDATE(),0,@AuditorId)
	set @UserId = @@identity	
	--添加工单	
	insert into XC_WorkInfo(SenderID,SenderTime,WorkType,WorkState,WorkBody,AuditorID)
	values(@UserId,GETDATE(),0,0,@Message,@AuditorId) 
	set @WorkID = @@identity
	--添加消息
	insert into Data_Message(Sender, Receiver, SendTime, MsgText, ReadFlag) 
	values(@UserId, @AuditorId, GETDATE(), @Message, 0)
	--添加日志
	insert into Log_Operation(StoreID, UserID, AuthorID, Content)
	values(@StoreId, @UserId, @AuditorId, '微信新用户注册审核' + @Message)
	
	commit transaction tran1
	set @Return = 1
	select * from XC_WorkInfo where WorkID = @WorkID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO
/****** Object:  StoredProcedure [dbo].[SP_GetMenus]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_GetMenus](@LogType int, @LogID int, @MerchID varchar(15), @StoreID varchar(15))
as
begin
	
	CREATE TABLE #MENU (FunctionID INT NULL)	
	if @LogType=3 --商户用户
	begin
		declare @MerchType int 
		declare @MerchTag int 
		SELECT @MerchType=MerchType, @MerchTag=MerchTag FROM Base_MerchantInfo WHERE MerchID=@MerchID
		insert into #MENU
		select FunctionID from Dict_FunctionMenu where MenuType=(case when @MerchType in (1, 2) then 3 when @MerchType=3 then 4 else null end)
														and UseType in (0, @MerchTag)
	end
	else if @LogType=1 --莘宸管理员
	begin
		insert into #MENU
		select FunctionID from Dict_FunctionMenu where MenuType=0
	end
	else if @LogType=0 --莘宸员工
	begin
		insert into #MENU
		select a.FunctionID from Dict_FunctionMenu a left join Base_UserGroup_Grant b on a.FunctionID=b.FunctionID
		left join Base_UserInfo c on b.GroupID=c.UserGroupID and c.UserID=@LogID
		where a.MenuType=0 and b.IsAllow=1
	end
	else if @LogType=2 --门店用户
	begin
		declare @StoreTag int 
		SELECT @StoreTag=StoreTag FROM Base_StoreInfo WHERE StoreID=@StoreID
		insert into #MENU
		select distinct a.FunctionID
		from Dict_FunctionMenu a 
		left join (select b.MerchID,c.FunctionID,c.FunctionEN from Base_MerchantInfo a 
					inner join Base_MerchantInfo b on a.MerchID=b.CreateUserID
					inner join Base_MerchFunction c on a.MerchID=c.MerchID where a.MerchType=3
					) b on a.FunctionID=b.FunctionID and b.MerchID=@MerchID
		left join Base_MerchFunction c on a.FunctionID=c.FunctionID and c.MerchID=@MerchID 
		where a.MenuType = 1 and a.UseType in (0, @StoreTag) and (c.FunctionEN=1 or b.FunctionEN=1)
	end
					
	;WITH 
	LOCS(FunctionID,ParentID,FunctionName,PageName,Icon,OrderID)
	AS
	(
	SELECT FunctionID,ParentID,FunctionName,PageName,Icon,OrderID FROM Dict_FunctionMenu WHERE FunctionID in (select FunctionID from #MENU)
	UNION ALL
	SELECT A.FunctionID,A.ParentID,A.FunctionName,A.PageName,A.ICON,A.OrderID FROM Dict_FunctionMenu A JOIN LOCS B ON 
	B.ParentID=A.FunctionID 
	--A.pid = B.id
	)
	
	SELECT DISTINCT FunctionID,ParentID,FunctionName,PageName,Icon,OrderID from LOCS order by OrderID
	
 end
GO
/****** Object:  StoredProcedure [dbo].[SP_DictionaryNodes]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_DictionaryNodes](@MerchID nvarchar(15), @DictKey nvarchar(50), @RootID int output)
as
 begin
	--declare @sql nvarchar(max)
	--SET @sql = ''
	--SET @sql = @sql + 'select * from Dict_System where 1=1'

	--SET @sql = @sql + ' and IsNull(MerchID, '''')=IsNull(@MerchID, '''')'
	--if(IsNull(@DictKey, '') <> '')
	--	begin	
	--		select @RootID=ID from (select top 1 ID from Dict_System where DictKey=@DictKey 
	--		and IsNull(MerchID, '')=IsNull(@MerchID, '')) m
	--		if not exists (select 0 from Dict_System where PID=@RootID)
	--			return
	--		SET @sql = @sql + ' and PID=@RootID'
	--	end
	----exec (@sql)
	--SET @sql = @sql + ' order by OrderID'
	--exec sp_executesql @sql, N'@MerchID nvarchar(15), @DictKey nvarchar(50), @RootID int', @MerchID, @DictKey, @RootID
	SET @RootID = 0	
	if(IsNull(@DictKey, '') <> '')
	begin	
		select @RootID=ID from (select top 1 ID from Dict_System where DictKey=@DictKey 
		and IsNull(MerchID, '')=IsNull(@MerchID, '')) m
		if not exists (select 0 from Dict_System where PID=@RootID)
			return		
	end
	
	;WITH 
	LOCS(ID,PID,DictKey,DictValue,Comment,OrderID,[Enabled],MerchID,DictLevel)
	AS
	(
	SELECT ID,PID,DictKey,DictValue,Comment,OrderID,[Enabled],MerchID,DictLevel FROM Dict_System WHERE PID=@RootID
	UNION ALL
	SELECT A.ID,A.PID,A.DictKey,A.DictValue,A.Comment,A.OrderID,A.[Enabled],A.MerchID,A.DictLevel FROM Dict_System A JOIN LOCS B ON 
	--B.ParentID=A.FunctionID 
	A.PID = B.ID
	)
	
	SELECT DISTINCT ID,PID,DictKey,DictValue,Comment,OrderID,[Enabled],MerchID,DictLevel from LOCS order by OrderID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectXcUserGrant]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectXcUserGrant](@UserID int)
as
 begin
	select a.ID, a.PID, a.DictKey, a.DictValue, a.OrderID, c.GrantEN
	from Dict_System a 
	left join Dict_System b on a.PID=b.ID
	left join Base_UserGrant c on a.ID=c.GrantID and c.UserID=@UserID
	where a.Enabled=1 and b.DictKey='权限列表' and (a.MerchID is null or a.MerchID='') 
	order by OrderID,PID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectUserGroupGrant]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectUserGroupGrant](@GroupID int, @MerchID varchar(15))
as
 begin
	select ID,GroupName,Note from Base_UserGroup where ID=@GroupID
	CREATE TABLE #MENU (FunctionID INT NULL, IsAllow INT NULL)
	
	insert into #MENU
	select a.FunctionID, c.IsAllow
	from Base_MerchFunction a left join Base_UserGroup b on a.MerchID=b.MerchID and b.ID=@GroupID
	left join Base_UserGroup_Grant c on a.FunctionID=c.FunctionID and b.ID=c.GroupID 
	where a.FunctionEN=1 and a.MerchID=@MerchID 
	order by a.ID
	
		
	;WITH 
	LOCS(FunctionID,ParentID,FunctionName)
	AS
	(
	SELECT FunctionID,ParentID,FunctionName FROM Dict_FunctionMenu WHERE FunctionID in (select FunctionID from #MENU)
	UNION ALL
	SELECT A.FunctionID,A.ParentID,A.FunctionName FROM Dict_FunctionMenu A JOIN LOCS B ON 
	B.ParentID=A.FunctionID 
	--A.pid = B.id
	)
	select DISTINCT a.FunctionID,a.ParentID,a.FunctionName,b.IsAllow from LOCS a Left JOIN #MENU b on a.FunctionID=b.FunctionID
	
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectUserGroup]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectUserGroup](@UserID int)
as
 begin
	select a.ID, a.MerchID, a.GroupName, a.Note
	from Base_UserGroup a inner join Base_StoreInfo c on a.MerchID=c.MerchID inner join Base_UserInfo b on c.StoreID=b.StoreID 	
	union
	select a.ID, a.MerchID, a.GroupName, a.Note
	from Base_UserGroup a inner join Base_UserInfo b on a.MerchID=b.MerchID
	where b.UserID=@UserID
	order by a.ID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectUserGrant]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectUserGrant](@UserID int)
as
 begin
	select a.ID, a.PID, a.DictKey, a.DictValue, a.OrderID, d.GrantEN
	from Dict_System a 
	left join Dict_System b on a.PID=b.ID
	--inner join Base_StoreInfo c on a.MerchID=c.MerchID 
	--inner join Base_UserInfo b on c.StoreID=b.StoreID
	left join Base_UserGrant d on a.ID=d.GrantID and d.UserID=@UserID	
	where a.Enabled=1 and b.DictKey='权限列表' and a.MerchID='1'
	order by OrderID,PID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectStoreUnchecked]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectStoreUnchecked](@UserID int)
as
 begin
	select a.StoreID, a.MerchID, a.StoreName, a.Password, a.AuthorExpireDate, a.AreaCode, a.Address, a.Contacts, a.Mobile, b.DictKey as SelttleTypeStr, c.DictKey as StoreStateStr from Base_StoreInfo a 
	left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='结算类型') b on a.SelttleType=convert(int,b.DictValue) 
	left join (select b.* from Dict_System a inner join Dict_System b on a.ID=b.PID where a.DictKey='门店状态') c on a.StoreState=convert(int,c.DictValue) 
	left join Base_MerchantInfo d on a.MerchID=d.MerchID and d.CreateType=1 
	left join Base_MerchantInfo e on a.MerchID=e.MerchID and e.CreateType=2 
	left join Base_MerchantInfo f on e.CreateUserID=f.MerchID 
	where a.StoreState=0 and (d.CreateUserID=@UserID or f.CreateUserID=@UserID)
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectMerchFunction]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectMerchFunction](@MerchID varchar(15))
as
 begin
	declare @MerchTag int = 0
	select @MerchTag=MerchTag from Base_MerchantInfo where MerchID=@MerchID
	select distinct a.ParentID, a.FunctionID, a.FunctionName, (case when c.FunctionEN is null then (case when b.FunctionEN is null then null else b.FunctionEN end) else c.FunctionEN end) as FunctionEN
	from Dict_FunctionMenu a 
	left join (select b.MerchID,c.FunctionID,c.FunctionEN from Base_MerchantInfo a 
				inner join Base_MerchantInfo b on a.MerchID=b.CreateUserID
				inner join Base_MerchFunction c on a.MerchID=c.MerchID where a.MerchType=3
				) b on a.FunctionID=b.FunctionID and b.MerchID=@MerchID
	left join Base_MerchFunction c on a.FunctionID=c.FunctionID and c.MerchID=@MerchID 
	where a.MenuType = 1 and a.UseType in (0, @MerchTag)
	order by a.ParentID, a.FunctionID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectFunctionForXA]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SelectFunctionForXA](@GroupID int)
as
begin
	select ID,GroupName,Note from Base_UserGroup where ID=@GroupID
	select distinct a.ParentID, a.FunctionID, a.FunctionName, b.IsAllow 
	from Dict_FunctionMenu a left join Base_UserGroup_Grant b on a.FunctionID=b.FunctionID and b.GroupID=@GroupID
	where a.MenuType=0
	order by a.ParentID, a.FunctionID
end
GO
/****** Object:  StoredProcedure [dbo].[SaveCouponNotAssigned]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[SaveCouponNotAssigned](
@CouponID int,@StoreID varchar(15),@Total int,
@NoArrayType [NoArrayType] readonly,
@Result int output,@ErrMsg varchar(200) output)
as	
	--判断Total是否超出可调拨数量
	declare @NotAssignedCount int = 0
	select @NotAssignedCount=COUNT(ID) from Data_CouponList where CouponID=@CouponID and State=0
	if(@Total > @NotAssignedCount)
	begin
		set @Result = -1
		set @ErrMsg = '超出可调拨数量，应该小于' + CONVERT(varchar, @NotAssignedCount)
		return
	end
	
	--判断调拨门店是否存在
	if(ISNULL(@StoreID,'')='')
	begin
		set @Result = -1
		set @ErrMsg = '门店ID不能为空'
		return
	end
	
	if not exists (select 1 from Base_StoreInfo where StoreID=@StoreID)
	begin
		set @Result = -1
		set @ErrMsg = '调拨门店不存在，编号为' + @StoreID
		return
	end
	
    --调拨优惠券记录表
    declare @row int
    declare @tempNoArray table (StartNo int NULL, EndNo int NULL)	
	insert @tempNoArray select StartNo, EndNo from @NoArrayType
    declare @count int = 0
    select @count=COUNT(StartNo) from @tempNoArray
    print @count
    declare @startNo int
    declare @endNo int
    declare @i int
    declare @ID int
    declare @State int
    
    while(@count>0)
    begin
		SET ROWCOUNT 1
		select @startNo=StartNo, @endNo=EndNo from @tempNoArray			
		SET ROWCOUNT 0			
		delete from @tempNoArray where StartNo=@startNo
		
		set @i = @startNo
		while(@i<=@endNo)
		begin			
			select @ID=ID, @State=[State] from Data_CouponList where CouponID=@CouponID and CouponIndex=@i
			select @row=@@ROWCOUNT
			if(@row=0)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张不存在'
				print @ErrMsg
				return
			end
			
			if(@row>1)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '序号为' + Convert(varchar, @i) + '的优惠券存在' + Convert(varchar, @row) + '张'
				print @ErrMsg
				return
			end
			
			if(@State!=0)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张已调拨'
				print @ErrMsg
				return
			end
			
			update Data_CouponList set [State]=1, StoreID=@StoreID where ID=@ID
			select @row=@@ROWCOUNT
			if(@row!=1)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张调拨失败'
				print @ErrMsg
				return
			end
					
			set @i=@i+1
		end 
		
		set @count=@count-1		
    end    
    
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[SaveCouponNotActivated]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[SaveCouponNotActivated](
@CouponID int,@StoreID varchar(15),@Total int,
@NoArrayType [NoArrayType] readonly,
@Result int output,@ErrMsg varchar(200) output)
as	
	--判断Total是否超出可派发数量
	declare @NotActivatedCount int = 0
	if(ISNULL(@StoreID,'')='')
		select @NotActivatedCount=COUNT(ID) from Data_CouponList where CouponID=@CouponID and State=1
	else
		select @NotActivatedCount=COUNT(ID) from Data_CouponList where CouponID=@CouponID and State=1 and StoreID=@StoreID
				
	if(@Total > @NotActivatedCount)
	begin
		set @Result = -1
		set @ErrMsg = '超出可派发数量，应该小于' + Convert(varchar, @NotActivatedCount)
		return
	end	
	
    --派发优惠券记录表
    declare @row int
    declare @tempNoArray table (StartNo int NULL, EndNo int NULL)	
	insert @tempNoArray select StartNo, EndNo from @NoArrayType
    declare @count int = 0
    select @count=COUNT(StartNo) from @tempNoArray
    declare @startNo int
    declare @endNo int
    declare @i int
    declare @ID int
    declare @State int
    
    while(@count>0)
    begin
		SET ROWCOUNT 1
		select @startNo=StartNo, @endNo=EndNo from @tempNoArray			
		SET ROWCOUNT 0			
		delete from @tempNoArray where StartNo=@startNo
		
		set @i = @startNo
		while(@i<=@endNo)
		begin			
			if(ISNULL(@StoreID,'')='')
				select @ID=ID, @State=[State] from Data_CouponList where CouponID=@CouponID and CouponIndex=@i
			else
				select @ID=ID, @State=[State] from Data_CouponList where CouponID=@CouponID and CouponIndex=@i and StoreID=@StoreID
			select @row=@@ROWCOUNT
			if(@row=0)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张不存在或已调拨给其
他门店'
				return
			end

			if(@row>1)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '序号为' + Convert(varchar, @i) + '的优惠券存在' + Convert
(varchar, @row) + '张'
				return
			end
			
			if(@State!=1)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张已派发'
				return
			end
			
			update Data_CouponList set [State]=2 where ID=@ID
			select @row=@@ROWCOUNT
			if(@row!=1)
			begin
				set @Result = -1
				set @ErrMsg = '编号为' + Convert(varchar, @CouponID) + '的优惠券第' + Convert(varchar, @i) + '张派发失败'
				return
			end
					
			set @i=@i+1
		end 
		
		set @count=@count-1		
    end    
    
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[OpenSchedule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[OpenSchedule](@CheckDate varchar(10),@StoreId varchar(50),@UserId int,@ScheduleName varchar(50),@WorkStation varchar(50),@CurrentSchedule int output,@OpenTime varchar(20) output,@errMsg varchar(20) output)
as
 declare @Return int 
 declare @AllowScheduleCount int 
 declare @CheckDateCount int
 declare @CheckDateId int
 
 if exists (select 0 from flw_schedule where StoreID = @StoreId and UserID = @UserId and WorkStation = @WorkStation and State = 0)
	begin
		select @CurrentSchedule = id,@OpenTime = CONVERT(char(19),OpenTime,120) from flw_schedule where StoreID = @StoreId and UserID = @UserId and WorkStation = @WorkStation
		return 1
	end
 
 --获取门店每天允许的最大开班次数
 select @AllowScheduleCount = ParameterValue from dbo.Data_Parameters where StoreID = @StoreId and System = '1' and ParameterName = 'ScheduleCountPerDay'
 if @AllowScheduleCount = 0
   begin
	 set @errMsg = '每日班次数量设置不正确'
	 return 0
   end

 --获取门店当天的开班数
 select @CheckDateCount = COUNT(*) from flw_checkdate where StoreId = @StoreId and CheckDate = @CheckDate
 set @CurrentSchedule = 0
 if(@CheckDateCount >= @AllowScheduleCount)
  begin
    set @errMsg = '最大开班次数已超过限制'
	return 0
  end
 else 
  begin  
	insert into flw_checkdate(StoreID,CheckDate,ScheduleName,Status)
	values(@StoreId,@CheckDate,@ScheduleName,0)
	set @CheckDateId = @@IDENTITY
	declare @CurrentTime datetime = GETDATE()	
	insert into flw_schedule(StoreID,CheckDateID,UserID,OpenTime,ShiftTime,CheckDate,State,RealCash,RealCredit,RealCoin,WorkStation,Note)
	values(@StoreID,@CheckDateId,@UserId,@CurrentTime,null,@CheckDate,0,0,0,0,@WorkStation,'')
	set @CurrentSchedule = @@IDENTITY
	set @OpenTime = CONVERT(char(19),@CurrentTime,120)
	return 1
  end
GO
/****** Object:  StoredProcedure [dbo].[LockCouponRecord]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[LockCouponRecord](
@CouponID int,@ID int,@StoreID varchar(15),@IsLock int,
@Result int output)
as	
	if(@ID > 0)
		update Data_CouponList set IsLock=@IsLock where ID=@ID
	else if(ISNULL(@StoreID,'')='')
		update Data_CouponList set IsLock=@IsLock where CouponID=@CouponID
	else
		update Data_CouponList set IsLock=@IsLock where CouponID=@CouponID and StoreID=@StoreID
    	
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[IsExistOtherStoreOpenCardRecardInALLGroupRule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[IsExistOtherStoreOpenCardRecardInALLGroupRule](@StoreId varchar(15),@Mobile varchar(11))
as
--当前门店所在商户是否存在（已开通当前手机号的会员卡）完全规则的门店
declare @RecordCount int
select @RecordCount = COUNT(0) from 
(
select a.StoreID,case when InGroupRuleCount = ALLGoupRuleCount then 1 else 0 end as IsALLGroupRule from 
(
select a.StoreID,COUNT(0) as InGroupRuleCount from 
(
select StoreID,RuleType from 
( select StoreID from Base_StoreInfo where MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId) and StoreID <> @StoreId ) a 
cross join 
( select DictValue as RuleType from dict_system where PID = (select Id from dict_system where DictKey = '规则类别' and DictLevel = 0 and MerchID is null) ) b
) a
left join 
( select a.StoreID,RuleType from Base_ChainRule_Store a inner join Base_ChainRule b on a.RuleGroupID = b.RuleGroupID where b.MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId)) b
on a.StoreID = b.StoreID and a.RuleType = b.RuleType
group by a.StoreID
) a
cross join 
( select COUNT(0) as ALLGoupRuleCount from Base_ChainRule where MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId)) b
) a 
inner join 
(
select c.StoreID,a.ICCardID from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID inner join Data_Member_Card_Store c on a.ID = c.CardID 
inner join Base_StoreInfo d on d.StoreID =  c.StoreID inner join Base_MerchantInfo e on d.MerchID = e.MerchID
where e.MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId) and b.Mobile = @Mobile and c.StoreID <> @StoreId
) b 
on a.StoreID = b.StoreID
where IsALLGroupRule = 1

if @RecordCount = 0 
  return 0
else
  return 1
GO
/****** Object:  StoredProcedure [dbo].[GetMemberRepeatCode]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[GetMemberRepeatCode](@ICCardID varchar(50),@StoreID varchar(50),@RepeatCode int output,@ErrMsg varchar(200) output)
as
	--获取卡号的卡ID,
	declare @CardID int = 0 
	select @CardID = isnull(max(CardID),0) from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID
	if @CardID = 0
	   begin
		 set @ErrMsg = '会员卡不存在'
		 return 0
		 return 
	   end 
    
    select @RepeatCode = RepeatCode from Data_Member_Card where ID = @CardID
    
    return 1
GO
/****** Object:  StoredProcedure [dbo].[GetFoodInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodInfo](@StoreId varchar(15),@FoodId int,@CustomerType int,@MemberLevelId int,@FoodName varchar(200) output,@FoodType int output,
@FoodSalePrice decimal(18,2) output,@AllowPrint int output,@ForeAuthorize int output,@AllowInternet int output,@ErrMsg varchar(200) output)
as
  --@CustomerType--0:散客,1:新会员注册,2:会员
  --如果是散客,@MemberLevelId值为0
  declare @DataFoodLevelId int = 0
  declare @DataFoodLevelVIPPrice decimal(18,2) = 0
  declare @DataFoodLevelClientPrice decimal(18,2) = 0
  declare @TmpMemberLevelId int = 0
  declare @TmpFoodType int = 0
  declare @StartTime datetime
  declare @EndTime datetime
  declare @ClientPrice decimal(18,2) = 0
  declare @MemberPrice decimal(18,2) = 0
  declare @Week varchar(20)
  declare @DaySaleCount int = 0
  declare @MemberDaySaleCount int = 0

 --获取套餐主表ID
  if not exists (select COUNT(0)
  from Data_FoodInfo a inner join Data_Food_StoreList b on
  a.FoodID = b.FoodID and b.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
  where a.FoodID = @FoodId)
    begin
      set @ErrMsg = '存在无效套餐'
      return 0
    end
    
    create table #TmpGetFoodInfo_FoodInfo(FoodID int,FoodName varchar(200),FoodType int,AllowInternet int,AllowPrint int,ForeAuthorize int,FoodPrice decimal(18,2),ImageUrl varchar(200))
    insert into #TmpGetFoodInfo_FoodInfo(FoodID,FoodName,FoodType,AllowInternet,AllowPrint,ForeAuthorize,FoodPrice,ImageUrl)
  	select a.FoodId,a.FoodName,a.FoodType,AllowInternet,AllowPrint,ForeAuthorize,isnull(b.FoodPrice,a.FoodPrice) as FoodPrice,a.ImageURL
	from 
	(
		select a.FoodId,a.FoodName,a.FoodType,AllowPrint,AllowInternet,ForeAuthorize,
		isnull(case @CustomerType when 0 then a.ClientPrice else a.MemberPrice end,0) AS FoodPrice,a.ImageURL 
		from Data_FoodInfo a where a.FoodId = @FoodId
	) a
	left join 
	(
		select a.FoodId,isnull(case @CustomerType when 0 then ClientPrice else VIPPrice end,0) AS FoodPrice 
		from 
		(
			select a.FoodId,MAX(ID) as MemberLevelId from Data_Food_Level a 
			left join Data_Food_Record b on a.ID = b.FoodLevelID and DATEDIFF(DY,RecordDate,GETDATE()) = 0
			where a.FoodID = @FoodId and DATEDIFF(DY,StartDate,GETDATE()) >= 0 and DATEDIFF(dy,EndDate,GETDATE()) <= 0
			and DATEDIFF(DY,StartTime,cast(GETDATE() as time)) >= 0 and DATEDIFF(dy,EndTime,cast(GETDATE() as time)) <= 0
			and dbo.CheckWeekStr(a.Week) = 1 and a.MemberLevelID = @MemberLevelId
			and isnull((case @CustomerType when 0 then b.day_sale_count else b.member_day_sale_count end),0) - isnull((case @CustomerType when 0 then b.day_sale_count else b.member_day_sale_count end),0) > 0
			group by a.FoodID
		) a
		inner join Data_Food_Level b on a.MemberLevelId = b.ID
	) b
	on a.FoodId = b.FoodId  
          
  select top 1 
  @FoodName = FoodName,@FoodType = FoodType,@FoodSalePrice = FoodPrice,@AllowPrint = AllowPrint,
  @ForeAuthorize = ForeAuthorize,@AllowInternet = AllowInternet
  from #TmpGetFoodInfo_FoodInfo
  drop table #TmpGetFoodInfo_FoodInfo 
 
  select a.FoodId,a.BalanceType,TypeName,UseCount from Data_Food_Sale a
  inner join Dict_BalanceType b on a.BalanceType = b.ID
  where a.FoodID = @FoodId and b.State = 1
  order by a.FoodId,a.BalanceType
     
  return 1
GO
/****** Object:  StoredProcedure [dbo].[GetFoodDetailCount]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodDetailCount](@FoodDetail [FoodDetailType] readonly,@FoodDetailCount int output)
as
  select @FoodDetailCount = sum(FoodContainCount * b.FoodCount) from 
  (
	 select a.FoodID,SUM(c.ContainCount) as FoodContainCount from Data_FoodInfo a inner join @FoodDetail b on a.FoodID = b.FoodID inner join Data_Food_Detial c on a.FoodID = c.FoodId where a.FoodState = 1 and c.Status = 1
	 group by a.FoodID
  ) a
  inner join @FoodDetail b
  on a.FoodID = b.FoodId
GO
/****** Object:  StoredProcedure [dbo].[GetMemberOpenCardFood]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberOpenCardFood](@StoreId varchar(20),@CurtomerType int,@MemberLevelId int)
as
if @CurtomerType = 0 or @CurtomerType = 2
  begin
     select a.FoodID from 
	 (
		 select a.FoodID
		 from Data_Food_StoreList a inner join Data_FoodInfo b on a.FoodID = b.FoodID
		 where a.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
	 ) a
	 inner join
	 (
		 select c.FoodID from Base_StoreInfo a inner join Data_MemberLevel b on a.MerchID = b.MerchID 
		 inner join Data_MemberLevel_Food c on c.MemberLevelID = b.MemberLevelID
		 where StoreID = @StoreId
	 ) b 
	 on a.FoodID = b.FoodID
  end
if @CurtomerType = 1
  begin
     select a.FoodID from 
	 (
		 select a.FoodID
		 from Data_Food_StoreList a inner join Data_FoodInfo b on a.FoodID = b.FoodID
		 where a.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
	 ) a
	 inner join
	 (
		 select c.FoodID from Base_StoreInfo a inner join Data_MemberLevel b on a.MerchID = b.MerchID 
		 inner join Data_MemberLevel_Food c on c.MemberLevelID = b.MemberLevelID
		 where StoreID = @StoreId and c.MemberLevelID = @MemberLevelId
	 ) b 
	 on a.FoodID = b.FoodID
  end
GO
/****** Object:  StoredProcedure [dbo].[GetMemberLevel]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberLevel](@StoreId varchar(15))
as
  select a.MemberLevelID,a.MemberLevelName,CoverURL,OpenFee,Deposit from Data_MemberLevel a
  where a.MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId) and State = 1
GO
/****** Object:  StoredProcedure [dbo].[GetMemberId]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberId](@StoreId varchar(15),@ICCardId int,@MemberId varchar(32) output,@CanUse bit output,@CardStatusName varchar(200) output,@ErrMsg varchar(200) output)
  as
    declare @CardStatus int = 0    
    set @CanUse = 0
    select @MemberId = MemberID,@CardStatus = CardStatus from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where a.StoreId = @StoreId and b.ICCardID = @ICCardId
    select @CardStatusName = dbo.F_GetCardStatus(@CardStatus)
    if @MemberId = ''
      begin
        set @ErrMsg = '会员卡不存在'
        return 0
      end
    
    if @CardStatus <> 1
      begin
        set @ErrMsg = '会员卡不能使用' + '(卡状态[' + @CardStatusName + '])'
        return 0
      end  
    
    set @CanUse = 1  
    return 1
GO
/****** Object:  StoredProcedure [dbo].[GetSchedule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetSchedule](@StoreId varchar(50),@UserId int,@WorkStation varchar(50),@CurrentSchedule int output,@OpenTime varchar(20) output,@errMsg varchar(20) output)
as
select @CurrentSchedule = id,@OpenTime = CONVERT(char(19),OpenTime,120) from flw_schedule where StoreID = @StoreId and UserID = @UserId and WorkStation = @WorkStation and State = 0
if @CurrentSchedule > 0 
  return 1
else
  begin
    set @errMsg = '班次信息不存在'
	return 0
  end
GO
/****** Object:  StoredProcedure [dbo].[GetSameGroupRuleTypeStoreInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetSameGroupRuleTypeStoreInfo](@StoreId varchar(15),@GroupRuleType int)
as
--@GroupRuleType0 代币,1 积分,2 彩票,3 储值金
--当前门店所在商户是否存在完全规则的门店已开通当前手机号的会员卡
select a.StoreID from Base_ChainRule_Store a inner join Base_ChainRule b on a.RuleGroupID = b.RuleGroupID 
where b.MerchID = (select MerchID from Base_StoreInfo where StoreID = @StoreId) and RuleType = 2 and a.StoreID <> @StoreId
GO
/****** Object:  StoredProcedure [dbo].[GetOrdersCheck]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[GetOrdersCheck](@CheckDate varchar(10), @MerchId varchar(50))
as
 begin
	select MAX(T1.ID) as ID, T1.CheckDate,T1.StoreName, T1.ScheduleState, SUM(T2.PayCount) as PayCount, SUM(T3.WechatRealPay) as WechatRealPay,
			SUM(T4.AliRealPay) as AliRealPay, SUM(T5.GroupBuyRealPay) as GroupBuyRealPay, SUM(T6.CoinMoney) as CoinMoney,
			SUM(T7.TicketCoins) as TicketCoins, SUM(T7.TicketCoinMoney) as TicketCoinMoney, SUM(T8.FreeCoin) as FreeCoin, 
			SUM(T9.SaveCoin) as SaveCoin, SUM(T10.OpenCount) as OpenCount, SUM(T10.OpenDeposit) as OpenDeposit, SUM(T10.OpenFee) as OpenFee,
			SUM(T11.RefundCount) as RefundCount, SUM(T11.RefundDeposit) as RefundDeposit, SUM(T12.TokenRealPay) as TokenRealPay, SUM(T13.RechargeRealPay) as RechargeRealPay,
			SUM(T14.CoinRealPay) as CoinRealPay, SUM(T15.GoodRealPay) as GoodRealPay, SUM(T16.TicketRealPay) as TicketRealPay, SUM(T17.GroupBuyCount) as GroupBuyCount		
	from 
	(
	SELECT     c.ID, b.CheckDate, a.StoreName, b.ID AS ScheduleID, (CASE WHEN EXISTS
						  (SELECT 1
							FROM dbo.Flw_Schedule
							where ID=b.ID and CheckDateID is null) then '有空班' WHEN EXISTS
                          (SELECT     1
                            FROM          dbo.Flw_Schedule
                            WHERE      [State] = 0 AND CheckDateID = c.ID and c.ID is not null) THEN '进行中' WHEN NOT EXISTS
                          (SELECT     1
                            FROM          dbo.Flw_Schedule
                            WHERE      [State] IN (0, 2) AND CheckDateID = c.ID and c.ID is not null) THEN '未审核' ELSE '已审核' END) AS ScheduleState
	FROM         dbo.Base_StoreInfo AS a INNER JOIN
				dbo.Flw_Schedule AS b on a.StoreID = b.StoreID left join
				  dbo.Flw_CheckDate AS c ON b.CheckDateID = c.ID and a.StoreID = c.StoreID and b.CheckDate = c.CheckDate
    where b.CheckDate=CONVERT(date, @CheckDate) and a.MerchID=@MerchId
    ) as T1 left join
    (
	  SELECT     ScheduleID, PayCount
	  FROM		dbo.Flw_Order
    ) as T2 on T1.ScheduleID=T2.ScheduleID left join
    (
		SELECT     ScheduleID, RealPay AS WechatRealPay
        FROM          dbo.Flw_Order 
        WHERE      (PayType = 1) 
    ) as T3 on T1.ScheduleID=T3.ScheduleID left join
    (
		SELECT     ScheduleID, RealPay AS AliRealPay
        FROM          dbo.Flw_Order
        WHERE      (PayType = 2)
    ) as T4 on T1.ScheduleID=T4.ScheduleID left join
    (
		SELECT     ScheduleID, RealPay AS GroupBuyRealPay
        FROM          dbo.Flw_Order
        WHERE      (OrderSource IN (2, 3, 4))
    ) as T5 on T1.ScheduleID=T5.ScheduleID left join
    (
		SELECT     ScheduleID, CoinMoney
		FROM         dbo.Flw_Coin_Exit 
		WHERE     (FlowType = 0)
    ) as T6 on T1.ScheduleID=T6.ScheduleID left join
    (
		SELECT     ScheduleID, Coins AS TicketCoins, CoinMoney AS TicketCoinMoney
		FROM         dbo.Flw_Ticket_Exit
		WHERE     (State = 1) 
    ) as T7 on T1.ScheduleID=T7.ScheduleID left join
    (
		SELECT     ScheduleID, Coins AS FreeCoin
	   FROM          dbo.Flw_Coin_Sale
	   WHERE      (WorkType IN (2, 5, 8))
    ) as T8 on T1.ScheduleID=T8.ScheduleID left join
    (
		SELECT     ScheduleID, Coins AS SaveCoin
		FROM          dbo.Flw_Coin_Sale
		WHERE      (WorkType = 4)
    ) as T9 on T1.ScheduleID=T9.ScheduleID left join
    (
		select a.ScheduleID, c.ID as OpenCount, c.Deposit as OpenDeposit, c.OpenFee
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=1
    ) as T10 on t1.ScheduleID=t10.ScheduleID left join 
    (
		select a.ScheduleID, c.ID as RefundCount, c.Deposit as RefundDeposit
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=7
    ) as t11 on t1.ScheduleID=t11.ScheduleID left join
    (
		select a.ScheduleID, a.RealPay as TokenRealPay
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=2
    ) as T12 on t1.ScheduleID=t12.ScheduleID left join
    (
		select a.ScheduleID, a.RealPay as RechargeRealPay
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=0 and c.BuyFoodType=1
    ) as T13 on t1.ScheduleID=t13.ScheduleID left join
    (
		select a.ScheduleID, a.RealPay as CoinRealPay
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=0 and c.BuyFoodType in (2,3)
    ) as T14 on t1.ScheduleID=t14.ScheduleID left join
    (
		select a.ScheduleID, a.RealPay as GoodRealPay
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=3 
    ) as T15 on t1.ScheduleID=t15.ScheduleID left join
    (
		select a.ScheduleID, a.RealPay as TicketRealPay
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		where c.FlowType=4 
    ) as T16 on t1.ScheduleID=t16.ScheduleID left join
    (
		select a.ScheduleID, d.ContainCount as GroupBuyCount
		from dbo.Flw_Order a
		inner join dbo.Flw_Order_Detail b on a.ID=b.OrderFlwID
		inner join dbo.Flw_Food_Sale c on b.FoodFlwID=c.ID
		inner join dbo.Flw_Food_SaleDetail d on c.ID=d.FlwFoodID
		where a.OrderSource in (2,3,4) and d.SaleType=0
    ) as T17 on t1.ScheduleID=t17.ScheduleID
    
    group by T1.CheckDate, T1.StoreName, T1.ScheduleState      
                      
 end
GO
/****** Object:  StoredProcedure [dbo].[GetStoreGroupRule]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetStoreGroupRule](@StoreID varchar(15),@StoreGroupRule int output)
as
 --获取门店的组规则设置
 --StoreGroupRule门店组规则设置类型，2-完全组规则，1-部分组规则，0-没有组规则
select a.StoreID,a.RuleType,case when b.RuleType IS null then 0 else 1 end as InGroupRule into #TmpStoreGroupRule from  
(
select StoreID,RuleType from 
( select @StoreID as StoreID ) a
cross join 
( select DictValue as RuleType from dict_system where PID = (select Id from dict_system where DictKey = '规则类别' and DictLevel = 0 and MerchID is null) ) b
) a
left join 
( select a.StoreID,RuleType from Base_ChainRule_Store a inner join Base_ChainRule b on a.RuleGroupID = b.RuleGroupID where a.StoreID = @StoreID) b 
on a.StoreID = b.StoreID and a.RuleType = b.RuleType

declare @InGroupRuleCount int = 0
declare @NoInGroupRuleCount int = 0
select @InGroupRuleCount = COUNT(0) from #TmpStoreGroupRule where InGroupRule = 1
select @NoInGroupRuleCount = COUNT(0) from #TmpStoreGroupRule where InGroupRule = 0

if @InGroupRuleCount = 0
  set @StoreGroupRule = 0 --没有组规则
else if @NoInGroupRuleCount = 0
  set @StoreGroupRule = 2 --完全组规则
else
  set @StoreGroupRule = 0 --部分组规则
GO
/****** Object:  StoredProcedure [dbo].[GetOrderContainById]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetOrderContainById](
@StoreId varchar(15),
@OrderFlwId varchar(32),
@CustomerType int output,
@ICCardId int output,
@PayCount decimal(18,2) output,
@RealPay decimal(18,2) output,
@FreePay decimal(18,2) output,
@FoodCount int output,
@DetailsGoodsCount int output,
@MemberLevelId int output,
@MemberLevelName varchar(200) output,
@OpenFee decimal(18,2) output,
@Deposit decimal(18,2) output,
@RenewFee decimal(18,2) output,
@ChangeFee decimal(18,2) output
)
as
 declare @CardId int = 0
 
 select a.CardID,a.ID as OrderFlwID,StoreID,PayCount,RealPay,FreePay,a.FoodCount,
 a.GoodCount as DetailGoodsCount,a.CreateTime into #TmpOrder 
 from dbo.Flw_Order a where a.StoreID = @StoreId and a.ID = @OrderFlwId
  
 select b.FoodID,b.SaleCount,MemberLevelID,Deposit,OpenFee,RenewFee,ChangeFee,BuyFoodType,
 isnull(c.BalanceIndex,0) as BalanceIndex,isnull(PayCount,0.00) as PayCount
 into #TmpFoodSale
 from 
 ( 
	 select b.OrderFlwID,b.ID as OrderDetailFlwId,b.GoodsCount as DetailGoodsCount,FoodFlwID
	 from dbo.Flw_Order a inner join dbo.Flw_Order_Detail b on a.ID = b.OrderFlwID
	 where a.ID = @OrderFlwId
 ) a
 inner join Flw_Food_Sale b on a.FoodFlwID = b.ID
 left join Flw_Food_Sale_Pay c on a.FoodFlwID = c.FlwFoodID

 select top 1 @MemberLevelID = MemberLevelID from #TmpFoodSale
 select top 1 @CardId = CardId from #TmpOrder

 if @MemberLevelID = 0 
   begin
	 set @CustomerType = 0
   end
 else 
   begin
	 if @CardId = 0 
	   set @CustomerType = 1
	 else
	   set @CustomerType = 2
   end

 select
 @ICCardId = (select ICCardID from Data_Member_Card where ID = @CardId),
 @PayCount = PayCount,@RealPay = RealPay,@FreePay = FreePay,
 @FoodCount = FoodCount,@DetailsGoodsCount = DetailGoodsCount,
 @MemberLevelName = (select MemberLevelName from Data_MemberLevel where MemberLevelID = @MemberLevelID )
 from #TmpOrder
 
 select a.*,b.FoodName into #TmpFoodSale2 from 
 (
	select FoodID,SaleCount as FoodCount,BalanceIndex as PayType,PayCount as PayNum,Deposit,OpenFee,ChangeFee,RenewFee from #TmpFoodSale
 ) a
 left join Data_FoodInfo b on a.FoodID = b.FoodID
 
 select @OpenFee = OpenFee,@Deposit = Deposit,@ChangeFee = ChangeFee,@RenewFee = RenewFee from #TmpFoodSale2 where OpenFee <> 0 or Deposit <> 0
 
 select FoodID,FoodCount,PayType,
 case PayType when 0 then '现金' else b.TypeName end AS PayTypeName,
 PayNum,FoodName from #TmpFoodSale2 a left join Dict_BalanceType b on a.PayType = b.ID
GO
/****** Object:  StoredProcedure [dbo].[GetMerchOtherStoreMemberCardInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMerchOtherStoreMemberCardInfo](@StoreId varchar(20),@Mobile varchar(20))
as 
  --获取用户
  begin
	select a.ICCardID,d.StoreID,d.StoreName from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID inner join Data_Member_Card_Store c on c.CardIndex = a.ID inner join 
    Base_StoreInfo d on c.StoreID = d.StoreID where b.Mobile = @Mobile and d.MerchID = ( select MerchID from Base_StoreInfo where StoreID = @StoreId ) and d.StoreID <> @StoreId
  end
GO
/****** Object:  StoredProcedure [dbo].[GetMemberSumInfo(不用了）]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberSumInfo(不用了）](@StoreID varchar(15),@ICCardID int,@MemberLevelId int,@CardID int output,@IsBirthday bit output,@MemberPoints int output,@MemberLotterys int output,@MemberCoins int output,@ErrMsg varchar(200) output)
as
  declare @MemberBirthday varchar(10) = ''		
  --获取会员基础信息,设置相关变量
  CREATE TABLE dbo.#Tmp_Member(CardId int,ICCardID varchar(20) NULL,MemberLevelID int NULL,MemberName varchar(50) NULL,Gender int NULL,Birthday varchar(100) NULL,
  IDCard varchar(18) NULL,Mobile varchar(11) NULL,Deposit numeric(18, 2) NULL,Note text NULL,EndDate varchar(100) NULL,RepeatCode int NULL,MemberState int NULL,CardStatus int NULL,
  MemberLevelName varchar(20) NULL,StoreID varchar(15) NOT NULL,StoreName varchar(50) NULL,Storage decimal(10, 2) NOT NULL,Banlance decimal(10, 2) NOT NULL,
  Point decimal(10, 2) NOT NULL,Lottery decimal(10, 2) NOT NULL)
  declare @GetMemberReturn int 
  insert into #Tmp_Member
  exec dbo.GetMember @ICCardID,@StoreID,@GetMemberReturn output,@ErrMsg output
  if @GetMemberReturn = 0 
    begin
	  return 0
    end
  
  select @CardID = CardId,@MemberPoints = Point,@MemberLotterys = Lottery,@MemberCoins = Banlance,@MemberLevelId = MemberLevelID,@MemberBirthday = Birthday from #Tmp_Member
  if @MemberBirthday = CONVERT(char(10),getdate(),120) 
    set @IsBirthday = 1
    
  return 1
GO
/****** Object:  StoredProcedure [dbo].[GetMemberCouponList]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberCouponList](@StoreId varchar(15),@ICCardId int)
as
  declare @MemberId varchar(32)
  set @MemberId = dbo.F_GetMemberId(@StoreId,@ICCardId)
  --0 礼品;1 门票;2 代币
  select DictKey as CouponTypeName,cast(DictValue as int) as CouponType  from Dict_System where PID = 482
  select a.CouponId,CouponName,a.CouponType,b.CouponCode,Convert(char(10),StartDate,120) as StartDate,Convert(char(10),EndDate,120) as EndDate,
  Convert(char(8),StartTime,120) as StartTime,Convert(char(8),EndTime,120) as EndTime,Convert(char(10),NoStartDate,120) as NoStartDate,Convert(char(10),NoEndDate,120) as NoEndDate,
  dbo.GetWeekDate(WeekType,Week) as UseDay,
  case CouponType 
  when 0 then case CouponDiscount when 100 then '满' + CAST(OverMoney AS varchar) + ',优惠' + cast(CouponValue as varchar) + '%,最高优惠' + cast(CouponThreshold as varchar) + '元'
  else '满' + CAST(OverMoney AS varchar) + ',折扣' + cast(CouponDiscount as varchar) + '%,再优惠' + cast(CouponValue as varchar) + ',最高优惠' + cast(CouponThreshold as varchar) + '元' End 
  when 1 then '满' + CAST(OverMoney AS varchar) + ',折扣' + cast(CouponDiscount as varchar) + '%,最高优惠' + cast(CouponThreshold as varchar) + '元'  
  when 2 then '兑换' + case ChargeType when 0 then dbo.F_GetGoodDesc(GoodID) + '(1件)' when 1 then dbo.F_GetProjectDesc(ProjectID) + '(1张)' when 2 then CAST(ChargeCount AS varchar) + '代币' end 
  end AS UseDesc,dbo.[F_GetJackpotDesc](JackpotId,JackpotCount) as JackpotDesc, 
  '满' + CAST(OverMoney AS varchar) + '送' + CAST(FreeCouponCount AS varchar) + '张券' as FreeCouponDesc,
  dbo.F_GetCouponStatusName(b.State) as StateName,dbo.F_GetCouponLockName(b.IsLock) as LockStatuName
  from 
  ( select a.StoreID,b.ID as CouponId,b.CouponName,b.StartDate,b.EndDate,b.StartTime,b.EndTime,b.WeekType,b.Week,b.CouponType,
    OverMoney,AllowOverOther,CouponValue,CouponDiscount,CouponThreshold,OverUseCount,ChargeType,ChargeCount,GoodID,ProjectID,
    FreeCouponCount,JackpotCount,JackpotId,NoStartDate,NoEndDate
    from Data_Coupon_StoreList a inner join Data_CouponInfo b on a.CouponID = b.ID where a.StoreID = @StoreId ) a
  inner join Data_CouponList b on a.StoreID = b.StoreID and a.CouponId = b.CouponID and MemberID = @MemberId
  where b.State = 2
GO
/****** Object:  StoredProcedure [dbo].[GetFoodDetail]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodDetail](@StoreId varchar(15),@Category int,@FoodID int)
as
--0 套餐;1 门票单品;2 商品单品
if @Category = 0 
  begin
	create table #TmpDetailFood(DetailId int ,DetailFoodType int,DetailFoodTypeName varchar(200),ContainCount int,ContainName varchar(200))
	insert #TmpDetailFood(DetailId,DetailFoodType,DetailFoodTypeName,ContainCount,ContainName)
	
	select
	c.ID as DetailId,c.FoodType as DetailFoodType,
	case c.FoodType when 0 then '' when 1 then '餐饮' when 2 then '礼品' when 3 then '门票' end as DetailFoodTypeName,
	case c.FoodType when 0 then ContainCount when 1 then ContainCount when 2 then ContainCount when 3 then ContainCount end as ContainCount,
	case c.FoodType
	when 0 then (select TypeName from Dict_BalanceType where ID = c.BalanceType)
	when 1 then (select isnull(GoodName,'') From Base_GoodsInfo where ID = ContainID and Status = 1)
	when 2 then (select isnull(GoodName,'') From Base_GoodsInfo where ID = ContainID and Status = 1)
	when 3 then (select dbo.F_GetTicketDesc(ContainID,ContainCount,Days))
	end as ContainName
	from Data_FoodInfo a inner join Data_Food_StoreList b on
	a.FoodID = b.FoodID and b.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
	inner join Data_Food_Detial c on a.FoodID = c.FoodID and c.Status = 1
	where b.FoodID = @FoodID

	update #TmpDetailFood set DetailFoodTypeName = ContainName,ContainName = cast(ContainCount as varchar) + ContainName where DetailFoodType = 0
	update #TmpDetailFood set ContainName = ContainName + '(' + CAST(ContainCount as varchar) + '件' + ')' where DetailFoodType <> 0
	select * from #TmpDetailFood	
  end
else if @Category = 1
  begin
    select TicketName as Note from Data_ProjectTicket where StoreID = @StoreId and ID = @FoodID
  end
else if @Category = 2
  begin
	select Note from Base_GoodsInfo where StoreID = @StoreId and id = @FoodID
  end
GO
/****** Object:  UserDefinedFunction [dbo].[F_GetUsedFoodLevelCount]    Script Date: 05/17/2018 09:37:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE function [dbo].[F_GetUsedFoodLevelCount](@CustomerType int,@FoodLevelId int) returns int
as
  begin
    declare @StartDate date
    declare @EndDate date
    declare @AllFreqType int
    declare @MemberFreqType int
    
    select @AllFreqType = AllFreqType,@MemberFreqType = MemberFreqType from Data_Food_Level where ID = @FoodLevelId
    
    declare @SaleCount int = 0
    if @CustomerType = 0 or @CustomerType = 1
      begin
		select @StartDate = dbo.F_GetStartDate(@AllFreqType)
		select @EndDate = dbo.F_GetEndDate(@AllFreqType)
		select @SaleCount = sum(day_sale_count) from Data_Food_Record 
        where FoodLevelID = @FoodLevelId and DATEDIFF(YEAR,@StartDate,RecordDate) >= 0 and DATEDIFF(YEAR,@EndDate,RecordDate) <= 0
      end
    else if @CustomerType = 2
      begin
		select @StartDate = dbo.F_GetStartDate(@MemberFreqType)
		select @EndDate = dbo.F_GetEndDate(@MemberFreqType)
		select @SaleCount = sum(member_day_sale_count) from Data_Food_Record 
        where FoodLevelID = @FoodLevelId and DATEDIFF(YEAR,@StartDate,RecordDate) >= 0 and DATEDIFF(YEAR,@EndDate,RecordDate) <= 0
      end
    return @SaleCount	
  end
GO
/****** Object:  StoredProcedure [dbo].[CheckOrderPay]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckOrderPay](@StoreId varchar(15),@CustomerType int,@MemberLevelId int,@MemberBalanceList [MemberBalanceListType] readonly,@FoodDetailType [FoodDetailType] readonly,@PayCount decimal(18,2),@OpenFee decimal(18,2),@Deposit decimal(18,2),@Result int output,@ErrMsg varchar(200) output)
as
    --套餐详情用的变量
    declare @FoodId int = 0
    declare @FoodCount int = 0
	declare @FoodName varchar(200) = ''
	declare @FoodType int = 0
	declare @ForeAuthorize int = 0
	declare @AllowInternet int = 0
	declare @AllowPrint int = 0
	declare @TmpFoodSalePrice decimal(18,6) = 0--累计购物车明细中的价格
	declare @FoodSalePrice decimal(18,2)
	declare @PayType int
	declare @PayNum decimal(18,2)
	declare @OpenCardFoodTypeNum int = 0--开卡套餐类型数量
	set @Result = 0
	
	--验证散客和开卡的支付方式
	if @CustomerType = 0 or @CustomerType = 1
	  begin
		if exists ( select 1 from @FoodDetailType where PayType <> 0 )
		  begin
		    set @ErrMsg = '当前用户只能使用现金或第三方支付购买'
		    return 0
		  end
	  end

	--添加默认现金类型
	insert #PayTypeSum(PayType,PaySum)
	select distinct PayType,0 as PaySum from @FoodDetailType

	--select FoodId,FoodCount,PayType,PayNum into Flw_FoodDetailType from @FoodDetailType

    --历便套餐明细,验证支付明细数据
    declare @GetFoodInfoReturn int
	declare cur1 cursor for
	select FoodId,FoodCount,PayType,PayNum from @FoodDetailType
	open cur1  
	fetch next from cur1 into @FoodId,@FoodCount,@PayType,@PayNum
	WHILE @@FETCH_STATUS = 0
	  begin	    
	    --获取套餐信息
	    create table #TmpGetFoodInfo_BalanceInfo(FoodId int,BalanceType int,TypeName varchar(200),UseCount decimal(18,2))
	    insert #TmpGetFoodInfo_BalanceInfo(FoodId,BalanceType,TypeName,UseCount)
	    exec @GetFoodInfoReturn = GetFoodInfo @StoreId,@FoodId,@CustomerType,@MemberLevelId,@FoodName output,@FoodType output,@FoodSalePrice output,@AllowPrint output,@ForeAuthorize output,@AllowInternet output,@ErrMsg output
	    if @GetFoodInfoReturn = 0 
	      return 0
	    insert #TmpGetFoodInfo_BalanceInfo(FoodId,BalanceType,TypeName,UseCount) values(@FoodId,0,'默认现金',@FoodSalePrice)
	     
	    --合计购买商品所需的余额总额，并验证购买商品是否可以使用对应的余额
        --验证套餐的支付方式是否有效
		if not exists (select 0 from #TmpGetFoodInfo_BalanceInfo where BalanceType = @PayType)
		  begin
		    set @ErrMsg = '套餐' + @FoodName + '的支付方式无效'
			return 0
		  end
		--验证套餐所选支付方式的支付总额是否正确
		declare @TmpPayNum decimal(18,2) = (select top 1 UseCount from #TmpGetFoodInfo_BalanceInfo where BalanceType = @PayType) * @FoodCount
		if @PayNum <> @TmpPayNum
		  begin
			set @ErrMsg = '套餐' + @FoodName + '的支付金额不正确' + CAST(@TmpPayNum as varchar) +'_' + CAST(@PayNum as varchar)
			return 0
		  end
	    --增加当前支付类型的支付金额
	    update #PayTypeSum set PaySum = PaySum + @PayNum where PayType = @PayType
	    drop table #TmpGetFoodInfo_BalanceInfo    
		fetch next from cur1 into @FoodId,@FoodCount,@PayType,@PayNum
	  end
	close cur1 
	deallocate cur1

	--验证现金金额
	declare @TmpPaySum decimal(18,2) = 0
	select @TmpPaySum = PaySum from #PayTypeSum where PayType = 0
	--print '@TmpPaySum:' + cast(@TmpPaySum as varchar)
	--print '@TmpPaySum:' + cast(@TmpPaySum as varchar)
	--print '@Deposit:' + cast(@Deposit as varchar)
	--print '@OpenFee:' + cast(@OpenFee as varchar)
	if @PayCount <> @TmpPaySum + @Deposit + @OpenFee
	  begin
	    set @ErrMsg = '实付现金金额不正确' 
	    return
	  end
	  
	declare @BalanceName varchar(50) = '' 
	if exists (select 0 from @MemberBalanceList a inner join #PayTypeSum b on a.BalanceIndex = b.PayType where a.Balance < b.PaySum)
	  begin
		select top 1 @BalanceName = a.BalanceName from @MemberBalanceList a inner join #PayTypeSum b on a.BalanceIndex = b.PayType where a.Balance < b.PaySum
		set @ErrMsg = '会员' + @BalanceName + '余额不足'
		return 0
	  end 
	  
	set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[CheckCacheOrder]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckCacheOrder](@StoreId varchar(15),@FlwOrderId varchar(32),@CustomerType int,@ICCardId int,@ErrMsg varchar(200) output)
as
  declare @CardId int = 0
  declare @Id int = 0
  declare @OrderStatus int = 0
  select @Id = id,@CardId = CardId,@OrderStatus = OrderStatus from Flw_Order where StoreID = @StoreId and ID = @FlwOrderId
  if @Id = 0
    begin
	  set @ErrMsg = '订单不存在'
	  return 0
    end
    
  if @OrderStatus <> 1
    begin
	  set @ErrMsg = '订单装填不正确'
	  return 0
    end
 
  if @CustomerType <> 0 and @CustomerType <> 1 and @CustomerType <> 2
    begin
      set @ErrMsg = '客人类型不正确(1)'
	  return 0
    end
 
  if @CustomerType = 0
    begin
      if @CardId <> 0 
        begin
          set @ErrMsg = '客人类型不正确(2)'
          return 0
        end
      if @ICCardId <> 0 
        begin
		  set @ErrMsg = '客人类型不正确(2)'
          return 0
        end  
    end
 
  if @CustomerType = 1
    begin
      if @CardId <> 0 
        begin
          set @ErrMsg = '客人类型不正确(3)'
          return 0
        end
      if @ICCardId = 0 
        begin
		  set @ErrMsg = '客人类型不正确(3)'
          return 0
        end
    end
  
  if @CustomerType = 2
    begin
      if @CardId = 0 
        begin
          set @ErrMsg = '客人类型不正确(3)1'
          return 0
        end
      declare @MemberId int = 0
      set @MemberId = dbo.F_GetMemberId(@StoreId,@ICCardId)
      if @CardId <> @MemberId 
        begin
		  set @ErrMsg = '客人类型不正确(3)2'
          return 0
        end
    end  
  
  return 1
GO
/****** Object:  StoredProcedure [dbo].[GetDiscount]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetDiscount](@MerchId varchar(15),@StoreId varchar(15),@CustomerType int,@MemberId varchar(32),@FoodPrice decimal(18,2),@DiscountRuleID int output,@SubPrice decimal(18,2) output,@DiscountRuleName varchar(200) output,@ErrMsg varchar(200) output)
as
declare @CurrentDatetime datetime = getdate()
declare @CurrentDate date = @CurrentDatetime
declare @CurrentTime time = @CurrentDatetime
declare @StoreFreq int = 0--门店频率
declare @StoreCount int = 0--门店次数
declare @ShareCount int = 0--门店次数共享
declare @MemberFreq int = 0--会员频率
declare @MemberCount int = 0--会员次数
declare @UsedStoreCount int = 0--已使用的门店数量
declare @UsedShareCount int = 0--已使用的门店数量
declare @UsedMemberCount int = 0--已使用的门店数量
declare @AllowGuest int = 0--允许散客
set @DiscountRuleID = 0
select top 1 @DiscountRuleID = DiscountRuleID from 
(
select a.DiscountRuleID,MIN(RuleLevel) as RuleLevel from Data_Discount_StoreList a inner join Data_DiscountRule b on a.DiscountRuleID = b.Id
where a.StoreID = @StoreId 
and DATEDIFF(dy,StartDate,@CurrentDatetime) >= 0 and DATEDIFF(dy,EndDate,@CurrentDatetime) <= 0
and @CurrentTime >= StartTime and @CurrentTime <= EndTime
and not (DATEDIFF(dy,NoStartDate,@CurrentDatetime) >= 0 and DATEDIFF(dy,NoEndDate,@CurrentDatetime) <= 0)
and ( (weektype = 0 and dbo.CheckWeekStr(b.Week) = 1) or (weektype = 1 and dbo.F_IsWorkDay() = 1) or (weektype = 2 and dbo.F_IsWeekendDay() = 1) or (WeekType = 3 and dbo.GetFestival() = 1) )
and ( (@CustomerType = 0 and b.AllowGuest = 1) or @CustomerType = 1 or @CustomerType = 2 )
and State = 1
group by a.DiscountRuleID
) a
order by a.DiscountRuleID

if @DiscountRuleID = 0 
  begin
    set @ErrMsg = '满减规则不存在'
    return 0
  end

--获取规则数据
select @StoreFreq = StoreFreq,@StoreCount = StoreCount,@ShareCount = ShareCount,@MemberFreq = MemberFreq,@MemberCount = MemberCount,@AllowGuest = AllowGuest,@DiscountRuleName = RuleName from Data_DiscountRule a where a.ID = @DiscountRuleID
--获取规则使用记录
select @UsedShareCount = dbo.GetDiscountRecordByMerchId(@MerchId,@DiscountRuleID)
select @UsedStoreCount = dbo.GetDiscountRecordByStoreId(@MerchId,@StoreId,@DiscountRuleID)
select @UsedMemberCount = dbo.GetDiscountRecordByMemberId(@MerchId,@StoreId,@MemberId,@DiscountRuleID)

--如果是开卡用户，与散客一同处理
if @CustomerType = 0 or @CustomerType = 1
  begin	
	if @AllowGuest = 0 
	  begin
		set @ErrMsg = '满减规则不能用于散客'
		return 0
	  end  
	if @UsedShareCount + 1 > @ShareCount
	  begin
		set @ErrMsg = '已超过门店共享次数限制'
		return 0
	  end
	if @UsedStoreCount + 1 > @StoreCount
	  begin
		set @ErrMsg = '已超过门店次数限制'
		return 0
	  end
  end
else if @CustomerType = 2
  begin
	if @UsedShareCount + 1 > @ShareCount
	  begin
		set @ErrMsg = '已超过门店共享次数限制'
		return 0
	  end
	if @UsedStoreCount + 1 > @StoreCount
	  begin
		set @ErrMsg = '已超过门店次数限制'
		return 0
	  end
	if @UsedMemberCount + 1 > @MemberCount
	  begin
		set @ErrMsg = '已超过会员次数限制'
		return 0
	  end
  end

--计算满减金额
  select @SubPrice = dbo.GetDiscountFullSubPrice(@DiscountRuleID,@FoodPrice)

  return 1
GO
/****** Object:  StoredProcedure [dbo].[CreateOrderTicket]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CreateOrderTicket](@FlwOrderId int,@CardId int,@OrderCreateTime datetime)
as
   --添加门票数据
   declare @FoodFlwId int
   declare @TicketId int
   declare @TicketCount int
   declare @Days int
   declare @ValidType int
   declare cur cursor for
   select b.id as FoodFlwId,c.ContainID as TicketId,c.ContainCount as TicketCount,DATEDIFF(dy,@OrderCreateTime,ExpireDay) as Days,ValidType
   from Flw_Order_Detail a inner join Flw_Food_Sale b on a.FoodFlwID = b.ID 
   inner join Flw_Food_SaleDetail c on b.ID = c.FlwFoodID
   where OrderFlwID = @FlwOrderId and c.SaleType = 3 order by b.ID
   open cur
   fetch next from cur into @FoodFlwId,@TicketId,@TicketCount,@Days,@ValidType
	  WHILE @@FETCH_STATUS = 0
		begin
		  exec AddTicket @TicketId,@CardId,@FoodFlwId,@Days,@TicketCount
		  fetch next from cur into @FoodFlwId,@TicketId,@TicketCount,@Days,@ValidType
		end		
   close cur
   deallocate cur
GO
/****** Object:  StoredProcedure [dbo].[AddOrder]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddOrder](
@StoreID varchar(15),@CustomerType int,@MemberLevelId int,@CardID int,@SaleCoinType int,@OrderSource int,
@PayCount decimal(18,2),@OpenCardFoodId int,@Deposit decimal(18,2),@OpenFee decimal(18,2),
@UserID int,@CurrentSchedule int,@WorkStation varchar(50),@AuthorID int,@Note varchar(200), @PayTime datetime,@ModifyTime datetime,
@FoodDetail [FoodDetailType] readonly,@FlwSeedId varchar(29),@OrderFlwID varchar(32) output,@ErrMsg varchar(200) output)
as
   --插入订单表
    declare @TmpDeposit decimal(18,2)
    declare @TmpOpenFee decimal(18,2)
    declare @DetailId int
    declare @DetailFoodType int
    declare @NewId varchar(200)
	declare @OrderID varchar(20)
	declare @Emp_FoodDetailId varchar(50)
	declare @CurrentDatetime datetime
	declare @GetPoint int
	declare @OrderFoodCount int
	declare @OrderGetPoint int
	declare @OrderDetailFoodGoodCount int
	declare @OrderDetailCount int
	declare @PayType int
	declare @PayNum decimal(18,2)
	declare @FoodType int	
	declare @WeightType int
	declare @WeightValue decimal(18,2)
	declare @Days int
	declare @OrderStatus int = 0
	declare @FoodSaleTotalMoney decimal(18,2)	
	declare @FoodId int = 0
	declare @FlwIndex int = 1
	declare @Balance decimal(18,2) = 0	
	declare @FoodDetailTypeId int = 0	
	declare @ContainID int = 0
	declare @ContainCount int = 0
	declare @Status int = 0
	declare @ValidType int = 0
	declare @ExpireDay datetime	
	declare @FlwFoodSaleId varchar(32) = ''
	declare @FlwFoodSalePayId varchar(32) = ''
	declare @FlwFoodSaleDetailId varchar(32) = ''
	declare @FlwOrderDetailId varchar(32) = ''
	
    set @OrderStatus = 1
	set @PayTime = GETDATE()
	set @ModifyTime = GETDATE()
	set @CurrentDatetime = GETDATE()
	select @OrderFoodCount = SUM(FoodCount) from @FoodDetail
		
	--获取订单号
	exec GetOrderDateFormatStr @CurrentDatetime,@OrderID output
	set @OrderID =  @OrderID + @StoreID
	
	--获取商品总数
	declare @FoodDetailCount int
	exec GetFoodDetailCount @FoodDetail,@FoodDetailCount output
	
	--添加主订单信息
	select @OrderFlwID = dbo.F_GetFlwId(@FlwSeedId,@FlwIndex)
	set @FlwIndex = @FlwIndex + 1
	INSERT INTO Flw_Order(ID,StoreID,OrderID,FoodCount,GoodCount,CardID,OrderSource,CreateTime,PayType,PayCount,RealPay,FreePay,UserID,ScheduleID,WorkStation,AuthorID,OrderStatus,SettleFlag,Note,PayFee,PayTime,ModifyTime)
	VALUES (@OrderFlwID,@StoreID,@OrderID,@OrderFoodCount,@FoodDetailCount,@CardID,@OrderSource,GETDATE(),0,@PayCount,@PayCount,0,@UserID,@CurrentSchedule,@WorkStation,@AuthorID,@OrderStatus,0,@Note,0,@PayTime,@ModifyTime)
	
	--历便套餐明细,保存数据
	declare cur cursor for
	select a.FoodId,FoodCount,PayType,PayNum,b.FoodType as FoodType from @FoodDetail a inner join Data_FoodInfo b on a.FoodId = b.FoodID
	open cur
	fetch next from cur into @FoodId,@OrderDetailCount,@PayType,@PayNum,@FoodType
	WHILE @@FETCH_STATUS = 0
	  begin
	    set @OrderGetPoint = 0
	    set @FoodSaleTotalMoney = 0
	    
	    --如果是开卡套餐，写入押金和开卡费用
	    if @FoodId = @OpenCardFoodId
	      begin
	        set @TmpDeposit = @Deposit
	        set @TmpOpenFee = @OpenFee
	      end
	    else
	      begin
	        set @TmpDeposit = 0
	        set @TmpOpenFee = 0
	      end
	
		--获取套餐商品总数
		set @OrderDetailFoodGoodCount = ( select isnull(SUM(ContainCount),0) from Data_Food_Detial a where a.FoodID = @FoodId and a.Status = 1 ) * @OrderDetailCount
		--获取代币积分数
		exec GetFoodPoint @StoreID,@FoodId,@GetPoint output
		set @OrderGetPoint = @OrderGetPoint + @GetPoint			
	  				
		--添加商品销售主表和明细表
		select @FlwFoodSaleId = dbo.F_GetFlwId(@FlwSeedId,@FlwIndex)
		set @FlwIndex = @FlwIndex + 1
		insert into Flw_Food_Sale(ID,StoreID,FlowType,FoodID,SaleCount,Point,PointBalance,MemberLevelID,Deposit,OpenFee,RenewFee,ChangeFee,TotalMoney,Note,BuyFoodType)
		values(@FlwFoodSaleId,@StoreID,@FoodType,@FoodId,@OrderDetailCount,@GetPoint,0,@MemberLevelId,@TmpDeposit,@TmpOpenFee,0,0,@FoodSaleTotalMoney,@Note,@SaleCoinType)
			
		--添加商品销售信息
		declare cur1 cursor for
		select b.FoodType,b.ContainID,ContainCount,GETDATE(),0,b.Status from Data_FoodInfo a 
		inner join Data_Food_Detial b on a.FoodID = b.FoodID where a.FoodID = @FoodId and b.Status = 1
		open cur1
		fetch next from cur1 into @FoodDetailTypeId,@ContainID,@ContainCount,@ExpireDay,@ValidType,@Status
		WHILE @@FETCH_STATUS = 0
		  begin
		    select @FlwFoodSaleDetailId = dbo.F_GetFlwId(@FlwSeedId,@FlwIndex)
			set @FlwIndex = @FlwIndex + 1  
		    insert into Flw_Food_SaleDetail(ID,FlwFoodID,SaleType,ContainID,ContainCount,ExpireDay,ValidType,Status)
			values(@FlwFoodSaleDetailId,@FlwFoodSaleId,@FoodDetailTypeId,@ContainID,@ContainCount,@ExpireDay,@ValidType,@Status)    						
			fetch next from cur1 into @FoodDetailTypeId,@ContainID,@ContainCount,@ExpireDay,@ValidType,@Status
		  end
		close cur1  
		deallocate cur1
					
		--添加销售支付明细
		if @CustomerType = 2
		  begin
	         select @FlwFoodSalePayId = dbo.F_GetFlwId(@FlwSeedId,@FlwIndex)
		     set @FlwIndex = @FlwIndex + 1	
    	     insert Flw_Food_Sale_Pay(ID,FlwFoodID,BalanceIndex,PayCount,Balance)
		     values(@FlwFoodSalePayId,@FlwFoodSaleId,@PayType,@PayNum,@Balance)						
		  end
							
		--插入订单明细
		select @FlwOrderDetailId = dbo.F_GetFlwId(@FlwSeedId,@FlwIndex)
		set @FlwIndex = @FlwIndex + 1
		insert into Flw_Order_Detail(ID,OrderFlwID,FoodFlwID,GoodsCount)
		values(@FlwOrderDetailId,@OrderFlwID,@FlwFoodSaleId,@OrderDetailFoodGoodCount)
						
		fetch next from cur into @FoodId,@OrderDetailCount,@PayType,@PayNum,@FoodType
	  end
	close cur  
	deallocate cur
	
    return 1
GO
/****** Object:  StoredProcedure [dbo].[CheckStoreCanOpenCard]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[CheckStoreCanOpenCard](@StoreId varchar(15),@Mobile varchar(11),@ICCardId int,@ErrMsg varchar(200) output)
as
 --验证当前门店是否开通过会员卡(已注销的卡不计入)
 if dbo.CheckMobile(@Mobile) <> 1
   begin
     set @ErrMsg = '开通会员的手机号码无效'
	 return 0
   end
 
 if  exists ( select 0 from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID inner join Data_Member_Card_Store c on a.ID = c.CardID where c.StoreID = @StoreId and b.Mobile = @Mobile)
   begin
     set @ErrMsg = '用户在本门店已开通过会员卡,不能重复开通'
	 return 0
   end

 --验证组规则，（如果当前门店未完全设置到组规则中，用户未在当前门店开通过会员卡，则开通）
 declare @isAllGroupRule int = 0 
 exec @isAllGroupRule = CheckStoreIsInAllGroupRule @StoreId
 if @isAllGroupRule = 1 
   begin
     --如果是完全组规则,判断用户是否在其他完全组规则门店办理过会员卡（如果已办理则不能重复办理）
     declare @ResultOpenCard int 
	 exec @ResultOpenCard = IsExistOtherStoreOpenCardRecardInALLGroupRule @StoreId,@Mobile
	 if @ResultOpenCard = 1
	   begin
	     set @ErrMsg = '当前用户已在其他门店开通过会员卡'
		 return 0
	   end   
   end
 
 if @ICCardID <= 0 
   begin
	set @ErrMsg = '会员卡号无效'
    return 0
   end
  
 if exists (select 0 from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID)
   begin
   	 set @ErrMsg = '会员卡号已使用'
     return 0
   end
   
 return 1
GO
/****** Object:  StoredProcedure [dbo].[CheckExistOpenCardFood]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckExistOpenCardFood](@StoreId varchar(15),@CustomerType int,@MemberLevelId int,@FoodDetail [FoodDetailType] readonly)
as
  declare @openCardFoodCount int = 0
  declare @IdList as IdListType
  insert into @IdList(FoodID)
  exec GetMemberOpenCardFood @StoreId,@CustomerType,@MemberLevelId 
  select @openCardFoodCount = COUNT(0) from @IdList a inner join @FoodDetail b on a.FoodId = b.FoodId
  if @openCardFoodCount > 0
    return 1
  else
    return 0
GO
/****** Object:  StoredProcedure [dbo].[CheckExistEffectiveOpenCardFood]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckExistEffectiveOpenCardFood](@StoreId varchar(15),@CustomerType int,@MemberLevelId int,@FoodDetail [FoodDetailType] readonly,@OpenCardFoodId int output,@ErrMsg varchar(200) output)
as
  declare @openCardFoodCount int = 0
  declare @IdList as IdListType
  insert into @IdList(FoodID)
  exec GetMemberOpenCardFood @StoreId,@CustomerType,@MemberLevelId 
  select @openCardFoodCount = COUNT(0) from @IdList a inner join @FoodDetail b on a.FoodId = b.FoodId
  if @openCardFoodCount = 1
    begin
      select @OpenCardFoodId = a.FoodId from @IdList a inner join @FoodDetail b on a.FoodId = b.FoodId
	  return 1
    end  
  else if @openCardFoodCount = 0
    begin
      Set @ErrMsg = '没有选择开卡套餐'
      return 0
    end
  else if @openCardFoodCount > 1
    begin
      Set @ErrMsg = '开卡套餐只能选一种'
      return 0
    end
GO
/****** Object:  StoredProcedure [dbo].[AddMemberBanlance]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddMemberBanlance](
@StoreId varchar(15),
@MemberId int,
@BalanceIndex int,
@Balance decimal(18,2))
as
  declare @CardBalanceId int
  --获取同规则门店
  create table #TmpStore(StoreId varchar(15))
  insert #TmpStore(StoreId)
  exec [GetSameGroupRuleTypeStoreInfo] @StoreId,@BalanceIndex  
  --添加卡余额信息
  insert Data_Card_Balance(MemberID,BalanceIndex,Balance,UpdateTime)
  values(@MemberId,@BalanceIndex,@Balance,GETDATE()) 
  select @CardBalanceId = @@IDENTITY from Data_Card_Balance  
  --添加门店会员卡列表
  insert Data_Card_Balance_StoreList(CardBalanceID,StoreID)
  select @CardBalanceId,StoreId from #TmpStore
GO
/****** Object:  StoredProcedure [dbo].[CheckCouponInvalid]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckCouponInvalid](@StoreId varchar(15),@FoodPrice decimal(18,2),@CouponList [CouponListType] READONLY,@ErrMsg varchar(8000) output)
as
  declare @Str varchar(8000) = ''
  declare @CouponCode1 varchar(32) = ''
  declare @CouponCode2 varchar(32) = ''
  declare @State int = 0--0 未分配(创建初始状态);1 未激活(调拨到门店);2(已激活    门店派发);3(已使用    用户核销)
  declare @IsLock int = 0--0正常;1 锁定
  declare @AllowOverOther int = 0;--允许与其他规则叠加
  declare @OverUseCount int = 0;--叠加使用张数
  declare @CouponID int = 0
  declare @CouponType int = 0
  declare @CouponValue decimal(18,2) = 0
  declare @CouponDiscount decimal(18,2) = 0
  declare @CouponThreshold decimal(18,2) = 0
  declare @OverMoney decimal(18,2) = 0
  declare @CouponDetailCount int = 0
  declare @ChargeType int = 0
  declare @ChargeCount int = 0
  declare @GoodID int = 0
  declare @ProjectID int = 0
  
  --获取@CouponListType优惠券表对应的优惠券信息（券代码和状态） 
  select b.CouponID,b.CouponType,a.CouponCode as CouponCode1,b.CouponCode as CouponCode2,AllowOverOther,OverUseCount,b.State,IsLock into #TmpCouponDetail 
  from @CouponList a 
  left join 
  (
    select c.CouponID,c.CouponCode,b.CouponType,b.AllowOverOther,b.OverUseCount,c.State,c.IsLock
    from Data_Coupon_StoreList a inner join Data_CouponInfo b on a.CouponID = b.ID 
    inner join Data_CouponList c on b.ID = c.CouponID
    inner join @CouponList d on c.CouponCode = d.CouponCode
    where a.StoreID = @StoreId and DATEDIFF(DY,StartDate,GETDATE()) >= 0 and DATEDIFF(DY,EndDate,GETDATE()) <= 0 and cast(GETDATE() as time) >= StartTime and CAST(GETDATE() as time) <= EndTime
  ) b
  on a.CouponCode = b.CouponCode
  
  --验证优惠券是否存在  
  if (select COUNT(0) from #TmpCouponDetail where CouponCode2 is null ) > 0
    begin
      declare b_cur1 cursor for select CouponCode1 from #TmpCoupon where CouponCode2 is null
	  open b_cur1
	  fetch next from b_cur1 into @CouponCode1
	  while @@FETCH_STATUS =0 
		begin
			begin
			  if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'
			  set @Str = @Str + @CouponCode1
			end
		  fetch next from b_cur into @CouponCode1
		end
	  close b_cur1
	  deallocate b_cur1
	  
	  set @ErrMsg = '下列优惠券无效：' + @Str
	  return 0
    end
     
  --验证是否有不存在的券
  set @Str = ''
  if (select COUNT(0) from #TmpCouponDetail where CouponCode2 is not null and ( State <> 2 or IsLock = 1) ) > 0
    begin
	  declare b_cur2 cursor for select CouponId,CouponCode1,CouponCode2,State,IsLock from #TmpCouponDetail where CouponCode2 is not null and ( State <> 2 or IsLock = 1)
	  open b_cur2
	  fetch next from b_cur2 into @CouponId,@CouponCode1,@CouponCode2,@State,@IsLock
	  while @@FETCH_STATUS =0 
		begin
		  if @State = 0 
			begin
			  if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'			  
			  set @Str = @Str + @CouponCode1 + '(未分配)'
			end
		  else if @State = 1 
			begin
		      if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'
			  set @Str = @Str + @CouponCode1 + '(未激活)'
			end
		  else if @State = 3 
			begin
			  if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'
			  set @Str = @Str + @CouponCode1 + '(已使用)'
			end
		  else if @State <> 2
			begin
			  if @Str = '' set @Str = @Str + '\n' else set @Str = @Str + ',\n'
			  set @Str = @Str + @CouponCode1 + '(无效状态)'
			end
		  fetch next from b_cur2 into @CouponId,@CouponCode1,@CouponCode2,@State,@IsLock
		end
	  close b_cur2
	  deallocate b_cur2
	  
	  set @ErrMsg = '下列优惠券无效：' + @Str
      return 0  		
    end

	create table #TmpCoupon(ID int IDENTITY(1,1),CouponID int,CouponType int,CouponValue decimal(18,2),CouponDiscount decimal(18,2),CouponThreshold decimal(18,2)
	,OverMoney decimal(18,2),CouponDetailCount int,AllowOverOther int,OverUseCount int,ChargeType int,ChargeCount int,GoodID int,ProjectID int)
	insert #TmpCoupon(CouponID,CouponType,CouponValue,CouponDiscount,CouponThreshold,OverMoney,CouponDetailCount,AllowOverOther,OverUseCount,ChargeType,ChargeCount,GoodID,ProjectID)
	select CouponID,CouponType,CouponValue,CouponDiscount,CouponThreshold,OverMoney,CouponDetailCount,AllowOverOther,OverUseCount,ChargeType,ChargeCount,GoodID,ProjectID from 
	( select CouponID,COUNT(0) as CouponDetailCount from #TmpCouponDetail group by CouponID ) a
	inner join Data_CouponInfo b on a.CouponID = b.ID
	
	--检查券是否可以同时使用
	declare @TmpCouponCount int = 0
	declare @TmpCouponIndex int = 1
	select @TmpCouponCount = COUNT(0) from #TmpCoupon
	while @TmpCouponIndex <= @TmpCouponCount
	  begin
		select @CouponID = CouponID,@AllowOverOther = AllowOverOther,@OverUseCount = OverUseCount,@CouponDetailCount = CouponDetailCount,
		@OverMoney = OverMoney from #TmpCoupon where ID = @TmpCouponIndex
		--验证券是否可以同时使用
		if @CouponDetailCount > @OverUseCount
		  begin
			declare @TmpCouponList1 [CouponListType]
			insert into @TmpCouponList1(CouponCode) select CouponCode1 from #TmpCouponDetail where CouponID = @CouponID
			set @Str = dbo.GetCouponCodeListStr(@TmpCouponList1)
			set @ErrMsg = '下列优惠券不能同时使用：' + @Str 
			return 0			
		  end
		--验证购物金额
		if @FoodPrice > @OverMoney
		  begin
			declare @TmpCouponList2 [CouponListType]
			insert into @TmpCouponList2(CouponCode) select CouponCode1 from #TmpCouponDetail where CouponID = @CouponID
			set @Str = dbo.GetCouponCodeListStr(@TmpCouponList2)
			set @ErrMsg = '购物金额不足下列优惠券不能使用：' + @Str
			return 0 
		  end
		--验证是否与其他券同时使用
		if @AllowOverOther = 0 and @OverUseCount > 1
		  begin
			declare @TmpCouponList3 [CouponListType]
			insert into @TmpCouponList3(CouponCode) select CouponCode1 from #TmpCouponDetail where CouponID = @CouponID
			set @Str = dbo.GetCouponCodeListStr(@TmpCouponList3)
			set @ErrMsg = '下列优惠券不能同其他优惠券使用：' + @Str
			return 0 
		  end  
		  
		set @TmpCouponIndex = @TmpCouponIndex + 1 	  
	  end
	
    return 1
GO
/****** Object:  StoredProcedure [dbo].[CheckCoupon]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckCoupon](
@CouponList [CouponListType] readonly,
@StoreId varchar(15),
@FoodPrice decimal(18,2),
@AuthorFlag int output,--需要授权
@FreePrice decimal(18,2) output,
@ErrMsg varchar(200) output)
as
declare @CouponId int
declare @CouponType int--0 代金券;1 折扣券;2 兑换券
declare @EntryCouponFlag int --实物券标记
declare @OverUseCount int--叠加使用张数
declare @CouponValue decimal(18,2)--优惠券价值
declare @CouponDiscount decimal(18,2)--优惠券折扣
declare @CouponThreshold decimal(18,2)--优惠阈值
declare @OverMoney decimal(18,2)--消费满减金额
declare @SendType int--派发方式
declare @FreeCouponCount int--送券数
declare @JackpotCount int--抽奖规则编号
declare @ChargeType int--兑换方式
declare @ChargeCount int--兑换数量
declare @GoodId int--礼品编号
declare @ProjectId int--游乐项目编号
declare @CanUseCouponCount int = 0 --可使用优惠券张数
declare @CouponCount int = 0 --优惠券数量(输入参数) 

--验证券代码是否有效(状态，是否存在，是否多个券类型混合使用)
declare @RSCheckCouponInvalid int = 0
exec @RSCheckCouponInvalid = CheckCouponInvalid @StoreId,@CouponList,@CouponId output,@CouponCount output,@ErrMsg output

if @RSCheckCouponInvalid = 0
  return 0

--获取券的变量
select @CouponType = CouponType,@AuthorFlag = AuthorFlag,@CouponValue = CouponValue,@CouponDiscount = CouponDiscount,
@SendType = SendType,@OverMoney = OverMoney,@FreeCouponCount = FreeCouponCount,@CouponThreshold = CouponThreshold
from Data_CouponInfo where ID = @CouponId

if @CouponType = 0 
  begin
    /*代金券验证
    （1）满减金额:购买商品金额必须大于等于“满减金额”才能使用
    （2）阀值验证（购买金额必须满足阀值才可以使用券）
    （3）张数验证:最大可以使用的券数（在满足购买金额/阀值计算可使用券数的基础上，在进行最大可使用券数的限制）*/
    --套餐金额除以券的面值(舍弃小数),获得可使用张数
    set @CanUseCouponCount = cast(@FoodPrice / @CouponValue as int) 
    --如果可使用张数大于阀值，可使用张数等于阀值
    if @CanUseCouponCount > @CouponThreshold
      begin
        set @CanUseCouponCount = @CouponThreshold
      end
    if @CouponCount > @CanUseCouponCount
      begin
        set @ErrMsg = '使用的优惠券张数超过限制（最多使用' + CAST(@CanUseCouponCount as varchar) + '张）'
        return 0
      end
     set @FreePrice = @CouponValue * @CouponCount
  end
else if @CouponType = 1
  begin
	exec GetDiscountPrice @FoodPrice,@CouponDiscount,@CouponCount,@FreePrice output
  end
else if @CouponType = 2
  begin
    print '222222222222222222'
  end 
return 1
GO
/****** Object:  StoredProcedure [dbo].[CreateOrder]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[CreateOrder] 
(
@FoodDetail [FoodDetailType] readonly,
@StoreID varchar(15),
@CustomerType int,--0:散客,1:新会员注册,2:会员
@Mobile varchar(11),--手机号
@MemberLevelId int,--会员等级ID
@ICCardID int,
@PayCount decimal(18,2),
@Deposit decimal(18,2),
@OpenFee decimal(18,2),
@UserID int,
@WorkStation varchar(50),
@AuthorID int,
@Note VARCHAR(500),
@OrderSource int,
@SaleCoinType int,--(0-手工实物币提币;1-售币机实物币提币)
@FlwSeedId varchar(29),
@OrderFlwID varchar(32) output,
@ErrMsg varchar(200) output
) 
AS
BEGIN
	--0代币;1 餐饮;2 礼品品;3 门票;4 扩展赠送跟会员级别绑定
	declare @MemberBalanceList [MemberBalanceListType]
	declare @OpenCardFoodId int = 0
	declare @ForeAuthorize int = 0
	declare @AllowInternet int = 0
	declare @AllowPrint int = 0
    declare @MerchID int = 0
	declare @MemberId varchar(32) = ''
	declare @OrderStatus int = 2--0 未结算,1 等待支付,2 已支付,3 支付异常(订单支付状态默认为2，如果存在第三方支付，订单支付状态设置为1)
	declare @PayFee decimal(18,2)--第三方支付手续费
	declare @CardID int = 0 --会员卡表ID
	declare @PayTime datetime = null
	declare @ModifyTime datetime = null
	declare @OpenTime varchar(20) = ''
	
	select @MerchID = MerchID from Base_StoreInfo where StoreID = @StoreID and StoreState = 1
	if @MerchID = 0 
	  begin
    	set @ErrMsg = '门店ID无效'
		return 0	
	  end
	  
	--验证套餐
	declare @CheckEffectiveFood int = 0
	exec @CheckEffectiveFood = CheckEffectiveFood @StoreID,@FoodDetail,@ErrMsg output
	if @CheckEffectiveFood = 0 
	  return 0 
 
	--验证会员级别
	if @CustomerType = 1 or @CustomerType = 2
	  begin
	    declare @RSCheckMemberLevel int = 0
	    exec @RSCheckMemberLevel = dbo.CheckMemberLevel @StoreId,@MemberLevelId,@ErrMsg output
	    if @RSCheckMemberLevel = 0
	      begin
	        return 0
	      end
	  end  
	
	--验证会员
	if @CustomerType = 0
	  begin
	    set @CardID = 0
	    set @MemberLevelId = 0
	    set @Deposit = 0
	    set @OpenFee = 0
	    --验证是否存在开卡套餐  
		declare @CheckExistOpenCardFood int = 0
		exec @CheckExistOpenCardFood = CheckExistOpenCardFood @StoreId,@CustomerType,@MemberLevelId,@FoodDetail
	    if @CheckExistOpenCardFood = 1
	      begin
	        set @ErrMsg = '散客购买不能包含开卡套餐'
	        return 0
	      end
	  end  
	else if @CustomerType = 1  
	  begin
	    set @CardID = 0
		--检查当前手机号码门店是否可以开卡  
		declare @CheckStoreCanOpenCard int 
		exec @CheckStoreCanOpenCard = CheckStoreCanOpenCard @StoreId,@Mobile,@ICCardID,@ErrMsg output
		if @CheckStoreCanOpenCard = 0
		  begin
		    return 0
		  end  
		--验证是否存在开卡套餐
		declare @CheckExistEffectiveOpenCardFood int
		exec @CheckExistEffectiveOpenCardFood = CheckExistEffectiveOpenCardFood @StoreId,@CustomerType,@MemberLevelId,@FoodDetail,@OpenCardFoodId output,@ErrMsg output  
		if @CheckExistEffectiveOpenCardFood = 0
		  return 0 
		--获取开卡费和押金
		declare @TmpOpenFee decimal(18,2)
		declare @TmpDeposit decimal(18,2)
	    select @TmpOpenFee = OpenFee,@TmpDeposit = Deposit from Data_MemberLevel where MemberLevelID = @MemberLevelId
	    if @TmpOpenFee <> @OpenFee
	      begin
	        set @ErrMsg = '开卡费金额不正确'
	        return 0
	      end
	    if @TmpDeposit <> @Deposit
	      begin
	        set @ErrMsg = '押金金额不正确'
	        return 0
	      end   	  	       	           	   	  	       	           	
	  end
    else if @CustomerType = 2
      begin
      	set @Deposit = 0
	    set @OpenFee = 0      		   
		--获取会员基础信息,设置相关变量
		declare @GetMemberInfoReturn int = 0
		CREATE TABLE dbo.#Tmp_MemberBaseInfo(CardId int,ICCardID varchar(20) NULL,MemberLevelID int NULL,MemberName varchar(50) NULL,Gender int NULL,Birthday varchar(100) NULL,
		IDCard varchar(18) NULL,Mobile varchar(11) NULL,Deposit numeric(18, 2) NULL,Note text NULL,EndDate varchar(100) NULL,RepeatCode int NULL,MemberState int NULL,CardStatus int NULL,
		MemberLevelName varchar(20) NULL,StoreID varchar(15) NOT NULL,StoreName varchar(50) NULL)
		insert into #Tmp_MemberBaseInfo
		exec [GetMemberBaseInfo] @ICCardID,@StoreID,@GetMemberInfoReturn output,@MemberId output,@CardId output,@ErrMsg output
	    if @GetMemberInfoReturn = 0 
		  begin
			return 0
		  end
		--获取会员等级
		select @MemberLevelId = MemberLevelID from #Tmp_MemberBaseInfo
		--验证是否存在开卡套餐  
		declare @CheckExistOpenCardFood2 int = 0
		exec @CheckExistOpenCardFood2 = CheckExistOpenCardFood @StoreId,@CustomerType,@MemberLevelId,@FoodDetail
	    if @CheckExistOpenCardFood2 = 1
	      begin
	        set @ErrMsg = '会员购买不能包含开卡套餐'
	        return 0
	      end 
		--获取会员余额信息
		insert into @MemberBalanceList(BalanceIndex,BalanceName,Balance)
		exec GetMemberBalanceInfo @MemberId		
      end  
    
	--验证班次信息
	declare @CurrentSchedule int = 0
	declare @GetScheduleReturn int = 0
	exec @GetScheduleReturn = GetSchedule @StoreID,@UserID,@WorkStation,@CurrentSchedule output,@OpenTime output,@errMsg output
	if @GetScheduleReturn = 0
	  begin
		return 0
	  end
	
	--设置Flw_Coin_Sale表workType
	if @SaleCoinType <> 1 and @SaleCoinType <> 2
	  begin
	    set @ErrMsg = '售币类型不正确'
		return 0
	  end
     	   	 	  	
	--验证套餐是否可以支付
	declare @CheckOrderPayResult int = 0
	declare @IsThirdPay int = 0
	create table #PayTypeSum(PayType int,PaySum decimal(18,2))
	exec CheckOrderPay @StoreId,@CustomerType,@MemberLevelId,@MemberBalanceList,@FoodDetail,@PayCount,@OpenFee,@Deposit,@CheckOrderPayResult output,@ErrMsg output
	if @CheckOrderPayResult = 0
	  begin
		return 0
	  end
    --添加订单
    declare @AddOrderReturn int
	exec @AddOrderReturn = AddOrder @StoreID,@CustomerType,@MemberLevelId,@CardID,@SaleCoinType,@OrderSource,@PayCount,@OpenCardFoodId,@Deposit,@OpenFee,
	@UserID,@CurrentSchedule,@WorkStation,@AuthorID,@Note,@PayTime,@ModifyTime,@FoodDetail,@FlwSeedId,@OrderFlwID output,@ErrMsg output
	if @AddOrderReturn = 0
	  begin
		return 0
	  end

	return 1     
END
GO
/****** Object:  StoredProcedure [dbo].[GetDiscountForAPI]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetDiscountForAPI](@MerchId varchar(15),@StoreId varchar(15),@CustomerType int,@ICCardId int,@FoodPrice decimal(18,2),@DiscountRuleID int output,@SubPrice decimal(18,2) output,@DiscountRuleName varchar(200) output,@ErrMsg varchar(200) output)
as 
  --接口给API调用，验证卡ID信息
  declare @MemberId varchar(32) = ''
  declare @GetMemberIdRS int = 0
  declare @CanUse bit = 0
  declare @CardStatusName varchar(200)
  declare @CheckCustomerTypeParams bit
  
  select @CheckCustomerTypeParams = dbo.F_CheckCustomerTypeParams(@CustomerType,@ICCardId)
  
  if @CheckCustomerTypeParams = 0
    begin
      set @ErrMsg = '客户类型不正确'
      return 0
    end
  
  if @CustomerType = 2
    begin
	  exec @GetMemberIdRS = GetMemberId @StoreId,@ICCardId,@MemberId output,@CanUse output,@CardStatusName output,@ErrMsg output
	  if @GetMemberIdRS = 0 or @CanUse = 0
		begin
		  return 0
		end		
    end

  declare @GetDiscount int = 0      
  exec @GetDiscount = GetDiscount @MerchId,@StoreId,@CustomerType,@MemberId,@FoodPrice,@DiscountRuleID output,@SubPrice output,@DiscountRuleName output,@ErrMsg output
  return @GetDiscount
GO
/****** Object:  StoredProcedure [dbo].[GetCommonFoodListInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodListInfo](@FoodDetailLevelQuery [FoodDetailLevelQueryType] readonly,@CustomerType int,@MemberLevelId int)
as
	select a.FoodId,a.FoodName,a.FoodType,AllowInternet,AllowPrint,ForeAuthorize,isnull(b.FoodPrice,a.FoodPrice) as FoodPrice,a.ImageURL
	from 
	(
		select a.FoodId,b.FoodName,b.FoodType,AllowPrint,AllowInternet,ForeAuthorize,
		isnull(case @CustomerType when 0 then b.ClientPrice else b.MemberPrice end,0) AS FoodPrice,b.ImageURL 
		from @FoodDetailLevelQuery a inner join Data_FoodInfo b on a.FoodId = b.FoodID
	) a
	left join 
	(
		select a.FoodId,isnull(case @CustomerType when 0 then ClientPrice else VIPPrice end,0) AS FoodPrice 
		from 
		(
			select distinct a.FoodId,b.ID as FoodLevelId
			from @FoodDetailLevelQuery a inner join Data_Food_Level b on a.FoodId = b.FoodID
			left join Data_Food_Record c on b.ID = c.FoodLevelID 
			where DATEDIFF(DY,StartDate,GETDATE()) >= 0 and DATEDIFF(dy,EndDate,GETDATE()) <= 0
			and DATEDIFF(DY,StartTime,cast(GETDATE() as time)) >= 0 and DATEDIFF(dy,EndTime,cast(GETDATE() as time)) <= 0
			and dbo.CheckWeekStr(b.Week) = 1 and b.MemberLevelID = @MemberLevelId
			and isnull((case @CustomerType when 0 then AllCount else MemberCount end),0) - dbo.F_GetUsedFoodLevelCount(@CustomerType,b.ID) > 0
			group by a.FoodId,b.ID
		) a
		inner join Data_Food_Level b on a.FoodLevelId = b.ID
	) b
	on a.FoodId = b.FoodId
GO
/****** Object:  StoredProcedure [dbo].[AddMemberCard]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddMemberCard](@RegisterMember [dbo].[RegisterMemberType] READONLY,@StoreId varchar(15),@MemberLevelId int,@WorkStation varchar(50),@UserId int,@Points int)
as
  declare @MemberId int
  declare @ValidDay int
  declare @Mobile varchar(20)
  declare @WeChat varchar(64)
  declare @QQ varchar(64)
  declare @IMME varchar(64)
  declare @CardShape int
  declare @MemberName varchar(50)
  declare @MemberPassword varchar(20)
  declare @Birthday varchar(16)
  declare @Gender varchar(1)
  declare @IdentityCard varchar(50)
  declare @EMail varchar(50)
  declare @LeftHandCode varchar(5000)
  declare @RightHandCode varchar(5000)
  declare @Photo varchar(100)
  declare @RepeatCode int
  declare @ICCardId varchar(20)
  declare @Deposit int
  declare @ICCardUID bigint
  declare @CardID int = 0
  declare @CardBalanceID int = 0
  declare @Note varchar(200)

  select @Mobile=Mobile,@WeChat=WeChat,@QQ=QQ,@IMME=IMME,
         @CardShape=CardShape,@MemberName=MemberName,@MemberPassword=MemberPassword,@Birthday=Birthday,@Gender=Gender,
         @IdentityCard=IdentityCard,@EMail=EMail,@LeftHandCode=LeftHandCode,@RightHandCode=RightHandCode,@Photo=Photo,
         @RepeatCode=RepeatCode,@ICCardId=ICCardId,@Deposit=Deposit,
         @ICCardUID=ICCardUID,@Note=Note
         from @RegisterMember
    --添加会员基本信息
  if not exists ( select 0 from Base_MemberInfo where Mobile = @Mobile )
    begin
      --如果会员基本信息不存在，添加会员基本信息
	  insert into Base_MemberInfo(Wechat,QQ,IMME,UserName,UserPassword,Gender,IDCard,Mobile,EMail,Birthday,LeftHandCode,RightHandCode,MemberState,Photo,Note,CreateTime)
	  values(@WeChat,@QQ,@IMME,@MemberName,@MemberPassword,@Gender,@IdentityCard,@Mobile,@EMail,@Birthday,@LeftHandCode,@RightHandCode,1,@Photo,@Note,GETDATE())
	  select @MemberId = @@IDENTITY from Base_MemberInfo		
    end
  else
    select @MemberId = id from Base_MemberInfo where Mobile = @Mobile
   
  --添加会员ICCard信息
  select @ValidDay = Validday from Data_MemberLevel where MemberLevelID = @MemberLevelId
  insert into Data_Member_Card(ICCardID,ParentCard,CardType,CardShape,CardLimit,MemberID,MemberLevelID,CreateTime,EndDate,LastStore,UpdateTime,Deposit,RepeatCode,UID,CardStatus)
  values(@ICCardId,'',0,@CardShape,0,@MemberId,@MemberLevelId,GETDATE(),Convert(char(10),DATEADD(dy,@ValidDay,getdate()),120),@StoreId,GETDATE(),@Deposit,@RepeatCode,@ICCardUID,1)
  select @CardID = @@IDENTITY from Data_Member_Card
  --添加会员门店信息
  insert into Data_Member_Card_Store(CardID,StoreID)
  values(@CardID,@StoreId)
  --添加会员积分余额
  if @Points > 0
    begin
	  exec AddMemberBanlance @StoreId,@MemberId,0,@Points
    end
GO
/****** Object:  StoredProcedure [dbo].[UpdateOrder]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[UpdateOrder] 
(
@FoodDetail [FoodDetailType] readonly,
@StoreID varchar(15),
@CustomerType int,--0:散客,1:新会员注册,2:会员
@Mobile varchar(11),--手机号
@MemberLevelId int,--会员等级ID
@ICCardID int,
@PayCount decimal(18,2),
@RealPay decimal(18,2),
@FreePay decimal(18,2),
@UserID int,
@WorkStation varchar(50),
@AuthorID int,
@Note VARCHAR(500),
@OrderSource int,
@SaleCoinType int,--(0-手工实物币提币;1-售币机实物币提币)
@OrderFlwID int,
@ErrMsg varchar(200) output
) 
AS
BEGIN
--0售币;1入会;2数字币;3商品;4门票;5餐饮;6混合;
--0代币;1 餐饮;2 礼品品;3 门票;4 扩展赠送跟会员级别绑定
    declare @FoodFlwID int = 0
    declare @OrderGoodCount int = 0
    declare @OrderFoodCount int = 0
    declare @OrderDetailCount int = 0
    declare @OrderID varchar(20)
    declare @FoodId int = 0
    declare @PayType int = 0
    declare @PayNum decimal(18,6) = 0
    declare @FoodNum int = 0
	declare @GetPoint int = 0--获取币销售获得的积分数
	declare @OrderGetPoint int = 0
	declare @TmpFoodSalePrice decimal(18,6) = 0--累计购物车明细中的价格
	declare @TmpFoodPoint int = 0--累计购物车明细中的积分数
	declare @TmpFoodLottery int = 0--累计购物车明细中的彩票数
	declare @TmpFoodCoin int = 0--累计购物车明细中的代币数
	declare @OpenCardFoodTypeNum int = 0--开卡套餐类型数量
	declare @Deposit decimal(18,2) = 0
	declare @Emp_FoodDetailId varchar(50)
	
	--套餐详情用的变量
	declare @FoodName varchar(200) = ''
	declare @FoodType int = 0
	declare @AllowCoin int = 0
	declare @Coins int = 0
	declare @AllowPoint int = 0
	declare @Points int = 0
	declare @AllowLottery int = 0
	declare @Lotterys int = 0
	declare @ForeAuthorize int = 0
	declare @AllowInternet int = 0
	declare @AllowPrint int = 0

	declare @TmpSumCoins int = 0--代币总额
	declare @TmpSumPoints int = 0--积分总额
	declare @TmpSumLotterys int = 0--彩票总额
	declare @TmpSumPrice decimal(18,6) = 0
	
	declare @MemberPoints int = 0
	declare @MemberLotterys int  = 0
	declare @MemberCoins int = 0
	declare @MemberPrice decimal(18,2) = 0
	declare @FoodSaleTotalMoney decimal(18,2) = 0
	declare @UseCoin int = 0
	declare @UsePoint int = 0
	declare @UseLottery int = 0
	declare @OrderStatus int = 2--0 未结算,1 等待支付,2 已支付,3 支付异常(订单支付状态默认为2，如果存在第三方支付，订单支付状态设置为1)
	declare @PayFee decimal(18,2)--第三方支付手续费
	declare @PayTime datetime = null
	declare @ModifyTime datetime = null
	declare @OrderDetailFoodGoodCount int

	declare @DetailFoodType int = 0
	declare @DetailContainID int = 0
	declare @DetailContainCount int = 0
	declare @IsBirthday int = 0
	declare @MemberBirthday varchar(10)= ''
	declare @GoodsMoney decimal(18,6) = 0 
	declare @CardID int = 0 --会员卡表ID
	declare @FoodSalePrice decimal(18,2) = 0	
	declare @GetFoodInfoReturn int = 0
	declare @OpenTime varchar(20) = ''
	
	--验证订单
	if not exists( select 0 from Flw_Order where StoreID = @StoreId and ID = @OrderFlwId )
	  begin
	    set @ErrMsg = '订单无效'
		return 0
	  end
	
	select @OrderStatus = OrderStatus from Flw_Order where StoreID = @StoreId and ID = @OrderFlwId
	if @OrderStatus <> 1
      begin
        set @ErrMsg = '订单状态不正确'
	    return 0	
      end
	
	--验证会员级别
	if @CustomerType = 1  
	  begin
	    if @MemberLevelId <= 0 
	      begin
			set @ErrMsg = '注册会员等级不正确'
			return 0			
	      end
	    
        if not exists ( select 0 from Data_MemberLevel where MemberLevelID = @MemberLevelId and StoreID = @StoreID and State = 1 )
		  begin
		    set @ErrMsg = '会员级别不存在'
		    return 0
		  end
		
		if @ICCardID <= 0 
		  begin
			set @ErrMsg = '会员卡号无效'
		    return 0
		  end
		  
		if exists (select 0 from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID)
		  begin
		   	set @ErrMsg = '会员卡号已开通'
		    return 0
		  end

		--检查当前手机号码门店是否可以开卡  
		declare @CheckStoreCanOpenCard int 
		exec @CheckStoreCanOpenCard = CheckStoreCanOpenCard @StoreId,@Mobile,@ErrMsg output
		if @CheckStoreCanOpenCard = 0
		  begin
		    return 0
		  end	   	           	
	  end
  
    --会员或散客
    if @CustomerType = 0  
      begin
		set @CardID = 0
      end
    else if @CustomerType = 1
      begin
		set @CardID = 0
      end
    else
      begin
		--获取会员基础信息,设置相关变量
		declare @GetMemberSumInfoReturn int = 0
		exec @GetMemberSumInfoReturn = GetMemberSumInfo @StoreID,@ICCardID,@MemberLevelId,@CardID output,@IsBirthday output,@MemberPoints output,@MemberLotterys output,@MemberCoins output,@ErrMsg output
		if @GetMemberSumInfoReturn = 0 
		  begin
			return 0
		  end
      end  
      
	--验证班次信息
	declare @CurrentSchedule int = 0
	declare @GetScheduleReturn int = 0
	exec @GetScheduleReturn = GetSchedule @StoreID,@UserID,@WorkStation,@CurrentSchedule output,@OpenTime output,@errMsg output
	if @GetScheduleReturn = 0
	  begin
		return 0
	  end
	
	--设置Flw_Coin_Sale表workType
	if @SaleCoinType <> 1 and @SaleCoinType <> 2
	  begin
	    set @ErrMsg = '售币类型不正确'
		return 0
	  end
	    
	--表值变量数据写入临时表
	create table #Emp_FoodDetail(Emp_FoodDetailId varchar(50),FoodId int,FoodCount int,PayType int,PayNum decimal(18,2),Deposit decimal(18,2),FoodType int)
    insert #Emp_FoodDetail(Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType)
    select NEWID(),FoodId,FoodCount,PayType,PayNum,0 as Deposit,null as FoodType from @FoodDetail
    
    --请求支付类型转为订单支付类型0-金额,1-代币,2-彩票,3-积分 ------>   0 现金,1 微信,2 支付宝,3 银联,4 储值金,5 代币,6 彩票,7 积分（如果是第三方支付 -1）
    update #Emp_FoodDetail set PayType = case PayType when 1 then 5 when 2 then 6 when 3 then 7 else 0 end   
        	   	 	  	
	--验证套餐是否可以支付
	declare @checkOrderPayReturn int = 0
	declare @IsThirdPay int = 0
	declare @CheckFoodDetail [CheckFoodDetailType]	
	insert @CheckFoodDetail(Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType)
	select a.Emp_FoodDetailId,a.FoodId,a.FoodCount,a.PayType,a.PayNum,a.Deposit,b.FoodType as FoodType from #Emp_FoodDetail a inner join Data_FoodInfo b on a.FoodId = b.FoodID
	exec @checkOrderPayReturn = CheckOrderPay @StoreId,@FoodId,@CustomerType,@MemberLevelId,@MemberCoins,@MemberPoints,@MemberLotterys,@PayCount,@RealPay,@FreePay,@CheckFoodDetail,@IsThirdPay output,@ErrMsg output
	if @checkOrderPayReturn = 0
	  begin
		return 0
	  end

    --添加订单
    declare @AddOrderReturn int
	exec @AddOrderReturn = AddOrder @StoreID,@CardID,@MemberLevelId,@SaleCoinType,@OrderSource,@FoodId,@PayCount,@RealPay,@FreePay,@UserID,@CurrentSchedule,
	@WorkStation,@AuthorID,@OrderStatus,@Note,@PayFee,@PayTime,@ModifyTime,@MemberCoins,@MemberPoints,@MemberLotterys,@IsThirdPay,@CheckFoodDetail,@OrderFlwId output,@ErrMsg output
	if @AddOrderReturn = 0
	  begin
		return 0
	  end
  
	return 1     
END
GO
/****** Object:  StoredProcedure [dbo].[GetFoodListInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodListInfo](@StoreId varchar(20),@CustomerType int,@MemberLevelId int,@FoodTypeStr nvarchar(100))
as
 declare @Coins int = 0
 declare @QuerySingleType bit = 0 
 --获取会员开卡套餐
 declare @CurrentDateTime datetime = getdate()
 declare @CurrentTime time = getdate()
 
 create table #TempFoodTypeDic(Id int)
 insert #TempFoodTypeDic(Id)
 exec dbo.GetSplitTable @FoodTypeStr

 create table #TempQueryResultMain(FoodID int,FoodName varchar(200),FoodType int,AllowInternet int,AllowPrint int,ForeAuthorize int,FoodPrice decimal(18,2),ImageUrl varchar(200))
 create table #TempQueryResultGood(FoodId int,FoodName varchar(200),FoodType int,Price decimal(18,2),ImageUrl varchar(200))
 create table #TempQueryResultTicket(FoodId int,FoodName varchar(200),FoodType int,Price decimal(18,2))
 create table #TempQueryResultDetail(FoodId int,BalanceType int,TypeName varchar(200),UseCount int)

 if exists ( select 0 from #TempFoodTypeDic where Id = -1 )
   begin
     set @QuerySingleType = 1
     delete from #TempFoodTypeDic where Id = -1  
   end
 
 if exists ( select 0 from #TempFoodTypeDic )
   begin
	 --获取套餐主表ID
	 select FoodID,FoodType into #TempFoodType from 
	 (   
	   select a.FoodID,a.FoodType
	   from Data_FoodInfo a inner join Data_Food_StoreList b on
	   a.FoodID = b.FoodID and b.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
	   where a.FoodType in (select ID from #TempFoodTypeDic)
	   group by a.FoodID,a.FoodType   
	 ) a
	 
	 declare @FoodDetailLevelQuery as FoodDetailLevelQueryType
	 insert into @FoodDetailLevelQuery(FoodID)
     select FoodID from #TempFoodType
	 
	 insert into #TempQueryResultMain(FoodID,FoodName,FoodType,AllowInternet,AllowPrint,ForeAuthorize,FoodPrice,ImageUrl)
	 exec GetCommonFoodListInfo @FoodDetailLevelQuery,@CustomerType,@MemberLevelId
 
     insert into #TempQueryResultDetail(FoodId,BalanceType,TypeName,UseCount)
     exec GetCommonFoodBalanceListInfo @FoodDetailLevelQuery      
   end

   if @QuerySingleType = 1
     begin
       insert #TempQueryResultGood(FoodId,FoodName,FoodType,Price,ImageUrl)
       select ID as FoodId,GoodName as FoodName,GoodType as FoodType,Price,GoodPhoteUrl as ImageUrl from Base_GoodsInfo where StoreID = @StoreId and Status = 1
 
	   insert #TempQueryResultTicket(FoodId,FoodName,FoodType,Price)
       select ID as FoodId,TicketName as FoodName,TicketType as FoodType,Price from Data_ProjectTicket 
       where StoreID = @StoreId
       and DATEDIFF(dy,VaildStartDate,GETDATE()) >= 0 and DATEDIFF(dy,VaildEndDate,GETDATE()) <= 0
       and cast(GETDATE() as time) >= StartTime and cast(GETDATE() as time) <= EndTime
       and not (DATEDIFF(dy,NoStartDate,GETDATE()) >= 0 and DATEDIFF(dy,NoEndDate,GETDATE()) <= 0)
       and ( (weektype = 0 and dbo.CheckWeekStr(Week) = 1) or (weektype = 1 and dbo.F_IsWorkDay() = 1) or (weektype = 2 and dbo.F_IsWeekendDay() = 1) or (WeekType = 3 and dbo.GetFestival() = 1) )
     end
   select * from #TempQueryResultMain
   select * from #TempQueryResultGood
   select * from #TempQueryResultTicket
   select * from #TempQueryResultDetail
GO
/****** Object:  StoredProcedure [dbo].[GetMemberOpenCardFoodInfo]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberOpenCardFoodInfo](@StoreId varchar(20),@MemberLevelId int,@Result int output,@ErrMsg varchar(200) output)
as
 exec @Result = CheckMemberLevel @StoreId,@MemberLevelId,@ErrMsg output
 if @Result = 0
   return

 --获取套餐主表ID
 declare @FoodDetailLevelQuery as FoodDetailLevelQueryType
 insert into @FoodDetailLevelQuery(FoodID)
 exec GetMemberOpenCardFood @StoreId,2,@MemberLevelId
 
 create table #TempQueryResultMain(FoodID int,FoodName varchar(200),FoodType int,AllowInternet int,AllowPrint int,ForeAuthorize int,FoodPrice decimal(18,2),ImageUrl varchar(200))
 insert into #TempQueryResultMain(FoodID,FoodName,FoodType,AllowInternet,AllowPrint,ForeAuthorize,FoodPrice,ImageUrl)
 exec GetCommonFoodListInfo @FoodDetailLevelQuery,0,@MemberLevelId
  
 select * from #TempQueryResultMain
GO
/****** Object:  StoredProcedure [dbo].[FinishOrderPayment]    Script Date: 05/17/2018 09:37:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[FinishOrderPayment](
@RegisterMember [dbo].[RegisterMemberType] readonly,
@StoreID varchar(15),
@OrderFlwId int,
@OpenICCardId int,
@RealPay decimal(18,2),
@UserID int,
@WorkStation varchar(50),
@AuthorID int,
@ErrMsg varchar(200) output)
as
	declare @FoodSaleFlwId int
	declare @FoodID int
	declare @Point decimal(18,2)
	declare @UseCoin int
	declare @CoinBalance int
	declare @UsePoint int
	declare @PointBalance int
	declare @UseLottery int
	declare @LotteryBalance int
	declare @Deposit decimal(18,2)
	declare @TotalMoney decimal(18,2)
	declare @UseSumCoin int
	declare @UseSumPoint int
	declare @UseSumLottery int
	declare @CustomerType int
	declare @CardId int
	declare @ICCardID int
	declare @MemberLevelID int
	declare @IsBirthday bit
	declare @MemberPoints int
	declare @MemberLotterys int
	declare @MemberCoins int
	declare @OrderCreateTime datetime
	declare @OrderStatus int
	declare @OrderRealPay decimal(18,2)
	declare @OpenTime datetime
	   
    select @OrderStatus = OrderStatus,@OrderCreateTime = CreateTime,@OrderRealPay = RealPay from Flw_Order where StoreID = @StoreId and ID = @OrderFlwId
  
	if @OrderStatus = 2
	  begin
	    return 1
	  end
   
    if @OrderStatus <> 1
      begin
        set @ErrMsg = '订单状态不正确'
	    return 0	
      end
    
    if @RealPay <> @OrderRealPay
      begin
		set @ErrMsg = '实付金额不正确'
	    return 0	
      end
    
    
    --验证班次信息
	declare @CurrentSchedule int = 0
	declare @GetScheduleReturn int = 0
	exec @GetScheduleReturn = GetSchedule @StoreID,@UserID,@WorkStation,@CurrentSchedule output,@OpenTime output,@errMsg output
	if @GetScheduleReturn = 0
	  begin
		return 0
	  end
     
   --销售主表详情写入临时表  
   select b.Id as FoodSaleFlwId,b.FoodID,Point,UseCoin,CoinBalance,UsePoint,PointBalance,UseLottery,LotteryBalance,Deposit,TotalMoney,MemberLevelID into #TempFoodSaleDetail from Flw_Order_Detail a inner join Flw_Food_Sale b on a.FoodFlwID = b.ID where OrderFlwID = @OrderFlwId
   select @UseSumCoin = SUM(UseCoin),@UseSumPoint = SUM(UsePoint),@UseSumLottery = SUM(UseLottery) from #TempFoodSaleDetail
   select @CardId = CardID from Flw_Order where ID = @OrderFlwId
   
   --判断客户类型
   if @OpenICCardId > 0 
     begin
	   	if @UseSumCoin > 0 or @UseSumPoint > 0 or @UseSumLottery > 0
	   	  begin
	   	    set @ErrMsg = '当前订单销售商品有误，不能开通会员（存在使用积分、币、彩票月的情况）'
	   		return 0
	   	  end   	  
	   	else if @CardId > 0
	   	  begin
	   		set @ErrMsg = '当前订单销售商品有误，不能开通会员（存在会员卡Id）'
	   		return 0
	   	  end
	   	else
	   	  begin
	   		set @CustomerType = 1    
	   	  end
     end
   else if @CardId > 0
     begin
	   select @ICCardID = ICCardID from Data_Member_Card where ID = @CardId
	   if @ICCardID > 0
	     begin
		   set @CustomerType = 2    
	     end
	   else
	     begin
       	   set @ErrMsg = '当前订单销售商品有误，不能开通会员（会员信息不存在）'
   		   return 0
	     end
     end
   else
     begin
       set @CustomerType = 0
     end
     
      
   --获取会员基础信息,设置相关变量
   if @CustomerType = 2
     begin
	   declare @GetMemberSumInfoReturn int = 0
	   exec @GetMemberSumInfoReturn = GetMemberSumInfo @StoreID,@ICCardID,@MemberLevelId,@CardID output,@IsBirthday output,@MemberPoints output,@MemberLotterys output,@MemberCoins output,@ErrMsg output
	   if @GetMemberSumInfoReturn = 0 
		  begin
			return 0
		  end
	   
	   if @MemberPoints < @UseSumPoint
	     begin
	       set @ErrMsg = '会员积分余额不足'
		   return 0
	     end
	    
	   if @MemberLotterys < @UseSumLottery
	     begin
	       set @ErrMsg = '会员彩票余额不足'
		   return 0
	     end
	   	
	   if @MemberCoins < @UseSumCoin
	     begin
	       set @ErrMsg = '会员彩票数不足'
		   return 0
	     end
	   
	   --更新销售主表的会员余额（积分、彩票、代币）信息  
	   declare cur cursor for
	   select b.Id as FoodSaleFlwId,b.FoodID,Point,UseCoin,CoinBalance,UsePoint,PointBalance,UseLottery,LotteryBalance,Deposit,TotalMoney from Flw_Order_Detail a inner join Flw_Food_Sale b on a.FoodFlwID = b.ID where OrderFlwID = @FoodSaleFlwId order by b.ID
	   open cur
	   fetch next from cur into @FoodSaleFlwId,@FoodID,@Point,@UseCoin,@CoinBalance,@UsePoint,@PointBalance,@UseLottery,@LotteryBalance,@Deposit,@TotalMoney
		 WHILE @@FETCH_STATUS = 0
		   begin
			 set @MemberPoints = @MemberPoints + @Point - @UsePoint
			 set @MemberCoins = @MemberCoins - @UseCoin
			 set @MemberLotterys = @MemberLotterys - @UseLottery
			 update Flw_Food_Sale set PointBalance = @MemberPoints,LotteryBalance = @MemberLotterys,CoinBalance = @MemberCoins where ID = @FoodSaleFlwId
			 fetch next from cur into @FoodSaleFlwId,@FoodID,@Point,@UseCoin,@CoinBalance,@UsePoint,@PointBalance,@UseLottery,@LotteryBalance,@Deposit,@TotalMoney
		   end		
	   close cur
	   deallocate cur  
	   --更新会员余额
	   exec UpdateMemberBalance @StoreId,@ICCardId,0,@MemberPoints,@MemberLotterys,@MemberCoins 		 	  	   	
     end
	else if @CustomerType = 1
	  begin
	    --更新销售主表的会员余额（积分、彩票、代币）信息
	    set @MemberPoints = 0  
	    declare cur cursor for
	    select b.Id as FoodSaleFlwId,b.FoodID,Point,UseCoin,CoinBalance,UsePoint,PointBalance,UseLottery,LotteryBalance,Deposit,TotalMoney from Flw_Order_Detail a inner join Flw_Food_Sale b on a.FoodFlwID = b.ID where OrderFlwID = @FoodSaleFlwId order by b.ID
	    open cur
	    fetch next from cur into @FoodSaleFlwId,@FoodID,@Point,@UseCoin,@CoinBalance,@UsePoint,@PointBalance,@UseLottery,@LotteryBalance,@Deposit,@TotalMoney
		  WHILE @@FETCH_STATUS = 0
		    begin
			  set @MemberPoints = @MemberPoints + @Point
			  update Flw_Food_Sale set PointBalance = @MemberPoints where ID = @FoodSaleFlwId
			  fetch next from cur into @FoodSaleFlwId,@FoodID,@Point,@UseCoin,@CoinBalance,@UsePoint,@PointBalance,@UseLottery,@LotteryBalance,@Deposit,@TotalMoney
		   end		
	    close cur
	    deallocate cur
        --添加新会员信息
	    exec AddMemberCard @RegisterMember,@MemberLevelID,@WorkStation,@UserID
	  end	  
	--添加门票信息
	exec CreateOrderTicket @OrderFlwId,@CardId,@OrderCreateTime
	--修改订单状态为已支付
	update Flw_Order set ModifyTime = GETDATE(),PayTime = GETDATE(),UserID = @UserID,WorkStation = @WorkStation,AuthorID = @AuthorID,PayFee = @RealPay,OrderStatus = 2 where StoreID = @StoreId and ID = @OrderFlwId
	
    return 1
GO
/****** Object:  Default [DF__Base_Devi__Allow__3864608B]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_DeviceInfo] ADD  CONSTRAINT [DF__Base_Devi__Allow__3864608B]  DEFAULT ((0)) FOR [AllowPrint]
GO
/****** Object:  Default [DF_Base_MerchantInfo_MerchTag]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_MerchantInfo] ADD  CONSTRAINT [DF_Base_MerchantInfo_MerchTag]  DEFAULT ((1)) FOR [MerchTag]
GO
/****** Object:  Default [DF_Base_StoreInfo_StoreTag]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_StoreInfo] ADD  CONSTRAINT [DF_Base_StoreInfo_StoreTag]  DEFAULT ((0)) FOR [StoreTag]
GO
/****** Object:  Default [DF__Base_User__Group__4B7734FF]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_UserGroup] ADD  CONSTRAINT [DF__Base_User__Group__4B7734FF]  DEFAULT ((0)) FOR [GroupName]
GO
/****** Object:  Default [DF__Base_User__Funct__4E53A1AA]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_UserGroup_Grant] ADD  CONSTRAINT [DF__Base_User__Funct__4E53A1AA]  DEFAULT ((0)) FOR [FunctionID]
GO
/****** Object:  Default [DF_Base_UserInfo_SwitchMerch]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Base_UserInfo] ADD  CONSTRAINT [DF_Base_UserInfo_SwitchMerch]  DEFAULT ((0)) FOR [SwitchMerch]
GO
/****** Object:  Default [DF__Data_Food__day_s__6EC0713C]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Food_Level] ADD  CONSTRAINT [DF__Data_Food__day_s__6EC0713C]  DEFAULT ((0)) FOR [AllCount]
GO
/****** Object:  Default [DF__Data_Food__membe__6FB49575]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Food_Level] ADD  CONSTRAINT [DF__Data_Food__membe__6FB49575]  DEFAULT ((0)) FOR [MemberCount]
GO
/****** Object:  Default [DF__Data_Food__day_s__679450C0]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Food_Record] ADD  DEFAULT ((0)) FOR [day_sale_count]
GO
/****** Object:  Default [DF__Data_Food__membe__688874F9]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Food_Record] ADD  DEFAULT ((0)) FOR [member_day_sale_count]
GO
/****** Object:  Default [DF__Data_Food__FoodT__671F4F74]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_FoodInfo] ADD  CONSTRAINT [DF__Data_Food__FoodT__671F4F74]  DEFAULT ((0)) FOR [FoodType]
GO
/****** Object:  Default [DF__Data_Food__FoodS__690797E6]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_FoodInfo] ADD  CONSTRAINT [DF__Data_Food__FoodS__690797E6]  DEFAULT ((1)) FOR [FoodState]
GO
/****** Object:  Default [DF__Data_Food__ForeA__69FBBC1F]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_FoodInfo] ADD  CONSTRAINT [DF__Data_Food__ForeA__69FBBC1F]  DEFAULT ((0)) FOR [ForeAuthorize]
GO
/****** Object:  Default [DF_Data_FoodInfo_ClientPrice]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_FoodInfo] ADD  CONSTRAINT [DF_Data_FoodInfo_ClientPrice]  DEFAULT ((0)) FOR [ClientPrice]
GO
/****** Object:  Default [DF_Data_FoodInfo_MemberPrice]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_FoodInfo] ADD  CONSTRAINT [DF_Data_FoodInfo_MemberPrice]  DEFAULT ((0)) FOR [MemberPrice]
GO
/****** Object:  Default [DF__Data_Game__ExitC__18D6A699]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameFreeRule] ADD  DEFAULT ((0)) FOR [ExitCoin]
GO
/****** Object:  Default [DF__Data_Game__State__72910220]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__State__72910220]  DEFAULT ('0') FOR [State]
GO
/****** Object:  Default [DF__Data_Game__Retur__73852659]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Retur__73852659]  DEFAULT ((1)) FOR [ReturnCheck]
GO
/****** Object:  Default [DF__Data_Game__Outsi__74794A92]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Outsi__74794A92]  DEFAULT ((1)) FOR [OutsideAlertCheck]
GO
/****** Object:  Default [DF__Data_Game__ICTic__756D6ECB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__ICTic__756D6ECB]  DEFAULT ((0)) FOR [ICTicketOperation]
GO
/****** Object:  Default [DF__Data_Game__NotGi__76619304]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__NotGi__76619304]  DEFAULT ((0)) FOR [NotGiveBack]
GO
/****** Object:  Default [DF__Data_Game__Lotte__7755B73D]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Lotte__7755B73D]  DEFAULT ((0)) FOR [LotteryMode]
GO
/****** Object:  Default [DF__Data_Game__OnlyE__7849DB76]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OnlyE__7849DB76]  DEFAULT ((0)) FOR [OnlyExitLottery]
GO
/****** Object:  Default [DF__Data_Game__Allow__793DFFAF]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Allow__793DFFAF]  DEFAULT ((0)) FOR [AllowElecPush]
GO
/****** Object:  Default [DF__Data_Game__Allow__7A3223E8]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Allow__7A3223E8]  DEFAULT ((0)) FOR [AllowDecuplePush]
GO
/****** Object:  Default [DF__Data_Game__Guard__7B264821]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Guard__7B264821]  DEFAULT ((0)) FOR [GuardConvertCard]
GO
/****** Object:  Default [DF__Data_Game__Allow__7C1A6C5A]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Allow__7C1A6C5A]  DEFAULT ((0)) FOR [AllowRealPush]
GO
/****** Object:  Default [DF__Data_Game__BanOc__7D0E9093]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__BanOc__7D0E9093]  DEFAULT ((0)) FOR [BanOccupy]
GO
/****** Object:  Default [DF__Data_Game__Stron__7E02B4CC]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Stron__7E02B4CC]  DEFAULT ((0)) FOR [StrongGuardConvertCard]
GO
/****** Object:  Default [DF__Data_Game__PushC__7EF6D905]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushC__7EF6D905]  DEFAULT ((0)) FOR [PushControl]
GO
/****** Object:  Default [DF__Data_Game__Allow__7FEAFD3E]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Allow__7FEAFD3E]  DEFAULT ((0)) FOR [AllowElecOut]
GO
/****** Object:  Default [DF__Data_Game__Allow__00DF2177]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Allow__00DF2177]  DEFAULT ((0)) FOR [AllowRealOut]
GO
/****** Object:  Default [DF__Data_Game__PushR__01D345B0]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushR__01D345B0]  DEFAULT ((1)) FOR [PushReduceFromCard]
GO
/****** Object:  Default [DF__Data_Game__PushA__02C769E9]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushA__02C769E9]  DEFAULT ((1)) FOR [PushAddToGame]
GO
/****** Object:  Default [DF__Data_Game__PushS__03BB8E22]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushS__03BB8E22]  DEFAULT ((0)) FOR [PushSpeed]
GO
/****** Object:  Default [DF__Data_Game__PushP__04AFB25B]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushP__04AFB25B]  DEFAULT ((0)) FOR [PushPulse]
GO
/****** Object:  Default [DF__Data_Game__PushL__05A3D694]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushL__05A3D694]  DEFAULT ((0)) FOR [PushLevel]
GO
/****** Object:  Default [DF__Data_Game__PushS__0697FACD]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__PushS__0697FACD]  DEFAULT ((0)) FOR [PushStartInterval]
GO
/****** Object:  Default [DF__Data_Game__UseSe__078C1F06]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__UseSe__078C1F06]  DEFAULT ((0)) FOR [UseSecondPush]
GO
/****** Object:  Default [DF__Data_Game__Secon__0880433F]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__0880433F]  DEFAULT ((1)) FOR [SecondReduceFromCard]
GO
/****** Object:  Default [DF__Data_Game__Secon__09746778]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__09746778]  DEFAULT ((10)) FOR [SecondAddToGame]
GO
/****** Object:  Default [DF__Data_Game__Secon__0A688BB1]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__0A688BB1]  DEFAULT ((0)) FOR [SecondSpeed]
GO
/****** Object:  Default [DF__Data_Game__Secon__0B5CAFEA]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__0B5CAFEA]  DEFAULT ((0)) FOR [SecondPulse]
GO
/****** Object:  Default [DF__Data_Game__Secon__0C50D423]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__0C50D423]  DEFAULT ((0)) FOR [SecondLevel]
GO
/****** Object:  Default [DF__Data_Game__Secon__0D44F85C]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Secon__0D44F85C]  DEFAULT ((0)) FOR [SecondStartInterval]
GO
/****** Object:  Default [DF__Data_Game__OutSp__0E391C95]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OutSp__0E391C95]  DEFAULT ((0)) FOR [OutSpeed]
GO
/****** Object:  Default [DF__Data_Game__OutPu__0F2D40CE]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OutPu__0F2D40CE]  DEFAULT ((0)) FOR [OutPulse]
GO
/****** Object:  Default [DF__Data_Game__Count__10216507]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Count__10216507]  DEFAULT ((0)) FOR [CountLevel]
GO
/****** Object:  Default [DF__Data_Game__OutLe__11158940]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OutLe__11158940]  DEFAULT ((0)) FOR [OutLevel]
GO
/****** Object:  Default [DF__Data_Game__OutRe__1209AD79]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OutRe__1209AD79]  DEFAULT ((1)) FOR [OutReduceFromGame]
GO
/****** Object:  Default [DF__Data_Game__OutAd__12FDD1B2]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OutAd__12FDD1B2]  DEFAULT ((1)) FOR [OutAddToCard]
GO
/****** Object:  Default [DF__Data_Game__OnceO__13F1F5EB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OnceO__13F1F5EB]  DEFAULT ((0)) FOR [OnceOutLimit]
GO
/****** Object:  Default [DF__Data_Game__OnceP__14E61A24]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__OnceP__14E61A24]  DEFAULT ((0)) FOR [OncePureOutLimit]
GO
/****** Object:  Default [DF__Data_Game__SSRTi__15DA3E5D]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__SSRTi__15DA3E5D]  DEFAULT ((0)) FOR [SSRTimeOut]
GO
/****** Object:  Default [DF__Data_Game__Excep__16CE6296]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Excep__16CE6296]  DEFAULT ((0)) FOR [ExceptOutTest]
GO
/****** Object:  Default [DF__Data_Game__Excep__17C286CF]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Excep__17C286CF]  DEFAULT ((0)) FOR [ExceptOutSpeed]
GO
/****** Object:  Default [DF__Data_Game__Frequ__18B6AB08]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_GameInfo] ADD  CONSTRAINT [DF__Data_Game__Frequ__18B6AB08]  DEFAULT ((0)) FOR [Frequency]
GO
/****** Object:  Default [DF__Data_Memb__Depos__3805392F]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Depos__3805392F]  DEFAULT ((0.0)) FOR [Deposit]
GO
/****** Object:  Default [DF__Data_Memb__Valid__38F95D68]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Valid__38F95D68]  DEFAULT ((365)) FOR [Validday]
GO
/****** Object:  Default [DF__Data_Memb__FreeR__39ED81A1]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__FreeR__39ED81A1]  DEFAULT ((0)) FOR [FreeRate]
GO
/****** Object:  Default [DF__Data_Memb__FreeC__3AE1A5DA]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__FreeC__3AE1A5DA]  DEFAULT ((0)) FOR [FreeCoin]
GO
/****** Object:  Default [DF__Data_Memb__FreeT__3BD5CA13]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__FreeT__3BD5CA13]  DEFAULT ((0)) FOR [FreeType]
GO
/****** Object:  Default [DF__Data_Memb__FreeN__3CC9EE4C]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__FreeN__3CC9EE4C]  DEFAULT ((0)) FOR [FreeNeedWin]
GO
/****** Object:  Default [DF__Data_Memb__Birth__3DBE1285]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Birth__3DBE1285]  DEFAULT ((0)) FOR [BirthdayFree]
GO
/****** Object:  Default [DF__Data_Memb__MinCo__3EB236BE]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__MinCo__3EB236BE]  DEFAULT ((0.0)) FOR [MinCoin]
GO
/****** Object:  Default [DF__Data_Memb__MaxCo__3FA65AF7]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__MaxCo__3FA65AF7]  DEFAULT ((0)) FOR [MaxCoin]
GO
/****** Object:  Default [DF__Data_Memb__Allow__409A7F30]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Allow__409A7F30]  DEFAULT ((0)) FOR [AllowExitCard]
GO
/****** Object:  Default [DF__Data_Memb__Allow__418EA369]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Allow__418EA369]  DEFAULT ((0)) FOR [AllowExitMoney]
GO
/****** Object:  Default [DF__Data_Memb__Allow__4282C7A2]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__Allow__4282C7A2]  DEFAULT ((0)) FOR [AllowExitCoinToCard]
GO
/****** Object:  Default [DF__Data_Memb__State__4376EBDB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_MemberLevel] ADD  CONSTRAINT [DF__Data_Memb__State__4376EBDB]  DEFAULT ((1)) FOR [State]
GO
/****** Object:  Default [DF__Data_Para__Syste__40C49C62]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Parameters] ADD  CONSTRAINT [DF__Data_Para__Syste__40C49C62]  DEFAULT ('0') FOR [System]
GO
/****** Object:  Default [DF__Data_Para__IsAll__41B8C09B]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Parameters] ADD  CONSTRAINT [DF__Data_Para__IsAll__41B8C09B]  DEFAULT ((1)) FOR [IsAllow]
GO
/****** Object:  Default [DF__Data_Work__State__7C8F6DA6]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Workstation] ADD  CONSTRAINT [DF__Data_Work__State__7C8F6DA6]  DEFAULT ((0)) FOR [State]
GO
/****** Object:  Default [DF__Data_Work__UserO__7D8391DF]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Workstation] ADD  CONSTRAINT [DF__Data_Work__UserO__7D8391DF]  DEFAULT ((0)) FOR [UserOnlineState]
GO
/****** Object:  Default [DF__Data_Work__Sched__7E77B618]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Data_Workstation] ADD  CONSTRAINT [DF__Data_Work__Sched__7E77B618]  DEFAULT ((0)) FOR [ScheduleSender]
GO
/****** Object:  Default [DF_Dict_FunctionMenu_MenuType]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Dict_FunctionMenu] ADD  CONSTRAINT [DF_Dict_FunctionMenu_MenuType]  DEFAULT ((0)) FOR [MenuType]
GO
/****** Object:  Default [DF_Dict_FunctionMenu_UseType]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Dict_FunctionMenu] ADD  CONSTRAINT [DF_Dict_FunctionMenu_UseType]  DEFAULT ((0)) FOR [UseType]
GO
/****** Object:  Default [DF__Flw_485_C__Coins__51EF2864]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_485_Coin] ADD  CONSTRAINT [DF__Flw_485_C__Coins__51EF2864]  DEFAULT ((0)) FOR [Coins]
GO
/****** Object:  Default [DF__Flw_485_C__Balan__52E34C9D]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_485_Coin] ADD  CONSTRAINT [DF__Flw_485_C__Balan__52E34C9D]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_485_S__Balan__55BFB948]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_485_SaveCoin] ADD  CONSTRAINT [DF__Flw_485_S__Balan__55BFB948]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_Coin___CoinM__5A846E65]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Exit] ADD  CONSTRAINT [DF__Flw_Coin___CoinM__5A846E65]  DEFAULT ((0.0)) FOR [CoinMoney]
GO
/****** Object:  Default [DF__Flw_Coin___Balan__5B78929E]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Exit] ADD  CONSTRAINT [DF__Flw_Coin___Balan__5B78929E]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_Coin___Coins__5D60DB10]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Sale] ADD  CONSTRAINT [DF__Flw_Coin___Coins__5D60DB10]  DEFAULT ((0)) FOR [Coins]
GO
/****** Object:  Default [DF__Flw_Coin___Balan__5E54FF49]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Sale] ADD  CONSTRAINT [DF__Flw_Coin___Balan__5E54FF49]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_Coin___WorkT__5F492382]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Sale] ADD  CONSTRAINT [DF__Flw_Coin___WorkT__5F492382]  DEFAULT ((2)) FOR [WorkType]
GO
/****** Object:  Default [DF__Flw_Coin___IsBir__603D47BB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Coin_Sale] ADD  CONSTRAINT [DF__Flw_Coin___IsBir__603D47BB]  DEFAULT ((0)) FOR [IsBirthday]
GO
/****** Object:  Default [DF__Flw_Food___CoinB__66EA454A]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Exit] ADD  CONSTRAINT [DF__Flw_Food___CoinB__66EA454A]  DEFAULT ((0)) FOR [CoinBalance]
GO
/****** Object:  Default [DF__Flw_Food___Depos__6F1576F7]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Sale] ADD  CONSTRAINT [DF__Flw_Food___Depos__6F1576F7]  DEFAULT ((0.00)) FOR [Deposit]
GO
/****** Object:  Default [DF__Flw_Food___OpenF__70099B30]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Sale] ADD  CONSTRAINT [DF__Flw_Food___OpenF__70099B30]  DEFAULT ((0.00)) FOR [OpenFee]
GO
/****** Object:  Default [DF__Flw_Food___Renew__70FDBF69]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Sale] ADD  CONSTRAINT [DF__Flw_Food___Renew__70FDBF69]  DEFAULT ((0.00)) FOR [RenewFee]
GO
/****** Object:  Default [DF__Flw_Food___Chang__71F1E3A2]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Sale] ADD  CONSTRAINT [DF__Flw_Food___Chang__71F1E3A2]  DEFAULT ((0.00)) FOR [ChangeFee]
GO
/****** Object:  Default [DF__Flw_Food___BuyFo__72E607DB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Food_Sale] ADD  CONSTRAINT [DF__Flw_Food___BuyFo__72E607DB]  DEFAULT ((1)) FOR [BuyFoodType]
GO
/****** Object:  Default [DF__Flw_Game___FreeC__753864A1]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Game_Free] ADD  CONSTRAINT [DF__Flw_Game___FreeC__753864A1]  DEFAULT ((0)) FOR [FreeCoin]
GO
/****** Object:  Default [DF__Flw_Game___Balan__762C88DA]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Game_Free] ADD  CONSTRAINT [DF__Flw_Game___Balan__762C88DA]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_Giveb__WinMo__7814D14C]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Giveback] ADD  CONSTRAINT [DF__Flw_Giveb__WinMo__7814D14C]  DEFAULT ((0.0)) FOR [WinMoney]
GO
/****** Object:  Default [DF__Flw_Giveb__MayCo__7908F585]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Giveback] ADD  CONSTRAINT [DF__Flw_Giveb__MayCo__7908F585]  DEFAULT ((0)) FOR [MayCoins]
GO
/****** Object:  Default [DF__Flw_Giveb__Balan__79FD19BE]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Giveback] ADD  CONSTRAINT [DF__Flw_Giveb__Balan__79FD19BE]  DEFAULT ((0)) FOR [Balance]
GO
/****** Object:  Default [DF__Flw_Goods__PayTy__7EC1CEDB]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__PayTy__7EC1CEDB]  DEFAULT ((0)) FOR [IsCancel]
GO
/****** Object:  Default [DF__Flw_Goods__Goods__7FB5F314]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__Goods__7FB5F314]  DEFAULT ((0.00)) FOR [GoodsMoney]
GO
/****** Object:  Default [DF__Flw_Goods__Coins__00AA174D]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__Coins__00AA174D]  DEFAULT ((0)) FOR [Coins]
GO
/****** Object:  Default [DF__Flw_Goods__Point__019E3B86]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__Point__019E3B86]  DEFAULT ((0)) FOR [Point]
GO
/****** Object:  Default [DF__Flw_Goods__Lotte__02925FBF]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__Lotte__02925FBF]  DEFAULT ((0)) FOR [Lottery]
GO
/****** Object:  Default [DF__Flw_Goods__CoinB__047AA831]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Goods] ADD  CONSTRAINT [DF__Flw_Goods__CoinB__047AA831]  DEFAULT ((0)) FOR [CoinBalance]
GO
/****** Object:  Default [DF__Flw_Lotte__State__093F5D4E]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Lottery] ADD  DEFAULT ((0)) FOR [State]
GO
/****** Object:  Default [DF__Flw_Sched__State__0E04126B]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Schedule] ADD  CONSTRAINT [DF__Flw_Sched__State__0E04126B]  DEFAULT ((0)) FOR [State]
GO
/****** Object:  Default [DF__Flw_Ticke__State__10E07F16]    Script Date: 05/17/2018 09:37:14 ******/
ALTER TABLE [dbo].[Flw_Ticket_Exit] ADD  CONSTRAINT [DF__Flw_Ticke__State__10E07F16]  DEFAULT ((0)) FOR [State]
GO
/****** Object:  UserDefinedTableType [dbo].[CouponStateListType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[CouponStateListType] AS TABLE(
	[CouponCode] [varchar](32) NULL,
	[State] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FoodBalanceType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[FoodBalanceType] AS TABLE(
	[BalanceType] [int] NULL,
	[TypeName] [varchar](200) NULL,
	[UseCount] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FoodDetailCommonQueryType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[FoodDetailCommonQueryType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodType] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FoodDetailListType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[FoodDetailListType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodType] [int] NULL,
	[FoodCount] [int] NULL,
	[PayType] [int] NULL,
	[PayNum] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[OpenCardFoodDetailCommonQueryType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[OpenCardFoodDetailCommonQueryType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodType] [int] NULL,
	[Deposit] [decimal](18, 2) NULL,
	[OpenFee] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[PayTypeSumType]    Script Date: 05/17/2018 09:37:18 ******/
CREATE TYPE [dbo].[PayTypeSumType] AS TABLE(
	[PayType] [int] NULL,
	[PaySum] [decimal](18, 2) NULL
)
GO
