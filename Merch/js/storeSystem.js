﻿var now = new Date();                    //当前日期
var nowDayOfWeek = now.getDay();         //今天本周的第几天
var nowDay = now.getDate();              //当前日
var nowMonth = now.getMonth();           //当前月
var nowYear = now.getYear();             //当前年
nowYear += (nowYear < 2000) ? 1900 : 0;  //

//格式化日期：yyyy-MM-dd
function formatDate(date) {
    var myyear = date.getFullYear();
    var mymonth = date.getMonth()+1;
    var myweekday = date.getDate();
    if(mymonth < 10){
        mymonth = "0" + mymonth;
    }
    if(myweekday < 10){
        myweekday = "0" + myweekday;
    }
    // return (myyear+"-"+mymonth + "-" + myweekday);

    //格式化日期：yyyy-MM-dd HH:MM:SS
    var myHour = date.getHours();
    var myMinute = date.getMinutes();
    var mySecond = date.getSeconds();
    if(myHour < 10){
        myHour = "0" + myHour;
    }
    if(myMinute < 10){
        myMinute = "0" + myMinute;
    }
    if(mySecond < 10){
        mySecond = "0" + mySecond;
    }
    return (myyear+"-"+mymonth + "-" + myweekday+" "+myHour+":"+myMinute+":"+mySecond);
}
//获得某月的天数
function getMonthDays(myMonth){
    var monthStartDate = new Date(nowYear, myMonth, 1);
    var monthEndDate = new Date(nowYear, myMonth + 1, 1);
    var   days   =   (monthEndDate   -   monthStartDate)/(1000   *   60   *   60   *   24);
    return   days;
}
//获得本季度的开始月份
function getQuarterStartMonth(){
    var quarterStartMonth = 0;
    if(nowMonth<3){
        quarterStartMonth = 0;
    }
    if(2<nowMonth && nowMonth<6){
        quarterStartMonth = 3;
    }
    if(5<nowMonth && nowMonth<9){
        quarterStartMonth = 6;
    }
    if(nowMonth>8){
        quarterStartMonth = 9;
    }
    return quarterStartMonth;
}
//获得本天(i=0)的开始时间
function getDayStartDate() {
    var nowStartDay = new Date(nowYear, nowMonth, nowDay);
    return formatDate(nowStartDay).slice(0,10);
}
//获得本天(i=0)的结束时间
function getDayEndDate() {
    var nowStartDay = new Date(nowYear, nowMonth, nowDay,23,59,59);
    return formatDate(nowStartDay).slice(0,10);
}
//获得本周(i=0)的开始日期
function getWeekStartDate() {
    // var weekStartDate = new Date(nowYear, nowMonth, nowDay - nowDayOfWeek);
    var weekStartDate = new Date(nowYear, nowMonth, nowDay - nowDayOfWeek+1);
    return formatDate(weekStartDate).slice(0,10);
}
//获得本周的结束日期
function getWeekEndDate() {
    // var weekEndDate = new Date(nowYear, nowMonth, nowDay + (6 - nowDayOfWeek));
    var weekEndDate = new Date(nowYear, nowMonth, nowDay + (7 - nowDayOfWeek),23,59,59);
    return formatDate(weekEndDate).slice(0,10);
}
//获得本月的开始日期
function getMonthStartDate(){
    //var monthStartDate = new Date(nowYear, nowMonth, 1);
    var monthStartDate = new Date(nowYear, nowMonth, 1);
    return formatDate(monthStartDate).slice(0,10);
}
//获得本月的结束日期
function getMonthEndDate(){
    //var monthEndDate = new Date(nowYear, nowMonth, getMonthDays(nowMonth));
    var monthEndDate = new Date(nowYear, nowMonth, getMonthDays(nowMonth),23,59,59);
    return formatDate(monthEndDate).slice(0,10);
}
//获得本季度的开始日期
function getQuarterStartDate(){
    //var quarterStartDate = new Date(nowYear, getQuarterStartMonth(), 1);
    var quarterStartDate = new Date(nowYear, getQuarterStartMonth(), 1);
    return formatDate(quarterStartDate).slice(0,10);
}

//或的本季度的结束日期
function getQuarterEndDate(){
    var quarterEndMonth = getQuarterStartMonth() + 2;
    //var quarterStartDate = new Date(nowYear, quarterEndMonth, getMonthDays(quarterEndMonth));
    var quarterStartDate = new Date(nowYear, quarterEndMonth, getMonthDays(quarterEndMonth),23,59,59);
    return formatDate(quarterStartDate).slice(0,10);
}

//获得本年度的开始日期
function getYearStartDate(){
    var quarterStartDate = new Date(nowYear, 0, 1);
    return formatDate(quarterStartDate).slice(0,10);
}

