<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeviceInit.aspx.cs" Inherits="XXCloudService.Test.DeviceInit" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <ul id="base-form">
            <li class="row-item">
                <div class="label-wrap">
                    <label for="txtMerchId">商户ID</label>
                </div>
                <div class="ctn-wrap">
                    <asp:TextBox ID="txtMerchId" runat="server"></asp:TextBox>
                </div>
            </li>
            <li class="row-item">
                <div class="label-wrap">
                    <label for="txtStoreId">门店ID</label>
                </div>
                <div class="ctn-wrap">
                    <asp:TextBox ID="txtStoreId" runat="server"></asp:TextBox>
                </div>
            </li>
            <li class="row-item">
                <div class="label-wrap">
                    <label for="txtDBIP">数据库IP</label>
                </div>
                <div class="ctn-wrap">
                    <asp:TextBox ID="txtDBIP" runat="server" Text="192.168.1.119"></asp:TextBox>
                </div>
            </li>
            <li class="row-item">
                <div class="label-wrap">
                    <label for="txtDBPwd">数据库密码</label>
                </div>
                <div class="ctn-wrap">
                    <asp:TextBox ID="txtDBPwd" runat="server" Text="xinchen"></asp:TextBox>
                </div>
            </li>
            <li class="row-item">
                <div class="label-wrap">
                    <label for="txtUdpPort">UDP端口</label>
                </div>
                <div class="ctn-wrap">
                    <asp:TextBox ID="txtUdpPort" runat="server" Text="6066"></asp:TextBox>
                </div>
            </li>
            <li class="row-item">
                <div class="label-wrap">
                    <label>&nbsp;</label>
                </div>
                <div class="ctn-wrap">
                    <asp:Button ID="Button1" runat="server" Text="   提   交   " OnClick="Button1_Click" />
                    <asp:Button ID="Button2" runat="server" Text="   写入序列号   " OnClick="Button2_Click" />
                </div>
            </li>
        </ul>
    </div>
    </form>
</body>
</html>
