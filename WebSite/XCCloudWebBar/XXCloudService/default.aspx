﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="XXCloudService._default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <title>云雷达监测服务</title>
    <meta name="description" content="莘宸科技云雷达检测服务">
    <meta name="keywords" content="index">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="renderer" content="webkit">
    <meta http-equiv="Cache-Control" content="no-siteapp" />
    <link rel="icon" type="image/png" href="assets/i/favicon.png">
    <link rel="apple-touch-icon-precomposed" href="assets/i/app-icon72x72@2x.png">
    <meta name="apple-mobile-web-app-title" content="Amaze UI" />
    <link rel="stylesheet" href="assets/css/amazeui.min.css" />
    <link rel="stylesheet" href="assets/css/admin.css">
    <link rel="stylesheet" href="assets/css/app.css">
    <style>
        .task-configChange {
            display: block;
            height: 20px;
            position: relative;
        }

        .tpl-task-list-dropdownChange {
            top: -4px;
            left: 10px;
            width: 100%;
        }

        .tpl-task-list-hoverChange {
            display: block;
            width: 100%;
            height: 100%;
        }

        .tpl-task-list-dropdown-ulChange {
            width: 140px !important;
        }

            .tpl-task-list-dropdown-ulChange li a {
                display: block !important;
                padding: 10px !important;
                text-align: left;
            }

            .tpl-task-list-dropdown-ulChange li {
                border-bottom: 1px solid #eee;
            }

                .tpl-task-list-dropdown-ulChange li:last-child {
                    border-bottom: none;
                }

        .am-dropdown-contentChange {
            width: 200px;
        }

        .note-infoChange {
            background-color: #FFF;
            height: 400px;
            overflow-y: auto;
        }

        .am-active {
            color: #00CD00;
        }
        .pagingList{text-align: right;}
        .pagingLi{display: inline-block;}
        .pagingLi button{border-radius: 5px;background-color: transparent;padding: 5px 10px;color: #555555;}
        .am-table-striped>tbody>tr.backgroundH>td{background: rgba(222, 230, 100,0.5);!important;}
        body{font-size:1rem;}
    </style>
</head>
<body data-type="index">
    <div id="ajax-loader" style="cursor: progress; position: fixed; top: -50%; left: -50%; width: 200%; height: 200%; background: #fff; z-index: 10000; overflow: hidden;">
        <img src="assets/img/ajax-loader.gif" style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; margin: auto;" />
    </div>
    <div id="app">
        <header class="am-topbar am-topbar-inverse admin-header">
            <div class="am-topbar-brand">
                <a href="javascript:;" class="tpl-logo">
                    <img src="assets/img/logo.png" alt="">
                </a>
            </div>

            <div class="am-collapse am-topbar-collapse" id="topbar-collapse" style="text-align:center">

                <span class="tpl-header-nav-hover-ico">
                    云雷达监测服务
                </span>

                <ul class="am-nav am-nav-pills am-topbar-nav am-topbar-right admin-header-list tpl-header-list">
                    <li class="am-hide-sm-only"><a href="javascript:;" id="admin-fullscreen" class="tpl-header-list-link"><span class="am-icon-arrows-alt"></span> <span class="admin-fullText">开启全屏</span></a></li>

                    <li class="am-dropdown" data-am-dropdown data-am-dropdown-toggle>
                        <a class="am-dropdown-toggle tpl-header-list-link" href="javascript:;">
                            <span class="tpl-header-list-user-ico"> <span class="am-icon-cog"></span><span class="tpl-header-list-user-nick">设置</span></span>
                        </a>
                        <ul class="am-dropdown-content">
                            <li><a href="#" @click="doInitDevice"><span id="reInitDevice" class="am-icon-refresh"></span> 初始化设备</a></li>
                            <!--<li><a href="#"><span class="am-icon-cog"></span> 设置</a></li>-->
                            <li><a href="#"><span class="am-icon-power-off"></span> 退出</a></li>
                        </ul>
                    </li>
                </ul>
            </div>
        </header>

        <div class="tpl-page-container tpl-page-header-fixed">

            <div class="tpl-content-wrapper">

                <div class="row">
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 300}">
                                <dt class="cs"><i class="am-icon-bullhorn"></i></dt>
                                <dd v-text="RecvData.Instructions.Total">loading...</dd>
                                <dd class="f12">当前总指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 600}">
                                <dt class="qs"><i class="am-icon-search"></i></dt>
                                <dd v-text="RecvData.Instructions.Querys">loading...</dd>
                                <dd class="f12">当前查询指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 900}">
                                <dt class="hs"><i class="am-icon-flag"></i></dt>
                                <dd v-text="RecvData.Instructions.Coins">loading...</dd>
                                <dd class="f12">当前币业务指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 1200}">
                                <dt class="ls"><i class="am-icon-bell"></i></dt>
                                <dd v-text="RecvData.Instructions.ICCardQueryRepeats">loading...</dd>
                                <dd class="f12">当前IC卡查询重复指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 300}">
                                <dt class="as"><i class="am-icon-history "></i></dt>
                                <dd v-text="RecvData.Instructions.ICCardCoinRepeats">loading...</dd>
                                <dd class="f12">当前IC卡进出币指令重复数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 600}">
                                <dt class="ss"><i class="am-icon-print"></i></dt>
                                <dd v-text="RecvData.Instructions.Receipts">loading...</dd>
                                <dd class="f12">当前小票指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 900}">
                                <dt class="ds"><i class="am-icon-warning "></i></dt>
                                <dd v-text="RecvData.Instructions.Errors">loading...</dd>
                                <dd class="f12">当前错误指令数</dd>
                            </dl>
                        </div>
                    </div>
                    <div class="am-u-lg-3 am-u-md-6 am-u-sm-12">
                        <div class="admin-index">
                            <dl data-am-scrollspy="{animation: 'slide-right', delay: 1200}">
                                <dt class="fs"><i class="am-icon-reply"></i></dt>
                                <dd v-text="RecvData.Instructions.Returns">loading...</dd>
                                <dd class="f12">当前返还指令数</dd>
                            </dl>
                        </div>
                    </div>
                </div>

