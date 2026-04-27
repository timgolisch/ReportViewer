<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Diagnostics.aspx.cs" Inherits="ReportViewer2013.Diagnostics" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Diagnostics</title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Diagnostics for the Report Viewer project</h1>
        <div>
            <b>Intro:</b> If you are having problems getting these example files to run, 
            check this information and confirm that the settings are correct, 
            the permissions are valid and required files are present.
        </div>
        <div>
            <b>Diagnostics:</b><br />
            <ul>
                <li>DB Connection String: <asp:Label ID="lblConnectionString" runat="server" /></li>
                <li>Report Path: <asp:Label ID="lblReportPath" runat="server" /></li>
                <li>Connection Test Results: <asp:Label ID="lblConnectionTest" runat="server" /></li>
                <li>Report Path Permissions: </li>
            </ul>

        </div>
    </form>
</body>
</html>
