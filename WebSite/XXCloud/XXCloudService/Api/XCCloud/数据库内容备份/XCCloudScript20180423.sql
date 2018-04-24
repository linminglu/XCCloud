USE [XCCloud]
GO
/****** Object:  StoredProcedure [dbo].[GetSplitTable]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetSplitTable](@Str varchar(200))
as
  create table #TmpSplitTable(Id int)
  declare @Index int = 1 
  declare @Len int = LEN(@Str)
  while @Index <= @Len
    begin	
		if SUBSTRING(@Str,@Index,1) <> ','
		  begin
			insert #TmpSplitTable(Id)
			values(SUBSTRING(@Str,@Index,1))
		  end
		set @Index = @Index + 1
    end
  select * from #TmpSplitTable
GO
/****** Object:  StoredProcedure [dbo].[GetOrderDateFormatStr]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  UserDefinedTableType [dbo].[RegisterMemberType]    Script Date: 04/23/2018 19:35:57 ******/
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
/****** Object:  UserDefinedTableType [dbo].[FoodDetailType]    Script Date: 04/23/2018 19:35:57 ******/
CREATE TYPE [dbo].[FoodDetailType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodCount] [int] NULL,
	[PayType] [int] NULL,
	[PayNum] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FoodDetailCommonQueryType]    Script Date: 04/23/2018 19:35:57 ******/
