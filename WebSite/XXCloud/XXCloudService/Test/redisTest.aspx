<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="redisTest.aspx.cs" Inherits="XXCloudService.Test.redisTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="Button1" runat="server" Text="生成流水号" OnClick="Button1_Click" />

        <asp:Button ID="Button2" runat="server" Text="生成加密连接字符串" OnClick="Button2_Click" />
    </div>
    </form>
</body>
</html>
