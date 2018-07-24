USE [XCCloudRS232]
GO
/****** Object:  StoredProcedure [dbo].[RemoveMerchRouter]    Script Date: 12/14/2017 17:01:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--控制器解除绑定
CREATE PROCEDURE [RemoveMerchRouter]
(
	@RouterId INT
)
AS
BEGIN TRAN T

	SET NOCOUNT ON 
	SET XACT_ABORT ON 
	                 
	--删除商户与当前控制中的分组关联
	DELETE Data_MerchSegment
	WHERE ParentID = @RouterId;
	
	--删除当前控制器中的分组
	DELETE Data_GameInfo
	WHERE DeviceID = @RouterId;
	
	--删除当前控制器中的外设
	DELETE Data_MerchDevice
	WHERE ParentID = @RouterId;
	
	--重置控制器绑定商户
	UPDATE Base_DeviceInfo
	SET MerchID = 0, [Status] = 0
	WHERE ID = @RouterId;
		
			
	IF (@@ERROR<>0)
	BEGIN
		ROLLBACK TRAN T
		RETURN 0
	END
	ELSE
	BEGIN
		COMMIT TRAN T
		RETURN 1
	END
GO

/****** Object:  StoredProcedure [dbo].[UpdateMemberBalance]    Script Date: 10/31/2017 17:05:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--替换设备
CREATE PROCEDURE [ReplaceDevice]
(
	@oldDeviceId INT, --旧设备令牌
	@newDeviceId INT, --新设备令牌
	@merchId INT,  --商户ID
	@category INT --设备类别 1：控制器 2：外设 3：终端
)
AS
BEGIN TRAN T

	SET NOCOUNT ON 
	SET XACT_ABORT ON 
	                 
	----删除商户与当前控制中的分组关联
	--DELETE Data_MerchSegment
	--WHERE ParentID = @RouterId;
	
	----删除当前控制器中的分组
	--DELETE Data_GameInfo
	--WHERE DeviceID = @RouterId;
	
	----删除当前控制器中的外设
	--DELETE Data_MerchDevice
	--WHERE ParentID = @RouterId;
	
	--重置旧设备绑定的商户为空,状态修改为未激活
	UPDATE Base_DeviceInfo
	SET MerchID = 0, [Status] = 0
	WHERE ID = @oldDeviceId;
	
	--将新设备与当前商户绑定，状态改为激活
	UPDATE Base_DeviceInfo
	SET MerchID = @merchId, [Status] = 1
	WHERE ID = @newDeviceId;
	
	IF(@category = 1)
	BEGIN
		--将旧控制器的分组与新设备关联
		UPDATE Data_GameInfo
		SET DeviceID = @newDeviceId
		WHERE DeviceID = @oldDeviceId
		
		--将终端与新控制器关联
		UPDATE Data_MerchSegment
		SET ParentID = @newDeviceId
		WHERE ParentID = @oldDeviceId
		
		--将外设与新控制器关联
		UPDATE Data_MerchDevice
		SET ParentID = @newDeviceId
		WHERE ParentID = @oldDeviceId
	END
	ELSE IF(@category = 2)
	BEGIN
		--将旧外设替换为新外设
		UPDATE Data_MerchDevice
		SET DeviceID = @newDeviceId
		WHERE DeviceID = @oldDeviceId
	END
	ELSE IF (@category = 3)
	BEGIN
		--将终端设备替换为新终端设备
		UPDATE Data_MerchSegment
		SET DeviceID = @newDeviceId
		WHERE DeviceID = @oldDeviceId
	END
			
	IF (@@ERROR<>0)
	BEGIN
		ROLLBACK TRAN T
		RETURN 0
	END
	ELSE
	BEGIN
		COMMIT TRAN T
		RETURN 1
	END
GO



USE [XCCloud]
GO
/****** Object:  StoredProcedure [dbo].[GetMemberBalanceAndExchangeRate]    Script Date: 05/31/2018 17:57:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--查询会员余额及兑换比例
ALTER PROC [dbo].[GetMemberBalanceAndExchangeRate]
(
	@ICCardID VARCHAR(20),
	@StoreID VARCHAR(15)
)
AS
DECLARE @MerchID VARCHAR(15)
DECLARE @MemberID VARCHAR(32)
DECLARE @MemberLevelID INT

SELECT @MerchID=MerchID, @MemberID=MemberID, @MemberLevelID=MemberLevelID FROM Data_Member_Card A
INNER JOIN Data_Member_Card_Store B ON B.CardID = A.ID AND B.StoreID = @StoreID
WHERE ICCardID = @ICCardID;

WITH MemberBalance
AS
(
	SELECT A.MerchID, (ISNULL(A.Balance, 0) + ISNULL(B.Balance, 0)) AS Total, A.MemberID, A.BalanceIndex, A.Balance, B.Balance AS BalanceFree FROM Data_Card_Balance A
	INNER JOIN Data_Card_Balance_StoreList S ON S.CardBalanceID = A.ID AND S.StoreID = @StoreID
	LEFT JOIN Data_Card_Balance_Free B ON B.MemberID = A.MemberID AND B.MerchID = A.MerchID AND B.BalanceIndex = A.BalanceIndex
	WHERE A.MemberID = @MemberID
),
MemberLevelBalanceCharge
AS
(
	SELECT A.MerchID, A.SourceBalanceIndex, A.SourceCount, A.TargetBalanceIndex, A.TargetCount, B.TypeName FROM Data_MemberLevel_BalanceCharge A
	INNER JOIN Dict_BalanceType B ON B.ID = A.TargetBalanceIndex AND B.MappingType = 4
	WHERE A.MerchID = @MerchID AND A.MemberLevelID = @MemberLevelID
),
BalanceType
AS
(
	SELECT ID, TypeName, MappingType, DecimalNumber, AddingType FROM Dict_BalanceType
	WHERE MerchID = @MerchID AND [State] = 1
),
Result
AS
(
	SELECT C.ID AS BalanceIndex, C.TypeName, Total, A.Balance, A.BalanceFree, D.DecimalNumber, D.AddingType, 
	--特殊处理储值金兑换储值金的比列，条件：映射类型为储值金，并且原类型和目标类型都为空
	CASE WHEN C.MappingType = 4 AND SourceBalanceIndex IS NULL AND TargetBalanceIndex IS NULL THEN C.ID ELSE B.TargetBalanceIndex END AS TargetBalanceIndex,
	CASE WHEN C.MappingType = 4 AND SourceBalanceIndex IS NULL AND TargetBalanceIndex IS NULL THEN C.TypeName ELSE D.TypeName END AS TargetTypeName,
	CASE WHEN C.MappingType = 4 AND SourceBalanceIndex IS NULL AND TargetBalanceIndex IS NULL THEN 1 ELSE B.SourceCount END AS SourceCount,
	CASE WHEN C.MappingType = 4 AND SourceBalanceIndex IS NULL AND TargetBalanceIndex IS NULL THEN 1 ELSE B.TargetCount END AS TargetCount 
	FROM MemberBalance A
	LEFT JOIN MemberLevelBalanceCharge B ON B.SourceBalanceIndex = A.BalanceIndex
	LEFT JOIN BalanceType C ON C.ID = A.BalanceIndex
	LEFT JOIN BalanceType D ON D.ID = B.TargetBalanceIndex
	WHERE A.MemberID = @MemberID AND A.MerchID = @MerchID
)
SELECT BalanceIndex, TypeName, Total, Balance, BalanceFree, TargetBalanceIndex, TargetTypeName, DecimalNumber, AddingType, --SourceCount, TargetCount,
CASE WHEN SourceCount IS NULL AND TargetCount IS NULL THEN 0 ELSE ROUND(CAST(TargetCount AS FLOAT) / SourceCount, 2) END AS ExchangeRate
--CASE WHEN SourceCount IS NULL AND TargetCount IS NULL THEN 0 ELSE 1 END AS IsExchange 
FROM Result




--获取当前会员等级可用套餐
USE [XCCloud]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetFoodInfoList]    Script Date: 07/23/2018 15:39:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROC [dbo].[sp_GetFoodInfoList]
(
	@merchId VARCHAR(15),
	@storeId VARCHAR(15),
	@memberlevelId INT
)
AS 
DECLARE @currDate DATETIME

SET @currDate = GETDATE()

-- 套餐级别销售记录表		
DECLARE @tmpFoodRecord TABLE (
	MerchID VARCHAR(32),
	FoodID INT,
	FoodLevelID INT,
	RecordDate DATETIME,
	day_sale_count INT,
	member_day_sale_count INT
)

INSERT INTO @tmpFoodRecord(MerchID, FoodID, FoodLevelID, RecordDate, day_sale_count, member_day_sale_count)
(
	SELECT MerchID, FoodID, FoodLevelID, RecordDate, day_sale_count, member_day_sale_count FROM Data_Food_Record 
	WHERE DATEDIFF(DAY, @currDate, RecordDate) = 0
)

-- 套餐对应会员级别		
DECLARE @tmpFoodLevel TABLE (
	Id INT IDENTITY(1,1),
	LevelID INT,
	MerchID VARCHAR(32),
	FoodID INT,
	MemberLevelID INT,
	VIPPrice NUMERIC(10,2),
	ClientPrice NUMERIC(10,2),
	AllCount INT,
	MemberCount INT,
	PriorityLevel INT
)

INSERT INTO @tmpFoodLevel
SELECT ID, MerchID, FoodID, MemberLevelID, VIPPrice, ClientPrice, AllCount, MemberCount, PriorityLevel FROM Data_Food_Level 
WHERE MerchID = @merchId AND MemberLevelID = @memberlevelId
	AND @currDate BETWEEN StartDate AND EndDate 
	AND CONVERT(varchar(100), @currDate, 108) BETWEEN StartTime AND EndTime	--判断日期及时间是否可用
	AND (
		(TimeType=0 AND CHARINDEX(CAST(DATEPART(WEEKDAY, CONVERT(DATETIME, GETDATE())) AS VARCHAR(1)),week)>0)	--自定义方法是否包含当前周，周一1 周日7
		OR (TimeType=1 AND (SELECT COUNT(DayType) n from XC_HolidayList WHERE DATEDIFF(DAY, @currDate, WorkDay)=0 AND DayType=0)>0) --工作日方法判断当前是否为工作日
		OR (TimeType=2 AND (SELECT COUNT(DayType) n from XC_HolidayList where DATEDIFF(DAY, @currDate, WorkDay)=0 AND DayType=1)>0) --周末方式判断当前是否为周末
		OR (TimeType=3 AND (SELECT COUNT(DayType) n from XC_HolidayList where DATEDIFF(DAY, @currDate, WorkDay)=0 AND DayType=2)>0) --周末方式判断当前是否为周末
		)

DECLARE @i INT
DECLARE @count INT
DECLARE @now DATETIME

SET @i = 1
SELECT @count = COUNT(*) FROM @tmpFoodLevel
SET @now = GETDATE()

WHILE(@i<=@count)
BEGIN
	DECLARE @FoodLevelID INT		
	SELECT @FoodLevelID = LevelID FROM @tmpFoodLevel WHERE Id = @i
	IF NOT EXISTS( SELECT 1 FROM @tmpFoodRecord WHERE FoodLevelID = @FoodLevelID )
	BEGIN
		INSERT INTO @tmpFoodRecord(MerchID, FoodID, FoodLevelID, RecordDate, day_sale_count, member_day_sale_count)
		(
			SELECT MerchID, FoodID, LevelID, CONVERT(varchar(100), @currDate, 23), 0, 0 FROM @tmpFoodLevel 
			WHERE Id = @i
		)
	END
	SET @i = @i+1
END

SELECT A.ID AS FoodId, FoodName, Note, ImageURL, FoodType, 
CASE ISNULL(VIPPrice, 0) WHEN 0 THEN MemberPrice ELSE VIPPrice END AS Price
FROM Data_FoodInfo A
INNER JOIN Data_Food_StoreList B ON B.FoodID = A.ID
LEFT JOIN (
	SELECT LevelID, FoodID, MemberLevelID, VIPPrice FROM (
		SELECT  FL.LevelID, FL.FoodID, MemberLevelID, VIPPrice, ROW_NUMBER() OVER(PARTITION BY FL.FoodID ORDER BY PriorityLevel DESC) rn FROM @tmpFoodLevel FL
		INNER JOIN @tmpFoodRecord FR ON FR.FoodLevelID = FL.LevelID
		WHERE FR.member_day_sale_count < FL.MemberCount
	) AS FoodLevel 
	WHERE rn = 1
) C ON C.FoodID = A.ID
WHERE AllowInternet = 1 AND FoodState = 1 AND StartTime <= @now AND EndTime >= @now AND (ForbidStart > @now OR ForbidEnd < @now) AND B.StoreID = @storeId
AND @currDate BETWEEN StartTime AND EndTime
AND @currDate NOT BETWEEN ForbidStart AND ForbidEnd








