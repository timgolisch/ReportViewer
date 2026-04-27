<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ReportViewer2008.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>Welcome.  This is an example of showing RDL (SSRS Report) files without the use of a SSRS server.</p>
        <p>This is the older version that is designed to work with SSRS 2008 reports and .NET 3.5.  If it doesn't work, try the diagnostic page, or try the 2013 project.</p>
        <p>I hope you find this demo useful.  If you can't figure it out, leave me a message in CodeProject.com.  I monitor it weekly and I am glad to help others.</p>

    <ul>
        <li><a href="View.aspx">View Report</a></li>
        <li><a href="Diagnostic.aspx">Diagnostic page</a> for troubleshooting.</li>
        <li><asp:LinkButton ID="lnkDownload" runat="server" onclick="lnkDownload_Click">Download Report</asp:LinkButton></li>
    </ul>
    </div>
    </form>
</body>
</html>
