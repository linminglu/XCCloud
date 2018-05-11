using Stateless;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.XCCloud;
using XCCloudService.SocketService.TCP.Business;
using XCCloudService.WorkFlow.Base;

namespace XCCloudService.WorkFlow
{
    public enum Trigger
    {
        /// <summary>
        /// 申请
        /// </summary>
        [Description("申请")]
        Request,
        /// <summary>
        /// 申请审核
        /// </summary>
        [Description("申请审核")]
        RequestVerify,        
        /// <summary>
        /// 处理
        /// </summary>
        [Description("处理")]
        Deal,
        /// <summary>
        /// 处理审核
        /// </summary>
        [Description("处理审核")]
        DealVerify,
        /// <summary>
        /// 撤销
        /// </summary>
        [Description("撤销")]
        Cancel,
        /// <summary>
        /// 关闭
        /// </summary>
        [Description("关闭")]
        Close
    }

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
        [Description("申请")]
        Requested = 1,
        /// <summary>
        /// 申请已审核通过
        /// </summary>
        [Description("申请已审核通过")]
        RequestVerifiedPass = 2,
        /// <summary>
        /// 申请已审核拒绝
        /// </summary>
        [Description("申请已审核拒绝")]
        RequestVerifiedRefuse = 3,
        /// <summary>
        /// 处理
        /// </summary>
        [Description("处理")]
        Dealed = 4,       
        /// <summary>
        /// 处理已审核通过
        /// </summary>
        [Description("处理已审核通过")]
        DealVerifiedPass = 5,
        /// <summary>
        /// 处理已审核拒绝
        /// </summary>
        [Description("处理已审核拒绝")]
        DealVerifiedRefuse = 6, 
        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        End = 7
    }

    /// <summary>
    /// 礼品调拨
    /// </summary>
    public class GoodReqWorkFlow : BaseWorkFlow<State, Trigger>
    {
        State _state = State.Open;

        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setCancelTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setDealTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setCloseTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int, int, string> _setRequestVerifyTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int, int, string> _setDealVerifyTrigger;

        private string workId;
        private string eventId;
        private int userId;
        private State state;
        private string note;

        public GoodReqWorkFlow(string workId, string eventId, State state)
        {
            _state = state;
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setRequestTrigger = _machine.SetTriggerParameters<int>(Trigger.Request);
            _setCancelTrigger = _machine.SetTriggerParameters<int>(Trigger.Cancel);
            _setDealTrigger = _machine.SetTriggerParameters<int>(Trigger.Deal);
            _setCloseTrigger = _machine.SetTriggerParameters<int>(Trigger.Close);
            _setRequestVerifyTrigger = _machine.SetTriggerParameters<int, int, string>(Trigger.RequestVerify);
            _setDealVerifyTrigger = _machine.SetTriggerParameters<int, int, string>(Trigger.DealVerify);

            _machine.Configure(State.Open)
                .Permit(Trigger.Request, State.Requested);

            _machine.Configure(State.Requested)
                .OnEntryFrom(_setRequestTrigger, (userId) => OnRequested(userId), "提交调拨申请")
                .PermitIf(_setCancelTrigger, State.End, (userId) => IsCanceled(userId), "撤销申请")
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedPass, (userId, s, note) => s == 1)
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedRefuse, (userId, s, note) => s == 0);

            _machine.Configure(State.RequestVerifiedPass)
                .SubstateOf(State.Requested)
                .OnEntryFrom(_setRequestVerifyTrigger, (userId, s, note) => OnRequestVerified(userId, s, note), "申请已审核通过")
                .PermitIf(_setCancelTrigger, State.Requested, (userId) => IsCanceled(userId), "撤销申请审核，返回上一步")
                .Permit(Trigger.Deal, State.Dealed);

            _machine.Configure(State.RequestVerifiedRefuse)
                .SubstateOf(State.Requested)
                .OnEntryFrom(_setRequestVerifyTrigger, (userId, s, note) => OnRequestVerified(userId, s, note), "申请已审核拒绝")
                .PermitIf(_setCancelTrigger, State.Requested, (userId) => IsCanceled(userId), "撤销申请审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.Dealed)
                .SubstateOf(State.RequestVerifiedPass)
                .OnEntryFrom(_setDealTrigger, (userId) => OnDealed(userId), "处理调拨申请单")
                .PermitIf(_setCancelTrigger, State.RequestVerifiedPass, (userId) => IsCanceled(userId), "撤销处理，返回上一步")
                .PermitIf(_setDealVerifyTrigger, State.DealVerifiedPass, (userId, s, note) => s == 1)
                .PermitIf(_setDealVerifyTrigger, State.DealVerifiedRefuse, (userId, s, note) => s == 0);

            _machine.Configure(State.DealVerifiedPass)
                .SubstateOf(State.Dealed)
                .OnEntryFrom(_setDealVerifyTrigger, (userId, s, note) => OnDealVerified(userId, s, note), "处理已审核通过")
                .PermitIf(_setCancelTrigger, State.Dealed, (userId) => IsCanceled(userId), "撤销处理审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.DealVerifiedRefuse)
                .SubstateOf(State.Dealed)
                .OnEntryFrom(_setDealVerifyTrigger, (userId, s, note) => OnDealVerified(userId, s, note), "处理已审核拒绝")
                .PermitIf(_setCancelTrigger, State.Dealed, (userId) => IsCanceled(userId), "撤销处理审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.End)
                .OnEntryFrom(_setCloseTrigger, (userId) => OnClosed(userId), "流程结束");
        }

        #region 属性
        public new State State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        #endregion

        #region 事件

        bool IsCanceled(int userId)
        {

            return true;
        }


        void OnRequested(int userId)
        {
            
        }

        void OnRequestVerified(int userId, int state, string note)
        {

        }

        void OnDealed(int userId)
        {

        }

        void OnDealVerified(int userId, int state, string note)
        {

        }
        
        void OnClosed(int userId)
        {

        }
        
        #endregion

        #region 方法

        public bool Request(int userId, out string errMsg)
        {
            errMsg = string.Empty;
            return false;
        }        

        #endregion

    }
}