//获得本年度的结束日期
function getYearEndDate(){
    var quarterStartDate = new Date(nowYear, 11, getMonthDays(11),23,59,59);
    return formatDate(quarterStartDate).slice(0,10);
}
//定义公用方法
var xcActionSystem=xcActionSystem || {};
xcActionSystem.prototype= {
    //登录
    login:function(){
        window.localStorage.clear();
        var username=$('#username_login').val();
        var password=$('#password_login').val();
        var obj={'userName':username,'password':password,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        var url='/XCCloud/Login?action=CheckUser';
        var parasJson = JSON.stringify(obj);
        if(username!=''&&password!=''){
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: { parasJson: parasJson },
                success: function (data) {
                    data=JSON.parse(data);
                    console.log(data);
                    if(data.result_code==1){
                        setStorage('logMsg',JSON.stringify(data.result_data));
                        setStorage('token',data.result_data.token);
                        setStorage('usernames',username);
                        if(data.result_data.logType==2){
                            window.location.href='indexStore.html?'+(Date.parse(new Date())/1000);
                        }else {
                            window.location.href='index1.html?'+(Date.parse(new Date())/1000);
                        }
                    }
                }
            })
        }
},
    initLeftMenu: function (layuiFilterName) {
        layui.use('element', function () {
            var element = layui.element;
            element.render('nav', layuiFilterName);
        });
    },
    createTopMenu: function (nodeClassName) {
        layui.use(['element', 'form', 'layer', 'jquery'], function () {
            var element = layui.element,
                $ = layui.jquery,
                form = layui.form,
                layer = layui.layer;
            $('.xcLeftMenu').find('a[class=' + nodeClassName + ']').on('click', function () {
                var _text = $(this).text();
                var _id = $(this).attr('id');
                if (_text == '数字币入库') {
                    layer.open({
                        type: 2,
                        content: ['digitStorage.html', 'no'],
                        closeBtn: true,
                        shade: true,
                        shadeClose: true,
                        area: ['800px', '500px']
                    });
                } else {
                    var flag = false;
                    var _length = $("#xcTabMenu").find("li").length;
                    for (var i = 0; i < _length; i++) {
                        if ($("#xcTabMenu").find("li").eq(i).attr('lay-id') == _id) {
                            flag = true;
                        }
                    }
                    if (flag == true) {
                        element.tabChange('demo', _id);
                    } else {
                        if (_length == 10) {
                            var thisId = $("#xcTabMenu").find("li").eq(1).attr('lay-id');
                            element.tabDelete('demo', thisId);
                            element.tabAdd('demo', {
                                title: _text
                                , content: '<iframe src="' + _id + '" style="width: 100%;height: 100%"></iframe>'	 //支持传入html
                                , id: _id
                            });
                        } else {
                            element.tabAdd('demo', {
                                title: _text
                                , content: '<iframe src="' + _id + '" style="width: 100%;height: 100%"></iframe>'	 //支持传入html
                                , id: _id
                            });
                            element.tabChange('demo', _id);
                        }

                    }
                }


            });
            form.render();
        })
    },
    setStorage: function (key, value) {
        if (!window.localStorage) {
            layui.use('layer', function () {
                var layer = layui.layer;
                layer.msg("当前浏览器不支持该网站，为了您的体验请使用最新浏览器！")
            });
        } else {
            var storage = window.localStorage;
            storage.setItem(key, value);
        }
    },
    getStorage: function (key) {
        if (!window.localStorage) {
            layui.use('layer', function () {
                var layer = layui.layer;
                layer.msg("当前浏览器不支持该网站，为了您的体验请使用最新浏览器！")
            });
        } else {
            var storage = window.localStorage;
            value = localStorage.getItem(key);
            return value;
        }
    },
    removeStorage: function (key) {
        if(!window.localStorage){
            layui.use('layer', function() {
                var layer = layui.layer;
                layer.msg("当前浏览器不支持该网站，为了您的体验请使用最新浏览器！")
            });
        }else {
            var storage=window.localStorage;
            storage.removeItem(key);
        }
    },
    getInitData: function (parm) {
         layui.use(['table', 'layer'], function () {
                        var table = layui.table;
                        var layer = layui.layer;
                        var index = layer.load(0, {shade: false});
                        var tableData = [], obj = parm.obj, url = parm.url;
                        $.ajax({
                            type: "post", url: url,
                            contentType: "application/json; charset=utf-8",
                            data: {parasJson: JSON.stringify(obj)},
                            success: function (data) {
                                data = JSON.parse(data);
                                console.log(data);
                                if (data.Result_Code == "1"||data.result_code==1) {
                                    tableData = data.Result_Data||data.result_data;
                                    table.render({
                                            elem: parm.elem
                                            , data: tableData
                                            , height:'full-125'
                                            , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                                            , cols: [parm.cols]
                                            , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                                            , limit: 10
                                            ,total:parm.total
                                            ,done:function () {
                                             if(this.total){
                                                let _col=this.cols[0];
                                                let _data=this.data;
                                                var intHtml = '<tr style="background-color: #93D1FF">';
                                                intHtml+='<td style="text-align:center;">合计:'+this.data.length+'条</td>';
                                                for(let i=1;i<_col.length;i++){
                                                    if(_col[i].total){
                                                        let index=_col[i].field;
                                                        let count=0;
                                                        for(let j in _data){
                                                            count+=parseInt(_data[j][index])
                                                        }
                                                        intHtml+='<td style="color: red;font-weight: bold;text-align:center;">'+count+'</td>'
                                                    }else {
                                                        intHtml+='<td></td>'
                                                    }
                                                }
                                                $(".layui-table-body.layui-table-main tbody").append(intHtml);
                                            }
                                            }
                                        });
                                    layer.close(index);
                                }  else {
                                 layer.msg(data.result_msg||data.return_msg);
                                }
                            }
                        })
                    });
    },
    closeAll: function (layer) {
        layer.closeAll();
    },
    //时间戳转日期  yy--mm--dd  hh--mm-ss
    timeStamp2String: function (time) {
        if (time != null) {
            var datetime = new Date();
            datetime.setTime(time.substring(6, 19));
            var year = datetime.getFullYear();
            var month = datetime.getMonth() + 1 < 10 ? "0" + (datetime.getMonth() + 1) : datetime.getMonth() + 1;
            var date = datetime.getDate() < 10 ? "0" + datetime.getDate() : datetime.getDate();
            var hour = datetime.getHours() < 10 ? "0" + datetime.getHours() : datetime.getHours();
            var minute = datetime.getMinutes() < 10 ? "0" + datetime.getMinutes() : datetime.getMinutes();
            var second = datetime.getSeconds() < 10 ? "0" + datetime.getSeconds() : datetime.getSeconds();
            return year + "-" + month + "-" + date + " " + hour + ":" + minute + ":" + second;
        } else {
            return "";
        }
    },
    timeStamp3String: function (time) {
        if (time != null) {
            var datetime = new Date();
            datetime.setTime(time.substring(6, 19));
            var year = datetime.getFullYear();
            var month = datetime.getMonth() + 1 < 10 ? "0" + (datetime.getMonth() + 1) : datetime.getMonth() + 1;
            var date = datetime.getDate() < 10 ? "0" + datetime.getDate() : datetime.getDate();
            var hour = datetime.getHours() < 10 ? "0" + datetime.getHours() : datetime.getHours();
            var minute = datetime.getMinutes() < 10 ? "0" + datetime.getMinutes() : datetime.getMinutes();
            var second = datetime.getSeconds() < 10 ? "0" + datetime.getSeconds() : datetime.getSeconds();
            return  hour + ":" + minute + ":" + second;
        } else {
            return "";
        }
    },
    timeStampDate: function (time) {
        if (time != null) {
            var datetime = new Date();
            datetime.setTime(time.substring(6, 19));
            var year = datetime.getFullYear();
            var month = datetime.getMonth() + 1 < 10 ? "0" + (datetime.getMonth() + 1) : datetime.getMonth() + 1;
            var date = datetime.getDate() < 10 ? "0" + datetime.getDate() : datetime.getDate();
            var hour = datetime.getHours() < 10 ? "0" + datetime.getHours() : datetime.getHours();
            var minute = datetime.getMinutes() < 10 ? "0" + datetime.getMinutes() : datetime.getMinutes();
            var second = datetime.getSeconds() < 10 ? "0" + datetime.getSeconds() : datetime.getSeconds();
            return year + "-" + month + "-" + date;
        } else {
            return "";
        }
    },
    //时间戳转日期  yy--mm--dd
    dateStamp: function (time) {
        if (time != null) {
            var datetime = new Date();
            datetime.setTime(time.substring(6, 19));
            var year = datetime.getFullYear();
            var month = datetime.getMonth() + 1 < 10 ? "0" + (datetime.getMonth() + 1) : datetime.getMonth() + 1;
            var date = datetime.getDate() < 10 ? "0" + datetime.getDate() : datetime.getDate();
            var hour = datetime.getHours() < 10 ? "0" + datetime.getHours() : datetime.getHours();
            var minute = datetime.getMinutes() < 10 ? "0" + datetime.getMinutes() : datetime.getMinutes();
            var second = datetime.getSeconds() < 10 ? "0" + datetime.getSeconds() : datetime.getSeconds();
            return year + "-" + month + "-" + date;
        } else {
            return "";
        }
    },
    // 导出表格
    JSONToExcelConvertor : function (JSONData, FileName, cols) {
    //先转化json
    var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;
    var excel = '<table>';
    //设置表头
    var row = "<tr>";
    for (var i = 0, l = cols.length; i < l; i++) {
        row += "<td>" + cols[i].title +"("+cols[i].field+")"+ '</td>';
    }
    //换行
    excel += row + "</tr>";
    for(let i in arr){
        var row = "<tr>";
        for(let j in arr[i]){
            // console.log(arr[i][j])
            var value = arr[i][j] =="" ? "--" : arr[i][j];
            row += '<td style="text-align: center">' + value + '</td>';
        }
        excel += row + "</tr>";
    }

    excel += "</table>";
    var excelFile = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>";
    excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">';
    excelFile += '<meta http-equiv="content-type" content="application/vnd.ms-excel';
    excelFile += '; charset=UTF-8">';
    excelFile += "<head>";
    excelFile += "<!--[if gte mso 9]>";
    excelFile += "<xml>";
    excelFile += "<x:ExcelWorkbook>";
    excelFile += "<x:ExcelWorksheets>";
    excelFile += "<x:ExcelWorksheet>";
    excelFile += "<x:Name>";
    excelFile += "{worksheet}";
    excelFile += "</x:Name>";
    excelFile += "<x:WorksheetOptions>";
    excelFile += "<x:DisplayGridlines/>";
    excelFile += "</x:WorksheetOptions>";
    excelFile += "</x:ExcelWorksheet>";
    excelFile += "</x:ExcelWorksheets>";
    excelFile += "</x:ExcelWorkbook>";
    excelFile += "</xml>";
    excelFile += "<![endif]-->";
    excelFile += "</head>";
    excelFile += "<body>";
    excelFile += excel;
    excelFile += "</body>";
    excelFile += "</html>";
    var uri = 'data:application/vnd.ms-excel;charset=utf-8,' + encodeURIComponent(excelFile);
    var link = document.createElement("a");
    link.href = uri;
    link.style = "visibility:hidden";
    link.download = FileName + ".xls";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
},
    //弹出框
    showLayer: function (layer,title,types,areas,shades,contents) {
        layer.open({
            title:title,
            type:types,
            area:areas,
            shade:shades,
            scrollbar:true,
            content:contents
        })
    },
    //代币管理..........................................................................................................
    // .........................................代币入库.........................................
    AddCoinStorage: function (obj, token, layer) {
        obj.on('click', function() {
            layer.confirm('确定代币入库？', {
                btn: ['确定', '取消'] //按钮
            }, function () {
                let storageCounts = $('#storageCount').val();
                let notes = $('#storageNote').val();
                let obj = {
                    'storageCount': storageCounts,
                    'note': notes,
                    'userToken': token,
                    'signkey': '1f626576304bf5d95b72ece2222e42c3'
                };
                let url = '/XCCloud/Coins?action=AddCoinStorage';
                let parseJson = JSON.stringify(obj);
                $.ajax({
                    type: "post",
                    url: url,
                    contentType: "application/json; charset=utf-8",
                    data: {parasJson: parseJson},
                    success: function (data) {
                        data = JSON.parse(data);
                        console.log(data);
                        if (data.result_code == 1) {
                            layer.msg('代币入库成功');
                            $('#searchCoinBtn').trigger('click');
                        } else {
                            layer.msg(data.result_msg);
                        }
                    }
                })
            }, function () {
            });
        });
    },
    //查询代币入库
    SearchCoinStorage: function (obj, token, layer) {
        obj.on('click', function() {
            var tableData = [];
            let destroyTimes = $('#storageTime').val();
            let obj = {'destroyTime': destroyTimes, 'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
            let url = '/XCCloud/Coins?action=GetCoinStorage';
            let parseJson = JSON.stringify(obj);
            var index = layer.load(0, {shade: false});
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.Result_Code == 1) {
                        tableData = data.Result_Data;
                        layui.use(['table'], function () {
                            var table = layui.table;
                                layer.close(index);
                            table.render({
                                elem: '#tokenStorage'
                                , data: tableData
                                // ,height:full-200
                                , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                                , cols: [[
                                   {field: 'ID', title: '入库编号', align: 'center',sort:true} //width 支持：数字、百分比和不填写。你还可以通过 minWidth 参数局部定义当前单元格的最小宽度，layui 2.2.1 新增
                                    , {field: 'StoreID', title: '门店编号', align: 'center',sort:true}
                                    , {field: 'StorageCount', title: '入库数量', align: 'center',sort:true} //单元格内容水平居中
                                    , {
                                        field: 'DestroyTime',
                                        title: '入库时间',
                                        width: 170,
                                        align: 'center'
                                    }
                                    , {field: 'RealName', title: '操作人', align: 'center'}
                                    , {field: 'Note', title: '备注', align: 'center'} //单元格内容水平居中
                                    // ,{fixed: 'right', title: '操作', width:280, align:'center', toolbar: '#barDemo'}
                                ]]
                                , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                                , limit: 10
                            });
                        });
                    } else {
                        layer.msg(data.Result_Msg||data.result_msg);
                    }
                }
            })
        });

    },
    //render表格
    // .........................................代币销毁.........................................
    CoinDestroy: function (obj, token, layer) {
        obj.on('click', function() {
            layer.confirm('确定销毁此代币？', {
                btn: ['确定', '取消'] //按钮
            }, function () {
                let storageCount = $('#destroyCoinNumber').val();
                let note = $('#destroyCoinNote').val();
                let obj = {
                    'storageCount': storageCount,
                    'note': note,
                    'userToken': token,
                    'signkey': '1f626576304bf5d95b72ece2222e42c3'
                };
                let url = '/XCCloud/Coins?action=AddCoinDestory';
                let parseJson = JSON.stringify(obj);
                $.ajax({
                    type: "post",
                    url: url,
                    contentType: "application/json; charset=utf-8",
                    data: {parasJson: parseJson},
                    success: function (data) {
                        data = JSON.parse(data);
                        console.log(data);
                        if (data.result_code == 1) {
                            layer.msg('代币销毁成功');
                            $('#searchCoinBtn').trigger('click');
                        } else {
                            layer.msg(data.result_msg);
                        }
                    }
                })
            }, function () {
            });
        });
    },
    SearchCoinDestroy: function (obj, token, layer) {
        obj.on('click', function() {
            var tableData = [];
            let destroyTimes = $('#destroyTime').val();
            let obj = {'destroyTime': destroyTimes, 'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
            let url = '/XCCloud/Coins?action=GetCoinDestory';
            let parseJson = JSON.stringify(obj);
            var index = layer.load(0, {shade: false});
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.Result_Code == 1) {
                        tableData = data.Result_Data;
                        layui.use(['table'], function () {
                            var table = layui.table;
                                layer.close(index);
                            table.render({
                                elem: '#coinDestroyTb'
                                , data: tableData
                                , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                                , cols: [[
                                    {field: 'ID', title: '入库编号', align: 'center',sort:true} //width 支持：数字、百分比和不填写。你还可以通过 minWidth 参数局部定义当前单元格的最小宽度，layui 2.2.1 新增
                                    , {field: 'StoreID', title: '门店编号', align: 'center',sort:true}
                                    , {field: 'StorageCount', title: '销毁数量', align: 'center',sort:true} //单元格内容水平居中
                                    , {
                                        field: 'DestroyTime',
                                        title: '销毁时间',
                                        width: 170,
                                        align: 'center'
                                    }
                                    , {field: 'RealName', title: '操作人', align: 'center'}
                                    , {field: 'Note', title: '说明', align: 'center'} //单元格内容水平居中
                                    // ,{fixed: 'right', title: '操作', width:280, align:'center', toolbar: '#barDemo'}
                                ]]
                                , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                                , limit: 10
                            });
                        });
                    } else {
                        layer.msg(data.Result_Msg||data.result_msg);
                    }
                }
            })
        });

    },
    //数字币管理..........................................................................................................
    // .........................................数字币入库.........................................
    //查询数字币级别
    GetMemberLevel: function (token, form, layer) {
        let obj = {'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
        let url = '/XCCloud/Member?action=GetMemberLevelDic';
        let parseJson = JSON.stringify(obj);
        $('#digitLevel').html("<option >-请选择-</option>");
        form.render();
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: {parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                console.log(data);
                if (data.result_code == 1) {
                    for (var i in data.Result_Data) {
                        $('#digitLevel').append('<option value="' + data.Result_Data[i].Key + '">' + data.Result_Data[i].Value + '</option> ')
                    }
                    form.render();
                } else {
                    layer.msg(data.Result_Msg||data.result_msg)
                }
            }
        })
    },
    //查询数字币总数
    GetDigitCoin: function (token, layer) {
        let obj = {'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
        let url = '/XCCloud/Coins?action=GetDigitCoin';
        let parseJson = JSON.stringify(obj);
        $('#currStorage').html("");
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: {parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                console.log(data);
                if (data.result_code == 1) {
                    $('#currStorage').val(data.result_data.length);
                } else {
                    layer.msg("获取总数失败")
                }
            }
        })
    },
    //数字币入库
    AddDigitCoin: function (parm, form, layer) {
        var obj = parm.obj;
        var url = parm.url;
        let parseJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: {parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                console.log(data);
                if (data.result_code == 1) {
                    $('.cardBox').append('<p>' + parm.iCardID + '</p>');
                    $('#currStorage').val(parseInt($('#checkCard').val("")) + 1);
                } else {
                    layer.msg("操作失败")
                }
            }
        })
    },
    // .........................................数字币销毁.........................................
    //销毁
    DigitDestroy: function (obj, token, layer) {
        obj.on('click', function() {
            layer.confirm('确定销毁此数字币？', {
                btn: ['确定', '取消'] //按钮
            }, function () {
                let iCardIDs = $('#iCardIDs').val();
                let obj = {'iCardID': iCardIDs, 'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
                let url = '/XCCloud/Coins?action=AddDigitDestory';
                let parseJson = JSON.stringify(obj);
                $.ajax({
                    type: "post",
                    url: url,
                    contentType: "application/json; charset=utf-8",
                    data: {parasJson: parseJson},
                    success: function (data) {
                        data = JSON.parse(data);
                        console.log(data);
                        if (data.result_code == 1) {
                            layer.msg('数字币销毁成功');
                            $('#searchCoinBtn').trigger('click');
                        } else {
                            layer.msg(data.result_msg);
                        }
                    }
                })
            }, function () {
            });
        });
    },
    //查询数字币销毁
    SearchDigitDestroy: function (obj, token, layer) {
        obj.on('click', function() {
            var tableData = [];
            let destroyTimes = $('#storageTime').val();
            let iCardID = $('#destroyCardIDs').val();
            let obj = {
                'destroyTime': destroyTimes,
                'iCardID': iCardID,
                'userToken': token,
                'signkey': '1f626576304bf5d95b72ece2222e42c3'
            };
            let url = '/XCCloud/Coins?action=GetDigitDestroy';
            let parseJson = JSON.stringify(obj);
            var index = layer.load(0, {shade: false});
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.Result_Code == 1) {
                        tableData = data.Result_Data;
                        layui.use(['table'], function () {
                            var table = layui.table;
                                layer.close(index);
                            table.render({
                                elem: '#digitDestroyTb'
                                , data: tableData
                                , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                                , cols: [[
                                    {type: 'numbers'}
                                    , {field: 'ID', title: '入库编号', align: 'center'} //width 支持：数字、百分比和不填写。你还可以通过 minWidth 参数局部定义当前单元格的最小宽度，layui 2.2.1 新增
                                    , {field: 'StoreID', title: '门店编号', align: 'center'}
                                    , {field: 'ICCardID', title: '数字币编号', align: 'center'} //单元格内容水平居中
                                    , {
                                        field: 'DestroyTime',
                                        title: '销毁时间',
                                        width: 170,
                                        align: 'center',
                                        templet: '#titleTpl'
                                    }
                                    , {field: 'RealName', title: '操作人', align: 'center'}
                                    , {field: 'Note', title: '备注', align: 'center'} //单元格内容水平居中
                                    // ,{fixed: 'right', title: '操作', width:280, align:'center', toolbar: '#barDemo'}
                                ]]
                                , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                                , limit: 10
                            });
                        });
                    } else {
                        layer.msg(data.Result_Msg||data.result_msg);
                    }
                }
            })
        });
    },
    //..........................................会员级别设定..............................................
    //查询会员级别
    SearchMemberLevel: function (obj, token, layer) {
        obj.on('click', function() {
            var tableData = [];
            let memberLevelID = $('#levelID').val();
            let memberLevelName = $('#levelName').val();
            let obj = {
                'memberLevelID': memberLevelID,
                'memberLevelName': memberLevelName,
                'userToken': token,
                'signkey': '1f626576304bf5d95b72ece2222e42c3'
            };
            let url = '/XCCloud/Member?action=QueryMemberLevel';
            let parseJson = JSON.stringify(obj);
            var index = layer.load(0, {shade: false});
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.Result_Code == 1) {
                        tableData = data.Result_Data;
                        layui.use(['table'], function () {
                            var table = layui.table;
                                layer.close(index);
                            table.render({
                                elem: '#memberLevelTb'
                                , data: tableData
                                , cellMinWidth: 100 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                                , cols: [[
                                    {type: 'numbers',fixed:'left'}
                                    , {field: 'MemberLevelID', title: '级别编号',fixed:'left', align: 'center'} //width 支持：数字、百分比和不填写。你还可以通过 minWidth 参数局部定义当前单元格的最小宽度，layui 2.2.1 新增
                                    , {field: 'MemberLevelName', title: '级别名称',fixed:'left', align: 'center'}
                                    , {field: 'Deposit', title: 'IC卡押金', align: 'center'} //单元格内容水平居中
                                    , {field: 'Validday', title: '有效天数', align: 'center'}
                                    , {field: 'ExitMoney', title: '兑币限额', align: 'center'}
                                    , {field: 'ExitCoin', title: '兑币上限', align: 'center'} //单元格内容水平居中
                                    , {field: 'ExitPrice', title: '兑币单价', align: 'center'}
                                    , {field: 'MinExitCoin', title: '最小退币数', align: 'center'}
                                    , {field: 'MustInputStr', title: '入会时必须输入项', align: 'center'}
                                    , {field: 'BirthdayFreeStr', title: '生日送币方式', align: 'center'}
                                    , {field: 'BirthdayFreeCoin', title: '生日送币数', align: 'center'}
                                    , {field: 'FreeNeedWin', title: '送币前需要输赢的币数',width: 180, align: 'center'}
                                    , {field: 'FreeRate', title: '送币频率', align: 'center'}
                                    , {field: 'FreeCoin', title: '送币数', align: 'center'}
                                    , {field: 'AllowExitCoinToCard', title: '允许从游戏币退到卡里',width: 170, align: 'center'}
                                    , {field: 'StateStr', title: '会员级别状态', align: 'center'}
                                    , {field: 'AllowExitCard', title: '允许退卡', align: 'center'}
                                    , {field: 'AllowExitMoney', title: '允许退款', align: 'center'}
                                    , {field: 'LockHead', title: '投币后拔卡时锁机头',width: 170, align: 'center'}
                                    , {fixed: 'right', title: '操作', width: 120, align: 'center', toolbar: '#barDemo'}
                                ]]
                                , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                                , limit: 10
                            });
                        });
                    } else {
                        layer.msg(data.Result_Msg||data.result_msg);
                    }
                }
            })
        });
    },
    //查询会员级别1
    SearchMemberLevel1: function (id, token, layer) {
            $('#'+id).html("");
            let obj = {
                'memberLevelID': "",
                'memberLevelName': "",
                'userToken': token,
                'signkey': '1f626576304bf5d95b72ece2222e42c3'
            };
            let url = '/XCCloud/Member?action=GetMemberLevelDic';
            let parseJson = JSON.stringify(obj);
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    if (data.result_code == 1) {
                      let  tableData = data.result_data;
                        for (var i in tableData){
                            $('#'+id).append('<input type="checkbox" name="like[write]" lay-skin="primary" lay-filter="ml" value="'+tableData[i].Key+'" title="'+tableData[i].Value+'"><br>')
                        }
                        layui.use('form',function () {
                            var form=layui.form;
                            form.render();
                        })
                    } else {
                        layer.msg(data.Result_Msg||data.result_msg);
                    }
                }
            })

    },

    setSelect: function (objVal,id,m){
        var xc=xcActionSystem.prototype;
        var token=xc.getStorage('token');
        var  obj={"dictKey":objVal,"userToken":token,"signkey":"1f626576304bf5d95b72ece2222e42c3"};
        var url="/XCCloud/Dictionary?action=GetNodes";
        var parasJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: { parasJson: parasJson },
            success: function (data) {
                data=JSON.parse(data);
                if(data.result_code=="1"){
                    var arr=data.result_data;
                    $('#'+id).html('<option value="">-请选择-</option>');
                    for(i in arr){
                        $('#'+id).append("<option value='"+arr[i].dictValue+"' name='"+arr[i].name+"'title='"+arr[i].name+"'>"+arr[i].name+"</option>");
                    }
                    var _text= $('#'+id).find('option[value='+m+']').text();
                    $('#'+id).find('option[value='+m+']').remove();
                    $('#'+id).append("<option value='"+m+"' selected>"+_text+"</option>");
                    layui.use(['form'], function() {
                        var form = layui.form;
                        form.render('select');
                    });
                }else if(data.return_msg=="token无效"){
                    layer.msg(data.return_msg);
                }else {
                    // alert("添加失败，请核对后再次提交！");
                    layui.use(['layer'], function() {
                        var layer = layui.layer;
                        layer.msg('数据加载失败！');
                    });
                }
            }
        })
    },
    setSelect1: function (id,m){
        var xc=xcActionSystem.prototype;
        var token=xc.getStorage('token');
        var  obj={"userToken":token,"signkey":"1f626576304bf5d95b72ece2222e42c3"};
        var url="/XCCloud/Project?action=GetProjectInfoList";
        var parasJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: { parasJson: parasJson },
            success: function (data) {
                data=JSON.parse(data);
                console.log(data)
                if(data.Result_Code=="1"){
                    var arr=data.Result_Data;
                    $('#'+id).html('<option value="">-请选择-</option>');
                    for(i in arr){
                        $('#'+id).append("<option value='"+arr[i].ID+"'>"+arr[i].ProjectName+"</option>");
                    }
                    var _text= $('#'+id).find('option[value='+m+']').text();
                    $('#'+id).find('option[value='+m+']').remove();
                    $('#'+id).append("<option value='"+m+"' selected>"+_text+"</option>");
                    layui.use(['form'], function() {
                        var form = layui.form;
                        form.render('select');
                    });
                }else {
                    // alert("添加失败，请核对后再次提交！");
                    layui.use(['layer'], function() {
                        var layer = layui.layer;
                        layer.msg('数据加载失败！');
                    });
                }
            }
        })
    },
    setSelect2: function (id,m) {
        var xc=xcActionSystem.prototype;
        var token=xc.getStorage('token');
        var  obj={"userToken":token,"signkey":"1f626576304bf5d95b72ece2222e42c3"};
        var url="/XCCloud/Project?action=GetProjectInfoList";
        var parasJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: { parasJson: parasJson },
            success: function (data) {
                data=JSON.parse(data);
                console.log(data)
                if(data.Result_Code=="1"){
                    var arr=data.Result_Data;
                    $('#'+id).html('<option value="">-请选择-</option>');
                    for(i in arr){
                        $('#'+id).append("<option value='"+arr[i].ID+"'>"+arr[i].ProjectName+"</option>");
                    }
                    var _text= $('#'+id).find('option[value='+m+']').text();
                    $('#'+id).find('option[value='+m+']').remove();
                    $('#'+id).append("<option value='"+m+"' selected>"+_text+"</option>");
                    layui.use(['form'], function() {
                        var form = layui.form;
                        form.render('select');
                    });
                }else {
                    // alert("添加失败，请核对后再次提交！");
                    layui.use(['layer'], function() {
                        var layer = layui.layer;
                        layer.msg('数据加载失败！');
                    });
                }
            }
        })
    },
    //.....................优惠套餐设定...............................
    SearchMemberLevelOnly: function ( id,token,layer) {

            let obj = {
                'userToken': token,
                'signkey': '1f626576304bf5d95b72ece2222e42c3'
            };
            let url = '/XCCloud/Member?action=QueryMemberLevel';
            let parseJson = JSON.stringify(obj);
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.Result_Code == 1) {
                        var arr=data.Result_Data;
                        for (var i in arr){
                            $('#'+id).append('<input type="checkbox" name="like[write]" lay-filter="ml" value="'+arr[i].MemberLevelID+'" title="'+arr[i].MemberLevelName+'"><br>');
                        }
                        layui.use('form',function () {
                            var form=layui.form;
                            form.render();
                        })

                    } else {
                        layer.msg('操作失败');
                    }
                }
            })

    },
    //时段设定
    TimeSet: function (table,foodLevels,id,mLIDs,week) {
        var mlIDS1=[];  var mlIDS2=[];var week1=[];var week2=[];  var WeekStrs=''; var Week=';'
        for(var i in mLIDs){
            mlIDS1.push(mLIDs[i].value);mlIDS2.push(mLIDs[i].title);
        }
        if(timeType==1){
            for(var i in week){
                week1.push(week[i].value);week2.push(week[i].title);
                Week=week1.join('|'); WeekStrs= week2.join('|');
                typeChecks=3;
            }
        }else if(timeType==0){
            if(typeChecks==0){
                Week='1|2|3|4|5'; WeekStrs='工作日';
                console.log(WeekStrs)
            }else if(typeChecks==1){
                Week=''; WeekStrs='法定节假日';   console.log(WeekStrs)
            }else if(typeChecks==2){
                Week='6|7'; WeekStrs='周末';   console.log(WeekStrs)
            }
        }
        var memberLevelIDs=mlIDS1.join('|');  var memberLevels=mlIDS2.join('|');
        var StartTime  =$('#StartTime').val();
        var EndTime  =$('#EndTime').val();
        var ClientPrice =$('#ClientPrice').val();
        var VipPrice=$('#VipPrice').val();
        var dsc=$('#day_sale_count').val();
        var mdsc=$('#member_day_sale_count').val();
        var  allowCoins=allowCoin||'';
        var  coinss=$('#coins').val();
        let allowPoints=allowPoint||'';
        let points=$('#points').val();
        let allowLotterys=allowLottery||'';
        let lottery=$('#lottery').val();
        var time=StartTime+'~'+EndTime;
        if(memberLevelIDs!=""&&memberLevelIDs!=null&&memberLevelIDs!=undefined){
            foodLevels.push({'MemberLevelIDs':memberLevelIDs,'MemberLevels':memberLevels,'Week':Week,'PeriodType':typeChecks
                ,'SingleOrDouble':typeChecks,
                'WeekStr':WeekStrs,'StartTime':StartTime,'EndTime':EndTime,'Time':time,'coins':coinss,'allowCoin':allowCoins,
                'allowPoint':allowPoints,'points':points,'allowLottery':allowLotterys, 'lottery':lottery,
                'ClientPrice':ClientPrice,'VIPPrice':VipPrice,'day_sale_count':dsc,'member_day_sale_count':mdsc});
            console.log(foodLevels);
           $('#tosetTime').trigger('click');
            table.render({
                elem: '#'+id
                , height:'250px'
                , data: foodLevels
                , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
                , cols: [[{field:'MemberLevels', title:'适用级别', align: 'center',width:200}
                    ,{field:'WeekStr',title: '周', align: 'center',width:255} //width 支持：数字、百分比和不填写。
                    ,{field:'Time', title: '时段', align: 'center',width:200}
                    ,{field:'ClientPrice', title: '散客价格', align: 'center'}
                    ,{field:'VIPPrice', title: '会员价格', align: 'center'}
                    ,{field:'day_sale_count', title: '每天限额', align: 'center'}
                    ,{field:'member_day_sale_count', title: '每人每天限额', align: 'center'}

                    ,{field:'allowCoin', title: '允许支付代币', align: 'center'}
                    ,{field:'coins', title: '支付代币数量', align: 'center'}
                    ,{field:'allowPoint', title: '允许支付积分', align: 'center'}
                    ,{field:'points', title: '支付积分数量', align: 'center'}
                    ,{field:'allowLottery', title: '允许支付彩票', align: 'center'}
                    ,{field:'lottery', title: '支付彩票数量', align: 'center'}
                    ,{fixed: 'right', width:180, align:'center', toolbar: '#barDemo1'}]]
                , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
                , limit: 10
            });
        }
    },
    //加载周天
    AddWeeks: function (id) {
        $('#'+id).html("");
        $('#'+id).append(' <input type="checkbox" name="day[game]" lay-filter="weeks"value="1" lay-skin="primary" title="周一">' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="2" lay-skin="primary" title="周二" >' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="3" lay-skin="primary" title="周三">' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="4" lay-skin="primary" title="周四">' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="5" lay-skin="primary" title="周五">' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="6" lay-skin="primary" title="周六">' +
            '<input type="checkbox" name="day[game]" lay-filter="weeks" value="7" lay-skin="primary" title="周日">' +
            '');
        layui.use('form',function () {
            var form=layui.form;
            form.render();
        })
    },
    //充值售币保存、修改
    Recharge: function (token,layer) {
        let foodId='';
        let foodName=$('#foodName').val();
        let coins=$('#coins').val();
        let rechargeType=RechargeType;
        let startTime=$('#startTime').val();
        let endTime=$('#endTime').val();
        let note=$('#note').val();
        let foodState=foodState;
        let allowInternet=allowInternet;
        let allowPrint=allowPrint;
        let foreAuthorize=foreAuthorize;
        let clientPrice=$('#clientPrice').val();
        let memberPrice=$('#memberPrice').val();
        let obj={'FoodId':foodId,'foodName':foodName,'coins':coins,'rechargeType':rechargeType,'startTime':startTime,
        'endTime':endTime,'note':note,'foodState':foodState,'allowInternet':allowInternet,'allowPrint':allowPrint,'foreAuthorize':foreAuthorize,
        'clientPrice':clientPrice,'memberPrice':memberPrice,'foodLevels':foodLevels,'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let url='/XCCloud/Promotion?action=SaveFoodCoinsInfo';
        var parasJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: { parasJson: parasJson },
            success: function (data) {
                data=JSON.parse(data);
                console.log(data)
                if(data.result_code==1){
                    layer('保存成功，请继续下一项设定或者退出')
                }else {
                    layer.msg('操作失败')
                }
            }

        })
    },
    //数字币新增 修改
    Digit: function (layer,token,parm,foodId) {
        let FoodId=foodId||'';
        let foodName=$('#foodName').val();
        let coins=$('#coins').val();
        let _days=$('#days').val();
        let price=$('#price').val();
        let startTime=$('#startTime').val();
        let endTime=$('#endTime').val();
        let note=$('#note').val();
        let imgUrl=$('.layui-upload-img_md').attr('src');
        let foodState=parm.foodState;
        let allowPrint=parm.allowPrint;
        let foreAuthorize=parm.foreAuthorize;
        let obj={'FoodId':FoodId,'foodName':foodName,'coins':coins,'days':_days,'startTime':startTime,'price':price,'ImageURL':imgUrl,
            'endTime':endTime,'note':note,'foodState':foodState,'allowPrint':allowPrint,'foreAuthorize':foreAuthorize,
            'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let url='/XCCloud/Promotion?action=SaveFoodDigitInfo';
        var parasJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: { parasJson: parasJson },
            success: function (data) {
                data=JSON.parse(data);
                console.log(data)
                if(data.result_code==1){
                    layer.msg('保存成功');
                    let parm={'obj':{'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'},
                        'url':'/XCCloud/Promotion?action=GetFoodInfoList',
                        'elem':'#digitDestroyTb',
                        'cols':[
                            {field:'FoodID', title:'套餐编号', align: 'center', sort: true}
                            ,{field:'FoodName',title: '套餐名称', align: 'center'} //width 支持：数字、百分比和不填写。
                            ,{field:'FoodTypeStr', title: '套餐类别', align: 'center'}
                            ,{field:'RechargeTypeStr', title: '充值方式', align: 'center'}
                            ,{field:'AllowInternetStr', title: '是否允许第三方', align: 'center'}
                            ,{field:'AllowPrintStr', title: '是否允许打印', align: 'center'}
                            ,{field:'ForeAuthorizeStr', title: '是否允许前台授权', align: 'center'}
                            ,{field:'StartTimeStr', title: '启用时间', align: 'center'}
                            ,{field:'EndTimeStr', title: '停用时间', align: 'center'}
                            ,{fixed: 'right', title: '操作', width:100, align:'center', toolbar: '#barDemo'}]
                    };
                    if(xc.getStorage('dpsAdd')){
                        console.log(1);
                        //layer.closeAll();
                        //parent.layer.closeAll();
                        var index = top.layer.getFrameIndex(window.name); //先得到当前iframe层的索引
                        top.layer.close(index);
                        // parent.xc.getInitData(parm);
                    }else {
                        parent.layer.closeAll();
                        parent.xc.getInitData(parm);
                    }
                }else {
                    layer.msg('操作失败')
                }
            }

        })
    },
    //初始化页面
    initDiscountPage: function (xc,token,foodId) {
        var _obj={'foodId':foodId,'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        var url='/XCCloud/Promotion?action=GetFoodInfo';
        var parasJson = JSON.stringify(_obj);
        layui.use(['element','form','laydate','table'], function(){
            var element = layui.element, form=layui.form,$=layui.jquery,laydate=layui.laydate,table=layui.table;
        if(foodId){
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: { parasJson: parasJson },
                success: function (data) {
                    data=JSON.parse(data);  console.log(data)
                    if(data.Result_Code==1){
                       if(data.Result_Data.Digit){
                             var arr=data.Result_Data.Digit;
                             console.log(arr)
                            $('#foodName').val(arr.FoodName);
                            $('#coins').val(arr.Coins);
                            $('#price').val(arr.Price);
                            $('#startTime').val(xc.dateStamp(arr.StartTime));
                            $('#endTime').val(xc.dateStamp(arr.EndTime));
                            $('#note').val(arr.Note);
                            $('#days').val(arr.Days);
                            foodState=arr.FoodState;
                            allowPrint=arr.AllowPrint;
                            foreAuthorize=arr.ForeAuthorize;
                            if(arr.FoodState==1){
                                $('#foodState').attr({'checked':true});
                            }else {
                                $('#foodState').attr({'checked':false});
                            }
                            if(arr.AllowPrint==1){
                                $('#allowPrint').attr({'checked':true});
                            }else {
                                $('#allowPrint').attr({'checked':false});
                            }
                            if(arr.ForeAuthorize==1){
                                $('#foreAuthorize').attr({'checked':true});

                            }else {
                                $('#foreAuthorize').attr({'checked':false});
                            }
                            form.render();
                            if(arr.ImageURL!=""){
                                $('#demo2') .append('<div style="display: inline-block;position: relative" class="imgBox">' +
                                    '<button type="button" ><i class="layui-icon"><i class="layui-icon">&#xe640</i></i></button>' +
                                    '<img src="'+ arr.ImageURL +'" class="layui-upload-img_md" ' +
                                    'style="display: block;width: 300px;height: 170px"></div>');
                                $('.imgBox button').click(function () {
                                    $(this).parent('div').remove();
                                });
                                $('.imgBox img').click(function () {
                                    layer.open({
                                        title:'查看大图',
                                        type: 1,
                                        area: ['600px','400px'], //宽高
                                        content: '<div><img src="'+arr.CardUIURL+'" alt="" style="display: block;"></div>'
                                    });
                                });
                            }

                        }

                    }
                }
            })
          }
        })
    },
    //混合页面中间的表格
    CreateTable: function (table,datas) {
        table.render({
            elem: '#projectCon'
            , height:'250px'
            , size:"sm"
            , data: datas
            , cellMinWidth: 120 //全局定义常规单元格的最小宽度，layui 2.2.1 新增
            , cols: [[{type:'numbers'}
                ,{field:'ProjectName', title:'项目名称', align: 'center',width:200}
                ,{field:'FoodTypeStr',title: '类别', align: 'center',width:205} //width 支持：数字、百分比和不填写。
                ,{field:'ContainCount', title: '数量', align: 'center',width:100}
                ,{field:'Days', title: '有效期限', align: 'center'}
                ,{field:'WeightValue', title: '权重值', align: 'center'}
                ,{fixed: 'right',title:'操作', width:140, align:'center', toolbar: '#barDemo2'}]]
            , page: {page: true, limits: [10, 15, 20, 30, 50, 100]}
            , limit: 10
        });
     },
     //根据ProjectId设置属性
    SetProjectTIp: function (projectId,token,form,ticketsType) {
        if(ticketsType==0){
            $('#playTimes').val('');
        }
        if(projectId){
            var obj={'id':projectId,'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
            var url='/XCCloud/Promotion?action=GetProjectInfo';
            var parasJson = JSON.stringify(obj);
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: { parasJson: parasJson },
                success: function (data) {
                    data=JSON.parse(data);
                    if(data.Result_Code==1){
                        var arr=data.Result_Data;
                        if(arr.SignOutEN==1){
                            $('#signOutEn').attr('checked',true);
                        }else {
                            $('#signOutEn').attr('checked',false);
                        }
                        if(arr.WhenLock==1){
                            $('#whenLock').attr('checked',true);
                        }else {
                            $('#whenLock').attr('checked',false);
                        }
                        $('#ticketsType').val(arr.FeeTypeStr);
                        if($('#playTimes').val()==''){
                            if(arr.FeeTypeStr=='计次'){
                                $('#playTimes').val('1');
                            }else if(arr.FeeTypeStr=='计时'){
                                $('#playTimes').val('1');
                            }else  if(arr.FeeTypeStr=='月票'){
                                $('#playTimes').val('30');
                            }else  if(arr.FeeTypeStr=='季票'){
                                $('#playTimes').val('90');
                            }else  if(arr.FeeTypeStr=='年票'){
                                $('#playTimes').val('365');
                            }
                        }

                        form.render();

                    }else {
                        layer.msg(data.result_msg)
                    }
                }

            })
        }

    },
    //..........................................门票项目管理..............................................
    //弹出
    outIframe: function (layer) {
        layer.open({
            title:'设置游乐项目',
            shade:0.6,
            type:1,
            area:'1030px',
            content:$('#model')
        })
    },
    GetBindDeviceList: function (token) {
        //从缓存中获取表格数据，没有则新加入数据
       var tableData=[];
         xcActionSystem.prototype.removeStorage('deviceResult');

           let obj={'type':'','mcuId':'','bindDeviceIDs':[],'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
           let url='/XCCloud/Project?action=GetBindDeviceList';
           $.ajax({
               type: "post", url: url,
               contentType: "application/json; charset=utf-8",
               data: {parasJson: JSON.stringify(obj)},
               success: function (data) {
                   data = JSON.parse(data);
                   console.log(data);
                   if (data.Result_Code == "1"||data.result_code==1) {
                       tableData = data.Result_Data||data.result_data;
                       xcActionSystem.prototype.setStorage('deviceResult',JSON.stringify(tableData));
                   }
               }
           })

    },
    //获取门店列表
    GetStoreList:function (token,layer,form,id) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/StoreInfo?action=GetStoreList',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1){
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        $('#'+id).append('<option value="'+arr[i].StoreID+'">'+arr[i].StoreName+'</option>')
                    }
                    form.render('select');
                } else {
                    layer.msg(data.Result_Msg);
                }
            }
        });
    },
    //......................................游戏机档案维护
    //获取会员级别列表
    gameMemberLevel: function (token,form,layer) {
            let obj = {
                'userToken': token,
                'signkey': '1f626576304bf5d95b72ece2222e42c3'
            };
            let url = '/XCCloud/Member?action=GetMemberLevelDic';
            let parseJson = JSON.stringify(obj);
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: {parasJson: parseJson},
                success: function (data) {
                    data = JSON.parse(data);
                    console.log(data);
                    if (data.result_code == 1) {
                        $('#memberLevel').html('<option>-请选择-</option>');
                        let arr=data.result_data;
                        for(var i in arr){
                            $('#memberLevel').append('<option value="'+arr[i].Key+'" title="'+arr[i].Value+'">'+arr[i].Value+'</option>')
                        }
                        form.render();
                    } else {
                        layer.msg('操作失败');
                    }
                }
            })

    },
    //......................................设备管理
    //加载路由器列表
    getRouteDevice:function (token,layer,form,id) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/DeviceInfo?action=GetRouteDevice',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].DeviceName+'</option>')
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
},
    //加载游戏机列表
    getGameInfoDic:function (token,layer,form,id) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/GameInfo?action=GetGameInfoDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        $('#'+id).append('<option value="'+arr[i].Key+'">'+arr[i].Value+'</option>')
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
},
    //......................................门票设定
    //获取门票字典列表
    GetTicketProjectDic:function (token,layer,form,id,selected) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/Project?action=GetProjectDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        // $('#'+id).append('<option value="'+arr[i].ID+'" title="'+arr[i].ExpireDays+'">'+arr[i].ProjectName+'</option>')
                        if(selected){
                            if(arr[i].ID==selected){
                                $('#'+id).append('<option value="'+arr[i].ID+'" selected title="'+arr[i].ExpireDays+'">'+arr[i].ProjectName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].ID+'" title="'+arr[i].ExpireDays+'">'+arr[i].ProjectName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].ID+'" title="'+arr[i].ExpireDays+'">'+arr[i].ProjectName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //获取会员余额类别字典
    getBalanceTypeDic:function (token,layer,form,id,selected) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/BalanceType?action=GetBalanceTypeDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        if(selected){
                            if(arr[i].ID==selected){
                                $('#'+id).append('<option value="'+arr[i].ID+'" selected>'+arr[i].TypeName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].TypeName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].TypeName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //获取会员级别字典
    getMemberTypeDic:function (token,layer,form,id,selected) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/Member?action=QueryMemberLevel',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        if(selected){
                            if(arr[i].MemberLevelID==selected){
                                $('#'+id).append('<option value="'+arr[i].MemberLevelID+'" selected>'+arr[i].MemberLevelName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].MemberLevelID+'">'+arr[i].MemberLevelName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].MemberLevelID+'">'+arr[i].MemberLevelName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //获取数字币级别字典
    getDigitFoodDic:function (token,layer,form,id,selected) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/DigitFood?action=GetDigitFoodDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        if(selected){
                            if(arr[i].ID==selected){
                                $('#'+id).append('<option value="'+arr[i].ID+'" selected>'+arr[i].FoodName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].FoodName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].FoodName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //获取优惠券
    getCouponDic:function (token,layer,form,id,selected) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/Coupon?action=GetCouponDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        if(selected){
                            if(arr[i].ID==selected){
                                $('#'+id).append('<option value="'+arr[i].ID+'" selected   title="'+arr[i].EndTime+'">'+arr[i].CouponName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].ID+'" title="'+arr[i].EndTime+'">'+arr[i].CouponName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].ID+'"  title="'+arr[i].EndTime+'">'+arr[i].CouponName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //获取游乐项目字典
    getProjectGames:function (token,layer,form,id,arrChecked) {
        let _obj={'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/Project?action=GetProjectGames',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('');
                    for(let i in arr){
                        if(arrChecked){
                            let flag=false;
                            for(let j in arrChecked){
                                if(arr[i].ID==arrChecked[j].gameId){
                                    flag=true;
                                    break;
                                }
                            }
                            if(flag=='true'){
                                $('#'+id).append('<input value="'+arr[i].ID+'" checked lay-filter="project" type="checkbox" lay-skin="primary" title="'+arr[i].Name+'">')
                            }else {
                                $('#'+id).append('<input value="'+arr[i].ID+'" lay-filter="project" type="checkbox" lay-skin="primary" title="'+arr[i].Name+'">')
                            }
                        }else {
                            $('#'+id).append('<input value="'+arr[i].ID+'" lay-filter="project" type="checkbox" lay-skin="primary" title="'+arr[i].Name+'">')
                        }

                    }
                    form.render();
                } else {
                    layer.msg(data.result_msg);
                }
            }
        });
    },
    //..........................................商品管理..............................................
    addGamelists: function (id, token, layer) {
        var tableData = [];
        $('#'+id).html("");
        let obj = {'userToken': token, 'signkey': '1f626576304bf5d95b72ece2222e42c3'};
        let url = '/XCCloud/GameInfo?action=GetGameInfoDic';
        let parseJson = JSON.stringify(obj);
        $.ajax({
            type: "post",
            url: url,
            contentType: "application/json; charset=utf-8",
            data: {parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    tableData = data.result_data;
                    for (var i in tableData){
                        $('#'+id).append('<input type="checkbox" name="like[write]" lay-skin="primary" lay-filter="gameList" value="'+tableData[i].Key+'" title="'+tableData[i].Value+'"><br>')
                    }
                    layui.use('form',function () {
                        var form=layui.form;
                        form.render();
                    })
                } else {
                    layer.msg('操作失败');
                }
            }
        })
    },
    //商品管理---商品档案维护
    openProductAdd: function (layer,title,area,id) {
        layer.open({
            type:1,
            title:title,
            area:area,
            content:$('#'+id)
        })
    },
    //商品管理---商品入库--新增
    addProductStorage: function (token,layer,id,parm) {

        let _obj={    'id':id,
                       'barcode':$('#Barcode').val(),
                        'storageCount':$('#StorageCount').val(),
                        'price':$('#Price').val(),
                        'totalPrice':$('#TotalPrice').val(),
                        'discount':$('#Discount').val(),
                        'note':$('#Note').val(),
            'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let url='/XCCloud/GoodsInfo?action=AddGoodStorage';
        let parasJson = JSON.stringify(_obj);
        $.ajax({
            type: "post", url: url,
            contentType: "application/json; charset=utf-8",
            data: {parasJson: JSON.stringify(_obj)},
            success: function (data) {
                data = JSON.parse(data);
                console.log(data);
                if (data.result_code==1) {
                   layer.msg('保存成功！');
                   xcActionSystem.prototype.getInitData(parm);
                }else {
                    layer.msg(data.result_msg)
                }
            }
        })
    },
    //..........................................礼品管理..............................................
    //获取仓库字典
    getDepotDic:function (merchId,storeId,token,layer,form,id,selected) {
        let _obj={'merchId':merchId,'storeId':storeId,'userToken':token,'signkey':'1f626576304bf5d95b72ece2222e42c3'};
        let parseJson = JSON.stringify(_obj);
        $.ajax({
            type:'post',
            url:'/XCCloud/DepotInfo?action=GetDepotDic',
            contentType: "application/json; charset=utf-8",
            data:{parasJson: parseJson},
            success: function (data) {
                data = JSON.parse(data);
                if (data.result_code == 1) {
                    let arr=data.result_data;
                    $('#'+id).html('<option>-请选择-</option>');
                    for(let i in arr){
                        if(selected){
                            if(arr[i].ID==selected){
                                $('#'+id).append('<option value="'+arr[i].ID+'" selected>'+arr[i].DepotName+'</option>')
                            }else {
                                $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].DepotName+'</option>')
                            }
                        }else {
                            $('#'+id).append('<option value="'+arr[i].ID+'">'+arr[i].DepotName+'</option>')
                        }
                    }
                    form.render('select');
                } else {
                    layer.msg(data.result_msg||data.return_msg);
                }
            }
        });
    },
};