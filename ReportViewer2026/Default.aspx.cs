using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace ReportViewer
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void lnkDownload_Click(object sender, EventArgs e)
        {
            GenerateReportFile();
        }

        //adapted from http://www.codeproject.com/Articles/492739/Exporting-to-Word-PDF-using-Microsoft-Report-RDLC
        private void GenerateReportFile()
        {
            LocalReport report = new LocalReport();
            report.ReportPath = "Example.rdlc";

            // todo: get data into a dataset so the report has something to display
            ReportDataSource rds = new ReportDataSource();
            rds.Name = "dsTableList";//This refers to the dataset name in the RDLC file
            rds.Value = new System.Data.DataTable();// { TableName="TableList", Columns = { new System.Data.DataColumn("Name"), new System.Data.DataColumn("object_id"), new System.Data.DataColumn("type") } };
            report.DataSources.Add(rds);

            Byte[] mybytes = report.Render("PDF");

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.BinaryWrite(mybytes);
        }  


    }
}