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
    /// <summary>
    /// 礼品调拨
    /// </summary>
    public class GoodReqWorkFlow : BaseWorkFlow<State, Trigger>
    {
        State _state = State.Open;

        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestVerifyTrigger;        
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setSendDealVerifyTrigger;
        StateMachine<State, Trigger>.TriggerWithParameters<int> _setRequestDealVerifyTrigger;        

        private int _requestType;
        private int _workId;
        private int _eventId;
        private int _userId;
        private int _logType;
        private int _userType;
        private string _targetMerchId;
        private string _inStoreId;
        private string _outStoreId;
        private string _storeId;
        private string _merchId;
        private bool _result;
        private string _errMsg;
        
        /// <summary>
        /// 获取工作流ID
        /// </summary>
        /// <returns></returns>
        private int GetWorkId()
        {
            int result = 0;

            //开启EF事务
            using (TransactionScope ts = new TransactionScope())
            {
                try
                {
                    if (!Data_GoodRequestService.I.Any(a => a.ID == _eventId && (a.MerchID ?? "") != "")) return result;

                    var merchId = Data_GoodRequestService.I.GetModels(p => p.ID == _eventId && (p.MerchID ?? "") != "").Select(o => o.MerchID).FirstOrDefault() ?? "";
                    if (!Data_WorkFlowConfigService.I.Any(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.WorkType == (int)WorkflowType.GoodRequest))
                    {
                        var configModel = new Data_WorkFlowConfig { MerchID = merchId, WorkType = (int)WorkflowType.GoodRequest, State = 1 };

                        if (!Data_WorkFlowConfigService.I.Add(configModel)) return result;

                        foreach (Trigger t in Enum.GetValues(typeof(Trigger)))
                        {
                            var nodeModel = new Data_WorkFlow_Node();
                            nodeModel.WorkID = configModel.ID;
                            nodeModel.OrderNumber = (int)t;
                            Data_WorkFlow_NodeService.I.AddModel(nodeModel);
                        }

                        if (!Data_WorkFlow_NodeService.I.SaveChanges()) return result;

                        result = configModel.ID;
                    }
                    else
                    {
                        result = Data_WorkFlowConfigService.I.GetModels(a => a.MerchID.Equals(merchId, StringComparison.OrdinalIgnoreCase) && a.WorkType == (int)WorkflowType.GoodRequest).FirstOrDefault().ID;
                        foreach (Trigger t in Enum.GetValues(typeof(Trigger)))
                        {
                            if (!Data_WorkFlow_NodeService.I.Any(a => a.WorkID == result && a.OrderNumber == (int)t))
                            {
                                var nodeModel = new Data_WorkFlow_Node();
                                nodeModel.WorkID = result;
                                nodeModel.OrderNumber = (int)t;
                                Data_WorkFlow_NodeService.I.AddModel(nodeModel);
                            }
                        }

                        if (!Data_WorkFlow_NodeService.I.SaveChanges()) return result;
                    }

                    ts.Complete();
                }
                catch
                { }
            }
            
            return result;
        }

        /// <summary>
        /// 获取调拨方式
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        private int GetRequestType()
        {
            return Data_GoodRequestService.I.GetModels(p => p.ID == _eventId).Select(o => o.RequstType).FirstOrDefault() ?? 0;
        }

        /// <summary>
        /// 获取当前工作状态
        /// </summary>
        private void GetWorkState()
        {
            _state = (State)(Data_WorkFlow_EntryService.I.GetModels(p => p.EventID == _eventId && p.WorkID == _workId && p.EventType == (int)WorkflowEventType.GoodRequest).OrderByDescending(or => or.CreateTime)
                .Select(o => o.State).FirstOrDefault() ?? 0);
            var data_GoodRequest = Data_GoodRequestService.I.GetModels(p => p.ID == _eventId).FirstOrDefault() ?? new Data_GoodRequest();
            _targetMerchId = data_GoodRequest.MerchID ?? "";
            _inStoreId = data_GoodRequest.RequestInStoreID ?? "";
            _outStoreId = data_GoodRequest.RequestOutStoreID ?? "";
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        private void GetUserInfo()
        {
            var base_UserInfo = Base_UserInfoService.I.GetModels(p => p.UserID == _userId).FirstOrDefault() ?? new Base_UserInfo();
            _userType = base_UserInfo.UserType ?? 0;
            _merchId = base_UserInfo.MerchID ?? "";
        }

        private bool IsStoreUser()
        {
            //商户切换门店时有效
            return _logType == (int)RoleType.StoreUser;
        }

        private bool IsMerchUser()
        {
            //门店切换商户时有效
            return _logType == (int)RoleType.MerchUser;
        }
        
        /// <summary>
        /// 礼品调拨工作流
        /// </summary>
        /// <param name="eventId">调拨申请单ID</param>
        /// <param name="userId">当前登录用户ID</param>
        /// <param name="logType">当前用户登录类型</param>
        /// <param name="storeId">当前登录门店ID</param>
        public GoodReqWorkFlow(int eventId, int userId, int logType, string storeId)
        {
            _eventId = eventId;
            _userId = userId;
            _logType = logType;
            _requestType = GetRequestType();
            _workId = GetWorkId();
            _storeId = storeId;
            GetWorkState();
            GetUserInfo();
                                   
            _machine = new StateMachine<State, Trigger>(() => _state, s => _state = s);            

            _setRequestVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.RequestVerify);
            _setSendDealVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.SendDealVerify);
            _setRequestDealVerifyTrigger = _machine.SetTriggerParameters<int>(Trigger.RequestDealVerify);
            
            _machine.Configure(State.Open)
                .PermitIf(Trigger.Request, State.Requested, () => 
                   (
                    (_requestType == (int)RequestType.RequestStore && IsStoreUser()) || 
                    (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) || 
                    (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                   ) && (_merchId == _targetMerchId && _storeId == _inStoreId), "门店或总店申请")
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => 
                    _requestType == (int)RequestType.MerchSend && IsMerchUser()
                    && (_merchId == _targetMerchId && _storeId == _outStoreId), "总部派发");

            _machine.Configure(State.Requested)
                .OnEntryFrom(Trigger.Cancel, requestVerifyCancel, "申请审核撤销")
                .PermitDynamicIf(_setRequestVerifyTrigger, s =>
                    s == 1 ? State.RequestVerifiedPass : State.RequestVerifiedRefuse,
                    () => (
                     (_requestType == (int)RequestType.RequestStore && IsMerchUser())
                    ) && (_merchId == _targetMerchId && _storeId == ""))
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => (
                     _requestType == (int)RequestType.MerchRequest && IsStoreUser() ||
                     _requestType == (int)RequestType.RequestMerch && IsMerchUser()
                    ) && (_merchId == _targetMerchId && _storeId == _outStoreId))
                .PermitIf(Trigger.Delete, State.RequestDeleted, () => (
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
                    (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
                    (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                   ) && (_merchId == _targetMerchId && _storeId == _inStoreId), "门店或总店申请");

            _machine.Configure(State.RequestVerifiedPass)
                .PermitIf(Trigger.Cancel, State.Requested, () => IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
                .PermitIf(Trigger.SendDeal, State.SendDealed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _outStoreId));

            //_machine.Configure(State.RequestVerifiedRefuse)
            //    .PermitIf(Trigger.Close, State.Closed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId));

            _machine.Configure(State.SendDealed)
                .PermitReentryIf(Trigger.SendDeal, () =>
                    (
                     (_requestType == (int)RequestType.MerchRequest && IsStoreUser()) ||
                        //(_requestType == (int)RequestType.MerchSend && IsMerchUser()) ||  //总部是一次性派发
                     (_requestType == (int)RequestType.RequestMerch && IsMerchUser()) ||
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser())
                    ) && (_merchId == _targetMerchId && _storeId == _outStoreId))
                .PermitIf(Trigger.Cancel, State.Closed, () =>
                    _requestType == (int)RequestType.MerchSend && IsMerchUser() && (_merchId == _targetMerchId && _storeId == _outStoreId), "总部派货撤销")
                .PermitIf(Trigger.RequestDeal, State.RequestDealed, () =>
                    (
                     (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
                     (_requestType == (int)RequestType.MerchSend && IsStoreUser()) ||
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
                     (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                    ) && (_merchId == _targetMerchId && _storeId == _inStoreId));
                //.PermitIf(Trigger.RequestExit, State.RequestExited, () =>
                //    (
                //     (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
                //     (_requestType == (int)RequestType.MerchSend && IsStoreUser()) ||
                //     (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
                //     (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                //    ) && (_merchId == _targetMerchId && _storeId == _inStoreId));

            //_machine.Configure(State.SendDealVerifiedPass)
            //    //.PermitIf(Trigger.Cancel, State.SendDealed, () => IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
            //    .PermitIf(Trigger.RequestDeal, State.RequestDealed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId));

            //_machine.Configure(State.SendDealVerifiedRefuse)
            //    //.PermitIf(Trigger.Cancel, State.SendDealed, () => IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
            //    .PermitIf(Trigger.Close, State.Closed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _outStoreId));

            _machine.Configure(State.RequestDealed)
                .PermitReentryIf(Trigger.RequestDeal, () =>
                    (
                     (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
                        //(_requestType == (int)RequestType.MerchSend && IsStoreUser()) || //总部是一次性派发
                     (_requestType == (int)RequestType.RequestMerch && IsStoreUser()) ||
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser())
                    ) && (_merchId == _targetMerchId && _storeId == _inStoreId))
                //.PermitIf(Trigger.Cancel, State.SendDealVerifiedPass, () =>
                //    _requestType == (int)RequestType.RequestStore && IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId), "撤销调拨入库，返回上一步")
                //.PermitIf(Trigger.Cancel, State.SendDealed, () => 
                //    (
                //     (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) || 
                //     (_requestType == (int)RequestType.MerchSend && IsStoreUser()) || 
                //     (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                //    ) && (_merchId == _targetMerchId && _storeId == _inStoreId), "撤销调拨入库，返回上一步")
                //.PermitDynamicIf(_setRequestDealVerifyTrigger, s => s == 1 ? State.RequestDealVerifiedPass : State.RequestDealVerifiedRefuse, () =>
                //    _requestType == (int)RequestType.RequestStore && IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
                .PermitIf(Trigger.Close, State.Closed, () =>
                    (
                     (_requestType == (int)RequestType.MerchRequest && IsMerchUser() && (_merchId == _targetMerchId && _storeId == _inStoreId)) ||
                     (_requestType == (int)RequestType.MerchSend && IsMerchUser() && (_merchId == _targetMerchId && _storeId == _outStoreId)) ||
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId)) ||
                     (_requestType == (int)RequestType.RequestMerch && IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId))
                    ))
                .PermitIf(Trigger.SendDeal, State.SendDealed, () =>
                    (
                     (_requestType == (int)RequestType.MerchRequest && IsStoreUser()) ||
                        //(_requestType == (int)RequestType.MerchSend && IsMerchUser()) || //总部是一次性派发
                     (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
                     (_requestType == (int)RequestType.RequestMerch && IsMerchUser())
                    ) && (_merchId == _targetMerchId && _storeId == _outStoreId));
                //.PermitIf(Trigger.RequestExit, State.RequestExited, () =>
                //    (
                //     (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
                //     (_requestType == (int)RequestType.MerchSend && IsStoreUser()) ||
                //     (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
                //     (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
                //    ) && (_merchId == _targetMerchId && _storeId == _inStoreId));

            //_machine.Configure(State.RequestExited)
            //    .PermitReentryIf(Trigger.RequestExit, () =>
            //        (
            //         (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
            //         (_requestType == (int)RequestType.MerchSend && IsStoreUser()) ||
            //         (_requestType == (int)RequestType.RequestMerch && IsStoreUser()) ||
            //         (_requestType == (int)RequestType.RequestStore && IsStoreUser())
            //        ) && (_merchId == _targetMerchId && _storeId == _inStoreId))
            //    .PermitIf(Trigger.Close, State.Closed, () =>
            //        (
            //         (_requestType == (int)RequestType.MerchRequest && IsMerchUser() && (_merchId == _targetMerchId && _storeId == _inStoreId)) ||
            //         (_requestType == (int)RequestType.MerchSend && IsMerchUser() && (_merchId == _targetMerchId && _storeId == _outStoreId)) ||
            //         (_requestType == (int)RequestType.RequestStore && IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId)) ||
            //         (_requestType == (int)RequestType.RequestMerch && IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId))
            //        ))
            //    .PermitIf(Trigger.SendDeal, State.SendDealed, () =>
            //        (
            //         (_requestType == (int)RequestType.MerchRequest && IsStoreUser()) ||
            //            //(_requestType == (int)RequestType.MerchSend && IsMerchUser()) ||//总部是一次性派发
            //         (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
            //         (_requestType == (int)RequestType.RequestMerch && IsMerchUser())
            //        ) && (_merchId == _targetMerchId && _storeId == _outStoreId))
            //    .PermitIf(Trigger.RequestDeal, State.RequestDealed, () =>
            //        (
            //         (_requestType == (int)RequestType.MerchRequest && IsMerchUser()) ||
            //            //(_requestType == (int)RequestType.MerchSend && IsStoreUser()) || //总部是一次性派发
            //         (_requestType == (int)RequestType.RequestStore && IsStoreUser()) ||
            //         (_requestType == (int)RequestType.RequestMerch && IsStoreUser())
            //        ) && (_merchId == _targetMerchId && _storeId == _inStoreId));

            //_machine.Configure(State.RequestDealVerifiedPass)
            //    //.PermitIf(Trigger.Cancel, State.RequestDealed, () => IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
            //    .PermitIf(Trigger.SendDeal, State.SendDealed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _outStoreId))
            //    .PermitIf(Trigger.Close, State.Closed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId));

            //_machine.Configure(State.RequestDealVerifiedRefuse)
            //    //.PermitIf(Trigger.Cancel, State.RequestDealed, () => IsMerchUser() && (_merchId == _targetMerchId && _storeId == ""))
            //    .PermitIf(Trigger.Close, State.Closed, () => IsStoreUser() && (_merchId == _targetMerchId && _storeId == _inStoreId));

            _machine.Configure(State.Closed)
                .OnEntry(t => OnClosed(), "流程结束");
            _machine.Configure(State.Canceled)
                .OnEntryFrom(Trigger.Cancel, sendDealCancel, "总部派货撤销");
                //.PermitIf(Trigger.RequestExit, State.RequestExited, () =>
                //    (_requestType == (int)RequestType.MerchSend && IsStoreUser()) && (_merchId == _targetMerchId && _storeId == _inStoreId));
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

        private void requestVerifyCancel()
        {
            _errMsg = string.Empty;
            _result = addWorkEntry(_eventId, _workId, _userId, Trigger.Cancel, _state, out _errMsg);
        }

        private void sendDealCancel()
        {
            _errMsg = string.Empty;

            //开启EF事务
            using (TransactionScope ts = new TransactionScope())
            {
                try
                {
                    var data_GoodRequest_ListService = Data_GoodRequest_ListService.I;
                    foreach (var model in data_GoodRequest_ListService.GetModels(p => p.RequestID == _eventId))
                    {
                        //撤销冲红
                        if (!XCCloudBLLExt.UpdateGoodsStock(model.OutDepotID, model.GoodID, (int)SourceType.GoodRequest, model.RequestID, model.CostPrice, (int)StockFlag.In, model.SendCount, model.MerchID, model.OutStoreID, out _errMsg))
                        {
                            _result = false;
                            return;
                        }

                        //已入库的出库
                        if (model.StorageCount > 0)
                        {
                            if (!XCCloudBLLExt.UpdateGoodsStock(model.InDeportID, model.GoodID, (int)SourceType.GoodRequest, model.RequestID, model.CostPrice, (int)StockFlag.Out, model.StorageCount, model.MerchID, model.InStoreID, out _errMsg))
                            {
                                _result = false;
                                return;
                            }
                        }
                    }

                    if (!Data_GoodStock_RecordService.I.SaveChanges())
                    {
                        _errMsg = "添加入库存异动信息失败";
                        _result = false;
                        return;
                    }

                    _result = addWorkEntry(_eventId, _workId, _userId, Trigger.Cancel, _state, out _errMsg);

                    ts.Complete();
                }                
                catch (Exception e)
                {
                    _errMsg = e.Message;
                    _result = false;
                }
            }            
        }

        private bool addWorkEntry(int eventId, int workId, int userId, Trigger t, State s, out string errMsg, string note = "")
        {
            errMsg = string.Empty;

            try
            {
                //获取工作节点
                var workNode = Data_WorkFlow_NodeService.I.GetModels(p => p.WorkID == workId && p.OrderNumber == (int)t).FirstOrDefault();
                if (workNode == null)
                {
                    errMsg = "当前工作节点不存在";
                    return false;
                }

                //生成工作记录
                var nodeId = workNode.ID;
                var entry = new Data_WorkFlow_Entry();
                entry.WorkID = workId;
                entry.EventID = eventId;
                entry.EventType = (int)WorkflowEventType.GoodRequest;
                entry.NodeID = nodeId;
                entry.UserID = userId;
                entry.State = (int)s;
                entry.CreateTime = DateTime.Now;
                entry.Note = note;
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

        public bool Request(out string errMsg)
        {
            errMsg = string.Empty;            

            try
            {               
                if (_machine.CanFire(Trigger.Request))
                {                    
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.Request, State.Requested, out errMsg)) return false;

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

        public bool RequestVerify(int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestVerify))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.RequestVerify, state == 1 ? State.RequestVerifiedPass : State.RequestVerifiedRefuse, out errMsg, note)) return false;

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

        public bool SendDeal(out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.SendDeal))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.SendDeal, State.SendDealed, out errMsg)) return false;

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

        public bool SendDealVerify(int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.SendDealVerify))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.SendDealVerify, state == 1 ? State.SendDealVerifiedPass : State.SendDealVerifiedRefuse, out errMsg, note)) return false;

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

        public bool RequestDeal(out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDeal))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.RequestDeal, State.RequestDealed, out errMsg)) return false;

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

        public bool Delete(out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.Delete))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.Delete, State.RequestDeleted, out errMsg)) return false;

                    _machine.Fire(Trigger.Delete);

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

        public bool RequestDealVerify(int state, string note, out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDealVerify))
                {

                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.RequestDealVerify, state == 1 ? State.RequestDealVerifiedPass : State.RequestDealVerifiedRefuse, out errMsg, note)) return false;

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

        public bool Close(out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.Close))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.Close, State.Closed, out errMsg)) return false;

                    _machine.Fire(Trigger.Close);

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

        public bool Cancel(out string errMsg)
        {
            errMsg = string.Empty;
            
            try
            {
                if (_machine.CanFire(Trigger.Cancel))
                {                                                                                                      
                    _machine.Fire(Trigger.Cancel);

                    if (!_result)
                    {
                        errMsg = this._errMsg;
                        return _result;
                    } 

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

        public bool RequestExit(out string errMsg)
        {
            errMsg = string.Empty;

            try
            {
                if (_machine.CanFire(Trigger.RequestDeal))
                {
                    if (!addWorkEntry(_eventId, _workId, _userId, Trigger.RequestExit, State.RequestExited, out errMsg)) return false;

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

        #endregion

    }
}
