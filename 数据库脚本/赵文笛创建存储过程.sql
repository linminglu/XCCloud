use xcgamedb000002
go
--���
CREATE Proc [dbo].[UpdateMemberBalance](@Balance varchar(200),@ICCardID varchar(200),@Segment varchar(200),@HeadAddress varchar(200),@Coins varchar(200),@Return int output)
as
 begin transaction tran1
 begin try
	update t_member set Balance=@Balance where ICCardID=@ICCardID
	insert into flw_485_coin (Segment,HeadAddress,ICCardID,Coins,CoinType,Balance,RealTime)
	values(@Segment,@HeadAddress,@ICCardID,@Coins,0,@Balance,GETDATE())
	commit transaction tran1
	set @Return = 1
	select * from t_member where ICCardID = @ICCardID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch

GO

--����
CREATE Proc [dbo].[InsertFood](
@Balance varchar(200),
@ICCardID varchar(200),
@FoodID varchar(200),
@CoinQuantity varchar(200),
@Point varchar(200),
@MemberLevelID varchar(200),
@UserID varchar(200),
@ScheduleID varchar(200),
@WorkStation varchar(200),
@MacAddress varchar(200),
@OrderID varchar(200),
@FoodName varchar(200),
@Money varchar(200),
@Paymentype varchar(50),
@Return int output)
as
 begin transaction tran1
 begin try	
 declare @food_sale_ID int = 0;
	insert into flw_food_sale (FlowType,ICCardID,FoodID,CoinQuantity,Point,Balance,MemberLevelID,PayType,BuyFoodType,UserID,ScheduleID,RealTime,WorkStation,MacAddress,DiskID,OrderID,Note)
	values(1,@ICCardID,@FoodID,@CoinQuantity,@Point,@Balance,@MemberLevelID,1,3,@UserID,@ScheduleID,GETDATE(),@WorkStation,@MacAddress,'Զ�����',@OrderID,@Paymentype)
	select @food_sale_ID=@@IDENTITY from flw_food_sale
		
	insert into flw_cash(PayType,GoodType,GoodName,[Money],RealTime,WorkStation,CheckOut,ScheduleID,FlwID,FlwType,OrderID)
	values(@Paymentype,'�۱�',@FoodName,@Money,GETDATE(),@WorkStation,0,@ScheduleID,@food_sale_ID,'food_sale',@OrderID)
	insert into flw_coin_sale(ICCardID,WorkStation,Coins,Balance,WorkType,ScheduleID,RealTime,UserID,IsBirthday,MacAddress,DiskID)
	values(@ICCardID,@WorkStation,@CoinQuantity,@Balance,7,@ScheduleID,GETDATE(),@UserID,0,@MacAddress,'Զ�����')
	commit transaction tran1
	set @Return = 1
	select * from flw_food_sale where ICCardID = @ICCardID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO

use XCGameManagerDB
go
---�����Ŵ洢����
CREATE Proc [dbo].[InsertMPOrder](@CreateTime varchar(200),@OrderNumber varchar(200),@StoreId varchar(200),@Return int output)
as
 begin transaction tran1
 begin try
  declare @MPordercount int = 0;
  select @MPordercount=COUNT(*) from t_MPorder where CreateTime=@CreateTime and StoreId=@StoreId
 if(@MPordercount=0)
  begin
  	insert into t_MPorder (OrderNumber,CreateTime,StoreId)
	values(@OrderNumber,@CreateTime,@StoreId)
	end
	else
	begin	
	update t_MPorder set OrderNumber=@OrderNumber where CreateTime=@CreateTime and  StoreId=@StoreId
	end
	commit transaction tran1
	set @Return = 1
	select * from t_MPorder where CreateTime=@CreateTime and StoreId=@StoreId
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO

--��Ϣ��Ӵ洢����
CREATE Proc [dbo].[InsertTCP](
@ManageType varchar(2),
@SendStoreId varchar(20),
@SendMobile varchar(15),
@ReceiveStoreId varchar(20),
@ReceiveMobile varchar(15),
@TextData varchar(max),
@State varchar(20),
@Return int output)
as
 begin transaction tran1
 begin try
  declare @TCP_ID int = 0;
	insert into t_TCP (ManageType,SendStoreId,SendMobile,ReceiveStoreId,ReceiveMobile,TextData,[State],CreateTime)
	values(@ManageType,@SendStoreId,@SendMobile,@ReceiveStoreId,@ReceiveMobile,@TextData,@State,GETDATE())
	select @TCP_ID=@@IDENTITY from t_TCP
	commit transaction tran1
	set @Return = 1
	select * from t_TCP where ID = @TCP_ID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO
use XCGameManagerDB
go
--�û�ע��洢����
CREATE Proc [dbo].[InsertUserRegister](
@UserName varchar(50),
@PassWord varchar(200),
@Mobile varchar(15),
@StoreId varchar(20),
@Return int output)
as
 begin transaction tran1
 begin try
  declare @UserRegister_ID int = 0;
	insert into t_UserRegister (UserName,[PassWord],Mobile,StoreId,CreateTime)
	values(@UserName,@PassWord,@Mobile,@StoreId,GETDATE())
	select @UserRegister_ID=@@IDENTITY from t_UserRegister
	commit transaction tran1
	set @Return = 1
	select * from t_UserRegister where ID = @UserRegister_ID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO


use XCGameManagerDB
go
--�ܱ��ײ���ˮ
CREATE Proc [dbo].[InsertFood](
@Balance varchar(200),
@ICCardID varchar(200),
@FoodID varchar(200),
@CoinQuantity varchar(200),
@Point varchar(200),
@MemberLevelID varchar(200),
@UserID varchar(200),
@ScheduleID varchar(200),
@WorkStation varchar(200),
@MacAddress varchar(200),
@OrderID varchar(200),
@FoodName varchar(200),
@Money varchar(200),
@StoreId varchar(50),
@Return int output)
as
 begin transaction tran1
 begin try	
 declare @food_sale_ID int = 0;
	insert into t_food_sale (FlowType,ICCardID,FoodID,CoinQuantity,Point,Balance,MemberLevelID,PayType,BuyFoodType,UserID,ScheduleID,RealTime,WorkStation,MacAddress,DiskID,OrderID,Note,StoreId)
	values(1,@ICCardID,@FoodID,@CoinQuantity,@Point,@Balance,@MemberLevelID,1,3,@UserID,@ScheduleID,GETDATE(),@WorkStation,@MacAddress,'Զ�����',@OrderID,'΢��',@StoreId)
	select @food_sale_ID=@@IDENTITY from t_food_sale
	commit transaction tran1
	set @Return = 1
	select * from t_food_sale where ID = @food_sale_ID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO



CREATE Proc [dbo].[Selectmembercard](
@Mobile varchar(15))
as
 begin transaction tran1
 begin try
create table  #tmpmember
(
iccardid varchar(50),
memberName varchar(50),
gender varchar(100),
birthday varchar(50),
certificalID varchar(100),
mobile varchar(50),
balance varchar(50),
point varchar(50),
deposit varchar(50),
memberState varchar(50),
lottery varchar(50),
note varchar(50)
)
declare @storeId varchar(20)
declare @iccardId varchar(20)
declare @dbNmae varchar(20)
DECLARE MyCursor CURSOR FOR select storeid,iccardid from t_membercard where Mobile = @Mobile
--��һ���α�    
OPEN MyCursor

--ѭ��һ���α�
FETCH NEXT FROM  MyCursor INTO @storeId,@iccardId
WHILE @@FETCH_STATUS =0
    BEGIN
		--ҵ��
		select @dbNmae = store_dbname from t_store where id=@storeId
		declare @Sql varchar(500)
		set @Sql = ' insert into #tmpmember(iccardid,memberName,gender,birthday,certificalID,mobile,balance,point,deposit,memberState,lottery,note) ' + 
		 ' select  ICCardID,MemberName,Gender,Birthday,CertificalID,Mobile,Balance,Point,Deposit,MemberState,Lottery,Note from '  +  cast(@dbNmae as varchar) + '.dbo.t_member where ICCardID =' +  '''' + @iccardId + ''''
		print @SQl 
		Exec(@Sql)
		--ѭ��һ���α�
        FETCH NEXT FROM  MyCursor INTO @storeId,@iccardId
    END    

