<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ReportViewer2013._Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="frmDefault" runat="server">
        <h1>Report Viewer for Studio 2012+</h1>
        <div>
            <b>Intro:</b> This project is designed to run .RDL files (SSRS reports) which were made for SSRS 2012+, without installing SSRS.  
            My previous project was designed to work with .RDL files which were made for SSRS 2008.  It would not run reports for Report Builder 3+ or SSRS 2010+.
        </div>
        <div>
            <b>Examples:</b><br />
            <ul>
                <li><a href="View.aspx">Example</a> - SQL Admin report of all tables, by DB (requires admin read permissions)</li>
                <li><a href="View.aspx?Path=SSRS_Users&Database=SSRS">SSRS Users</a> - Shows all SSRS users (in SSRS user table)</li>
                <li><a href="Diagnostics.aspx">Diagnostics</a> - use this page to troubleshoot your settings and get ideas which will help get this demo running.</li>
            </ul>

        </div>
    </form>
</body>
</html>
