--�������
declare @BusinessDate varchar(10)
set  @BusinessDate = '2017-09-20 10:17:00'
select DestroyTime as StorageTime,isnull(StoreName,a.StoreID) as StoreName,ISNULL(a.UserID,c.RealName) as UserName,StorageCount,Note from dbo.Data_CoinStorage a left join Base_StoreInfo b on a.StoreID = b.StoreID left join Base_UserInfo c on a.UserID = c.UserID
where DATEDIFF(DY,DestroyTime,@BusinessDate) = 0


--��������
declare @DestroyDate varchar(10)
set  @DestroyDate = '2017-09-20 10:17:00'
select RowId,DestroyTime,StoreName,UserName,DestroyCount,Note from 
(
select ROW_NUMBER() over(order by DestroyTime desc) as RowId,DestroyTime as DestroyTime,isnull(StoreName,a.StoreID) as StoreName,ISNULL(a.UserID,c.RealName) as UserName,StorageCount as DestroyCount,Note from Data_CoinDestory a left join Base_StoreInfo b on a.StoreID = b.StoreID left join Base_UserInfo c on a.UserID = c.UserID
where DATEDIFF(DY,DestroyTime,@DestroyDate) = 0
) a

select RealTime,ICCardID,Coins,WorkStation,
(case WorkType when 0 then '�۱һ��ӱ�' when 1 then '�۱һ����' when 2 then '�ֹ�ʵ����ͱ�' when 3 then '�ֹ�ʵ������' 
when 4 then '�ֹ����' when 5 then '���ӱ��ͱ�' when 6 then '���ӱ����' when 7 then '�۱һ�ʵ������' when 8 then '�۱һ�ʵ����ͱ�'  end) as WorkTypeName,
'' as Note from Flw_Coin_Sale


0
1
2
3
4
5
6
7
8




select * from Flw_Coin_Sale



