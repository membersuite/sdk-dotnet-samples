<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs"
    Inherits="_Default" %>

<html>
<head>
    <title>Portal Single Sign On Sample</title>
</head>
<body>
    <form runat="server" DefaultButton="btnLogin">
    <h2>
        Portal Single Sign On Sample
    </h2>
    <p>
        <asp:Label runat="server" ForeColor="Red" ID="lblError" Visible="False"></asp:Label>
    </p>
    Portal User Name:
    <asp:TextBox runat="server" ID="tbUserName"></asp:TextBox>
    <br />
    Portal Password (optional):
    <asp:TextBox runat="server" ID="tbPassword" TextMode="Password"></asp:TextBox>
    <br />
    <br />
    <i>Leave the box below checked if you do not want to verify the password before initiating single sign on.</i>
    <br />
        <asp:CheckBox ID="cbVerifyCrentials" runat="server" Checked="true" Text="Do not verify login crendentials"/>
    <asp:Button runat="server" ID="btnLogin" Text="Login" OnClick="btnLogin_Click"  />
    </form>
</body>
</html>
