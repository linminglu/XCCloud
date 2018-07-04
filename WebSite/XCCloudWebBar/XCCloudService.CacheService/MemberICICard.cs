﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.CustomModel.XCGame;

namespace XCCloudWebBar.CacheService
{
    public class MemberICICard
    {
        /// <summary>
        /// 缓存中是否存在数据
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool IsExist(string key)
        {
            object obj = CacheHelper.Get(CacheType.MemberCardQuery + key);
            if (obj == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
       

        /// <summary>
        /// 缓存查询的会员信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <param name="expires"></param>
        public static void Add(string key, List<MenberICCardModel> list, int expires)
        {
            CacheHelper.Insert(CacheType.MemberCardQuery + key, list, expires);
        }

        /// <summary>
        /// 获取缓存中的信息
        /// </summary>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            object obj = CacheHelper.Get(CacheType.MemberCardQuery + key);
            return obj;
        }

        public static void Remove(string key)
        {
            CacheHelper.Remove(CacheType.MemberCardQuery + key);
        }
    }
}
