<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GoodReqWorkFlowTest.aspx.cs" Inherits="XXCloudService.Test.WorkFlowTest.GoodReqWorkFlowTest" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>礼品调拨</title>
    <script type="text/javascript" src="/website/js/jquery-1.8.3-min.js"></script>
    <script type="text/javascript">
        function common(url, parasJson) {
            $.ajax({
                type: "post",
                url: url,
                contentType: "application/json; charset=utf-8",
                data: parasJson,
                success: function (data) {
                    console.log(data);
                },
                error: function (error) {
                    alert(error); 
                }
            });
        }        

        function fireGoodRequest() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodRequest";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireGoodRequestV() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodRequestV";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireGoodOut() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodOut";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireGoodOutV() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodOutV";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireGoodIn() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodIn";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireGoodInV() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireGoodInV";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireCancel() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireCancel";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        function fireClose() {
            var parasObj = { "eventId": 1, "userId": 10 };
            var url = "/Test/WorkFlowTest/WorkFlowHandler.ashx?method=fireClose";
            var parasJson = JSON.stringify(parasObj);
            common(url, parasJson);
        }
        
    </script>
</head>
<body>
    <form id="sendForm">
        <input type="button" value="申请" onclick="fireGoodRequest()" />
        <input type="button" value="申请审核" onclick="fireGoodRequestV()" />
        <input type="button" value="出库" onclick="fireGoodOut()" />
        <input type="button" value="出库审核" onclick="fireGoodOutV()" />
        <input type="button" value="入库" onclick="fireGoodIn()" />
        <input type="button" value="入库审核" onclick="fireGoodInV()" />
        <input type="button" value="撤销" onclick="fireCancel()" />
        <input type="button" value="关闭" onclick="fireClose()" />        
    </form>
    <pre id="incomming"></pre>
</body>
</html>