CREATE TYPE [dbo].[FoodDetailCommonQueryType] AS TABLE(
	[FoodId] [int] NULL,
	[FoodType] [int] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[CheckWeekStr]    Script Date: 04/23/2018 19:35:56 ******/
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
/****** Object:  UserDefinedFunction [dbo].[F_GetNewId]    Script Date: 04/23/2018 19:35:56 ******/
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
/****** Object:  UserDefinedFunction [dbo].[CheckMobile]    Script Date: 04/23/2018 19:35:56 ******/
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
/****** Object:  UserDefinedTableType [dbo].[CheckFoodDetailType]    Script Date: 04/23/2018 19:35:57 ******/
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
/****** Object:  StoredProcedure [dbo].[AddTicket]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[DeleteOrderDetail]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[CheckOrders]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  UserDefinedFunction [dbo].[F_GetTicketDesc]    Script Date: 04/23/2018 19:35:56 ******/
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
/****** Object:  StoredProcedure [dbo].[CheckStoreIsInAllGroupRule]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[UpdateMemberBalance]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[UpdateMemberBalance](@StoreId varchar(15),@ICCardId varchar(20),@MemberStorages int,@MemberPoints int,@MemberLotterys int,@MemberCoins int)
as  
  declare @CardBalanceId int = 0
  declare @BalanceType int = 0

  declare cur cursor for
  select a.ID as CardBalanceId,a.BalanceType from Data_Card_Balance a inner join Data_Card_Balance_StoreList b on a.ID = b.CardBalanceID 
  where MemberID = (select MemberID from Data_Member_Card a inner join Data_Member_Card_Store b on a.ID = b.CardID 
  where StoreID = @StoreId and ICCardID = @ICCardId)
  order by b.Id,a.BalanceType 
  open cur
  fetch next from cur into @CardBalanceId,@BalanceType
  WHILE @@FETCH_STATUS = 0
	begin
	  if @BalanceType = 0 
	    begin
	      update Data_Card_Balance set Banlance = @MemberStorages,UpdateTime = GETDATE() where ID = @CardBalanceId
	    end
	  else if @BalanceType = 1 
        begin
          update Data_Card_Balance set Banlance = @MemberCoins,UpdateTime = GETDATE() where ID = @CardBalanceId
        end
      else if @BalanceType = 2
        begin
          update Data_Card_Balance set Banlance = @MemberPoints,UpdateTime = GETDATE() where ID = @CardBalanceId
        end
      else if @BalanceType = 3
        begin
		  update Data_Card_Balance set Banlance = @MemberLotterys,UpdateTime = GETDATE() where ID = @CardBalanceId	
        end
	  fetch next from cur into @CardBalanceId,@BalanceType
	end		
  close cur
  deallocate cur
GO
/****** Object:  StoredProcedure [dbo].[SP_RowToCol2]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SP_RowToCol]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SP_RegisterUserFromWx]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SP_GetMenus]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[SP_GetMenus](@LogType int, @LogID int, @MerchID varchar(15))
as
begin
	
	CREATE TABLE #MENU (FunctionID INT NULL)	
	if @LogType=3 --商户用户
	begin
		declare @MerchType int 
		SELECT @MerchType=MerchType FROM Base_MerchantInfo WHERE MerchID=@MerchID
		insert into #MENU
		select FunctionID from Dict_FunctionMenu where MenuType=(case when @MerchType in (1, 2) then 3 when @MerchType=3 then 4 else null end)
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
		insert into #MENU
		select FunctionID from Dict_FunctionMenu where MenuType=1
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
/****** Object:  StoredProcedure [dbo].[SP_DictionaryNodes]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create Proc [dbo].[SP_DictionaryNodes](@MerchID nvarchar(15), @DictKey nvarchar(50), @RootID int output)
as
 begin
	declare @sql nvarchar(max)
	SET @sql = ''
	SET @sql = @sql + 'select * from Dict_System where 1=1'
	if(@MerchID is not null and @MerchID <> '')
		SET @sql = @sql + ' and (MerchID=@MerchID or MerchID is null or MerchID='''')'
	if(@DictKey is not null and @DictKey <> '')
		begin	
			select @RootID=ID from (select top 1 ID from Dict_System where DictKey=@DictKey and (MerchID=@MerchID or MerchID is null or MerchID='')) m
			if not exists (select 0 from Dict_System where PID=@RootID)
				return
			SET @sql = @sql + ' and PID=@RootID'
		end
	--exec (@sql)
	exec sp_executesql @sql, N'@MerchID nvarchar(15), @DictKey nvarchar(50), @RootID int', @MerchID, @DictKey, @RootID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectXcUserGrant]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SelectUserGroupGrant]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SelectUserGroup]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SelectUserGrant]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SelectStoreUnchecked]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[SelectMerchFunction]    Script Date: 04/23/2018 19:35:55 ******/
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
	from Dict_FunctionMenu a left join (select b.MerchID,c.FunctionID,c.FunctionEN from Base_MerchantInfo a inner join Base_UserInfo b on a.CreateUserID=b.UserID inner join Base_MerchFunction c on b.MerchID=c.MerchID and a.MerchType=3) b on a.FunctionID=b.FunctionID
	left join Base_MerchFunction c on a.FunctionID=c.FunctionID and c.MerchID=@MerchID 
	where a.MenuType in (1, case when @MerchTag=1 then 5 else 1 end)
	order by a.ParentID, a.FunctionID
 end
GO
/****** Object:  StoredProcedure [dbo].[SelectFunctionForXA]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[OpenSchedule]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetCommonFoodListInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodListInfo](@FoodDetailCommonQueryType [FoodDetailCommonQueryType] readonly,@CustomerType int,@MemberLevelId int)
as
	select a.FoodId,a.FoodName,a.FoodType,AllowInternet,AllowPrint,ForeAuthorize,isnull(b.FoodPrice,a.FoodPrice) as FoodPrice,a.ImageURL
	from 
	(
		select a.FoodId,b.FoodName,a.FoodType,AllowPrint,AllowInternet,ForeAuthorize,
		isnull(case @CustomerType when 0 then b.ClientPrice else b.MemberPrice end,0) AS FoodPrice,b.ImageURL 
		from @FoodDetailCommonQueryType a inner join Data_FoodInfo b on a.FoodId = b.FoodID
	) a
	left join 
	(
		select a.FoodId,isnull(case @CustomerType when 0 then ClientPrice else VIPPrice end,0) AS FoodPrice 
		from 
		(
			select a.FoodId,MAX(ID) as MemberLevelId from @FoodDetailCommonQueryType a inner join Data_Food_Level b on a.FoodId = b.FoodID
			left join Data_Food_Record c on b.ID = c.FoodLevelID and DATEDIFF(DY,RecordDate,GETDATE()) = 0
			where DATEDIFF(DY,StartDate,GETDATE()) >= 0 and DATEDIFF(dy,EndDate,GETDATE()) <= 0
			and DATEDIFF(DY,StartTime,cast(GETDATE() as time)) >= 0 and DATEDIFF(dy,EndTime,cast(GETDATE() as time)) <= 0
			and dbo.CheckWeekStr(b.Week) = 1 and b.MemberLevelID = @MemberLevelId
			and isnull((case @CustomerType when 0 then b.day_sale_count else b.member_day_sale_count end),0) - isnull((case @CustomerType when 0 then c.day_sale_count else c.member_day_sale_count end),0) > 0
			group by a.FoodId
		) a
		inner join Data_Food_Level b on a.MemberLevelId = b.ID
	) b
	on a.FoodId = b.FoodId
GO
/****** Object:  StoredProcedure [dbo].[GetCommonFoodBalanceListInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetCommonFoodBalanceListInfo](@FoodDetailCommonQueryType [FoodDetailCommonQueryType] readonly)
as
  select a.FoodId,b.BalanceType,TypeName,UseCount from @FoodDetailCommonQueryType a inner join Data_Food_Sale b on a.FoodId = b.FoodID
  inner join Dict_BalanceType c on b.BalanceType = c.ID
  order by a.FoodId,b.BalanceType
GO
/****** Object:  StoredProcedure [dbo].[GetCoinSalePoint]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetMemberRepeatCode]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetMemberOpenCardFoodInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberOpenCardFoodInfo](@StoreId varchar(20),@MemberLevelIdStr nvarchar(100))
as
 create table #TmpSplitTable(MemberLevelId int)
 insert #TmpSplitTable(MemberLevelId)
 exec GetSplitTable @MemberLevelIdStr
 
 create table #TmpFood(FoodId int,MemberLevelId int)
 
 declare @CurrentTime time = getdate()
 insert #TmpFood(FoodId,MemberLevelId)
 select a.FoodID,Max(c.MemberLevelID) as MemberLevelId from 
 ( select * from Data_FoodInfo a where a.StoreID = @StoreId and a.FoodState = 1 and a.FoodType = 1 and DATEDIFF(dy,a.StartTime,GETDATE()) >= 0 and DATEDIFF(dy,a.EndTime,GETDATE()) <= 0 ) a
 inner join Data_Food_Level b on a.FoodID = b.FoodID
 inner join Data_MemberLevel c on a.StoreID = @StoreId and c.MemberLevelID = b.MemberLevelID
 where DATEDIFF(dy,b.StartDate,GETDATE()) >= 0 and DATEDIFF(dy,b.EndDate,GETDATE()) <= 0 and cast(GETDATE() as time) >= b.StartTime and cast(GETDATE() as time) <= b.EndTime and dbo.CheckWeekStr([Week]) = 1 
 group by a.FoodID
 having COUNT(c.MemberLevelID) = 1
  
 select a.FoodID,FoodName,AllowPrint,RechargeType,
 case RechargeType when 0 then '手动选择实物币或充值到卡' when 1 then '只允许实物币' when 2 then '只允许充值到卡' end as RechargeTypeName,ImageUrl,
 c.Deposit as FoodPrice,'押金' + Cast(CAST(c.Deposit as decimal(18,2)) as varchar) + '元' as ContainName
 from #TmpFood a inner join Data_FoodInfo b on a.FoodId = b.FoodID
 inner join Data_MemberLevel c on a.MemberLevelId = c.MemberLevelID
GO
/****** Object:  StoredProcedure [dbo].[GetMemberLevel]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberLevel](@StoreId varchar(15))
as
  select a.MemberLevelID,a.MemberLevelName from Data_MemberLevel a
  where a.StoreID = @StoreId and State = 1
GO
/****** Object:  StoredProcedure [dbo].[GetMember]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--获取会员信息
CREATE Proc [dbo].[GetMember](@ICCardID varchar(50),@StoreID varchar(50),@Result int output,@ErrMsg varchar(200) output)
as
	--获取卡号的卡ID,
	declare @CardID int = 0
	declare @MemberId int = 0 
	select @CardID = isnull(CardID,0) ,@MemberId = isnull(MemberID,0) from Data_Member_Card_Store a inner join Data_Member_Card b on a.CardID = b.ID where StoreID = @StoreID and ICCardID = @ICCardID
	if @CardID = 0
	   begin
		 set @Result = 0
		 set @ErrMsg = '会员卡不存在'
		 return 
	   end 
   
    --汇总卡ID对应的积分、余额、彩票数、储值金
	select b.StoreID,a.BalanceType,a.Banlance into #MemberCardInfo from Data_Card_Balance a inner join Data_Card_Balance_StoreList b on a.ID = b.CardBalanceID where MemberID = @MemberId
	declare @Storage decimal(18,2) = 0
	declare @Banlance decimal(18,2) = 0
	declare @Point decimal(18,2) = 0
	declare @Lottery decimal(18,2) = 0
	select 
	@Storage = sum(isnull(case BalanceType when 0 then Balance end,0)),
	@Banlance = sum(isnull(case BalanceType when 1 then Balance end,0)),
	@Point = sum(isnull(case BalanceType when 2 then Balance end,0)),
	@Lottery = sum(isnull(case BalanceType when 3 then Balance end,0))
	from 
	( select BalanceType,SUM(Banlance) as Balance from #MemberCardInfo group by BalanceType ) a
 
    --会员信息结果集
    select a.ID as CardId,a.ICCardID,a.MemberLevelID,b.UserName as MemberName,b.Gender,convert(char(10),b.Birthday,120) as Birthday,
    b.IDCard,b.Mobile,cast(a.Deposit as decimal(18,2)) as Deposit,b.Note,convert(char(10),a.EndDate,120) as EndDate,a.RepeatCode,b.MemberState,a.CardStatus,
	( select MemberLevelName from Data_MemberLevel where StoreID = @StoreID and MemberLevelID = a.MemberLevelID) as MemberLevelName,@StoreID as StoreId,
	( select StoreName from Base_StoreInfo where StoreID = @StoreID ) as StoreName,@Storage as Storage,@Banlance as Banlance,@Point as Point,@Lottery as Lottery
	from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID where a.ID = @CardID

	
    set @Result = 1
GO
/****** Object:  StoredProcedure [dbo].[GetSchedule]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetSameGroupRuleTypeStoreInfo]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetOrdersCheck]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[IsExistOtherStoreOpenCardRecardInALLGroupRule]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetStoreGroupRule]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetMerchOtherStoreMemberCardInfo]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[GetMemberSumInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetMemberSumInfo](@StoreID varchar(15),@ICCardID int,@MemberLevelId int,@CardID int output,@IsBirthday bit output,@MemberPoints int output,@MemberLotterys int output,@MemberCoins int output,@ErrMsg varchar(200) output)
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
/****** Object:  StoredProcedure [dbo].[GetFoodPoint]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodPoint](@StoreId varchar(20),@FoodId int,@Point int output)
as
	declare @FoodType int = 0
	declare @ContainID int = 0
	declare @ContainCount int = 0
	declare @TmpPoint int = 0
	set @Point = 0
	declare cur cursor for
	select b.FoodType,b.ContainID,ContainCount from Data_FoodInfo a inner join Data_Food_Detial b on a.FoodID = b.FoodID
	where a.FoodID = @FoodId and b.Status = 1
	open cur  
	fetch next from cur into @FoodType,@ContainID,@ContainCount
	WHILE @@FETCH_STATUS = 0
	  begin
		--0 代币;1 餐饮;2 礼品;3 门票	
		if @FoodType = 0
		  begin
			exec GetCoinSalePoint @StoreID,@ContainCount,@TmpPoint output
			set @Point = @Point + @TmpPoint
		  end
		else if @FoodType = 1
		  begin
		    set @TmpPoint = 0
			select @TmpPoint = Points from Base_GoodsInfo where ID = @ContainID
			set @Point = @Point + @TmpPoint * @ContainCount
		  end
		else if @FoodType = 2
		  begin
		    set @TmpPoint = 0
			select @TmpPoint = Points from Base_GoodsInfo where ID = @ContainID
			set @Point = @Point + @TmpPoint * @ContainCount
		  end
		fetch next from cur into @FoodType,@ContainID,@ContainCount
	  end
	close cur  
	deallocate cur
GO
/****** Object:  StoredProcedure [dbo].[GetFoodListInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodListInfo](@StoreId varchar(20),@CustomerType int,@MemberLevelId int,@FoodTypeStr nvarchar(100))
as
 declare @Coins int = 0 
 --获取会员开卡套餐
 declare @CurrentDateTime datetime = getdate()
 declare @CurrentTime time = getdate()
 
 create table #TempFoodTypeDic(Id int)
 insert #TempFoodTypeDic(Id)
 exec dbo.GetSplitTable @FoodTypeStr
 
 --获取套餐主表ID
 select FoodID,FoodType into #TempFoodType from 
 (   
   select a.FoodID,a.FoodType
   from Data_FoodInfo a inner join Data_Food_StoreList b on
   a.FoodID = b.FoodID and b.StoreID = @StoreId and DATEDIFF(dy,StartTime,getdate()) >= 0 and DATEDIFF(dy,EndTime,getdate()) <=0 and FoodState = 1
   inner join Data_Food_Detial c on a.FoodID = c.FoodID and c.Status = 1
   where a.FoodType in (select ID from #TempFoodTypeDic)
   group by a.FoodID,a.FoodType   
 ) a 

 declare @FoodDetailCommonQueryType as FoodDetailCommonQueryType;
 insert into @FoodDetailCommonQueryType(FoodID,FoodType)
 select FoodID,FoodType from #TempFoodType

 create table #TempQueryResultMain(FoodID int,FoodName varchar(200),FoodType int,AllowInternet int,AllowPrint int,ForeAuthorize int,FoodPrice decimal(18,2),ImageUrl varchar(200))
 insert into #TempQueryResultMain(FoodID,FoodName,FoodType,AllowInternet,AllowPrint,ForeAuthorize,FoodPrice,ImageUrl)
 exec GetCommonFoodListInfo @FoodDetailCommonQueryType,@CustomerType,@MemberLevelId
 
 create table #TempQueryResultDetail(FoodId int,BalanceType int,TypeName varchar(200),UseCount int)
 insert into #TempQueryResultDetail(FoodId,BalanceType,TypeName,UseCount)
 exec GetCommonFoodBalanceListInfo @FoodDetailCommonQueryType
 
 select * from #TempQueryResultMain
 select * from #TempQueryResultDetail
GO
/****** Object:  StoredProcedure [dbo].[GetFoodInfo]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodInfo](@StoreId varchar(15),@FoodId int,@CustomerType int,@MemberLevelId int,@FoodName varchar(200) output,@FoodType int output,
@FoodSalePrice decimal(18,2) output,@AllowPrint int output,@ForeAuthorize int output,@AllowInternet int output,
@AllowCoin int output,@Coins int output,@AllowPoint int output,@Points int output,@AllowLottery int output,@Lottery int output,@ErrMsg varchar(200) output)
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

  if not exists ( select 0 from Data_FoodInfo where StoreID = @StoreId and FoodID = @FoodId and FoodState = 1 )
    begin
		set @ErrMsg = '套餐不存在(' + cast(@FoodId as varchar) + ')'
		return 0
    end
    
  create table #TmpFoodInfo(FoodId int,FoodName varchar(50),FoodType int,AllowPrint int,RechargeType int,AllowInternet int,ForeAuthorize int,AllowCoin int,Coins int,AllowPoint int,Points int,AllowLottery int,Lottery int,FoodPrice decimal(18,2))
  declare @FoodDetailCommonQueryType [FoodDetailCommonQueryType]
  insert @FoodDetailCommonQueryType(FoodId,FoodType)
  values(@FoodId,null)
  insert #TmpFoodInfo(FoodId,FoodName,FoodType,AllowPrint,RechargeType,AllowInternet,ForeAuthorize,AllowCoin,Coins,AllowPoint,Points,AllowLottery,Lottery,FoodPrice)
  exec GetCommonFoodListInfo @FoodDetailCommonQueryType,@CustomerType,@MemberLevelId,1 
    
  if not exists ( select 0 from #TmpFoodInfo )
    begin
      set @ErrMsg = '套餐无效'
      return 0
    end
    
  select top 1 
  @FoodName = FoodName,@FoodType = FoodType,@FoodSalePrice = FoodPrice,@AllowPrint = AllowPrint,
  @ForeAuthorize = ForeAuthorize,@AllowInternet = AllowInternet,@AllowCoin = AllowCoin,@Coins = Coins,
  @AllowPoint = AllowPoint,@Points = Points,@AllowLottery = AllowLottery,@Lottery = Lottery
  from #TmpFoodInfo    
    
  return 1
GO
/****** Object:  StoredProcedure [dbo].[GetFoodDetail]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetFoodDetail](@StoreId varchar(15),@FoodID int)
as
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
GO
/****** Object:  StoredProcedure [dbo].[GetOrderContainById]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[GetOrderContainById](@StoreId varchar(15),@OrderFlwId int)
as
 declare @CardId int = 0
 declare @MemberLevelID int = 0
 declare @CustomerType int = 0
 
 select a.CardID,a.ID as OrderFlwID,StoreID,PayCount,RealPay,FreePay,a.FoodCount,
 a.GoodCount as DetailGoodsCount,a.CreateTime into #TempOrder 
 from dbo.Flw_Order a where a.StoreID = @StoreId and a.ID = @OrderFlwId
  
 select a.FoodID,DetailGoodsCount as GoodsCount,a.FlowType,a.UseCoin,a.UsePoint,
 a.UseLottery,Deposit,b.ContainID,b.ContainCount,MemberLevelID into #TempDetials from 
 (
	 select OrderFlwID,OrderDetailFlwId,DetailGoodsCount,FoodFlwID,FoodID,b.FlowType,
	 b.UseCoin,b.UsePoint,b.UseLottery,Deposit,MemberLevelID,BuyFoodType as SaleCoinsType from 
	 (
		 select b.OrderFlwID,b.ID as OrderDetailFlwId,b.GoodsCount as DetailGoodsCount,FoodFlwID
		 from dbo.Flw_Order a inner join dbo.Flw_Order_Detail b on a.ID = b.OrderFlwID
		 where a.ID = @OrderFlwId
	 ) a
	 inner join Flw_Food_Sale b on a.FoodFlwID = b.ID
 ) a
 inner join Flw_Food_SaleDetail b on a.FoodFlwID = b.FlwFoodID
  
 select top 1 @MemberLevelID = MemberLevelID from #TempDetials
 select top 1 @CardId = CardId from #TempOrder
 print @CardId
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

 declare @FoodDetailCommonQueryType as FoodDetailCommonQueryType
 insert into @FoodDetailCommonQueryType(FoodID,FoodType)
 select FoodID,FlowType as FoodType from #TempDetials
 
 create table #TempQueryResult(FoodID int,FoodName varchar(200),FoodType int,AllowPrint int,RechargeType int,AllowCoin int,
 Coins int,AllowPoint int,Points int,AllowLottery int,Lottery int,ImageUrl varchar(200),
 FoodPrice decimal(18,2),ContainCount int,ContainName varchar(200),DetailsCount int)
 insert into #TempQueryResult(FoodID,FoodName,FoodType,AllowPrint,RechargeType,AllowCoin,Coins,AllowPoint,Points,AllowLottery,Lottery,ImageUrl,FoodPrice,ContainCount,ContainName,DetailsCount)
 exec GetCommonFoodListInfo @FoodDetailCommonQueryType,@CustomerType,@MemberLevelId
 
 select a.StoreID,(select StoreName from Base_StoreInfo where StoreID = a.StoreID) as StoreName,
 (select ICCardID from Data_Member_Card where ID = a.CardID) as ICCardId,PayCount,RealPay,FreePay,FoodCount,DetailGoodsCount,
 @CustomerType as CustomerType,@MemberLevelId as MemberLevelId,(select MemberLevelName from Data_MemberLevel where MemberLevelID = @MemberLevelId) as MemberLevelName,CreateTime from #TempOrder a
 select b.*,a.GoodsCount,UseCoin,UsePoint,UseLottery,Deposit from #TempDetials a inner join #TempQueryResult b on a.FoodID = b.FoodID
GO
/****** Object:  StoredProcedure [dbo].[CheckStoreCanOpenCard]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Proc [dbo].[CheckStoreCanOpenCard](@StoreId varchar(15),@Mobile varchar(11),@ErrMsg varchar(200) output)
as
 --验证当前门店是否开通过会员卡(已注销的卡不计入)
 if dbo.CheckMobile(@Mobile) <> 1
   begin
     set @ErrMsg = '开通会员的手机号码无效'
	 return 0
   end
 
 if  exists ( select 0 from Data_Member_Card a inner join Base_MemberInfo b on a.MemberID = b.ID inner join Data_Member_Card_Store c on a.ID = c.CardID where c.StoreID = @StoreId and b.Mobile = @Mobile and a.CardStatus <> 0)
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
 return 1
GO
/****** Object:  StoredProcedure [dbo].[AddMemberBanlance]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddMemberBanlance](
@StoreId varchar(15),
@MemberId int,
@BalanceType int,--0 代币,1 积分,2 彩票,3 储值金
@Balance decimal(18,2))
as
  declare @CardBalanceId int
  --获取同规则门店
  create table #TmpStore(StoreId varchar(15))
  insert #TmpStore(StoreId)
  exec [GetSameGroupRuleTypeStoreInfo] @StoreId,@BalanceType  
  --添加卡余额信息
  insert Data_Card_Balance(MemberID,BalanceType,Banlance,UpdateTime)
  values(@MemberId,@BalanceType,@Balance,GETDATE()) 
  select @CardBalanceId = @@IDENTITY from Data_Card_Balance  
  --添加门店会员卡列表
  insert Data_Card_Balance_StoreList(CardBalanceID,StoreID)
  select @CardBalanceId,StoreId from #TmpStore
GO
/****** Object:  StoredProcedure [dbo].[CreateOrderTicket]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[AddOrder]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[AddOrder](
@StoreID varchar(15),@CardID int,@MemberLevelId int,@SaleCoinType int,@OrderSource int,
@FoodId int,@PayCount int,@RealPay decimal(18,2),@FreePay decimal(18,2),@UserID int,
@CurrentSchedule int,@WorkStation varchar(50),@AuthorID int,@OrderStatus int,
@Note varchar(200),@PayFee decimal(18,2),@PayTime datetime,@ModifyTime datetime,
@MemberCoins int,@MemberPoints int,@MemberLotterys int,@IsThirdPay bit,
@CheckFoodDetail [CheckFoodDetailType] readonly,@OrderFlwID int output,@ErrMsg varchar(200) output)
as
   --插入订单表
    declare @DetailId int
    declare @DetailFoodType int
    declare @NewId varchar(200)
	declare @OrderID varchar(20)
	declare @Emp_FoodDetailId varchar(50)
	declare @CurrentDatetime datetime
	declare @FoodFlwID int
	declare @GetPoint int
	declare @OrderGoodCount int
	declare @OrderFoodCount int
	declare @OrderGetPoint int
	declare @OrderDetailFoodGoodCount int
	declare @OrderDetailCount int
	declare @PayType int
	declare @PayNum decimal(18,2)
	declare @Deposit decimal(18,2)
	declare @FoodType int
	
	declare @WeightType int
	declare @WeightValue decimal(18,2)
	declare @ContainID int
	declare @ContainCount int
	declare @Days int
	declare @ValidType int
	
	declare @FoodSaleTotalMoney decimal(18,2)	
	declare @UseCoin int	
	declare @UsePoint int	
	declare @UseLottery int
	
    set @OrderStatus = 1
	set @PayTime = GETDATE()
	set @ModifyTime = GETDATE()
	select @OrderFoodCount = SUM(FoodCount) from @CheckFoodDetail
	set @CurrentDatetime = GETDATE()
	exec GetOrderDateFormatStr @CurrentDatetime,@OrderID output
	set @OrderID =  @OrderID + @StoreID
	
	--获取商品总数
	select @OrderGoodCount = sum(FoodContainCount * b.FoodCount) from 
	(
		select a.FoodID,SUM(b.ContainCount) as FoodContainCount from Data_FoodInfo a inner join Data_Food_Detial b on a.FoodID = b.FoodID inner join @CheckFoodDetail c on a.FoodID = c.FoodId where a.FoodState = 1 and b.Status = 1
		group by a.FoodID
	) a
	inner join @CheckFoodDetail b
	on a.FoodID = b.FoodId

	--添加主订单信息
	INSERT INTO Flw_Order(StoreID,OrderID,FoodCount,GoodCount,CardID,OrderSource,CreateTime,PayType,PayCount,RealPay,FreePay,UserID,ScheduleID,WorkStation,AuthorID,OrderStatus,SettleFlag,Note,PayFee,PayTime,ModifyTime)
	VALUES (@StoreID,@OrderID,@OrderFoodCount,@OrderGoodCount,@CardID,@OrderSource,GETDATE(),0,@PayCount,@RealPay,@FreePay,@UserID,@CurrentSchedule,@WorkStation,@AuthorID,@OrderStatus,0,@Note,@PayFee,@PayTime,@ModifyTime)
	select @OrderFlwID = @@IDENTITY from Flw_Order

	--历便套餐明细,保存数据
	declare cur2 cursor for
	select Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType from @CheckFoodDetail
	open cur2
	fetch next from cur2 into @Emp_FoodDetailId,@FoodId,@OrderDetailCount,@PayType,@PayNum,@Deposit,@FoodType
	WHILE @@FETCH_STATUS = 0
	  begin
	    set @OrderGetPoint = 0
	    set @FoodSaleTotalMoney = 0
	    set @UseCoin = 0
	    set @UsePoint = 0
	    set @UseLottery = 0
		--获取套餐商品总数
		set @OrderDetailFoodGoodCount = ( select SUM(ContainCount) from Data_FoodInfo a inner join Data_Food_Detial b on a.FoodID = b.FoodID where a.FoodID = @FoodId and b.Status = 1 ) * @OrderDetailCount
		--获取代币积分数
		exec GetFoodPoint @StoreID,@FoodId,@GetPoint output
		set @OrderGetPoint = @OrderGetPoint + @GetPoint			

		--添加套餐销售明细		
		--现金模式
		if @PayType = 0 or @PayType = 1 or @PayType = 2 or @PayType = 3
		  begin
			set @FoodSaleTotalMoney = @PayNum
		  end
		else if @PayType = 5
		  begin
			set @UseCoin = @PayNum
		  end
		else if @PayType = 6
		  begin
			set @UsePoint = @PayNum
		  end
		else if @PayType = 7
		  begin
			set @UseLottery = @PayNum
		  end
		  				
		--添加商品销售主表和明细表
		insert into Flw_Food_Sale(StoreID,FlowType,FoodID,SaleCount,Point,UseCoin,CoinBalance,UsePoint,PointBalance,UseLottery,LotteryBalance,MemberLevelID,Deposit,OpenFee,RenewFee,ChangeFee,TotalMoney,Note,BuyFoodType)
		values(@StoreID,@FoodType,@FoodId,@OrderDetailCount,@GetPoint,@UseCoin,0,@UsePoint,0,@UseLottery,0,@MemberLevelId,@Deposit,0,0,0,@FoodSaleTotalMoney,@Note,@SaleCoinType)
		
		--添加商品销售信息
		select @FoodFlwID = @@IDENTITY from Flw_Food_Sale	
		insert into Flw_Food_SaleDetail(FlwFoodID,SaleType,ContainID,ContainCount,ExpireDay,ValidType,Status)
		select @FoodFlwID as FlwFoodID,b.FoodType,b.ContainID,ContainCount,GETDATE(),0,b.Status from Data_FoodInfo a 
		inner join Data_Food_Detial b on a.FoodID = b.FoodID where a.FoodID = @FoodId and b.Status = 1
								
		--插入订单明细
		insert into Flw_Order_Detail(OrderFlwID,FoodFlwID,GoodsCount)
		values(@OrderFlwID,@FoodFlwID,@OrderDetailFoodGoodCount)
					
		fetch next from cur2 into @Emp_FoodDetailId,@FoodId,@OrderDetailCount,@PayType,@PayNum,@Deposit,@FoodType

	  end
	close cur2  
	deallocate cur2
	


    return 1
GO
/****** Object:  StoredProcedure [dbo].[AddMemberCard]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[CheckOrderPay]    Script Date: 04/23/2018 19:35:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[CheckOrderPay](@StoreId varchar(15),@FoodId int,@CustomerType int,@MemberLevelId int,@MemberCoins int,@MemberPoints int,@MemberLotterys int,@PayCount decimal(18,2),@RealPay decimal(18,2),@FreePay decimal(18,2),@CheckFoodDetail [CheckFoodDetailType] readonly,@IsThirdPay bit output,@ErrMsg varchar(200) output)
as
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
	declare @TmpFoodSalePrice decimal(18,6) = 0--累计购物车明细中的价格
	declare @FoodSalePrice decimal(18,2)
	declare @Emp_FoodDetailId varchar(50)
	declare @OrderDetailCount int
	declare @PayType int
	declare @PayNum decimal(18,2)
	declare @Deposit decimal(18,2)
	declare @OpenCardFoodTypeNum int = 0--开卡套餐类型数量
	declare @TmpSumCoins int = 0--代币总额
	declare @TmpSumPoints int = 0--积分总额
	declare @TmpSumLotterys int = 0--彩票总额
	set @IsThirdPay = 0
	
    --表值变量数据写入临时表
	create table #Emp_FoodDetail(Emp_FoodDetailId varchar(50),FoodId int,FoodCount int,PayType int,PayNum decimal(18,2),Deposit decimal(18,2),FoodType int)
    insert #Emp_FoodDetail(Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType)
    select Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType from @CheckFoodDetail

    --历便套餐明细,验证支付明细数据
    declare @GetFoodInfoReturn int
	declare cur1 cursor for
	select Emp_FoodDetailId,FoodId,FoodCount,PayType,PayNum,Deposit,FoodType from #Emp_FoodDetail
	open cur1  
	fetch next from cur1 into @Emp_FoodDetailId,@FoodId,@OrderDetailCount,@PayType,@PayNum,@Deposit,@FoodType
	WHILE @@FETCH_STATUS = 0
	  begin	    
	    --获取套餐信息
	    exec @GetFoodInfoReturn = GetFoodInfo @StoreId,@FoodId,@CustomerType,@MemberLevelId,@FoodName output,@FoodType output,@FoodSalePrice output,@AllowPrint output,@ForeAuthorize output,@AllowInternet output,
		@AllowCoin output,@Coins output,@AllowPoint output,@Points output,@AllowLottery output,@Lotterys output,@ErrMsg output
	    if @GetFoodInfoReturn = 0 
	      return 0
	         
	    --如果是入会套餐，验证是否开卡订单  
	    if @FoodType = 1 and @CustomerType <> 1
	      begin
	        set @ErrMsg = '用户不能办理开卡套餐'
			return 0
	      end
	      
	    if @FoodType = 1 
	      begin
	        --更新临时表字段（押金和套餐类型）
			update #Emp_FoodDetail set Deposit = @FoodSalePrice,FoodType = @FoodType where Emp_FoodDetailId = @Emp_FoodDetailId	        
			set @OpenCardFoodTypeNum = @OpenCardFoodTypeNum + @PayNum
			if @OpenCardFoodTypeNum > 1
			  begin
				set @ErrMsg = '开卡套餐数量不能大于1'
				return 0
			  end
	      end   
	    else 
	      begin
			--更新临时表字段（押金和套餐类型）
			update #Emp_FoodDetail set FoodType = @FoodType where Emp_FoodDetailId = @Emp_FoodDetailId
	      end
		--0 现金,1 微信,2 支付宝,3 银联,4 储值金,5 代币,6 彩票,7 积分
		if @PayType = 1 or @PayType = 2 or @PayType = 3 
		  begin
		    --如果存在第三方支付，订单支付状态设置为1(0-未结算,1-等待支付,2-已支付,3-支付异常)
			set @IsThirdPay = 1
		  end
	    --现金模式
	    if @PayType = 0 or @PayType = 1 or @PayType = 2 or @PayType = 3
		  begin
			if @FoodSalePrice * @OrderDetailCount <> @PayNum
		      begin
		        set @ErrMsg = '(' + @FoodName + '支付金额不正确' + ')' + CAST(@FoodSalePrice as varchar) + '-' + CAST(@PayNum as varchar)
				return 0						
		      end
		    else 
			  begin
				set @TmpFoodSalePrice = @TmpFoodSalePrice + @PayNum
			  end 
		  end
	    --代币模式
	    else if @PayType = 5 
	      begin
	        if @AllowCoin = 0 
			  begin
				set @ErrMsg = @FoodName + '支付方式不正确（不能使用代币）'
				return 0				
			  end
			else if @AllowCoin = 1 
			  begin
			    if @Coins * @OrderDetailCount <> @PayNum
			      begin
					set @ErrMsg = @FoodName + '支付代币数量不正确'
					return 0						
			      end
			    else 
				  begin
					set @TmpSumCoins = @TmpSumCoins + @PayNum
				  end
			  end 
	      end
	    --积分模式
	    else if @PayType = 6
		  begin
			if @AllowPoint = 0 
			  begin
				set @ErrMsg = @FoodName + '支付方式不正确（不能使用积分）'
				return 0				
			  end
			else if @AllowPoint = 1 
			  begin
			    if @Points * @OrderDetailCount <> @PayNum
			      begin
					set @ErrMsg = @FoodName + '支付积分数量不正确'
					return 0						
			      end
			    else 
				  begin
					set @TmpSumCoins = @TmpSumCoins + @PayNum
				  end
			  end
		  end
		--彩票模式
		else if @PayType = 7
		  begin
			if @AllowLottery = 0 
			  begin
				set @ErrMsg = @FoodName + '支付方式不正确（不能使用彩票）'
				return 0				
			  end
			else if @AllowLottery = 1 
			  begin
			    if @Lotterys * @OrderDetailCount <> @PayNum
			      begin
					set @ErrMsg = @FoodName + '支付彩票数量不正确'
					return 0						
			      end
			    else 
				  begin
					set @TmpSumLotterys = @Lotterys + @PayNum
				  end
			  end
		  end	 
		fetch next from cur1 into @Emp_FoodDetailId,@FoodId,@OrderDetailCount,@PayType,@PayNum,@Deposit,@FoodType
	  end
	close cur1 
	deallocate cur1

	if @PayCount <> @TmpFoodSalePrice
	  begin
		set @ErrMsg = '应付金额不正确'
		return 0
	  end
	if @PayCount < @FreePay
	  begin
		set @ErrMsg = '免费金额不正确'
		return 0
	  end
	if @PayCount - @FreePay <> @RealPay
	  begin
		set @ErrMsg = '实付金额不正确'
		return 0
	  end
	  
	--如果是会员，判断余额是否足够  
	if @CustomerType = 2
	  begin	
		if @MemberCoins < @TmpSumCoins
		  begin
			set @ErrMsg = '会员币余额不足'
			return 0
		  end
		if @MemberPoints < @TmpSumPoints
		  begin
			set @ErrMsg = '会员积分余额不足'
			return 0
		  end
		if @MemberLotterys < @TmpSumLotterys
		  begin
			set @ErrMsg = '会员彩票余额不足'
			return 0
		  end
	  end  

	  
	if @CustomerType = 1 and @OpenCardFoodTypeNum = 0
	  begin
	    set @ErrMsg = '开卡会员没有选择入会套餐'
		return 0
	  end  
	  
	return 1
GO
/****** Object:  StoredProcedure [dbo].[UpdateOrder]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[FinishOrderPayment]    Script Date: 04/23/2018 19:35:55 ******/
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
/****** Object:  StoredProcedure [dbo].[CreateOrder]    Script Date: 04/23/2018 19:35:55 ******/
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
@RealPay decimal(18,2),
@FreePay decimal(18,2),
@UserID int,
@WorkStation varchar(50),
@AuthorID int,
@Note VARCHAR(500),
@OrderSource int,
@SaleCoinType int,--(0-手工实物币提币;1-售币机实物币提币)
@OrderFlwID int output,
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
/****** Object:  UserDefinedTableType [dbo].[FoodDetailLevelQueryType]    Script Date: 04/23/2018 19:35:57 ******/
CREATE TYPE [dbo].[FoodDetailLevelQueryType] AS TABLE(
	[FoodId] [int] NULL
)
GO
