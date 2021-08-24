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
    public class HomeController : Controller
    {
        public string ServerProd = ConfigurationManager.ConnectionStrings["DB_ETAFASION"].ConnectionString;
        static FiltroBusqueda fechaReport = new FiltroBusqueda();
        List<Log> logs = new List<Log>();
        static List<Log> logsReporte;

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
            try
            {
                if (dateDesde != "00010101")
                {
                    using (SqlConnection connection = new SqlConnection(ServerProd))
                    {
                        string Consulta1 = "SELECT * " +
                           "FROM TBL_FORMULARIO  WHERE (FECHA>=@desde AND  [FECHA] < dateadd(day, 1, @hasta) )  ORDER BY FECHA DESC";
                        using (SqlCommand cmd = new SqlCommand(Consulta1))
                        {
                            cmd.Connection = connection;
                            connection.Open();
                            cmd.Parameters.AddWithValue("desde", dateDesde);
                            cmd.Parameters.AddWithValue("hasta", dateHasta);

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                               
                                string fechaString;
                                //string agendadas = GetTotalAgendada(fecha);
                                //string incompletas = GetCallsInconclusas(fecha);
                                while (sdr.Read())
                                {
                                    if (String.IsNullOrEmpty(sdr[12].ToString()))
                                    {
                                        fechaString = "2018-12-31 00:00:00.000";
                                    }
                                    else
                                    {
                                        fechaString = sdr[12].ToString();
                                    }

                                    var cont = 0;
                                    var posicion = 0;
                                    string[] transacciones = sdr[0].ToString().Split(',');
                                  
                                    foreach (var item in transacciones)
                                    {
                                        
                                        posicion++;
                                        if (item.Equals("INGRESO IDENTIFICACION"))
                                        {
                                            break;
                                        }
                                    }

                                    cont = (sdr[0].ToString().Split(',').Length - posicion);
                                    var contador = sdr[0].ToString().Split(',').Length;
                                                               
                                    

                                    try
                                    {
                                        logs.Add(new Log
                                        {
                                            formulario=(long)sdr[0],
                                            id=sdr[1].ToString(),
                                            nombre = sdr[2].ToString(),
                                            Cedula = sdr[3].ToString(),
                                            correo = sdr[4].ToString(),
                                            telefono = sdr[5].ToString(),
                                            comentario=sdr[6].ToString(),
                                            empresa = sdr[7].ToString(),
                                            servicio = sdr[8].ToString(),
                                            categoria = sdr[9].ToString(),
                                            accion = sdr[10].ToString(),
                                            tipo = sdr[11].ToString(),
                                            fecha = Convert.ToDateTime(fechaString),
                                            agente=sdr[13].ToString(),
                                            emisor_solicitud=sdr[14].ToString(),
                                            tienda=sdr[15].ToString()

                                            //totalAgendada=agendadas,
                                            //totalInconclusas= incompletas
                                           
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
                        logsReporte = new List<Log>();
                        logsReporte.Clear();
                        logsReporte = logs;
                        return new JsonResult() { Data = logs, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(ServerProd))
                    {
                        string Consulta1 = "SELECT * " +
                           "FROM TBL_FORMULARIO WHERE  [FECHA] >= GETDATE() - 2   ORDER BY FECHA DESC";
                        using (SqlCommand cmd = new SqlCommand(Consulta1))
                        {
                            cmd.Connection = connection;
                            connection.Open();
                       

                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {

                                string fechaString;
                                //string agendadas = GetTotalAgendada(fecha);
                                //string incompletas = GetCallsInconclusas(fecha);
                                while (sdr.Read())
                                {
                                    if (String.IsNullOrEmpty(sdr[12].ToString()))
                                    {
                                        fechaString = "2018-12-31 00:00:00.000";
                                    }
                                    else
                                    {
                                        fechaString = sdr[12].ToString();
                                    }


                                    try
                                    {
                                        logs.Add(new Log
                                        {
                                            formulario = (long)sdr[0],
                                            id = sdr[1].ToString(),
                                            nombre = sdr[2].ToString(),
                                            Cedula = sdr[3].ToString(),
                                            correo = sdr[4].ToString(),
                                            telefono = sdr[5].ToString(),
                                            comentario = sdr[6].ToString(),
                                            empresa = sdr[7].ToString(),
                                            servicio = sdr[8].ToString(),
                                            categoria = sdr[9].ToString(),
                                            accion = sdr[10].ToString(),
                                            tipo = sdr[11].ToString(),
                                            fecha = Convert.ToDateTime(fechaString),
                                            agente = sdr[13].ToString(),
                                            emisor_solicitud = sdr[14].ToString(),
                                            tienda = sdr[15].ToString()
                                            //totalAgendada=agendadas,
                                            //totalInconclusas= incompletas

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

                        logsReporte = new List<Log>();
                        logsReporte.Clear();
                        logsReporte = logs;

                        return new JsonResult() { Data = logs, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };

                    }
                }

                
                    
                
            }
            catch (Exception e)
            {
                ViewBag.Error = "Error: " + e.Message + e.Source;
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                return new JsonResult() { Data = logs, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
            }
        }

        public void ReporteETAFASHION()
        {

            ExcelPackage pck = new ExcelPackage();
            //ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report Trancciones");
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Reporte ETAFASHION-RM");
            var date = new DateTime(0001, 01, 01, 0, 0, 0);

            System.Diagnostics.Debug.WriteLine(date);

            ws.Cells["A1"].Value = "Report";
            ws.Cells["B1"].Value = "Reporte Gestiones";
            ws.Cells["A2"].Value = "Date";
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
            ws.Cells["B6"].Value = "NOMBRE";
            ws.Cells["C6"].Value = "CEDULA";
            ws.Cells["D6"].Value = "CORREO";
            ws.Cells["E6"].Value = "TELÉFONO";
            ws.Cells["F6"].Value = "EMPRESA";
            ws.Cells["G6"].Value = "SERVICIO";
            ws.Cells["H6"].Value = "CATEGORIA";
            ws.Cells["I6"].Value = "ACCION";
            ws.Cells["J6"].Value = "TIPO";
            ws.Cells["K6"].Value = "FECHA CREACIÓN";
            ws.Cells["L6"].Value = "AGENTE";
            ws.Cells["M6"].Value = "EMISOR SOLICITUD";
            ws.Cells["N6"].Value = "TIENDA";

            ws.Cells["A6:L6"].Style.Font.Bold = true;
            int rowStart = 7;


            foreach (var item in logsReporte)
            {

                ws.Cells[string.Format("A{0}", rowStart)].Value = item.id;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.nombre;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.Cedula;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.correo;    //string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.fecha);
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.telefono;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.empresa;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.servicio;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.categoria;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.accion;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.tipo;
                ws.Cells[string.Format("K{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.fecha);
                ws.Cells[string.Format("L{0}", rowStart)].Value = item.agente;
                ws.Cells[string.Format("M{0}", rowStart)].Value = item.emisor_solicitud;
                ws.Cells[string.Format("N{0}", rowStart)].Value = item.tienda;

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

        public ActionResult Index()
        {
            return View();
        }

    }
}