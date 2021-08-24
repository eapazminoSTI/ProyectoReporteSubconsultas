using AppReporteAgendamiento.Data;
using AppReporteAgendamiento.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppReporteAgendamiento.Controllers
{
    public class EncuestaController : Controller
    {
        public string ServerProd = ConfigurationManager.ConnectionStrings["DB_ETAFASION"].ConnectionString;
        static FiltroBusqueda fechaReport = new FiltroBusqueda();
        List<Encuesta> encuestas = new List<Encuesta>();
        static List<Encuesta> encuestaReporte;
        // GET: Encuesta
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetList(FiltroBusqueda fecha = null)
        {
            return ObtenerTransacciones(fecha);
        }

        public ActionResult ObtenerTransacciones(FiltroBusqueda fecha = null)
        {

            fechaReport = new FiltroBusqueda();
            string dateDesde = fecha.fechaDesde.ToString("yyyyMMdd", new CultureInfo("en-US", true));
            string dateHasta = fecha.fechaHasta.ToString("yyyyMMdd", new CultureInfo("en-US", true));
            fechaReport = fecha;
            DbContext db = new DbContext();
            Gestion ges = new Gestion();

            try
            {
                if (dateDesde != "00010101")
                {
                    using (SqlConnection connection = new SqlConnection(ServerProd))
                    {
                        string Consulta1 = "SELECT * " +
                           "FROM TBL_ENCUESTA  WHERE (FECHA>=@desde AND  [FECHA] < dateadd(day, 1, @hasta) )  ORDER BY FECHA DESC";
                        using (SqlCommand cmd = new SqlCommand(Consulta1))
                        {
                            cmd.Connection = connection;
                            connection.Open();
                            cmd.Parameters.AddWithValue("desde", dateDesde);
                            cmd.Parameters.AddWithValue("hasta", dateHasta);

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {

                                string fechaString;
                              
                                while (sdr.Read())
                                {
                                    if (String.IsNullOrEmpty(sdr[3].ToString()))
                                    {
                                        fechaString = "2018-12-31 00:00:00.000";
                                    }
                                    else
                                    {
                                        fechaString = sdr[3].ToString();
                                    }


                                    try
                                    {

                                        ges=db.ObtenerGestion(sdr[2].ToString());

                                        encuestas.Add(new Encuesta
                                        {
                                            conversationId = sdr[2].ToString(),
                                            pregunta1 = sdr[1].ToString(),
                                            pregunta2 = sdr[4].ToString(),                       
                                            date = Convert.ToDateTime(fechaString),
                                            cedula=ges.cedula,
                                            nombreCliente=ges.nombreCliente,
                                            nombreAgente=ges.nombreAgente

                                        });

                                    }
                                    catch (Exception e)
                                    {
                                        ViewBag.Error = "Error: " + e.Message + e.Source;
                                        System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                                    }
                                }
                            }
                            connection.Dispose();
                            connection.Close();
                        }
                        encuestaReporte = new List<Encuesta>();
                        encuestaReporte.Clear();
                        encuestaReporte = encuestas;
                        return new JsonResult() { Data = encuestas, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(ServerProd))
                    {
                        string Consulta1 = "SELECT * " +
                           "FROM TBL_ENCUESTA WHERE  [FECHA] >= GETDATE() - 2   ORDER BY FECHA DESC";
                        using (SqlCommand cmd = new SqlCommand(Consulta1))
                        {
                            cmd.Connection = connection;
                            connection.Open();


                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {

                                string fechaString;

                                while (sdr.Read())
                                {
                                    if (String.IsNullOrEmpty(sdr[3].ToString()))
                                    {
                                        fechaString = "2018-12-31 00:00:00.000";
                                    }
                                    else
                                    {
                                        fechaString = sdr[3].ToString();
                                    }


                                    try
                                    {
                                        ges = db.ObtenerGestion(sdr[2].ToString());

                                        encuestas.Add(new Encuesta
                                        {
                                            conversationId = sdr[2].ToString(),
                                            pregunta1 = sdr[1].ToString(),
                                            pregunta2 = sdr[4].ToString(),
                                            date = Convert.ToDateTime(fechaString),
                                            cedula = ges.cedula,
                                            nombreCliente = ges.nombreCliente,
                                            nombreAgente = ges.nombreAgente

                                        });

                                    }
                                    catch (Exception e)
                                    {
                                        ViewBag.Error = "Error: " + e.Message + e.Source;
                                        System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                                    }
                                }
                            }
                        }

                            connection.Dispose();
                            connection.Close();
                    }

                    encuestaReporte = new List<Encuesta>();
                    encuestaReporte.Clear();
                    encuestaReporte = encuestas;

                    return new JsonResult() { Data = encuestas, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };

                }
                

            }
            catch (Exception e)
            {
                ViewBag.Error = "Error: " + e.Message + e.Source;
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                return new JsonResult() { Data = encuestas, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
            }
        }


        public void ReporteEncuesta()
        {

            ExcelPackage pck = new ExcelPackage();
            //ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report Trancciones");
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Reporte Encuesta ETAFASHION-RM");
            var date = new DateTime(0001, 01, 01, 0, 0, 0);

            System.Diagnostics.Debug.WriteLine(date);

            ws.Cells["A1"].Value = "Report";
            ws.Cells["B1"].Value = "Reporte Encuesta";
            ws.Cells["A2"].Value = "Fecha";
            ws.Cells["B2"].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);
            ws.Cells["A1:A4"].Style.Font.Bold = true;


            if (fechaReport.fechaDesde == date)
            {
                System.Diagnostics.Debug.WriteLine(date);
                ws.Cells["A3"].Value = "Filtro Búsqueda Fecha Desde";
                ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy}", DateTimeOffset.Now);
                ws.Cells["A4"].Value = "Filtro Búsqueda Fecha Hasta";
                ws.Cells["B4"].Value = string.Format("{0:dd MMMM yyyy}", DateTimeOffset.Now);
            }
            else
            {
                ws.Cells["A3"].Value = "Filtro Búsqueda Fecha Desde";
                ws.Cells["B3"].Value = string.Format("{0:dd MMMM yyyy}", fechaReport.fechaDesde);
                ws.Cells["A4"].Value = "Filtro Búsqueda Fecha Hasta";
                ws.Cells["B4"].Value = string.Format("{0:dd MMMM yyyy}", fechaReport.fechaHasta);
            }


            ws.Cells["A6"].Value = "INTERACCION";
            ws.Cells["B6"].Value = "CÉDULA";
            ws.Cells["C6"].Value = "CLIENTE";
            ws.Cells["D6"].Value = "PREGUNTA 1";
            ws.Cells["E6"].Value = "PREGUNTA 2";
            ws.Cells["F6"].Value = "AGENTE";
            ws.Cells["G6"].Value = "FECHA";
    

            ws.Cells["A6:L6"].Style.Font.Bold = true;
            int rowStart = 7;


            foreach (var item in encuestaReporte)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.conversationId;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.cedula;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.nombreCliente;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.pregunta1;    
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.pregunta2;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.nombreAgente;
                ws.Cells[string.Format("G{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.date);
   
                rowStart++;
            }


            //ws2.Cells["M2"].Value = "FECHA CREACIÓN";
            //ws2.Cells["N2"].Value = "CONVERSATION ID";


            ws.Cells["A:AZ"].AutoFitColumns();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "ExcelReport.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();

        }

    }
}