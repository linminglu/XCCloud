﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCCloudService.Model.WeiXin
{
    public class MemberTokenModel
    {
        public MemberTokenModel()
        {
            this.MemberId = string.Empty;
            this.Mobile = string.Empty;
        }
        public string Token { get; set; }

        public string MemberId { get; set; }

        public string Mobile { set; get; }

        public WechatInfo Info { get; set; }

        public MemberCard CurrentCardInfo { get; set; }

        public MemberBalance MemberBalances { get; set; }
    }

    public class WechatInfo
    {
        /// <summary>
        /// 是否关注
        /// </summary>
        public int subscribe { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string headimgurl { get; set; }
    }

    public class MemberCard
    {
        public string CardId { get; set; }

        public string ICCardId { get; set; }

        public int MemberLevelId { get; set; }
    }

    public class MemberBalance
    {
        public int BalanceIndex { get; set; }

        public string BalanceName { get; set; }

        public decimal Quantity { get; set; }
    }
}