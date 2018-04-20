using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XCCloudService.DAL.Container;
using XCCloudService.DAL.IDAL.XCCloud;
using XCCloudService.BLL.Base;
using XCCloudService.BLL.IBLL.XCCloud;
using XCCloudService.Model.XCCloud;
namespace XCCloudService.BLL.XCCloud
{
	public class Log_GameAlarmService : BaseService<Log_GameAlarm>, ILog_GameAlarmService
	{
        public override void SetDal()
        {
        	
        }
        
        public Log_GameAlarmService()
        	: this(false)
        {
            
        }
        
        public Log_GameAlarmService(bool resolveNew)
        {
            Dal = DALContainer.Resolve<ILog_GameAlarmDAL>(resolveNew: resolveNew);
        }
	} 
}