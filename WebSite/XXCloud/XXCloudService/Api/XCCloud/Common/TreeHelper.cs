﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Model.CustomModel.XCCloud;

namespace XXCloudService.Api.XCCloud.Common
{
    public static class TreeHelper
    {
        #region "绑定树型结构"

        public static void LoopToAppendChildren(List<Base_MerchFunctionModel> base_MerchFunction, Base_MerchFunctionModel curItem)
        {
            var subItems = base_MerchFunction.Where(ee => ee.ParentID.Value == curItem.FunctionID).ToList();
            curItem.Children = new List<Base_MerchFunctionModel>();
            curItem.Children.AddRange(subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildren(base_MerchFunction, subItem);
            }
        }        

        public static void LoopToAppendChildren(List<DictionaryResponseModel> dict_System, DictionaryResponseModel curItem)
        {
            var subItems = dict_System.Where(ee => ee.PID.Value == curItem.ID).ToList();
            curItem.Children = new List<DictionaryResponseModel>();
            curItem.Children.AddRange(subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildren(dict_System, subItem);
            }
        }

        public static void LoopToAppendChildren(List<GameListModel> dict_System, GameListModel curItem, string storeId)
        {
            var subItems = dict_System.Where(ee => ee.PID.Value == curItem.ID).ToList();
            curItem.Children = new List<GameListModel>();
            curItem.Children.AddRange(subItems);
            curItem.Disabled = true;
            var gameType = curItem.ID;
            curItem.Children.AddRange(Data_GameInfoService.I.GetModels(p => p.GameType == gameType && p.StoreID.Equals(storeId, StringComparison.OrdinalIgnoreCase) && p.State == 1).AsEnumerable()
                .Select(o => new GameListModel { ID = o.ID, DictKey = o.GameName, Disabled = false, Children = new List<GameListModel>() }).ToList());

            foreach (var subItem in subItems)
            {                
                LoopToAppendChildren(dict_System, subItem, storeId);
            }
        }

        public static void LoopToAppendChildren(List<MenuInfoModel> menuInfoList, MenuInfoModel curItem)
        {
            var subItems = menuInfoList.Where(ee => ee.ParentID.Value == curItem.FunctionID).ToList();
            curItem.Children = new List<MenuInfoModel>();
            curItem.Children.AddRange(subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildren(menuInfoList, subItem);
            }
        }

        public static void LoopToAppendChildren(List<UserGroupGrantModel> treeNodes, UserGroupGrantModel curItem)
        {
            var subItems = treeNodes.Where(ee => ee.ParentID.Value == curItem.FunctionID).ToList();
            curItem.Children = new List<UserGroupGrantModel>();
            curItem.Children.AddRange(subItems);
            foreach (var subItem in subItems)
            {
                LoopToAppendChildren(treeNodes, subItem);
            }
        }

        #endregion
    }
}