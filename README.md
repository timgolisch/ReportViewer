# Report Viewer

This project is a web app, written in C#. 
It was originally written as an example on CodeProject.com (which was shut-down in 2025).
The purpose is to allow programmers/users to use/run a RDL/RDLC (SSRS report) without having to install SSRS.
The nuget package Microsoft.ReportViewer.WebForms and Microsoft.ReportViewer.Common are required.

# Introduction
This is a common scenario: You have a web site, written in ASP.NET (or MVC or SharePoint) and you would like to display some reports. You might be planning to write some new reports and you are trying to decide on which technology to use or you might have several SSRS reports that were already made, earlier, and you want to run them from your ASP.NET site.

There are many good articles on the internet that show how to use a RDLC file to run a report, either from ASP.NET or WinForms, etc. After reading them, it would seem that your choices are:

1. Put a report into SSRS and use the ReportViewer control to call SSRS to run your report
2. Add the .RDL or .RDLC file into your project and create some objects to house the data so the report has an interface to the database

I wanted a third choice:
3. Just run it like SSRS does, but without installing the SSRS server. I’m talking bare-minimum footprint. I just want to pass the name of the .RDL or .RDLC file to an ASPX page and have it run. That is how SSRS does it. I should be able to do that too.

This is an implementation of the third choice.

# Prerequisites
To run this, you still need to install the reporting run-time. You can get it from nuget. It is called Microsoft.ReportViewer. You need Common and either NETCore or the ole WebForms.
All of my reports were made Visual Studio using the VS Extension called "Microsoft RDLC Report Designer" or one of the editors that comes with SSRS, etc.

# Strategy
A .RDLC file is all XML. If you open it with Notepad and inspect the contents, you can see that the XML describes the display/design, but it also contains several other useful characteristics. One of them is the query (to get the data) for the report.

My strategy is to extract the database query, set up any parameters, run the query, store the results in a DataTable(s) and feed it into the report.

My goal is to run my reports from a generic page and pass the report name and any query parameters via the URL QueryString, like this:

`.../View.aspx?Report=Example.rdlc&StartDate=1/1/2012&EndDate=12/31/2012`

For simplicity, I will just use the same DB connection string that is used by the rest of the app but I will wrap it in a local factory method, for maintainability.

# Using the Code
Starting with the end in mind
This is where I started. I borrowed from other articles on CodeProject (which is gone now). The examples worked great, but didn't have a means to use the query that was embedded in the report. In the code-block (below), you can see that I created a class called Report (namespace RDL) to encapsulate the RDLC’s content/structure. My RDL.Report class also contains a factory method to help with converting the XML into objects.

### View.aspx.cs
```
protected void ShowReport()
{
  System.IO.FileInfo reportFullPath = this.ReportFile;
  //check to make sure the file ACTUALLY exists, before we start working on it
  if (reportFullPath != null)
  {
     //map the reporting engine to the .rdl/.rdlc file
     rvReportViewer.LocalReport.ReportPath = reportFullPath.FullName;  

     // 1. Clear Report Data
     rvReportViewer.LocalReport.DataSources.Clear();

     // 2. Get the data for the report
     // Look-up the DB query in the "DataSets" 
     // element of the report file (.rdl/.rdlc which contains XML)
     RDL.ReportreportDef = RDL.Report.GetReportFromFile(reportFullPath.FullName);

     // Run each query (usually, there is only one) and attach it to the report
     foreach (RDL.DataSet ds in reportDef.DataSets)
     {
        //copy the parameters from the QueryString into the ReportParameters definitions (objects)
        ds.AssignParameters(this.ReportParameters);

        //run the query to get real data for the report
        System.Data.DataTable tbl = ds.GetDataTable(this.DBConnectionString);

        //attach the data/table to the Report's dataset(s), by name
        ReportDataSource rds = new ReportDataSource();
        rds.Name = ds.Name; //This refers to the dataset name in the RDLC file
        rds.Value = tbl;
        rvReportViewer.LocalReport.DataSources.Add(rds);
     }
     rvReportViewer.LocalReport.Refresh();
  }
}  
```

# The rest of the story
The (above) block of code shows the heart of the application; running queries and attaching data to the report, then running the report. Now, let’s look at the parts that get the data.

