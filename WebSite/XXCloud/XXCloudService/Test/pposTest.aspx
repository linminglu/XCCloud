<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="pposTest.aspx.cs" Inherits="XXCloudService.Test.pposTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="Button1" runat="server" Text="微信公众号查询" OnClick="Button1_Click" />
    
    </div>
        <p>
        <asp:Button ID="Button2" runat="server" Text="微信公众号支付" OnClick="Button2_Click" />
    
        </p>
        <p>
            &nbsp;</p>
        <p>
            支付渠道订单号：<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    
        </p>
        <p>
            商户订单号：<asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
    
        </p>
        <asp:Button ID="Button3" runat="server" Text="退款" OnClick="Button3_Click" />
    
        <br />
        <br />
        <br />
        <asp:Button ID="Button4" runat="server" Text="订单查询" OnClick="Button4_Click" />
        <br />
        <br />
    <hr />
        <br /><br />

    <div>
        <asp:Button ID="Button5" runat="server" Text="购币测试" OnClick="Button5_Click" />
    </div>
    </form>
</body>
</html>
