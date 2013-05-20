<%@ Page Title="" Language="C#" MasterPageFile="~/IntegrationLink.master" AutoEventWireup="true"
    CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageTitle" runat="Server">
    MemberSuite Integration Links Demo
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div>
        The purpose of this demo is to show the power of MemberSuite integration links.
        When a user navigates to this site through an integration link, we will use the
        session ID to "re-login" and retrieve the user information. We can then provide
        a completely custom application to the user.
    </div>
    <p>
        &nbsp;</p>
    <div>
        <font color="green">MemberSuite Current User Name: </font>
        <asp:Label ID="lblUserName" runat="server" />
        <br />
        <font color="green">User's Current Department: </font>
        <asp:Label ID="lblUserDepartment" runat="server" />
        <br />
        <font color="green">Change User Department: </font>
        <asp:TextBox ID="tbUserDepartment" runat="server" />
        <p />
        When you click <b>Change Department</b>, this application will use the Concierge
        API to change the department of the current user.
        <p />
        <asp:Button ID="btnChangeDepartment" runat="server" Text="Change Department" OnClick="btnChangeDepartment_Click" />
    </div>
</asp:Content>