## Get the query from the RDLC file
Inside the .RDLC file, the XML for the query looks like this (after you remove everything else):

```
<Report>
  <DataSets>
    <DataSet Name=”IrrelevantToThisExample”>
      <Query>
        <DataSourceName>DataTableName</DataSourceName>
        <CommandText>SELECT * FROM sys.Tables</CommandText>
      </Query>
    </DataSet>
  </DataSets>
</Report>
```
On my first attempt I used XPath to extract the query from the XML (inside of the RDLC file). It worked for simple queries. However, I realized that things got messy if the query had any parameters (or for Stored Procedures, etc.).

On my second attempt I took a different approach. I realized that the code would be much easier if I deserialized the XML into a stack of objects. It sounds complicated and scary, but once you see it, you will realize how incredibly simple XML Serialization/Deserialization can be.

The (simplified) classes that match this XML look like this:

```
[Serializable(), System.Xml.Serialization.XmlRoot("Report")]
public class Report : SerializableBase
{
   public List<DataSet> DataSets = new List<DataSet>();
}

public class DataSet
{
   [System.Xml.Serialization.XmlAttribute]
   public string Name;
   public Query Query = new Query();
}

public class Query
{
   public string DataSourceName;
   public string CommandText;
}
```
Once you deserialize the XML, you can easily extract the query like this:

```
Report report = Report.Deserialize(xml, typeof(RDL.Report));
String commandText = report.DataSets[0].Query.CommandText;
```
The SerializableBase object is something I have reused from several projects. It makes it simple to serialize or deserialize any object to XML or vice-versa. Here is the code:

```
[Serializable]
public class SerializableBase
{
   public static SerializableBase Deserialize(String xml, Type type)
   {
      //... some code omitted for brevity. See downloads.
      System.Xml.Serialization.XmlSerializer ser = 
         new System.Xml.Serialization.XmlSerializer(type);
      using (System.IO.StringReadersr = new System.IO.StringReader(xml))
      {
         return (SerializableBase)ser.Deserialize(sr);
      }
   }
}  
```
## Set up any parameters
As I mentioned earlier, the code was very simple until I dealt with parameterized queries and stored procedures. I had to add several more classes for deserialization. For brevity, I will include them in the downloaded code but save you the headache of reading the code here. Don’t worry. They are very simple (boring) classes that match the structure of the XML, just like the serialization classes above.

# Refactored
The rest of this code, started out in utility classes. After looking at them, I realized that it would be more OO pure if I encapsulated the utility code within the serialization classes as methods rather than as external helper utility functions. It makes the serialization classes seem more complicated. That is why, for this article, I started by describing the original classes (above) in their simplest form.

## Report parameters/Query parameters
Unfortunately, in the RDLC files, the query block defines its parameters, but doesn’t define types for them. The DB will choke on types that don’t cast easily, such as: DateTime, Numeric, and Integer. Luckily, the parameter types are defined in a separate part of the RDLC’s XML. I just need to copy those into the query parameter definitions. Unfortunately, it makes the code seem a little hackish, but it does get the job done reliably.

```
//Report.cs
private void ResolveParameterTypes()
{
   //for each report parameter, find the matching query parameter and copy-in the data type
   foreach (ReportParameter rParam in this.ReportParameters)
   {
      foreach (DataSet ds in this.DataSets)
      foreach (QueryParameter qParam in ds.Query.QueryParameters)
      {
         if (qParam.Value == "=Parameters!" + rParam.Name + ".Value")
         {
            qParam.DataType = rParam.DataType;
         }
      }
   }
}
// override the constructor so the report param types are always resolved to the query params
//as a bonus, now you don't have to cast it after deserializing it
public static Report Deserialize(string xml, Type type)
{
   Report re;
   re = (Report)SerializableBase.Deserialize(xml, type);

   //copy the type-names from the ReportParameters to the QueryParameters
   re.ResolveParameterTypes();

   return re;
}
```

## URL Parameters
Now, I copy the parameters from the (URL) QueryString, into the report’s params. Naturally, I am making some big assumptions about the QueryString parameter names matching those of the report. If they don’t match, there will be an error but it should be simple to figure-out what went wrong. I could also add some diagnostics to detect which parameters didn’t get values assigned to them (maybe later).

