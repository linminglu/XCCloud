using Stateless;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using XCCloudService.BLL.XCCloud;
using XCCloudService.Common;
using XCCloudService.Common.Enum;
using XCCloudService.Model.XCCloud;
using XCCloudService.SocketService.TCP.Business;
using XCCloudService.WorkFlow.Base;

namespace XCCloudService.WorkFlow
{
    public enum NodeType
    {
        /// <summary>
        /// 开始
        /// </summary>
        [Description("开始")]
        Start = 0,
        /// <summary>
        /// 过程
        /// </summary>
        [Description("过程")]
        Process = 1,
        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        End = 2
    }
    public enum Trigger
    {
        /// <summary>
        /// 调拨申请
        /// </summary>
        [Description("调拨申请")]
        Request = 1,
        /// <summary>
        /// 调拨申请审核
        /// </summary>
        [Description("调拨申请审核")]
        RequestVerify = 2,        
        /// <summary>
        /// 调拨出库
        /// </summary>
        [Description("调拨出库")]
        SendDeal = 3,
        /// <summary>
        /// 调拨出库审核
        /// </summary>
        [Description("调拨出库审核")]
        SendDealVerify = 4,
        /// <summary>
        /// 调拨入库
        /// </summary>
        [Description("调拨入库")]
        RequestDeal = 5,
        /// <summary>
        /// 调拨入库审核
        /// </summary>
        [Description("调拨入库审核")]
        RequestDealVerify = 6,
        /// <summary>
        /// 撤销
        /// </summary>
        [Description("撤销")]
        Cancel = 7,
        /// <summary>
        /// 调拨流程关闭
        /// </summary>
        [Description("调拨流程关闭")]
        Close = 8
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
        [Description("已申请")]
        Requested = 1,
        /// <summary>
        /// 申请审核已通过
        /// </summary>
        [Description("申请审核已通过")]
        RequestVerifiedPass = 2,
        /// <summary>
        /// 申请审核已拒绝
        /// </summary>
        [Description("申请审核已拒绝")]
        RequestVerifiedRefuse = 3,
        /// <summary>
        /// 调拨已出库
        /// </summary>
        [Description("调拨已出库")]
        SendDealed = 4,
        /// <summary>
        /// 调拨出库审核已通过
        /// </summary>
        [Description("调拨出库审核已通过")]
        SendDealVerifiedPass = 5,
        /// <summary>
        /// 调拨出库审核已拒绝
        /// </summary>
        [Description("调拨出库审核已拒绝")]
        SendDealVerifiedRefuse = 6,
        /// <summary>
        /// 调拨已入库
        /// </summary>
        [Description("调拨已入库")]
        RequestDealed = 7,
        /// <summary>
        /// 调拨入库审核已通过
        /// </summary>
        [Description("调拨入库审核已通过")]
        RequestDealVerifiedPass = 8,
        /// <summary>
        /// 调拨入库审核已拒绝
        /// </summary>
        [Description("调拨入库审核已拒绝")]
        RequestDealVerifiedRefuse = 9,
        /// <summary>
        /// 结束
        /// </summary>
        [Description("结束")]
        Closed = 10
    }

    /// <summary>
    /// 礼品调拨
    /// </summary>
    public class GoodReqWorkFlow : BaseWorkFlow<State, Trigger>
    {
        State _state = State.Open;

        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestVerifyTrigger;        
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setSendDealVerifyTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestDealVerifyTrigger;        

        private int _workId;
        private int _eventId;
        private int _requestType;

        /// <summary>
        /// 获取工作流编号，如果没有就生成配置信息
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        private int GetWorkId(int eventId)
        {
            if (!Data_WorkFlow_EntryService.I.Any(a => a.EventID == eventId && a.EventType == (int)WorkflowEventType.GoodRequest))
            {
                if (!Data_GoodRequestService.I.Any(a => a.ID == eventId))
                {
                    return 0;
                }

                var merchId = Data_GoodRequestService.I.GetModels(p => p.ID == eventId).FirstOrDefault().MerchID;
                if (!Data_WorkFlowConfigService.I.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.WorkType == (int)WorkflowType.GoodRequest))
                {
                    //开启EF事务
                    using (TransactionScope ts = new TransactionScope())
                    {
                        try
                        {
                            var configModel = new Data_WorkFlowConfig { MerchID = merchId, WorkType = (int)WorkflowType.GoodRequest, State = 1 };
                            if (!Data_WorkFlowConfigService.I.Add(configModel))
                            {
                                return 0;
                            }
                            foreach (Trigger t in Enum.GetValues(typeof(Trigger)))
                            {
                                var nodeModel = new Data_WorkFlow_Node();
                                nodeModel.WorkID = configModel.ID;
                                nodeModel.OrderNumber = (int)t;
                                Data_WorkFlow_NodeService.I.AddModel(nodeModel);
                            }
                            if (!Data_WorkFlow_NodeService.I.SaveChanges())
                            {
                                return 0;
                            }

                            return configModel.ID;
                        }                        
                        catch
                        {
                            return 0;
                        }
                    }
                    
                }
                else
                {
                    return Data_WorkFlowConfigService.I.GetModels(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.WorkType == (int)WorkflowType.GoodRequest).FirstOrDefault().ID;
                }
            }
            else
            {
                return Data_WorkFlow_EntryService.I.GetModels(a => a.EventID == eventId && a.EventType == (int)WorkflowEventType.GoodRequest).FirstOrDefault().WorkID ?? 0;
            }
        }

