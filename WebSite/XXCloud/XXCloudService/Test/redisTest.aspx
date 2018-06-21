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
    <hr />
    <div>
        <asp:TextBox ID="txtToken" runat="server"></asp:TextBox>

        <asp:Button ID="Button3" runat="server" Text="读取令牌" OnClick="Button3_Click"  />
    </div>
        <hr />
    <div>
        openid:<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        subscribe:<asp:TextBox ID="TextBox2" Text="0" runat="server"></asp:TextBox>
        headimgurl:<asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
        nickname:<asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>

        <asp:Button ID="Button4" runat="server" Text="写入缓存" OnClick="Button4_Click"  />
    </div>
    <hr />
    <div>
        <asp:Button ID="Button5" runat="server" Text="获取博彩类" OnClick="Button5_Click"  />
        <asp:Button ID="Button6" runat="server" Text="添加会员" OnClick="Button6_Click"  />
        <asp:Button ID="Button7" runat="server" Text="读取会员缓存" OnClick="Button7_Click"  />
    </div>
    <hr />
    <div>

        <asp:Button ID="Button8" runat="server" OnClick="Button8_Click" Text="Redis过期时间连续写入测试" />

    </div>
    </form>
</body>
</html>
