using Stateless;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.BLL.XCCloud;
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
        /// 调拨出库
        /// </summary>
        [Description("调拨出库")]
        SendDeal,
        /// <summary>
        /// 调拨出库审核
        /// </summary>
        [Description("调拨出库审核")]
        SendDealVerify,
        /// <summary>
        /// 调拨入库
        /// </summary>
        [Description("调拨入库")]
        RequestDeal,
        /// <summary>
        /// 调拨入库审核
        /// </summary>
        [Description("调拨入库审核")]
        RequestDealVerify,
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
        /// 申请审核通过
        /// </summary>
        [Description("申请审核通过")]
        RequestVerifiedPass = 2,
        /// <summary>
        /// 申请审核拒绝
        /// </summary>
        [Description("申请审核拒绝")]
        RequestVerifiedRefuse = 3,
        /// <summary>
        /// 调拨出库
        /// </summary>
        [Description("调拨出库")]
        SendDealed = 4,
        /// <summary>
        /// 调拨出库审核通过
        /// </summary>
        [Description("调拨出库审核通过")]
        SendDealVerifiedPass = 5,
        /// <summary>
        /// 调拨出库审核拒绝
        /// </summary>
        [Description("调拨出库审核拒绝")]
        SendDealVerifiedRefuse = 6,
        /// <summary>
        /// 调拨入库
        /// </summary>
        [Description("调拨入库")]
        RequestDealed = 7,
        /// <summary>
        /// 调拨入库审核通过
        /// </summary>
        [Description("调拨入库审核通过")]
        RequestDealVerifiedPass = 8,
        /// <summary>
        /// 调拨入库审核拒绝
        /// </summary>
        [Description("调拨入库审核拒绝")]
        RequestDealVerifiedRefuse = 9,
        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        End = 10
    }

    /// <summary>
    /// 礼品调拨
    /// </summary>
    public class GoodReqWorkFlow : BaseWorkFlow<State, Trigger>
    {
        State _state = State.Open;

        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int, int, string> _setRequestVerifyTrigger;        
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setSendDealTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int, int, string> _setSendDealVerifyTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestDealTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int, int, string> _setRequestDealVerifyTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setCancelTrigger;        
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setCloseTrigger;        

        private string _workId;
        private string _eventId;
        private int _requestType;
        private int _userId;

        public GoodReqWorkFlow(string workId, string eventId, int requestType, State state)
        {
            _workId = workId;
            _eventId = eventId;
            _requestType = requestType;
            _state = state;            
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setRequestTrigger = _machine.SetTriggerParameters<int>(Trigger.Request);
            _setRequestVerifyTrigger = _machine.SetTriggerParameters<int, int, string>(Trigger.RequestVerify);
            _setSendDealTrigger = _machine.SetTriggerParameters<int>(Trigger.SendDeal);
            _setSendDealVerifyTrigger = _machine.SetTriggerParameters<int, int, string>(Trigger.SendDealVerify);
            _setRequestDealTrigger = _machine.SetTriggerParameters<int>(Trigger.RequestDeal);
            _setRequestDealVerifyTrigger = _machine.SetTriggerParameters<int, int, string>(Trigger.RequestDealVerify);
            _setCancelTrigger = _machine.SetTriggerParameters<int>(Trigger.Cancel);            
            _setCloseTrigger = _machine.SetTriggerParameters<int>(Trigger.Close);            
            
            _machine.Configure(State.Open)
                .PermitIf(Trigger.Request, State.Requested, () => _requestType == (int)RequestType.RequestStore 
                                                            || _requestType == (int)RequestType.MerchRequest
                                                            || _requestType == (int)RequestType.RequestMerch, "门店或总店申请")
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => _requestType == (int)RequestType.MerchSend, "总店配送");

            _machine.Configure(State.Requested)
                .SubstateOf(State.Open)
                .OnEntryFrom(_setRequestTrigger, (userId) => OnRequested(userId), "申请")
                .PermitIf(_setCancelTrigger, State.End, (userId) => IsCanceled(userId), "撤销申请")
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedPass, (userId, s, note) => (_requestType == (int)RequestType.RequestStore 
                                                                                                || _requestType == (int)RequestType.RequestMerch) && s == 1)
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedRefuse, (userId, s, note) => (_requestType == (int)RequestType.RequestStore
                                                                                                || _requestType == (int)RequestType.RequestMerch) && s == 0)
                .PermitIf(_setSendDealTrigger, State.SendDealed, (userId) => _requestType == (int)RequestType.MerchRequest);

            _machine.Configure(State.RequestVerifiedPass)
                .SubstateOf(State.Requested)
                .OnEntryFrom(_setRequestVerifyTrigger, (userId, s, note) => OnRequestVerified(userId, s, note), "申请审核通过")
                .PermitIf(_setCancelTrigger, State.Requested, (userId) => IsCanceled(userId), "撤销申请审核，返回上一步")
                .Permit(Trigger.SendDeal, State.SendDealed);

            _machine.Configure(State.RequestVerifiedRefuse)
                .SubstateOf(State.Requested)
                .OnEntryFrom(_setRequestVerifyTrigger, (userId, s, note) => OnRequestVerified(userId, s, note), "申请审核拒绝")
                .PermitIf(_setCancelTrigger, State.Requested, (userId) => IsCanceled(userId), "撤销申请审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.SendDealed)
                .SubstateOf(State.RequestVerifiedPass)
                .OnEntryFrom(_setSendDealTrigger, (userId) => OnSendDealed(userId), "调拨出库")
                .PermitIf(_setCancelTrigger, State.RequestVerifiedPass, (userId) => _requestType == (int)RequestType.RequestStore && IsCanceled(userId), "撤销调拨出库，返回上一步")
                .PermitIf(_setCancelTrigger, State.Requested, (userId) => _requestType == (int)RequestType.MerchRequest && IsCanceled(userId), "撤销调拨出库，返回上一步")
                .PermitIf(_setCancelTrigger, State.Open, (userId) => _requestType == (int)RequestType.MerchSend && IsCanceled(userId), "撤销调拨出库，返回上一步")
                .PermitIf(_setSendDealVerifyTrigger, State.SendDealVerifiedPass, (userId, s, note) => _requestType == (int)RequestType.RequestStore && s == 1)
                .PermitIf(_setSendDealVerifyTrigger, State.SendDealVerifiedRefuse, (userId, s, note) => _requestType == (int)RequestType.RequestStore && s == 0)
                .PermitIf(_setRequestDealTrigger, State.RequestDealed, (userId) => _requestType == (int)RequestType.MerchRequest 
                                                                                || _requestType == (int)RequestType.MerchSend
                                                                                || _requestType == (int)RequestType.RequestMerch);

            _machine.Configure(State.SendDealVerifiedPass)
                .SubstateOf(State.SendDealed)
                .OnEntryFrom(_setSendDealVerifyTrigger, (userId, s, note) => OnSendDealVerified(userId, s, note), "调拨出库审核通过")
                .PermitIf(_setCancelTrigger, State.SendDealed, (userId) => IsCanceled(userId), "撤销调拨出库审核，返回上一步")
                .Permit(Trigger.RequestDeal, State.RequestDealed);

            _machine.Configure(State.SendDealVerifiedRefuse)
                .SubstateOf(State.SendDealed)
                .OnEntryFrom(_setSendDealVerifyTrigger, (userId, s, note) => OnSendDealVerified(userId, s, note), "调拨出库审核拒绝")
                .PermitIf(_setCancelTrigger, State.SendDealed, (userId) => IsCanceled(userId), "撤销调拨出库审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.RequestDealed)
                .SubstateOf(State.SendDealVerifiedPass)
                .OnEntryFrom(_setRequestDealTrigger, (userId) => OnRequestDealed(userId), "调拨入库")
                .PermitIf(_setCancelTrigger, State.SendDealVerifiedPass, (userId) => _requestType == (int)RequestType.RequestStore && IsCanceled(userId), "撤销调拨入库，返回上一步")
                .PermitIf(_setCancelTrigger, State.SendDealed, (userId) => (_requestType == (int)RequestType.MerchRequest 
                                                                        || _requestType == (int)RequestType.MerchSend
                                                                        || _requestType == (int)RequestType.RequestMerch) && IsCanceled(userId), "撤销调拨入库，返回上一步")
                .PermitIf(_setRequestDealVerifyTrigger, State.RequestDealVerifiedPass, (userId, s, note) => _requestType == (int)RequestType.RequestStore && s == 1)
                .PermitIf(_setRequestDealVerifyTrigger, State.RequestDealVerifiedRefuse, (userId, s, note) => _requestType == (int)RequestType.RequestStore && s == 0)
                .PermitIf(Trigger.Close, State.End, () => _requestType == (int)RequestType.MerchRequest 
                                                    || _requestType == (int)RequestType.MerchSend
                                                    || _requestType == (int)RequestType.RequestMerch);

            _machine.Configure(State.RequestDealVerifiedPass)
                .SubstateOf(State.RequestDealed)
                .OnEntryFrom(_setRequestDealVerifyTrigger, (userId, s, note) => OnRequestDealVerified(userId, s, note), "调拨入库审核通过")
                .PermitIf(_setCancelTrigger, State.RequestDealed, (userId) => IsCanceled(userId), "撤销调拨入库审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.RequestDealVerifiedRefuse)
                .SubstateOf(State.RequestDealed)
                .OnEntryFrom(_setRequestDealVerifyTrigger, (userId, s, note) => OnRequestDealVerified(userId, s, note), "调拨入库审核拒绝")
                .PermitIf(_setCancelTrigger, State.RequestDealed, (userId) => IsCanceled(userId), "撤销调拨入库审核，返回上一步")
                .Permit(Trigger.Close, State.End);

            _machine.Configure(State.End)
                .OnEntryFrom(_setCloseTrigger, (userId) => OnClosed(userId), "流程结束");
        }

        #region 属性
        protected new State State
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
            var theUser = Base_UserInfoService.I.GetModels(p => p.UserID == userId).FirstOrDefault();            
            var lastUser = Base_UserInfoService.I.GetModels(p => p.UserID == _userId).FirstOrDefault();
            //如果用户角色不同不能撤销
            if (theUser.UserType != lastUser.UserType)
            {
                return false;
            }
           
            return true;
        }


        void OnRequested(int userId)
        {
            
        }

        void OnRequestVerified(int userId, int state, string note)
        {

        }

        void OnSendDealed(int userId)
        {

        }

        void OnSendDealVerified(int userId, int state, string note)
        {

        }

        void OnRequestDealed(int userId)
        {

        }

        void OnRequestDealVerified(int userId, int state, string note)
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

            try
            {
                if (_machine.CanFire(Trigger.Request))
                {
                    _machine.Fire(_setRequestTrigger, userId);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }
            
            return false;
        }

        public bool RequestVerify(int userId, int s, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestVerify))
                {
                    _machine.Fire(_setRequestVerifyTrigger, userId, s, note);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool SendDeal(int userId, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.SendDeal))
                {
                    _machine.Fire(_setSendDealTrigger, userId);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool SendDealVerify(int userId, int s, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.SendDealVerify))
                {
                    _machine.Fire(_setSendDealVerifyTrigger, userId, s, note);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool RequestDeal(int userId, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDeal))
                {
                    _machine.Fire(_setRequestDealTrigger, userId);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool RequestDealVerify(int userId, int s, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDealVerify))
                {
                    _machine.Fire(_setRequestDealVerifyTrigger, userId, s, note);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool Cancel(int userId, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.Cancel))
                {
                    _machine.Fire(_setCancelTrigger, userId);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        public bool Close(int userId, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.Close))
                {
                    _machine.Fire(_setCloseTrigger, userId);
                    return true;
                }
                else
                {
                    errMsg = "当前状态不能操作";
                }
            }
            catch (Exception e)
            {
                errMsg = e.Message;
            }

            return false;
        }

        #endregion

    }
}
