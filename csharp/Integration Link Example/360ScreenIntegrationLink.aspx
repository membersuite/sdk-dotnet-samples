<%@ Page Title="" Language="C#" MasterPageFile="~/IntegrationLink.master" AutoEventWireup="true" CodeFile="360ScreenIntegrationLink.aspx.cs" Inherits="_360ScreenIntegrationLink" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PageTitle" Runat="Server">
    360° Screen Integration Link
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <p>
    This page demonstrates the capability of embedding an integration link on a 360 screen. In addition to a single
    sign on, an additional piece of information is transmitted - the context of the record being viewed on the 360 screen.
    </p>
    <h2>Record Information</h2>
    <p>
        <b>Record Type:</b> <asp:Literal ID="lRecordType" runat="server"/><br />
        <b>Record ID:</b> <asp:Literal ID="lRecordID"  runat="server"/><br />
        <b>Record Name:</b> <asp:Literal ID="lRecordName"  runat="server"/>

    </p>
<h2>Record Fields/Properties:</h2>
<ul>
<asp:Repeater ID="rptFields" runat="server">
    <ItemTemplate>
        
        <li><b><%#DataBinder.Eval( Container.DataItem, "Key") %> </b>: <%#DataBinder.Eval( Container.DataItem, "Value") %></li>
    </ItemTemplate>

</asp:Repeater>
</ul>
</asp:Content>