<%--                <div class="tpl-content-scope">
                    <div class="note note-info">
                        <label for="doc-vld-name">控制器令牌：</label>
                        <input type="text" placeholder="控制器令牌" value="f482d424" ref="input1" style="width:100px;" />
                        <label for="doc-vld-name">机头地址：</label>
                        <input type="text" placeholder="机头地址" value="01" style="width:100px;" />
                        <label for="doc-vld-name">投币数：</label>
                        <input type="text" placeholder="投币数" value="10" style="width:100px;" />
                        <label for="doc-vld-name">卡号：</label>
                        <input type="text" placeholder="卡号" value="10000010" style="width:100px;" />
                        <button type="button" @click="doInCoin" class="am-btn am-btn-primary">投币</button>
                        <button type="button" @click="doOutCoin" class="am-btn am-btn-secondary">退币</button>
                    </div>
                </div>--%>

                <div class="row">
                    <div class="am-u-md-12 am-u-sm-12 row-mb">
                        <div class="tpl-portlet">
                            <div id="lineChatsMian" style="width:100%; height:300px;"></div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="am-u-md-6 am-u-sm-12 row-mb">

                        <div class="tpl-portlet">
                            <div class="tpl-portlet-title" style="overflow: inherit;">
                                <div class="tpl-caption font-green ">
                                    <span>
                                        <ol class="am-breadcrumb am-breadcrumbChange" style="margin-bottom: 0;">
                                            <template v-if="CurrVar.Level === 1">
                                                <li><a href="#" class="am-active">控制器</a></li>
                                            </template>
                                            <template v-if="CurrVar.Level === 2">
                                                <li><a href="#" @click="getRouter">控制器</a></li>
                                                <li><a href="#" class="am-active">{{CurrVar.CurrRouteName}}</a></li>
                                            </template>
                                        </ol>
                                    </span>
                                </div>
                            </div>
                            <div id="wrapper1" class="wrapper" style="overflow-y:auto">
                                <div id="scroller1" class="scroller1">
                                    <template v-if="CurrVar.Level === 1">
                                
                                        <table class="am-table am-table-bordered am-table-radius am-table-striped" style="border-top-width: 0px;">
                                            <thead>
                                                <tr>
                                                    <th>控制器名称</th>
                                                    <th>段号</th>
                                                    <th>地址</th>
                                                    <th>状态</th>    
                                                    <th>更新时间</th>                            
                                                    <%--<th>操作</th>--%>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="item in RecvData.RouteList" @click="selectRouter(item)">
                                                    <td><a href="javascript:;" @click="getChild(item.RouteId, item.RouteName)">
                                                            <span class="am-icon-plus-circle"></span>
                                                            {{ item.RouteName }}
                                                        </a>
                                                    </td>
                                                    <td>{{ item.Segment }}</td>
                                                    <td>{{ item.RemotePoint }}</td>
                                                    <td v-if="item.Online === true">在线</td>
                                                    <td v-else>离线</td>
                                                    <td>{{ item.UpdateTime }}</td>                                  
