IF EXISTS(SELECT * FROM sysobjects WHERE name='T_MemberToken' AND xtype='U') DROP TABLE T_MemberToken
GO
create table T_MemberToken
(
ID int identity(1,1)primary key ,  --ID
Token varchar(200),			--����
StoreId varchar(200),		--�ŵ�ID
Phone varchar(200) ,
CreateTime datetime default(GetDate()),	--��������
UpdateTime datetime default(GetDate()),	--��������
)

IF EXISTS(SELECT * FROM sysobjects WHERE name='t_MobileToken' AND xtype='U') DROP TABLE t_MobileToken
GO
create table t_MobileToken
(
ID int identity(1,1),  --ID
Token varchar(200),			--����
Phone varchar(200)primary key  ,
CreateTime datetime default(GetDate()),	--��������
UpdateTime datetime default(GetDate()),	--��������
)

IF EXISTS(SELECT * FROM sysobjects WHERE name='t_MPOrder' AND xtype='U') DROP TABLE t_MPOrder
GO
create table t_MPOrder
(
OrderNumber varchar(200),
StoreId varchar(200),
CreateTime varchar(200)primary key,	--��������
)

IF EXISTS(SELECT * FROM sysobjects WHERE name='t_TCP' AND xtype='U') DROP TABLE t_TCP
GO
create table t_TCP
(
ID int identity(1,1) primary key,  --ID
ManageType varchar(2),		--��Ϣ����
SendStoreId varchar(20),		--���ͷ��ŵ��
SendMobile varchar(15),		--���ͷ��ֻ�����
ReceiveStoreId varchar(20),	--���ܷ��ŵ��
ReceiveMobile varchar(15),		--���շ��ֻ�����
TextData varchar(max),			--��Ϣ����
[State] int default(0),			--״ֵ̬
CreateTime datetime default(GetDate()),--����ʱ��
)
use XCGameManagerDB
go

IF EXISTS(SELECT * FROM sysobjects WHERE name='t_UserRegister' AND xtype='U') DROP TABLE t_UserRegister
GO
create table t_UserRegister
(
ID int identity(1,1) primary key,  --ID
UserName	varchar(50),			--�û��˺�
[PassWord] varchar(200),			--�û�����
Mobile varchar(15),--�ֻ�����
StoreId	varchar(20),--�ŵ�ID
[State] int default(0),--0����δ����
CreateTime datetime default(GetDate()),--����ʱ��
AuditingTime datetime,					--���ʱ��
AuditingUser varchar(50)				--�����
)
use XCGameManagerDB
go
IF EXISTS(SELECT * FROM sysobjects WHERE name='[t_food_sale]' AND xtype='U') DROP TABLE [t_food_sale]
GO
CREATE TABLE [dbo].[t_food_sale](
	[ID] [int] IDENTITY(1,1) NOT NULL primary key,  --ID,
	[FlowType] [varchar](1) NULL,
	[ICCardID] [int] NULL,
	[FoodID] [int] NULL,
	[CoinQuantity] [int] NULL,
	[Point] [int] NULL,
	[Balance] [int] NULL,
	[MemberLevelID] [int] NULL,
	[Deposit] [decimal](18, 2) NULL,
	[OpenFee] [decimal](18, 2) NULL,
	[RenewFee] [decimal](18, 2) NULL,
	[ChangeFee] [decimal](18, 2) NULL,
	[CreditFee] [decimal](18, 2) NULL,
	[TotalMoney] [decimal](18, 2) NULL,
	[Note] [varchar](500) NULL,
	[PayType] [varchar](1) NULL,
	[BuyFoodType] [varchar](1) NULL,
	[UserID] [int] NULL,
	[ScheduleID] [int] NULL,
	[AuthorID] [int] NULL,
	[RealTime] [datetime] NULL,
	[WorkStation] [varchar](50) NULL,
	[MacAddress] [varchar](50) NULL,
	[DiskID] [varchar](50) NULL,
	[ExitRealTime] [datetime] NULL,
	[ExitBalance] [int] NULL,
	[ExitUserID] [int] NULL,
	[ExitScheduleID] [int] NULL,
	[ExitWorkStation] [varchar](50) NULL,
	[OrderID] [varchar](32) NULL,
	[StoreId] varchar(50)
)

IF EXISTS(SELECT * FROM sysobjects WHERE name='[t_membercard]' AND xtype='U') DROP TABLE [t_membercard]
GO
CREATE TABLE [dbo].[t_membercard](
	ID [int] IDENTITY(1,1),  --ID,
	ICCardID  varchar(50),
	StoreId  varchar(50),
	Mobile varchar(15),
	Createtime datetime default(Getdate())
)


use XCGameManagerDB
go
--����
IF EXISTS(SELECT * FROM sysobjects WHERE name='t_promotion' AND xtype='U') DROP TABLE t_promotion
GO
CREATE TABLE t_promotion
(
	[ID] [int] IDENTITY(1,1) NOT NULL primary key,  --ID,
	StoreName	varchar(100),						--�����
	StoreID varchar(50),							--�ŵ�ID
	[Time] datetime,								--�ʱ��
	ReleaseTime datetime,						--����ʱ��
	PicturePath varchar(200),						--ͼƬ·��
	Title varchar(100),								--����
	PagePath varchar(Max),							--��ת·��
	Publisher varchar(50),							--������
	[State] int default(1),							--1��ʾ0����ʾ
	PublishType int ,						--0�ֲ�1�б�
	PromotionType int						--0��ҵ����,1��ʱ������2Ԥ�ۿ��ţ�4 ��ɱ
)
IF EXISTS(SELECT * FROM sysobjects WHERE name='t_store_dog' AND xtype='U') DROP TABLE t_store_dog
GO
CREATE TABLE t_store_dog
(
Id [int] IDENTITY(1,1) NOT NULL primary key,  --ID,
StoreId varchar(10),
DogId varchar(50),
)