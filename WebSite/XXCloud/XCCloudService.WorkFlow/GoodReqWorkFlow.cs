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
        /// 申请已审核通过
        /// </summary>
        [Description("申请已审核通过")]
        RequestVerifiedPass,
        /// <summary>
        /// 申请已审核拒绝
        /// </summary>
        [Description("申请已审核拒绝")]
        RequestVerifiedRefuse,
        /// <summary>
        /// 处理
        /// </summary>
        [Description("处理")]
        Deal,
        /// <summary>
        /// 处理已审核通过
        /// </summary>
        [Description("处理已审核通过")]
        DealVerifiedPass,
        /// <summary>
        /// 处理已审核拒绝
        /// </summary>
        [Description("处理已审核拒绝")]
        DealVerifiedRefuse,
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
        /// 停用
        /// </summary>
        [Description("停用")]
        Stopped = 7
    }

    /// <summary>
    /// 礼品调拨
    /// </summary>
    public class GoodReqWorkFlow : BaseWorkFlow<State, Trigger>
    {
        State _state = State.Open;

        StateMachine<State, Trigger>.TriggerWithParameters<Data_WorkFlow_Entry> _setRequestTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setCancelTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<string> _setRequestVerifiedPassTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<string> _setRequestVerifiedRefuseTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<string> _setDealVerifiedPassTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<string> _setDealVerifiedRefuseTrigger;

        private Data_WorkFlow_Entry _curEntry;

        public GoodReqWorkFlow()
        {
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setRequestTrigger = _machine.SetTriggerParameters<Data_WorkFlow_Entry>(Trigger.Request);
            _setCancelTrigger = _machine.SetTriggerParameters<int>(Trigger.Cancel);
            _setRequestVerifiedPassTrigger = _machine.SetTriggerParameters<string>(Trigger.RequestVerifiedPass);
            _setRequestVerifiedRefuseTrigger = _machine.SetTriggerParameters<string>(Trigger.RequestVerifiedRefuse);
            _setDealVerifiedPassTrigger = _machine.SetTriggerParameters<string>(Trigger.DealVerifiedPass);
            _setDealVerifiedRefuseTrigger = _machine.SetTriggerParameters<string>(Trigger.DealVerifiedRefuse);

            _machine.Configure(State.Open)
                .Permit(Trigger.Request, State.Requested);

            _machine.Configure(State.Requested)
                .OnEntryFrom(_setRequestTrigger, (entry)=> OnRequested(entry), "提交调拨申请")
                .PermitIf(_setCancelTrigger, State.Stopped, (userId) => IsCanceled(userId), "撤销申请，停用流程")
                .Permit(Trigger.RequestVerifiedPass, State.RequestVerifiedPass)
                .Permit(Trigger.RequestVerifiedRefuse, State.RequestVerifiedRefuse);

            _machine.Configure(State.RequestVerifiedPass)
                .OnEntryFrom(_setRequestVerifiedPassTrigger, (note) => OnRequestVerifiedPass(note), "申请已审核通过")
                .PermitIf(Trigger.Cancel, State.Requested, () => IsCanceled, "撤销审核，返回上一步")
                .Permit(Trigger.Deal, State.Dealed);

            _machine.Configure(State.RequestVerifiedRefuse)
                .OnEntryFrom(_setRequestVerifiedRefuseTrigger, (note) => OnRequestVerifiedRefuse(note), "申请已审核拒绝")
                .PermitIf(Trigger.Cancel, State.Requested, () => IsCanceled, "撤销审核，返回上一步")
                .Permit(Trigger.Close, State.Stopped);

            _machine.Configure(State.Dealed)
                .OnEntry(t => OnDealed(), "处理调拨申请单")
                .PermitIf(Trigger.Cancel, State.RequestVerifiedPass, () => IsCanceled, "撤销处理操作，返回上一步")
                .Permit(Trigger.DealVerifiedPass, State.DealVerifiedPass)
                .Permit(Trigger.DealVerifiedRefuse, State.DealVerifiedRefuse);

            _machine.Configure(State.DealVerifiedPass)
                .OnEntryFrom(_setDealVerifiedPassTrigger, (note) => OnDealVerifiedPass(note), "处理已审核通过")
                .PermitIf(Trigger.Cancel, State.Dealed, () => IsCanceled, "撤销处理审核，返回上一步")
                .Permit(Trigger.Close, State.Stopped);

            _machine.Configure(State.DealVerifiedRefuse)
                .OnEntryFrom(_setDealVerifiedRefuseTrigger, (note) => OnDealVerifiedRefuse(note), "处理已审核拒绝")
                .PermitIf(Trigger.Cancel, State.Dealed, () => IsCanceled, "撤销处理审核，返回上一步")
                .Permit(Trigger.Close, State.Stopped);

            _machine.Configure(State.Stopped)
                .OnEntry(t => OnStopped(), "停用流程");
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


        void OnRequested(Data_WorkFlow_Entry entry)
        {
            _curEntry = entry;
        }

        void OnRequested(string handler, int count)
        {
            _handler = handler;
            _count = (int)count;
            _description = "检查入库限额, 当前申请数:" + _count;
            WorkFlowServiceBusiness.Send(_requester, new { answerMsg=_description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            if (_count > _upper)
            {
                _machine.Fire(StockTrigger.Abort);
            }
            else
            {
                _description = _requester + "向商户提交申请, 处理人:" + _handler + ", 请求数:" + _count;
                WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            }
        }

        void OnAborted()
        {
            _description = "超过入库限额数:" + _upper;
            WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }
               
        void OnDeferred()
        {
            _description = "延期处理";
            WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            WorkFlowServiceBusiness.Send(_handler, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }
        

        void OnRefused(string description)
        {
            _description = description;
            WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            WorkFlowServiceBusiness.Send(_handler, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        void OnStored()
        {
            _description = "入库成功, 处理人:" + _handler + ", 处理数:" + _count;
            WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            WorkFlowServiceBusiness.Send(_handler, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        #endregion

        #region 方法

        public void Warn(string sender, string requester)
        {            
            _machine.Fire(_setWarnTrigger, sender, requester);
        }

        public void Request(string handler, int count)
        {            
            if (_machine.CanFire(StockTrigger.Request))
            {
                _machine.Fire(_setRequestTrigger, handler, (int)count);
            }
            else
            {
                _description = "不能重复申请";
                WorkFlowServiceBusiness.Send(_requester, new { answerMsg = _description, answerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
            }
        }

        public void Defer()
        {
            _machine.Fire(StockTrigger.Defer);
        }

        public void Refuse()
        {
            _machine.Fire(StockTrigger.Refuse);
        }

        public void Store()
        {
            _machine.Fire(StockTrigger.Store);
        }

        #endregion

    }
}