        public GoodReqWorkFlow(int eventId, int requestType, State state)
        {
            _workId = GetWorkId(eventId);
            _eventId = eventId;
            _requestType = requestType;
            _state = state;            
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);

            _setRequestVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.RequestVerify);
            _setSendDealVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.SendDealVerify);
            _setRequestDealVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.RequestDealVerify);
            
            _machine.Configure(State.Open)
                .PermitIf(Trigger.Request, State.Requested, () => _requestType == (int)RequestType.RequestStore 
                                                            || _requestType == (int)RequestType.MerchRequest
                                                            || _requestType == (int)RequestType.RequestMerch, "门店或总店申请")
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => _requestType == (int)RequestType.MerchSend, "总店配送");

            _machine.Configure(State.Requested)
                .Permit(Trigger.Cancel, State.Closed)
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedPass, (s) => (_requestType == (int)RequestType.RequestStore 
                                                                               || _requestType == (int)RequestType.RequestMerch) && s == 1)
                .PermitIf(_setRequestVerifyTrigger, State.RequestVerifiedRefuse, (s) => (_requestType == (int)RequestType.RequestStore
                                                                                 || _requestType == (int)RequestType.RequestMerch) && s == 0)
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => _requestType == (int)RequestType.MerchRequest);

            _machine.Configure(State.RequestVerifiedPass)
                .Permit(Trigger.Cancel, State.Requested)
                .Permit(Trigger.SendDeal, State.SendDealed);

            _machine.Configure(State.RequestVerifiedRefuse)
                .Permit(Trigger.Cancel, State.Requested)
                .Permit(Trigger.Close, State.Closed);

            _machine.Configure(State.SendDealed)
                .PermitReentry(Trigger.SendDeal)
                .PermitIf(Trigger.Cancel, State.RequestVerifiedPass, () => _requestType == (int)RequestType.RequestStore, "撤销调拨出库，返回上一步")
                .PermitIf(Trigger.Cancel, State.Requested, () => _requestType == (int)RequestType.MerchRequest, "撤销调拨出库，返回上一步")
                .PermitIf(Trigger.Cancel, State.Open, () => _requestType == (int)RequestType.MerchSend, "撤销调拨出库，返回上一步")
                .PermitIf(_setSendDealVerifyTrigger, State.SendDealVerifiedPass, (s) => _requestType == (int)RequestType.RequestStore && s == 1)
                .PermitIf(_setSendDealVerifyTrigger, State.SendDealVerifiedRefuse, (s) => _requestType == (int)RequestType.RequestStore && s == 0)
                .PermitIf(Trigger.RequestDeal, State.RequestDealed, () => _requestType == (int)RequestType.MerchRequest 
                                                                    || _requestType == (int)RequestType.MerchSend
                                                                    || _requestType == (int)RequestType.RequestMerch);

            _machine.Configure(State.SendDealVerifiedPass)
                .Permit(Trigger.Cancel, State.SendDealed)
                .Permit(Trigger.RequestDeal, State.RequestDealed);

            _machine.Configure(State.SendDealVerifiedRefuse)
                .Permit(Trigger.Cancel, State.SendDealed)
                .Permit(Trigger.Close, State.Closed);

            _machine.Configure(State.RequestDealed)
                .PermitIf(Trigger.Cancel, State.SendDealVerifiedPass, () => _requestType == (int)RequestType.RequestStore, "撤销调拨入库，返回上一步")
                .PermitIf(Trigger.Cancel, State.SendDealed, () => _requestType == (int)RequestType.MerchRequest 
                                                            || _requestType == (int)RequestType.MerchSend
                                                            || _requestType == (int)RequestType.RequestMerch, "撤销调拨入库，返回上一步")
                .PermitIf(_setRequestDealVerifyTrigger, State.RequestDealVerifiedPass, (s) => _requestType == (int)RequestType.RequestStore && s == 1)
                .PermitIf(_setRequestDealVerifyTrigger, State.RequestDealVerifiedRefuse, (s) => _requestType == (int)RequestType.RequestStore && s == 0)
                .PermitIf(Trigger.Close, State.Closed, () => _requestType == (int)RequestType.MerchRequest 
                                                       || _requestType == (int)RequestType.MerchSend
                                                       || _requestType == (int)RequestType.RequestMerch);

            _machine.Configure(State.RequestDealVerifiedPass)
                .Permit(Trigger.Cancel, State.RequestDealed)
                .Permit(Trigger.Close, State.Closed);

            _machine.Configure(State.RequestDealVerifiedRefuse)
                .Permit(Trigger.Cancel, State.RequestDealed)
                .Permit(Trigger.Close, State.Closed);

            _machine.Configure(State.Closed)
                .OnEntry(t => OnClosed(), "流程结束");
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

        void OnClosed()
        {
            
        }
        
        #endregion

        #region 方法

        private bool IsValidUser(int userId, out string errMsg)
        {
            errMsg = string.Empty;
            var user = Base_UserInfoService.I.GetModels(p => p.UserID == userId).FirstOrDefault();
            if (user == null)
            {
                errMsg = "当前操作用户不存在";
                return false;
            }

            return true;
        }

        private bool AddWorkEntry(int userId, Trigger t, State s, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                //判断操作用户是否存在
                if (!IsValidUser(userId, out errMsg)) return false;

                //获取工作节点
                var workNode = Data_WorkFlow_NodeService.I.GetModels(p => p.WorkID == _workId && p.OrderNumber == (int)t).FirstOrDefault();
                if (workNode == null)
                {
                    errMsg = "当前工作节点不存在";
                    return false;
                }

                //生成工作记录
                var nodeId = workNode.ID;                
                var entry = new Data_WorkFlow_Entry();
                entry.WorkID = _workId;
                entry.EventID = _eventId;
                entry.EventType = (int)WorkflowEventType.GoodRequest;
                entry.NodeID = nodeId;
                entry.UserID = userId;
                entry.State = (int)s;
                entry.CreateTime = DateTime.Now;
                entry.Note = s.GetDescription();
                if (!Data_WorkFlow_EntryService.I.Add(entry))
                {
                    errMsg = t.GetDescription() + "失败";
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }            
        }

        public bool Request(int userId, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.Request))
                {
                    if (!AddWorkEntry(userId, Trigger.Request, State.Requested, out errMsg)) return false;

                    _machine.Fire(Trigger.Request);  
                  
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

        public bool RequestVerify(int userId, int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestVerify))
                {
                    if (!AddWorkEntry(userId, Trigger.RequestVerify, state == 1 ? State.RequestVerifiedPass : State.RequestVerifiedRefuse, out errMsg)) return false;

                    _machine.Fire(_setRequestVerifyTrigger, state);

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
                    if (!AddWorkEntry(userId, Trigger.SendDeal, State.SendDealed, out errMsg)) return false;

                    _machine.Fire(Trigger.SendDeal);

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

        public bool SendDealVerify(int userId, int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.SendDealVerify))
                {
                    if (!AddWorkEntry(userId, Trigger.SendDealVerify, state == 1 ? State.SendDealVerifiedPass : State.SendDealVerifiedRefuse, out errMsg)) return false;

                    _machine.Fire(_setSendDealVerifyTrigger, state);

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
                    if (!AddWorkEntry(userId, Trigger.RequestDeal, State.RequestDealed, out errMsg)) return false;

                    _machine.Fire(Trigger.RequestDeal);

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

        public bool RequestDealVerify(int userId, int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDealVerify))
                {
                    if (!AddWorkEntry(userId, Trigger.RequestDealVerify, state == 1 ? State.RequestDealVerifiedPass : State.RequestDealVerifiedRefuse, out errMsg)) return false;

                    _machine.Fire(_setRequestDealVerifyTrigger, state);

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
                    if (!IsValidUser(userId, out errMsg)) return false;
                        
                    var theUser = Base_UserInfoService.I.GetModels(p => p.UserID == userId).FirstOrDefault();
                    
                    //获取当前工作记录
                    var entry = Data_WorkFlow_EntryService.I.GetModels(p => p.EventType == (int)WorkflowEventType.GoodRequest && p.WorkID == _workId && p.EventID == _eventId).OrderByDescending(or => or.CreateTime).FirstOrDefault();
                    if (entry != null)
                    {
                        //获取当前工作用户
                        var workUserId = entry.UserID;
                        var workUser = Base_UserInfoService.I.GetModels(p => p.UserID == workUserId).FirstOrDefault();
                        if (workUser == null)
                        {
                            errMsg = "当前工作用户不存在";
                            return false;
                        }
                        
                        //如果用户角色不同不能撤销
                        if (theUser.UserType != workUser.UserType)
                        {
                            errMsg = "当前操作用户不能执行撤销操作";
                            return false;
                        }

                        //不能撤销其他门店用户流程
                        if (workUser.MerchID != theUser.MerchID || workUser.StoreID != theUser.StoreID)
                        {
                            errMsg = "不能撤销其他门店的工作";
                            return false;
                        }

                        if (!Data_WorkFlow_EntryService.I.Delete(entry))
                        {
                            errMsg = "撤销工作失败";
                            return false;
                        }
                    }
                    
                    _machine.Fire(Trigger.Cancel);
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
                if (_machine.CanFire(Trigger.Request))
                {
                    if (!AddWorkEntry(userId, Trigger.Close, State.Closed, out errMsg)) return false;

                    _machine.Fire(Trigger.Request);

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
