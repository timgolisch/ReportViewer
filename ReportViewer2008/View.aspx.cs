using System;
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;

namespace ReportViewer
{
    /// <summary>
    /// Usage: http://localhost/View.aspx?Path=Report1&Param1=216&OtherParam=etc
    /// example: http://localhost:1337/View.aspx?Path=Error%20Log.rdl
    /// </summary>
    public partial class View : System.Web.UI.Page
    {
        #region Private Properties 
        #region wrappers for web.config
        /// <summary>
        /// Report Path setting that was stored in the web.config
        /// </summary>
        private string ReportPath
        {
            get 
            { 
                if (ReportViewer2008.Properties.Settings.Default.ReportPath.StartsWith("/"))
                    return Server.MapPath(ReportViewer2008.Properties.Settings.Default.ReportPath);
                else if (System.IO.Directory.Exists(ReportViewer2008.Properties.Settings.Default.ReportPath))
                    return ReportViewer2008.Properties.Settings.Default.ReportPath;
                else
                    return Server.MapPath(ReportViewer2008.Properties.Settings.Default.ReportPath);
            }
            //set { }
        }
        
        /// <summary>
        /// Database connection string that was stored in the web.config
        /// </summary>
        private string DBConnectionString
        {
            get
            {
                return ReportViewer2008.Properties.Settings.Default.DsnReport;
            }
            //set { }

        }
        #endregion

        #region Wrapper for QueryString parameters
        /// <summary>
        /// all report params that were passed-in via the QueryString
        /// </summary>
        private System.Collections.Hashtable ReportParameters
        {
            get
            {
                System.Collections.Hashtable re = new System.Collections.Hashtable();
                //gather any params so they can be passed to the report
                foreach (string key in Request.QueryString.AllKeys)
                {
                    if (key.ToLower() != "path")//ignore the “path” param. It describes the report's file path
                    {
                        re.Add(key, Request.QueryString[key]);
                    }
                }
                return re;
            }
            //set { }
        }

        /// <summary>
        /// the report file info (filename, ext, path, etc)
        /// </summary>
        private System.IO.FileInfo ReportFile
        {
            get
            {
                System.IO.FileInfo re = null;
                try
                {
                    string reportFullPath = "", reportName = "";

                    if (Request.QueryString["path"] != null)
                    {
                        reportName = Request.QueryString["path"];
                    }

                    //check to make sure the file ACTUALLY exists, before we start working on it
                    if (System.IO.File.Exists(System.IO.Path.Combine(this.ReportPath, reportName)))
                    {
                        reportFullPath = System.IO.Path.Combine(this.ReportPath, reportName);
                        reportName = reportName.Substring(0, reportName.LastIndexOf(".") - 1);
                    }
                    else if (System.IO.File.Exists(System.IO.Path.Combine(this.ReportPath, reportName + ".rdl")))
                        reportFullPath = System.IO.Path.Combine(this.ReportPath, reportName + ".rdl");
                    else if (System.IO.File.Exists(System.IO.Path.Combine(this.ReportPath, reportName + ".rdlc")))
                        reportFullPath = System.IO.Path.Combine(this.ReportPath, reportName + ".rdlc");

                    if (reportFullPath != "")
                        re = new System.IO.FileInfo(reportFullPath);
                }
                finally { }

                return re;
            }
            //set { }
        }

        /// <summary>
        /// the Report file (.rdl/.rdlc) de-serialized into an object
        /// </summary>
        private RDL.Report ReportDefinition
        {
            get
            {
                System.IO.FileInfo reportFile = this.ReportFile;
                if (reportFile != null)
                    return RDL.Report.GetReportFromFile(reportFile.FullName);
                else
                    return new RDL.Report();
            }
        }
        #endregion
        #endregion

