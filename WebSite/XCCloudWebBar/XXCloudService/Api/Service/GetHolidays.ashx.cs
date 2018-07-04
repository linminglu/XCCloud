using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using XCCloudWebBar.Base;
using XCCloudWebBar.BLL.XCCloud;
using XCCloudWebBar.Common;
using XCCloudWebBar.Model.XCCloud;

namespace XXCloudService.Api.Service
{
    /// <summary>
    /// GetHolidays 的摘要说明
    /// </summary>
    public class GetHolidays : ApiBase
    {

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object IsHoliday(Dictionary<string, object> dicParas)
        {
            try
            {
                string errMsg = string.Empty;
                string date = dicParas.ContainsKey("date") ? Convert.ToString(dicParas["date"]) : string.Empty;

                //DateTimeFormatInfo dtFormat = new System.Globalization.DateTimeFormatInfo();
                //dtFormat.ShortDatePattern = "yyyyMMdd";
                //var dt = Convert.ToDateTime(date, dtFormat);
                var dt = DateTime.ParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                var result = HolidayHelper.GetInstance().IsHoliday(dt);
                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn, result);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }

        [ApiMethodAttribute(SignKeyEnum = SignKeyEnum.MethodToken, SysIdAndVersionNo = false)]
        public object ExportToDatabase(Dictionary<string, object> dicParas)
        {
            try
            {
                var errMsg = string.Empty;

                var list = HolidayHelper.GetConfigList();
                var xC_HolidayListService = XC_HolidayListService.I;
                foreach (var dateModel in list)
                {
                    foreach (var work in dateModel.Work)
                    {
                        var model = new XC_HolidayList();
                        model.DayType = 0;
                        model.WorkDay = DateTime.ParseExact(dateModel.Year + work, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        xC_HolidayListService.AddModel(model);
                    }

                    foreach (var dayOff in dateModel.DayOff)
                    {
                        var model = new XC_HolidayList();
                        model.DayType = 1;
                        model.WorkDay = DateTime.ParseExact(dateModel.Year + dayOff, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        xC_HolidayListService.AddModel(model);
                    }

                    foreach (var holiday in dateModel.Holiday)
                    {
                        var model = new XC_HolidayList();
                        model.DayType = 2;
                        model.WorkDay = DateTime.ParseExact(dateModel.Year + holiday, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                        xC_HolidayListService.AddModel(model);
                    }
                }

                if (!xC_HolidayListService.SaveChanges())
                {
                    errMsg = "保存失败";
                    return ResponseModelFactory.CreateFailModel(isSignKeyReturn, errMsg); 
                }

                return ResponseModelFactory.CreateSuccessModel(isSignKeyReturn);
            }
            catch (Exception e)
            {
                return ResponseModelFactory.CreateReturnModel(isSignKeyReturn, Return_Code.F, e.Message);
            }
        }
    }
}