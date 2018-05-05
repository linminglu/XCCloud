using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace XCCloudService.Common
{
    public static class ExcelHelper
    {
        /// <summary>
        /// 获取单元格的值
        /// </summary>
        /// <param name="sheet">Excel sheet表名称</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="cellIndex">列索引</param>
        /// <returns>行索引和列索引从0开始</returns>
        ///这个方法是用来检查如果你不知道你的单元格里的值是什么类型的就可以用这个方法检查
        ///返回的字符串代表了单元格内容的值类型
        public static string GetCellValue(this ISheet sheet, int rowIndex, int cellIndex)
        {
            string returnValue = string.Empty;
            //拼接的条件
            if (sheet != null)
            {
                //如果当前的sheet不等于空  则获取索引行的值
                var row = sheet.GetRow(rowIndex);
                if (row != null)
                {
                    //如果行内容不为空 则获取列
                    var cell = row.GetCell(cellIndex);
                    if (cell != null)
                    {
                      //列也不为空则判断列中的值的类型
                        switch (cell.CellType)
                        {
                          //如果为string类型，则返回String类型
                            case CellType.String:
                                returnValue = cell.StringCellValue;
                                break;
                          //如果是数字类类型
                            case CellType.Numeric:
                            //判断是否为日期类型
                                if (DateUtil.IsCellDateFormatted(cell))
                                {
                                //是日期类型则转化成日期输出
                                    returnValue =   DateTime.FromOADate(cell.NumericCellValue).ToString();
                                }
                                else
                                {
                                //输出数字类型
                                    returnValue = Convert.ToDouble(cell.NumericCellValue).ToString();
                                }
                                break;
                                //是否是bool类型
                            case CellType.Boolean:
                                returnValue = Convert.ToString(cell.BooleanCellValue);
                                break;
                                //错误函数类型
                            case CellType.Error:
                                returnValue = ErrorEval.GetText(cell.ErrorCellValue);
                                break;
                            case CellType.Formula:
                                switch (cell.CachedFormulaResultType)
                                {
                                    case CellType.String:
                                        string strFORMULA = cell.StringCellValue;
                                        if (strFORMULA != null && strFORMULA.Length > 0)
                                        {
                                            returnValue = strFORMULA.ToString();
                                        }
                                        break;
                                    case CellType.Numeric:
                                        returnValue = Convert.ToString(cell.NumericCellValue);
                                        break;
                                    case CellType.Boolean:
                                        returnValue = Convert.ToString(cell.BooleanCellValue);
                                        break;
                                    case CellType.Error:
                                        returnValue = ErrorEval.GetText(cell.ErrorCellValue);
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:

                                break;
                        }
                    }
                }
            }
            return returnValue.Trim();
        }
        /// <summary>
        /// Excel导入
        /// </summary>
        /// <typeparam name="T">这个方法是将Excle转化成泛型导入</typeparam>
        /// <param name="columns">数组形式 列头</param>
        /// <param name="excelStream">文件流</param>
        /// <param name="excelType">文件类型</param>
        /// <returns>这是Excel导入List泛型导入</returns>
        public static List<T> ReadExcel<T>(string[] columns, Stream excelStream, string excelType = ".xlsx")
        {
            //先建立一个泛型
            var result = new List<T>();
            try
            {
              //创建一个空的文件薄
                IWorkbook workbook = null;
                //如果文件类型为xlsx
                //做这个判断使检测Excel的版本的
                if (excelType == ".xlsx")
                {
                //空的文件博等于XSSFWorkbook读取文件流
                    workbook = new XSSFWorkbook(excelStream);
                }
                else
                {
                    //如果是.xls就是HSSFWorkbook读取文件流  
                    workbook = new HSSFWorkbook(excelStream);
                }
                //工作簿是否大于零？如果大于零获取第一个工作薄的内容，否则空值
                ISheet sheet = workbook.NumberOfSheets > 0 ? workbook.GetSheetAt(0) : null;
                if (sheet == null)
                {
                //工作薄如果为空值  转化失败
                    return result;
                }
                //这里定义的是反射类型
                var type = typeof(T);
                //遍历工作薄的内容
                for (int j = 1; j <= sheet.LastRowNum; j++)//第一行默认是表头
                {

                    //动态创建动态类
                    var model = Activator.CreateInstance(type);
                    //行的值来自于sheet获取坐标为j的内容 因为第一行默认为表头，所以j从1开始
                    IRow row = sheet.GetRow(j);
                    if (row != null)
                    {
                        //columns导入的Excel标题头  
                        for (int k = 0; k < columns.Length; k++)
                        {
                            //pro得到的列头的类型
                            var pro = type.GetProperty(columns[k]);
                            if (pro != null)
                            {
                                 //声明一个弱类型
                                dynamic value;
                                //列值获取
                                ICell cell = row.GetCell(k);
                                //处理获取到的列值类型
                                switch (cell.CellType)
                                {
                                    case CellType.Blank: //空数据类型处理
                                        value = null;
                                        break;
                                    case CellType.String: //字符串类型
                                        value = cell.StringCellValue;
                                        break;
                                    case CellType.Numeric: //数字类型                                   
                                        if (DateUtil.IsCellDateFormatted(cell))//日期类型
                                        {
                                            value = cell.DateCellValue;
                                        }
                                        else//其他数字类型
                                        {
                                            value = cell.NumericCellValue;
                                        }
                                        break;
                                    case CellType.Formula:
                                        HSSFFormulaEvaluator e = new HSSFFormulaEvaluator(workbook);
                                        value = e.Evaluate(cell).StringValue;
                                        break;
                                    default:
                                        value = null;
                                        break;
                                }
                                if (value != null)
                                {

                                    if (pro.GetType().Name == typeof(string).Name)
                                    {
                                    //插入值
                                        pro.SetValue(model, Convert.ChangeType(value.ToString(), pro.PropertyType), null);
                                    }
                                    else
                                        pro.SetValue(model, Convert.ChangeType(value,   pro.PropertyType), null);
                                }
                            }
                        }
                        //将得到的值插入到T中（将T强制转化为Model类）
                        result.Add((T)model);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("excel读取失败", ex);
            }
            return result;
        }

        /// <summary>
        /// 将sheet中的数据导出到datatable中
        /// </summary>
        /// <param name="sheet">需要导出的sheet</param>
        /// <param name="HeaderRowIndex">表头所在行号，-1表示没有表头</param>
        /// <returns>将Excel导出成Datatable</returns>
        private static DataTable ImportDt(ISheet sheet, int HeaderRowIndex, bool needHeader)
        {
            //新创建一个datatable
            DataTable table = new DataTable();
            IRow headerRow;
            int cellCount;
            try
            {
                //表头索引小于零时  输出表头
                if (HeaderRowIndex < 0 || !needHeader)
                {
                    //获取行索引为sheet表第一行
                    headerRow = sheet.GetRow(0);
                    //获取不为空的列个数
                    cellCount = headerRow.getPhysicalNumberOfCells;
                    //获取第一列的值
                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        DataColumn column = new DataColumn(Convert.ToString(i));
                        table.Columns.Add(column);
                    }
                }
                else
                {
                    //获取行内容
                    headerRow = sheet.GetRow(HeaderRowIndex);
                    cellCount = headerRow.LastCellNum;
                    //获取第一行第一个单元格内容
                    for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
                    {
                        if (headerRow.GetCell(i) == null)
                        {
                            if (table.Columns.IndexOf(Convert.ToString(i)) > 0)
                            {
                                DataColumn column = new DataColumn(Convert.ToString("重复列名" + i));
                                table.Columns.Add(column);
                            }
                            else
                            {
                                DataColumn column = new DataColumn(Convert.ToString(i));
                                table.Columns.Add(column);
                            }

                        }
                        else if (table.Columns.IndexOf(headerRow.GetCell(i).ToString()) > 0)
                        {
                            DataColumn column = new DataColumn(Convert.ToString("重复列名" + i));
                            table.Columns.Add(column);
                        }
                        else
                        {
                            DataColumn column = new DataColumn(headerRow.GetCell(i).ToString());
                            table.Columns.Add(column);
                        }
                    }
                }
                int rowCount = sheet.LastRowNum;
                for (int i = (HeaderRowIndex + 1); i <= sheet.LastRowNum; i++)
                {
                    try
                    {
                        IRow row;
                        if (sheet.GetRow(i) == null)
                        {
                            row = sheet.CreateRow(i);
                        }
                        else
                        {
                            row = sheet.GetRow(i);
                        }

                        DataRow dataRow = table.NewRow();

                        for (int j = row.FirstCellNum; j <= cellCount; j++)
                        {
                            try
                            {
                                if (row.GetCell(j) != null)
                                {
                                    switch (row.GetCell(j).CellType)
                                    {
                                        case CellType.String:
                                            string str = row.GetCell(j).StringCellValue;
                                            if (str != null && str.Length > 0)
                                            {
                                                dataRow[j] = str.ToString();
                                            }
                                            else
                                            {
                                                dataRow[j] = null;
                                            }
                                            break;
                                        case CellType.Numeric:
                                            if (DateUtil.IsCellDateFormatted(row.GetCell(j)))
                                            {
                                                dataRow[j] = DateTime.FromOADate(row.GetCell(j).NumericCellValue);
                                            }
                                            else
                                            {
                                                dataRow[j] = Convert.ToDouble(row.GetCell(j).NumericCellValue);
                                            }
                                            break;
                                        case CellType.Boolean:
                                            dataRow[j] = Convert.ToString(row.GetCell(j).BooleanCellValue);
                                            break;
                                        case CellType.Error:
                                            dataRow[j] = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                                            break;
                                        case CellType.Formula:
                                            switch (row.GetCell(j).CachedFormulaResultType)
                                            {
                                                case CellType.String:
                                                    string strFORMULA = row.GetCell(j).StringCellValue;
                                                    if (strFORMULA != null && strFORMULA.Length > 0)
                                                    {
                                                        dataRow[j] = strFORMULA.ToString();
                                                    }
                                                    else
                                                    {
                                                        dataRow[j] = null;
                                                    }
                                                    break;
                                                case CellType.Numeric:
                                                    dataRow[j] = Convert.ToString(row.GetCell(j).NumericCellValue);
                                                    break;
                                                case CellType.Boolean:
                                                    dataRow[j] = Convert.ToString(row.GetCell(j).BooleanCellValue);
                                                    break;
                                                case CellType.Error:
                                                    dataRow[j] = ErrorEval.GetText(row.GetCell(j).ErrorCellValue);
                                                    break;
                                                default:
                                                    dataRow[j] = "";
                                                    break;
                                            }
                                            break;
                                        default:
                                            dataRow[j] = "";
                                            break;
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                LogHelper.SaveLog("ExcelHelper错误:" + exception.ToString());
                            }
                        }
                        table.Rows.Add(dataRow);
                    }
                    catch (Exception exception)
                    {
                        LogHelper.SaveLog("ExcelHelper错误:" + exception.ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.SaveLog("ExcelHelper错误:" + exception.ToString());
            }
            return table;
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        /// <param name="file"></param>
        /// <returns>例子用的是list集合</returns>
        public JsonResult Import(HttpPostedFileBase file)
        {
            //接受到的文件流为空或者大小为0
            if (file == null || file.ContentLength == 0)
            {
                return Json(AjaxResult.Error("请选择上传文件"));
            }
            //获取文件名称
            var lastnam = Path.GetExtension(file.FileName);
            string[] fileType = { ".xls", ".xlsx" };
            if (!fileType.Contains(lastnam.ToLower()))
            {
                return Json(new
                {
                    Message = "请上传Exce文件"
                });
            }
            //要导入的Excel匹配类型 VM_Athletes是实体类 和Excel是要一一对应的
            var list = ExcelHelper.ReadExcel<VM_Athletes>(new[] {
                "Name",
                "SexText",
                "Mobile",
                "Email",
                "QQ",
                "IDCord",
                "Birthday",
                "Height",
                "Weight",
                "Department",
                "Remark",
            }, file.InputStream, lastnam);
            //这一步可以加也可以不加，因为在帮助类中已经对空值做了处理
            //加上可以让你更加明确是Excel导入时出现问题没有数据
            if (list.Count == 0)
            {
                return Json(new
                {
                    Message = "导入的Excel为空",
                    IsSuccess = "2",
                });
            }
            //将相同手机号分组，获取到的手机号重复的
            var mobileDict = list.GroupBy(t => t.Mobile).ToDictionary(t => t.Key);
            //创建新的集合
            var repeatList = new List<VM_Athletes>();
            var unRepeatList = new List<VM_Athletes>();
            //将去除重复后的值赋值给新的list集合
            foreach (var item in mobileDict)
            {
                if (item.Value.Count() > 1)
                {
                    repeatList.AddRange(item.Value);
                    continue;
                }
                unRepeatList.Add(item.Value.FirstOrDefault());
            }
        }


        /// <summary>
        /// 提供流导出excel
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileName">文件名</param>
        /// <param name="stream">文件流</param>
        public static void ExcelExport(this HttpContextBase context, string fileName, Stream stream)
        {
            //检查文件名
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("filename is null.");
            }
            //检查末尾是否为.xls或xlsx如果不是返回低版本xls
            if (!(fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx")))
            {
                fileName = fileName + ".xls";
            }
            //检查文件流是否为空
            if (stream == null)
            {
                throw new ArgumentException("stream is null.");
            }
            //通知转换格式为excel
            context.Response.ContentType = "application/vnd.ms-excel";

            context.Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fileName));
            //将流变成二进制数组
            var length = stream.Length;
            var bytes = new byte[length];
            stream.Read(bytes, 0, (int)length);
            //输出流
            context.Response.OutputStream.Write(bytes, 0, (int)length);
            context.Response.End();
        }

        /// <summary>
        /// 提供WorkBook文件导出
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileName"></param>
        /// <param name="workbook"></param>
        public static void ExcelExport(this HttpContextBase context, string fileName, Workbook workbook)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("filename is null.");
            }
            if (!(fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx")))
            {
                fileName = fileName + ".xls";
            }
            if (workbook == null)
            {
                throw new ArgumentException("workbook is null.");
            }
            context.Response.ContentType = "application/vnd.ms-excel";
            context.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
            workbook.Save(context.Response.OutputStream, FileFormatType.Excel97To2003);
            context.Response.End();
        }
    }        
}