--�ر��α�
CLOSE MyCursor
--�ͷ���Դ
DEALLOCATE MyCursor
COMMIT TRAN  
select * from #tmpmember
end try   
begin catch   
    ROLLBACK  
end catch  

--��ֵ�洢����
use xcgamedb000002
go
CREATE Proc [dbo].[RechargeFood](
@Balance varchar(200),
@ICCardID varchar(200),
@FoodID varchar(200),
@CoinQuantity varchar(200),
@Point varchar(200),
@MemberLevelID varchar(200),
@UserID varchar(200),
@ScheduleID varchar(200),
@WorkStation varchar(200),
@MacAddress varchar(200),
@OrderID varchar(200),
@FoodName varchar(200),
@Money varchar(200),
@Paymentype varchar(50),
@Return int output)
as
 begin transaction tran1
 begin try	
 declare @food_sale_ID int = 0;
 update t_member set Balance=@Balance where ICCardID=@ICCardID
	insert into flw_food_sale (FlowType,ICCardID,FoodID,CoinQuantity,Point,Balance,MemberLevelID,PayType,BuyFoodType,UserID,ScheduleID,RealTime,WorkStation,MacAddress,DiskID,OrderID,Note)
	values(1,@ICCardID,@FoodID,@CoinQuantity,@Point,@Balance,@MemberLevelID,1,3,@UserID,@ScheduleID,GETDATE(),@WorkStation,@MacAddress,'Զ�̳�ֵ',@OrderID,@Paymentype)
	select @food_sale_ID=@@IDENTITY from flw_food_sale
		
	insert into flw_cash(PayType,GoodType,GoodName,[Money],RealTime,WorkStation,CheckOut,ScheduleID,FlwID,FlwType,OrderID)
	values(@Paymentype,'��ֵ',@FoodName,@Money,GETDATE(),@WorkStation,0,@ScheduleID,@food_sale_ID,'food_sale',@OrderID)
	commit transaction tran1
	set @Return = 1
	select * from flw_food_sale where ICCardID = @ICCardID
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch

GO

CREATE Proc [dbo].[Selectclient](
@Return int output
)
as
 begin transaction tran1
 begin try	

 	commit transaction tran1
	set @Return = 1
	select * from client 
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch

GO

--��ѯ��Ա��Ϣ
use XCGameManagerDB
go
CREATE Proc [dbo].[SelectMenber](
@Mobile varchar(50),
@Return int output
)
as
 begin transaction tran1
 begin try	

 	commit transaction tran1
	set @Return = 1
	select * from T_MemberToken  where Phone=@Mobile
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO

--��ѯ�ŵ�ź��ŵ�����洢����
CREATE Proc [dbo].[Selectstore]
as
 begin transaction tran1
 begin try	

 	commit transaction tran1
	select ID as StoreID,store_password from t_store 
 end try
 begin catch
	rollback transaction tran1
 end catch
GO


--������ҳ��ѯ
CREATE proc ShowInfo
  @Table nvarchar(50),--��.��ͼ
    @PageSize int,--ÿҳ��ʾ����
    @PageIndex int,--��ǰ��ʾҳ��
      @Conditions nvarchar(300),--ɸѡ����
      @Pages int output--�����ܹ��ж���ҳ
  as
      declare @start int ,--��ǰҳ��ʼ��ʾ��No
     @end int,--��ǰҳ������ʾ��No
     @Context nvarchar(1024), --��̬sql���
     @pkey nvarchar(10)--����������
     set @start=(@PageIndex-1)+1
     set @end=@start+@PageSize-1
     set @pkey=index_col(@Table,1,1)--��ȡ������������
     --ͨ������������Ҫ������ݻ�۵���ʱ��#temp��
     set @Context='select row_number() over(order by '+@pkey+') as [No],OrderID,StoreID,Price,Fee,OrderType,PayStatus,CONVERT(varchar(100), PayTime, 23)as PayTime,CONVERT(varchar(100), CreateTime, 20)as CreateTime,Descript,Mobile,BuyType,StoreName,Coins into #temp  from '+@Table
     --�ж��Ƿ���ɸѡ��������
     if(@Conditions is not null and @Conditions <> '')
         set @Context=@Context+' where '+@Conditions
     --ͨ����ѯ#temp ��ʵ�ַ�ҳ.
     set @Context=@Context+'  select * from #temp where No between '+cast(@start as nvarchar(4))+' and '+cast(@end as nvarchar(4))
  
	 --���س��ܹ����Էֳɶ���ҳ
     set @Context=@Context+'  declare @count int  select @count=count(*) from #temp  set @Pages= @count/'+cast(@PageSize as nvarchar(4))+'  if(@count%'+cast(@PageSize as nvarchar(4))+'<>0) set @Pages=@Pages+1 '
 
     exec sp_executesql @Context,N'@Pages int output', @Pages output
     -- sp_executesql @��̬sql��䣬@��̬sql�������Ҫ�Ĳ�����@���붯̬sql��������ֵ��������⣩
 ---------------------------------------------------------------------------------------------------