<%--                                                    <td class="PopupK" style="width: 120px;">
                                                        <div class="task-config task-configChange dropdown-wrapper" style="width: 80px;">
                                                            <div class="am-dropdown tpl-task-list-dropdown tpl-task-list-dropdownChange dropdown-uler" data-am-dropdown>
                                                                <a href="###" class="am-dropdown-toggle tpl-task-list-hover tpl-task-list-hoverChange"
                                                                   data-am-dropdown-toggle>
                                                                    <span>管理</span>
                                                                    <i class="am-icon-cog"></i> <span class="am-icon-caret-down"></span>
                                                                </a>
                                                                <ul class="am-dropdown-content tpl-task-list-dropdown-ul tpl-task-list-dropdown-ulChange">
                                                                    <li @click.stop="doReset(item)">
                                                                        <a href="javascript:;">
                                                                            <i id="route-reset" class="am-icon-undo"></i> 复位
                                                                        </a>
                                                                    </li>
                                                                    <li @click.stop="showError(item)">
                                                                        <a href="javascript:;">
                                                                            <i class="am-icon-warning"></i> 报警日志
                                                                        </a>
                                                                    </li>
                                                                </ul>
                                                            </div>
                                                        </div>
                                                    </td>--%>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </template>
                                    <template v-else>
                                        <table id="deviceTable" class="am-table am-table-bordered am-table-radius am-table-striped"  style="border-top-width: 0px;">
                                            <thead>
                                                <tr>
                                                    <th>类别</th>
                                                    <th>设备名称</th>
                                                    <th>P位</th>
                                                    <th>序列号</th>
                                                    <th>短地址</th>
                                                    <th>状态</th>
                                                    <th>操作</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="item in RecvData.DeviceList" @click="selectDevice(item)">
                                                    <td>{{ item.DeviceType }}</td>
                                                    <td>{{ item.DeviceName }}</td>
                                                    <td>{{ item.SiteName }}</td>
                                                    <td>{{ item.MCUID }}</td>
                                                    <td>{{ item.Address }}</td>
                                                    <td>{{ item.State }}</td>
                                                    <td class="PopupK" style="width: 120px;">
                                                        <div class="task-config task-configChange dropdown-wrapper" style="width: 80px;">
                                                            <div class="am-dropdown tpl-task-list-dropdown tpl-task-list-dropdownChange dropdown-uler " data-am-dropdown>
                                                                <a href="###" class="am-dropdown-toggle tpl-task-list-hover tpl-task-list-hoverChange"
                                                                   data-am-dropdown-toggle>
                                                                    <span>管理</span>
                                                                    <i class="am-icon-cog"></i> <span class="am-icon-caret-down"></span>
                                                                </a>
                                                                <ul class="am-dropdown-content tpl-task-list-dropdown-ul tpl-task-list-dropdown-ulChange">
                                                                    <li @click="doLock(item, true, $event)">
                                                                        <a href="javascript:;">
                                                                            <i class="am-icon-lock"></i> 锁定
                                                                        </a>
                                                                    </li>
                                                                    <li @click="doLock(item, false, $event)">
                                                                        <a href="javascript:;">
                                                                            <i class="am-icon-unlock-alt"></i> 解锁
                                                                        </a>
                                                                    </li>
                                                                    <li @click.stop="showError(item)">
                                                                        <a href="javascript:;">
                                                                            <i class="am-icon-warning"></i> 报警日志
                                                                        </a>
                                                                    </li>
                                                                </ul>
                                                            </div>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </template>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="am-u-md-6 am-u-sm-12 row-mb">

                        <div class="tpl-portlet">
                            <div class="tpl-portlet-title" style="overflow: inherit;">
                                <div class="tpl-caption font-green ">
                                    <span>数据区</span>
                                </div>
                                <div class="tpl-portlet-input">
                                    <button type="button" @click="doClearMsg" class="am-btn am-btn-warning">清除</button>
                                    <div class="am-checkbox-inline">
                                        <label>
                                            <input type="checkbox" v-model="isShow"> 显示
                                        </label>
                                    </div>
                                    <div class="am-checkbox-inline">
                                        <label>
                                            <input type="checkbox" v-model="autoClear"> 自动清除
                                        </label>
                                    </div>
                                    <div class="am-checkbox-inline">
                                        <label>
                                            <input type="checkbox" @click="doSkipEnd" v-model="isSkipEnd"> 跳转最后
                                        </label>
                                    </div>
                                    <div class="am-dropdown" data-am-dropdown>
                                        <button class="am-btn am-btn-primary am-dropdown-toggle" data-am-dropdown-toggle>更多设置<span class="am-icon-caret-down"></span></button>
                                        <ul class="am-dropdown-content am-dropdown-contentChange">
                                            <template v-if="CurrVar.Level === 1">
                                                <li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" v-model="Commands.NetworkState.isCheck" @click="doListenMessage(Commands.NetworkState)" /> 机头网络状态报告
                                                        </label>
                                                    </div>
                                                </li>
                                            </template>
                                            <template v-else>
                                                <li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" v-model="Commands.ParamApply.isCheck" @click="doListenMessage(Commands.ParamApply)"> 游戏机参数申请
                                                        </label>
                                                    </div>
                                                </li>
                                                <li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" v-model="Commands.AddressAllot.isCheck" @click="doListenMessage(Commands.AddressAllot)"> 机头地址动态分配
                                                        </label>
                                                    </div>
                                                </li>
                                                <!--<li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" value="133" @click="doListenMessage"> 电子币模式投币数据
                                                        </label>
                                                    </div>
                                                </li>-->
                                                                                            <!--<li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" value="4" @click="doListenMessage"> 电子币模式电子出票数据
                                                        </label>
                                                    </div>
                                                </li>-->
                                                <li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" v-model="Commands.ICCoins.isCheck" @click="doListenMessage(Commands.ICCoins)"> IC卡进出币数据
                                                        </label>
                                                    </div>
                                                </li>
                                                <!--<li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" value="6" @click="doListenMessage"> IC卡查询数据
                                                        </label>
                                                    </div>
                                                </li>-->
                                                <li>
                                                    <div class="am-checkbox-inline">
                                                        <label>
                                                            <input type="checkbox" v-model="Commands.Alarm.isCheck" @click="doListenMessage(Commands.Alarm)"> 机头卡片报警数据
                                                        </label>
                                                    </div>
                                                </li>
                                            </template>
                                            
                                        </ul>
                                    </div>
                                </div>
                            </div>
                            <div id="wrapper" class="wrapper" style="overflow-y:auto">
                                <div id="scroller" class="scroller1">
                                    <ul id="ul-scroller" class="tpl-task-list tpl-task-remind">
                                        <li v-for="item in Messages">
                                            <div class="cosA">
                                                <!--<span class="cosIco">
                                                    <i class="am-icon-bolt"></i>
                                                </span>-->

                                                <span>
                                                    <span class="task-title-sp" v-html="item.MsgContent"> </span>
                                                </span>
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                </div>
            </div>

        </div>
    </div>
    <script src="assets/js/jquery.min.js"></script>
    <script src="assets/js/amazeui.min.js"></script>
    <script src="assets/layer/layer.js"></script>
    <script src="assets/js/app.js"></script>
    <script src="Scripts/jquery.signalR-2.2.2.min.js"></script>
    <script src="signalr/hubs"></script>
    <script src="assets/js/vue.min.js"></script>
    <script src="assets/js/highcharts.js"></script>

    <script type="text/javascript">
        Date.prototype.Format = function (fmt) {
            var o = {
                "M+": this.getMonth() + 1, //月份 
                "d+": this.getDate(), //日 
                "H+": this.getHours(), //小时 
                "m+": this.getMinutes(), //分 
                "s+": this.getSeconds(), //秒 
                "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
                "S": this.getMilliseconds() //毫秒 
            };
            if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
            for (var k in o)
                if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
            return fmt;
        }

        var clientId = new Date().Format("ddHHmmssS") + "" + GetRandomNum(10000, 100000);

        var interval = null;
        var curHub = $.connection.MyHub;

        //接收服务端信息
        curHub.client.pullInst = function (data) {
            if (data.status == 200) {
                vm.RecvData = data;
                window.setTimeout(function () {
                    $('.dropdown-uler').dropdown({ justify: $('.dropdown-wrapper').parent() });
                }, 500)
            }
            else {
                console.log(data.msg);
            }
        };

        curHub.client.hubCall = function (data) {
            if(data.status !== 200)
            {
                layer.msg(data.msg);
            }
        };

        //接收指令消息
        curHub.client.showMessage = function (data) {
            vm.appendMessage(data);
        };

        //接收速率
        curHub.client.networkSpeed = function (data) {
            if (vm.SpeedData.RecvList.length >= 50) {
                vm.SpeedData.RecvList.shift();
                vm.SpeedData.SendList.shift();
            }
            vm.SpeedData.RecvList.push(data.RecvSpeed);
            vm.SpeedData.SendList.push(data.SendSpeed);
            vm.SpeedData.CurrSpeed = data;

            randomData(vm.SpeedData);
        };

        curHub.client.initDeviceCall = function (data) {
            window.setTimeout(function () {
                $("#route-reset").removeClass("am-icon-spin");
                $("#reInitDevice").removeClass("am-icon-spin");
            }, 2000)
            if (data.status !== 200) {
                layer.msg(data.msg);
            }
        }

        var vm = new Vue({
            el: '#app',
            data: {
                isShow: true,
                autoClear: false,
                isSkipEnd: false,
                RecvData: {
                    Instructions: {},
                    RouteList: [],
                    DeviceList: []
                },
                CurrVar: {
                    CurrRouteId: 0,
                    CurrRouteName: "",
                    Level: 1
                },
                Messages: [],
                SpeedData: {
                    RecvList: [],
                    SendList: [],
                    CurrSpeed: {}
                },
                Commands: {
                    NetworkState: { isCheck: false, value: 138 },
                    ParamApply: { isCheck: false, value: 128 },
                    AddressAllot: { isCheck: false, value: 141 },
                    ICCoins: { isCheck: false, value: 133 },
                    Alarm: { isCheck: false, value: 137 },
                },
                ListenRouter: "",
                ListenDevice: ""
            },
            mounted: function () {
                $.connection.hub.start().done(function () {
                    vm.clearListen();
                    vm.pushInst();
                });

                window.setTimeout(function () {
                    $('#ajax-loader').fadeOut();
                }, 300);

                this.$nextTick(function () {
                    interval = setInterval(vm.pushInst, 2000);
                })
            },
            watch: {
                Messages: function () {
                    if (this.isSkipEnd) {
                        this.$nextTick(function () {
                            document.getElementById('wrapper').scrollTop = document.getElementById('ul-scroller').scrollHeight;
                        })
                    }
                }
            },
            methods: {
                pushInst: function () {
                    var _vm = this;
                    curHub.server.pushRoutersAndInst(clientId, _vm.CurrVar.Level, _vm.CurrVar.CurrRouteId);
                },
                doLock: function (item, isLock, event) {
                    var _vm = this;
                    curHub.server.lockDevice(item.MCUID, isLock);
                    
                    var $dropdown = $(event.currentTarget).closest("div");

                    $dropdown.dropdown('close');
                },
                doInCoin: function () {
                    curHub.server.inCoins("f482d424", "01", "10000010", 10);
                },
                doOutCoin: function () {
                    curHub.server.outCoins("f482d424", "01");
                },
                clearListen: function () {
                    //curHub.server.clearMessageCommand();//清除所有消息指令
                },
                getRouter: function () {
                    var _vm = this;
                    _vm.clearListen();
                    _vm.CurrVar.CurrRouteId = 0;
                    _vm.CurrVar.CurrRouteName = "";
                    _vm.CurrVar.Level = 1;
                    _vm.pushInst();
                },
                getChild: function (id, name) {
                    var _vm = this;
                    _vm.clearListen();
                    _vm.clearHighlightClass();
                    _vm.Commands.NetworkState.isCheck = false; //取消机头网络报告的勾选
                    _vm.CurrVar.CurrRouteId = id;
                    _vm.CurrVar.CurrRouteName = name;
                    _vm.CurrVar.Level = 2;
                    _vm.pushInst();
                },
                doReset: function (item) {
                    $("#route-reset").addClass("am-icon-spin");
                    curHub.server.resetRouter(item.Segment);
                },
                showError: function (item) {
                    var _vm = this;
                    var params = "";
                    if (_vm.CurrVar.Level == 1) {
                        param = "?route=" + item.RouterToken;
                    }
                    else {
                        param = "?route=" + _vm.CurrVar.CurrRouteId + "&sn=" + item.SN;
                    }
                    layer.open({
                        type: 2,
                        title: '报警日志',
                        shadeClose: true,
                        shade: [0.3, '#393D49'],
                        shadeClose: false,
                        //maxmin: true, 
                        //btn: ['关闭'],
                        closeBtn: 1,
                        area: ['1300px', '600px'],
                        content: 'ServicePage/warnlog.html' + param
                    });
                    var $dropdown = $(event.currentTarget).closest("div");
                    $dropdown.dropdown('close');
                },
                appendMessage: function (item) {
                    var _this = this;
                    if (_this.autoClear && _this.Messages.length > 50) {
                        _this.Messages = [];
                    }
                    if (_this.isShow) {
                        _this.Messages.push(item);
                    }
                },
                doSkipEnd: function () {
                    document.getElementById('wrapper').scrollTop = document.getElementById('ul-scroller').scrollHeight;
                },
                doClearMsg: function () {
                    this.Messages = [];
                },
                doInitDevice: function () {
                    $("#reInitDevice").addClass("am-icon-spin");
                    curHub.server.initDevice();
                },
                doListenMessage: function (item) {
                    this.$nextTick(function () {
                        curHub.server.setMessageCommandType(item.value, item.isCheck);
                    });
                },
                selectRouter: function (item) { 
                    if (event.target.nodeName == "TD") {
                        var isListen = false;
                        this.setRowClass(event.currentTarget, function (flag) {
                            isListen = flag;
                        });
                        curHub.server.setListenRouter(item.Segment, isListen);
                    }
                },
                selectDevice: function (item) {
                    if (event.target.nodeName == "TD") {
                        var isListen = false;
                        this.setRowClass(event.currentTarget, function (flag) {
                            isListen = flag;
                        });
                        this.ListenDevice = item.Address;
                        curHub.server.setListenDevice(this.CurrVar.CurrRouteId, this.ListenDevice, isListen);
                    }
                },
                setRowClass: function (el, callback) {
                    var $el = $(el);
                    if ($el.hasClass("backgroundH")) {
                        $el.removeClass("backgroundH");
                        callback(false)
                    } else {
                        $el.addClass("backgroundH").siblings().removeClass("backgroundH");
                        callback(true);
                    }
                },
                clearHighlightClass: function () {
                    $("#deviceTable").find("tr").each(function () {
                        if ($(this).hasClass("backgroundH")) {
                            $(this).removeClass("backgroundH");
                            return false;
                        }
                    })
                }
            }
        });


        var chart = null;
        function randomData(speedData) {
            chart = Highcharts.chart('lineChatsMian', {
                chart: {
                    zoomType: 'x'
                },
                title: {
                    text: '当前网络速率:上行 ' + speedData.CurrSpeed.RecvSpeed + ' 下行 ' + speedData.CurrSpeed.SendSpeed
                },
                credits: {
                    text: '',
                    href: '',
                    enabled: false
                },
                xAxis: {
                    //type: 'category',
                    visible: false
                },
                yAxis: {
                    title: {
                        text: '速率'
                    },
                    min: 0
                },
                legend: {
                    enabled: true
                },
                plotOptions: {
                    area: {
                        pointStart: 1,
                        //fillOpacity: 0.3,
                        marker: {
                            enabled: false,
                            symbol: 'circle',
                            radius: 2,
                            states: {
                                hover: {
                                    enabled: true
                                }
                            }
                        },
                        threshold: null,
                        animation: false
                    }
                },
                series: [{
                    type: 'area',
                    name: '下行',
                    color: '#FF6347',
                    data: speedData.SendList
                }, {
                    type: 'area',
                    name: '上行',
                    color: '#00FA9A',
                    data: speedData.RecvList
                }]
            });
        }

        function GetRandomNum(n, m) {
            var random = Math.floor(Math.random() * (m - n + 1) + n);
            return random;
        }
    </script>
</body>
</html>