        #region Event Handlers
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //call the report
                if (Request.QueryString["path"] != null)
                    ShowReport(); //with data
                else
                    ShowBlankReport(); //blank (default)

            }
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadReportFile();
        }

        #endregion

        #region Private Helper Methods
        private void ShowBlankReport()
        { 
            //look up the report file
            string reportPath = ReportPath + "Error Log.rdl";

            //attach report parameters, if any
            if (ReportParameters.Count > 0)
            {
                ReportParameter[] param = new ReportParameter[ReportParameters.Count];
                int x = 0;
                foreach (string key in ReportParameters.Keys)
                {
                    param[x] = new ReportParameter(key, ReportParameters[key].ToString());
                    x++;
                }
                rvReportViewer.LocalReport.SetParameters(param);
            }

            //attach an empty dataset to fill
            System.Data.DataSet ds = new System.Data.DataSet() { Tables = { new System.Data.DataTable() } };
            ReportDataSource rds = new ReportDataSource("dsErrorLog", ds.Tables[0]);
            rvReportViewer.LocalReport.DataSources.Clear();
            rvReportViewer.LocalReport.DataSources.Add(rds);

            //load the data
            rvReportViewer.LocalReport.Refresh();

            //load the report definition
            rvReportViewer.LocalReport.ReportPath = reportPath;

        }

        /// <summary>
        /// Loads DataSources to the RDLC Report
        /// </summary>
        protected void ShowReport()
        {
            //adapted from http://www.codeproject.com/Articles/37845/Using-RDLC-and-DataSets-to-develop-ASP-NET-Reporti

            System.IO.FileInfo reportFullPath = this.ReportFile;
            //check to make sure the file ACTUALLY exists, before we start working on it
            if (reportFullPath != null)
            {
                //map the reporting engine to the .rdl/.rdlc file
                rvReportViewer.LocalReport.ReportPath = reportFullPath.FullName;

                //  1. Clear Report Data
                rvReportViewer.LocalReport.DataSources.Clear();

                //  2. Load new data
                // Look-up the DB query in the "DataSets" element of the report file (.rdl/.rdlc which contains XML)
                RDL.Report reportDef = this.ReportDefinition;

                // Run each query (usually, there is only one) and attach it to the report
                foreach (RDL.DataSet ds in reportDef.DataSets)
                {
                    //copy the parameters from the QueryString into the ReportParameters definitions (objects)
                    ds.AssignParameters(this.ReportParameters);

                    //run the query to get real data for the report
                    System.Data.DataTable tbl = ds.GetDataTable(DBConnectionString);

                    //attach the data/table to the Report's dataset(s), by name
                    ReportDataSource rds = new ReportDataSource();
                    rds.Name = ds.Name; //This refers to the dataset name in the RDLC file
                    rds.Value = tbl;
                    rvReportViewer.LocalReport.DataSources.Add(rds);
                }
                rvReportViewer.LocalReport.EnableExternalImages = true;
                rvReportViewer.LocalReport.Refresh();
            }
        }

        /// <summary>
        /// Generate a PDF (as a download)
        ///adapted from http://www.codeproject.com/Articles/492739/Exporting-to-Word-PDF-using-Microsoft-Report-RDLC
        /// </summary>
        /// <param name="reportFileName">the unqualified report file name.  Like "Report1" for Report1.rdl</param>
        /// <param name="reportParams">Name/Value pairs for report parameters</param>
        private void DownloadReportFile()
        {
            //this SSRS object allows us to run the report in-memory without a ReportViewer control
            LocalReport report = new LocalReport();

            System.IO.FileInfo reportFullPath = this.ReportFile;
            //check to make sure the file ACTUALLY exists, before we start working on it
            if (reportFullPath != null)
            {
                //report name, minus the file extension (so the PDF will have a similar file name)
                string reportName = reportFullPath.Name.Substring(0, reportFullPath.Name.Length - reportFullPath.Extension.Length - 1);

                //map the reporting engine to the .rdl/.rdlc file
                report.ReportPath = reportFullPath.FullName;

                //pull the query ("DataSets") info from the report file (.rdl/.rdlc)
                RDL.Report reportDef = this.ReportDefinition;

                foreach (RDL.DataSet ds in reportDef.DataSets)
                {
                    //copy the parameters from the QueryString into the ReportParameters definitions (objects)
                    ds.AssignParameters(this.ReportParameters);

                    //run the query to get real data for the report
                    System.Data.DataTable tbl = ds.GetDataTable(DBConnectionString);

                    //attach the data/table to the Report's dataset(s), by name
                    ReportDataSource rds = new ReportDataSource();
                    rds.Name = ds.Name; //This refers to the dataset name in the RDLC file
                    rds.Value = tbl;// { TableName="TableList", Columns = { new System.Data.DataColumn("Name"), new System.Data.DataColumn("object_id"), new System.Data.DataColumn("type") } };
                    report.DataSources.Add(rds);
                }
                

                //Run the report
                Byte[] mybytes = report.Render("PDF");

                //output the PDF via the binary response stream
                Response.Clear();
                Response.AddHeader("content–disposition", "attachment; filename=" + reportName + ".pdf");
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(mybytes);
            }
            else
            {
                //punt
                Response.Clear();
                Response.ContentType = "text/plain";
                Response.Write("Error: cannot find the report file [" + Request.QueryString["Path"] + "] in the configure report path.");
            }

        }  


        //Further reading:
        //sub-reports: http://www.codeproject.com/Articles/473844/Using-Custom-Data-Source-to-create-RDLC-Reports

        #endregion

    }
}