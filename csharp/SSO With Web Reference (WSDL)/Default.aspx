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
    <asp:Button runat="server" ID="btnLogin" Text="Login" OnClick="btnLogin_Click" />
    </form>
</body>
</html>
