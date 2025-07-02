using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using System.Reflection.Emit;
using System.Data;
using System.Drawing;
using System.Xml.Linq;
using System.Configuration;
using System.Web.DynamicData;
using ClosedXML.Excel;

namespace PaymentGetway
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        static string conStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString.ToString();
        SqlConnection con = new SqlConnection(conStr);
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        void SaveExcelData(List<string> data) 
        {
            SqlParameter[] sqlParameter = new SqlParameter[11]
             {
                new SqlParameter("@BUS_NO",data[0]),
                new SqlParameter("@BUS_NAME",data[1]),
                new SqlParameter("@Via",data[2]),
                new SqlParameter("@Timing",data[3]),
                new SqlParameter("@BUS_NUMBER",data[4]),
                new SqlParameter("@CITY",data[5]),
                new SqlParameter("@PICKUP_POINT",data[6]),
                new SqlParameter("@DROP_POINT",data[7]),
                new SqlParameter("@Drop_Timing",data[8]),
                new SqlParameter("@Seat_TICKET",data[9]),
                new SqlParameter("@Sleeper_TICKET",data[10]),
             };

            SqlCommand cmd = new SqlCommand("sp_Insert", con);
            cmd.CommandType = CommandType.StoredProcedure;
            foreach (var p in sqlParameter)
            {
                cmd.Parameters.Add(p);
            }
            con.Open();
            cmd.ExecuteNonQuery();
            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = "Data Added Successfully";
            con.Close();
            
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string conStringExcel = string.Empty;
                string path = Path.GetFileName(UploadBus.FileName);
                path = path.Replace(" ", "");
                UploadBus.SaveAs(Server.MapPath("~/ExcelFile/") + path);
                string ExcelPath = Server.MapPath("~/ExcelFile/") + path;
                string extension = Path.GetExtension(ExcelPath);

                switch (extension.ToLower())
                {
                    case ".xls":
                        conStringExcel = ConfigurationManager.ConnectionStrings["Excel03conStringExcel"].ConnectionString;
                        break;
                    case ".xlsx":
                        conStringExcel = ConfigurationManager.ConnectionStrings["Excel07+conStringExcel"].ConnectionString;
                        break;
                }
                conStringExcel = string.Format(conStringExcel, ExcelPath);   
                OleDbConnection mycon = new OleDbConnection(conStringExcel);
                mycon.Open(); 
                DataTable dtSheet = mycon.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null); 
                OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + dtSheet.Rows[0]["TABLE_NAME"] + "]", mycon); 
                OleDbDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    List<string> data = new List<string>
                    {
                        {dr[0].ToString()},
                        {dr[1].ToString()},
                        {dr[2].ToString()},
                        {dr[3].ToString()},
                        {dr[4].ToString()},
                        {dr[5].ToString()},
                        {dr[6].ToString()},
                        {dr[7].ToString()},
                        {dr[8].ToString()},
                        {dr[9].ToString()},
                        {dr[10].ToString()},
                    };  
                    SaveExcelData(data);
                }
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = ex.Message;
            }
        }

        protected void btn_Download_Click(object sender, EventArgs e)
        {
            SqlDataAdapter sda = new SqlDataAdapter("sp_View_Data", con);
            DataTable dataTable = new DataTable();
            sda.Fill(dataTable);

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable, "employee");

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=SqlFileExport.xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }
    }
}