go
declare @Pages int 
exec ShowInfo 'Data_Order','20','1','Mobile=15618920033',@Pages output
print @Pages



USE [xcgamedb000027]
GO

CREATE proc [dbo].[GetMemberInformation](@ICCardID varchar(20) )
as
    select ICCardID,MemberName,Gender,convert(char(10),Birthday,120) as Birthday,CertificalID,Mobile,Balance,Point,a.Deposit,MemberState,Lottery,Note,convert(char(10),EndDate,120) as EndDate,MemberLevelName from t_member a left join t_memberlevel b on a.MemberLevelID = b.MemberLevelID where ICCardID = @ICCardID

GO

use XCCloudRS232
go
CREATE Proc [dbo].[deleteDataGameInfo](
@GroupID varchar(2),
@Return int output)
as
 begin transaction tran1
 begin try
 delete  from Data_MerchSegment where GroupID=@GroupID
 delete from Data_GameInfo where GroupID=@GroupID
	commit transaction tran1
	set @Return = 1
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO



use XCCloud
go
--��ȡ��Ա��Ϣ
CREATE Proc [dbo].[selectmember](
@ICCardID varchar(50),
@StoreID varchar(50),
@Return int output)
as
 begin transaction tran1
 begin try
select a.*,ISNULL((select a.Banlance as Storage from Data_Card_Balance as a inner join Data_Card_Balance_StoreList as b on   ICCardID=@ICCardID  and b.StoreID=@StoreID and BalanceType=0 and a.ID=b.CardRightID),0) as Storage,
ISNULL((select a.Banlance  from Data_Card_Balance as a inner join Data_Card_Balance_StoreList as b on   ICCardID=@ICCardID  and b.StoreID=@StoreID and MemberID='1' and BalanceType=1 and a.ID=b.CardRightID),0)as Banlance,
isnull((select a.Banlance as Point from Data_Card_Balance as a inner join Data_Card_Balance_StoreList as b on   ICCardID=@ICCardID and b.StoreID=@StoreID and BalanceType=2 and a.ID=b.CardRightID),0)as Point,
isnull((select a.Banlance as Lottery from Data_Card_Balance as a inner join Data_Card_Balance_StoreList as b on   ICCardID=@ICCardID and b.StoreID=@StoreID and BalanceType=3 and a.ID=b.CardRightID),0)as Lottery 

from (
select a.ICCardID,b.MemberName,b.Gender,b.Birthday,b.IDCard,b.Mobile,c.Deposit,b.Note,c.MemberLevelName,a.EndDate,a.RepeatCode,b.MemberState,e.StoreID,e.StoreName from (
select ID,MemberID,ICCardID,CONVERT(varchar(100), EndDate, 23)EndDate,RepeatCode,MemberLevelID from Data_Member_Card ) a ,
(select UserName as MemberName,Gender,CONVERT(varchar(100), Birthday, 23) Birthday,IDCard,Mobile,Note,ID,MemberState from Base_MemberInfo) b,
(select MemberLevelID,MemberLevelName, Deposit from Data_MemberLevel where [State]='1' )as c,
(select * from Data_Member_Card_Store where StoreID=@StoreID) as d,
(select * from Base_StoreInfo where StoreID=@StoreID) as e
 where a.MemberID=b.ID and a.MemberLevelID=c.MemberLevelID  and a.ICCardID=@ICCardID and a.ID=d.CardIndex
)a
	commit transaction tran1
	set @Return = 1
 end try
 begin catch
	rollback transaction tran1
	set @Return = 0
 end catch
GO