```
//View.aspx.cs
private System.Collections.Hashtable ReportParameters
{
    get
    {
        System.Collections.Hashtable re = new System.Collections.Hashtable();
        //gather any params so they can be passed to the report
        foreach (string key in Request.QueryString.AllKeys)
        {
            if (key.ToLower() != "path")
            //ignore the “path” param. It describes the report’s file path
            {
                re.Add(key, Request.QueryString[key]);
            }
        }
        return re;
    }
}

//DataSet.cs
public void AssignParameters(System.Collections.HashtablewebParameters)
{
    foreach (RDL.QueryParameter param in this.Query.QueryParameters)
    {
        string paramName = param.Name.Replace("@", "");
        //if this report param was passed as an arg to the report, then populate it
        if (webParameters[paramName] != null)
           param.Value = webParameters[paramName].ToString();
    }
}
```

### Run the query and fill the DataTable
This is pretty 101. Set up the command object, add parameters, then just use a DataAdapter to populate a table.

```
//DataSet.cs
public System.Data.DataTable GetDataTable(string DBConnectionString)
{
  System.Data.DataTable re = new System.Data.DataTable();
  using (System.Data.OleDb.OleDbDataAdapter da = 
     new System.Data.OleDb.OleDbDataAdapter(this.Query.CommandText, DBConnectionString))
  {
     if (this.Query.QueryParameters.Count > 0)
     {
        foreach (RDL.QueryParameter param in this.Query.QueryParameters)
        {
           string paramName = param.Name.Replace("@", "");
           //OLEDB chokes on the @symbol, it prefers ? marks
           using (System.Data.OleDb.OleDbCommand cmd = da.SelectCommand)
               cmd.CommandText = cmd.CommandText.Replace(param.Name, "?");

           using (System.Data.OleDb.OleDbParameterCollection params = da.SelectCommand.Parameters)
               switch (param.DataType)
               {
                   case "Text":
                       params.Add(new OleDbParameter(paramName, OleDbType.VarWChar) 
                       { Value = param.Value });
                            break;
                   case "Boolean":
                        params.Add(new OleDbParameter(paramName, OleDbType.Boolean) 
                        { Value = param.Value });
                            break;
                   case "DateTime":
                        params.Add(new OleDbParameter(paramName, OleDbType.Date) 
                        { Value = param.Value });
                            break;
                   case "Integer":
                        params.Add(new OleDbParameter(paramName, OleDbType.Integer) 
                        { Value = param.Value });
                            break;
                   case "Float":
                        params.Add(new OleDbParameter(paramName, OleDbType.Decimal) 
                        { Value = param.Value });
                            break;
                   default:
                        params.Add(new OleDbParameter(paramName, param.Value));
                            break;
                   }
           }
      }
      da.fill(re);
      re.TableName = this.Name;
      return re;
}
```

# Follow-up (refactor)
I did refactor this code (you can see in source code) which made it a little messy. I wanted to make it flexible, so I could use it in several projects. Since I couldn’t be sure the db connection string will always be an OLEDB or SqlClient connection, I checked the connection string and used the appropriate set of libraries (OLEDB/SQLClient) for either one. The code became twice as long, but more portable.

# Conclusion
That is all it takes to extract the query from an RDLC file and run it in ASP.NET.

SSRS was originally written by Microsoft as an example of how to use these technologies to accomplish what I have shown here. Of course, SSRS has many features that go way beyond what I have shown, but if you don’t need all of those rich features, this code should be quick and portable for you and your .NET projects.

# Advanced Options
This article has covered a very simple way of running simple RDLC files. However, you may have some more complicated reports. Here are a few resources that you can continue reading about to cover more advanced topics that build on this concept:

- Embedded reports (sub-reports) - http://www.codeproject.com/Articles/473844/Using-Custom-Data-Source-to-create-RDLC-Reports
- Generate a PDF without using the Report Viewer control - http://www.codeproject.com/Articles/492739/Exporting-to-Word-PDF-using-Microsoft-Report-RDLC
- Report Parts (use the same strategy as sub-reports)
- Embedded graphics (just add more serializable objects to grab these settings)
