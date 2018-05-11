using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.Common;
using XCCloudService.Common.Enum;

namespace XCCloudService.WorkFlow.Base
{
    public abstract partial class BaseWorkFlow<TState, TTrigger>
    {
        protected StateMachine<TState, TTrigger> _machine;        
        protected DateTime _lastUpdate;
        protected string _description;

        protected TState State
        {
            get
            {
                return _machine.State;
            }            
        }
        
        public string Description
        {
            get
            {
                return _description;
            }
        }

        public DateTime LastUpdate
        {
            get
            {
                return _lastUpdate;
            }
        }

        public bool CanFire(TTrigger trigger)
        {
            return _machine.CanFire(trigger);
        }

        public void Print()
        {
            LogHelper.SaveLog(TxtLogType.WorkFlow, string.Format("[Status:] {0}", _machine));
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(_machine.GetInfo());
        }        
    }
}
