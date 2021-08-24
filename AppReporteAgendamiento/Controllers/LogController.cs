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
    public class LogController : Controller
    {
        public string ServerProd = ConfigurationManager.ConnectionStrings["DB_ETAFASION"].ConnectionString;
        List<LogTransaccion> logs = new List<LogTransaccion>();
        static List<LogTransaccion> logsReporte;
        List<ConteoTipifica> listconteo;
        static List<ConteoTipifica> listconteoReporte;
        List<string> tra = new List<string>();
        static FiltroBusqueda fechaReport = new FiltroBusqueda();

        static DashBoard das;
        // GET: Log
        public ActionResult Index()
        {
            //Select con todos los FLUJOS
            using (var context = new DB_ETAFASIONEntities())
            {
                ViewBag.NombreFlujo = context.TBL_FLUJOS.ToList();
            }

            //Select con todas las OPCIONES
            using (var context = new DB_ETAFASIONEntities())
            {
                ViewBag.NombreOpcion = context.TBL_OPCIONES.ToList();
            }

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

            if(fecha.flujoLlamada != null && fecha.opcionFlujo != null)
            {
                string flujoSelect = fecha.flujoLlamada.ToString();
                string opcionSelect = fecha.opcionFlujo.ToString();

                try
                {
                    if (dateDesde != "00010101")
                    {
                        using (SqlConnection connection = new SqlConnection(ServerProd))
                        {
                            string Consulta1 = "SELECT CONVERSATIONID,IDENTIFICACIONIVR,TRANSACCIONESIVR,FECHAIVR,IDIVRV,FLUJO " +
                              "FROM TBL_TRANSACCION_IVR WHERE (FECHAIVR>=@desde AND  [FECHAIVR] < dateadd(day, 1, @hasta) AND FLUJO=@flujo AND TRANSACCIONESIVR like + '%' + @opcion + '%' )  ORDER BY FECHAIVR DESC ";
                            using (SqlCommand cmd = new SqlCommand(Consulta1))
                            {
                                cmd.Connection = connection;
                                connection.Open();
                                cmd.Parameters.AddWithValue("desde", dateDesde);
                                cmd.Parameters.AddWithValue("hasta", dateHasta);
                                cmd.Parameters.AddWithValue("flujo", flujoSelect);
                                cmd.Parameters.AddWithValue("opcion", opcionSelect);

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


                                        var cont = 0;
                                        string[] transacciones = sdr[2].ToString().Split(',');
                                        

                                        for (int i = 0; i < transacciones.Length; i++)
                                        {
                                            if(transacciones[i] == opcionSelect)
                                            {
                                                cont++;
                                            }
                                        }

                                        string[] transaccionSeleccionada = new string[cont];

                                        for (int i = 0; i < cont; i++)
                                        {
                                            transaccionSeleccionada[i] = opcionSelect;
                                        }

                                        try
                                        {
                                            logs.Add(new LogTransaccion
                                            {
                                                id = Convert.ToInt32(sdr[4].ToString()),
                                                ConversationID = sdr[0].ToString(),
                                                Cedula = sdr[1].ToString(),
                                                Flujo = sdr[5].ToString(),
                                                Transaccion = sdr[2].ToString(),
                                                Transacciones = transaccionSeleccionada,
                                                fecha = Convert.ToDateTime(fechaString),
                                                Consultas = cont
                                            }); ;

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
                            logsReporte = new List<LogTransaccion>();
                            logsReporte.Clear();
                            logsReporte = logs;
                            return new JsonResult() { Data = logs, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
                        }
                    }
                    else
                    {
                        using (SqlConnection connection = new SqlConnection(ServerProd))
                        {
                            string Consulta1 = "SELECT CONVERSATIONID, IDENTIFICACIONIVR,TRANSACCIONESIVR,FECHAIVR,IDIVRV,FLUJO  " +
                               "FROM TBL_TRANSACCION_IVR WHERE  [FECHAIVR] >= GETDATE() - 1   ORDER BY FECHAIVR DESC";
                            using (SqlCommand cmd = new SqlCommand(Consulta1))
                            {
                                cmd.Connection = connection;
                                connection.Open();

                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    string fechaString;
                                    while (sdr.Read())
                                    {
                                        if (String.IsNullOrEmpty(sdr[4].ToString()))
                                        {
                                            fechaString = "2018-12-31 00:00:00.000";
                                        }
                                        else
                                        {
                                            fechaString = sdr[4].ToString();
                                        }

                                        var cont = 0;
                                        string[] transacciones = sdr[2].ToString().Split(',');

                                        cont = (sdr[2].ToString().Split(',').Length);

                                        try
                                        {
                                            logs.Add(new LogTransaccion
                                            {
                                                id = Convert.ToInt32(sdr[4].ToString()),
                                                ConversationID = sdr[0].ToString(),
                                                Cedula = sdr[1].ToString(),
                                                Flujo = sdr[5].ToString(),
                                                Transaccion = sdr[2].ToString(),
                                                Transacciones = sdr[2].ToString().Split(','),
                                                fecha = Convert.ToDateTime(fechaString),
                                                Consultas = cont
                                            }); ;


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
            else
            {
                try
                {
                    if (dateDesde != "00010101")
                    {
                        string flujoSelect = fecha.flujoLlamada.ToString();

                        using (SqlConnection connection = new SqlConnection(ServerProd))
                        {
                            string Consulta1 = "SELECT CONVERSATIONID,IDENTIFICACIONIVR,TRANSACCIONESIVR,FECHAIVR,IDIVRV,FLUJO " +
                              "FROM TBL_TRANSACCION_IVR WHERE (FECHAIVR>=@desde AND  [FECHAIVR] < dateadd(day, 1, @hasta) AND FLUJO=@flujo)  ORDER BY FECHAIVR DESC ";

                            using (SqlCommand cmd = new SqlCommand(Consulta1))
                            {
                                cmd.Connection = connection;
                                connection.Open();
                                cmd.Parameters.AddWithValue("desde", dateDesde);
                                cmd.Parameters.AddWithValue("hasta", dateHasta);
                                cmd.Parameters.AddWithValue("flujo", flujoSelect);

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

                                        var cont = 0;
                                        string[] transacciones = sdr[2].ToString().Split(',');

                                        cont = (sdr[2].ToString().Split(',').Length);

                                        try
                                        {
                                            logs.Add(new LogTransaccion
                                            {
                                                id = Convert.ToInt32(sdr[4].ToString()),
                                                ConversationID = sdr[0].ToString(),
                                                Cedula = sdr[1].ToString(),
                                                Flujo = sdr[5].ToString(),
                                                Transaccion = sdr[2].ToString(),
                                                Transacciones = sdr[2].ToString().Split(','),
                                                fecha = Convert.ToDateTime(fechaString),
                                                Consultas = cont
                                            }); ;
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
                            logsReporte = new List<LogTransaccion>();
                            logsReporte.Clear();
                            logsReporte = logs;
                            return new JsonResult() { Data = logs, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };
                        }
                    }
                    else
                    {
                        using (SqlConnection connection = new SqlConnection(ServerProd))
                        {
                            string Consulta1 = "SELECT CONVERSATIONID, IDENTIFICACIONIVR,TRANSACCIONESIVR,FECHAIVR,IDIVRV,FLUJO  " +
                               "FROM TBL_TRANSACCION_IVR WHERE  [FECHAIVR] >= GETDATE() - 1   ORDER BY FECHAIVR DESC";
                            using (SqlCommand cmd = new SqlCommand(Consulta1))
                            {
                                cmd.Connection = connection;
                                connection.Open();

                                using (SqlDataReader sdr = cmd.ExecuteReader())
                                {
                                    string fechaString;
                                    while (sdr.Read())
                                    {
                                        if (String.IsNullOrEmpty(sdr[4].ToString()))
                                        {
                                            fechaString = "2018-12-31 00:00:00.000";
                                        }
                                        else
                                        {
                                            fechaString = sdr[4].ToString();
                                        }

                                        var cont = 0;
                                        string[] transacciones = sdr[2].ToString().Split(',');

                                        cont = (sdr[2].ToString().Split(',').Length);

                                        try
                                        {
                                            logs.Add(new LogTransaccion
                                            {
                                                id = Convert.ToInt32(sdr[4].ToString()),
                                                ConversationID = sdr[0].ToString(),
                                                Cedula = sdr[1].ToString(),
                                                Flujo = sdr[5].ToString(),
                                                Transaccion = sdr[2].ToString(),
                                                Transacciones = sdr[2].ToString().Split(','),
                                                fecha = Convert.ToDateTime(fechaString),
                                                Consultas = cont
                                            }); ;


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
        }

        public ActionResult GetTransacciones(int id, string Consultas)
        {
            string[] transac = null;

            using (SqlConnection connection = new SqlConnection(ServerProd))
            {
                string Consulta1 = "SELECT TRANSACCIONESIVR " +
                    "FROM TBL_TRANSACCION_IVR WHERE IDIVRV=@id";
                using (SqlCommand cmd = new SqlCommand(Consulta1))
                {
                    cmd.Connection = connection;
                    connection.Open();
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            transac = sdr[0].ToString().Split(',');
                        }

                        foreach (string element in transac)
                        {
                            if(Consultas == "")
                            {
                                logs.Add(new LogTransaccion
                                {
                                    Transaccion = element
                                });
                            }

                            if(element.Equals(Consultas))
                            {
                                logs.Add(new LogTransaccion
                                {
                                    Transaccion = element
                                });
                            }

                        }
                    }
                    connection.Dispose();
                    connection.Close();

                }
                var json = Json(new { data = logs }, JsonRequestBehavior.AllowGet);
                return json;
            }
        }

        public ActionResult ViewDataOption(FiltroBusqueda fecha = null)
        {
            return Json(OpcionCount(fecha), JsonRequestBehavior.AllowGet);
        }

        public DashBoard OpcionCount(FiltroBusqueda fecha = null)
        {
            ObtenerTransacciones(fecha);
            listconteo = new List<ConteoTipifica>();

            int IVR_VP = 0;
            int IVR_CP = 0;
            int IVR_CD = 0;
            int IVR_CPTC = 0;
            int IVR_ESCD = 0;
            int IVR_IS = 0;
            int IVR_CW = 0;
            int IVR_EDS = 0;
            int IVR_ACD = 0;
            int IVR_CCC = 0;
            int IVR_ES = 0;

            double promedio = 0;
            double suma = 0;
            double totalPersonas = 0;

            List<IGrouping<string, LogTransaccion>> result = logs.GroupBy(x => x.Cedula).ToList();
            totalPersonas = result.Count();

            foreach (LogTransaccion item in logs)
            {
                suma = suma + item.Consultas;

                foreach (string element in item.Transacciones)
                {
                    switch (element)
                    {
                        case "VALOR A PAGAR":
                            IVR_VP++;
                            break;
                        case "CANALES DE PAGO":
                            IVR_CP++;
                            break;
                        case "CUPO DISPONIBLE":
                            IVR_CD++;
                            break;
                        case "COMPLICADO CON TU CUOTA":
                            IVR_CPTC++;
                            break;
                        case "ESTADO DE SOLICITUD CREDITO DIRECTO":
                            IVR_ESCD++;
                            break;
                        case "INFORMACION DE SERVICIOS":
                            IVR_IS++;
                            break;
                        case "COMPRAS WEB":
                            IVR_CW++;
                            break;
                        case "EJECUTIVO DE SERVICIOS":
                            IVR_EDS++;
                            break;
                        case "APLICAR CREDITO DIRECTO":
                            IVR_ACD++;
                            break;
                        case "COMUNICACION CON OFICINAS":
                            IVR_CCC++;
                            break;
                        case "EJECUTIVO SAC":
                            IVR_ES++;
                            break;

                    }
                }
            }

            double total = IVR_VP + IVR_CP + IVR_CD + IVR_CPTC + IVR_ESCD + IVR_IS + IVR_CW + IVR_EDS + IVR_ACD + IVR_CCC + IVR_ES;
            DashBoard d = new DashBoard();
            if (total != 0)
            {
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "VALOR A PAGAR",
                    numero = IVR_VP,
                    porcentaje = IVR_VP * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "CANALES DE PAGO",
                    numero = IVR_CP,
                    porcentaje = IVR_CP * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "CUPO DISPONIBLE",
                    numero = IVR_CD,
                    porcentaje = IVR_CD * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "COMPLICADO CON TU CUOTA",
                    numero = IVR_CPTC,
                    porcentaje = IVR_CPTC * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "ESTADO DE SOLICITUD CREDITO DIRECTO",
                    numero = IVR_ESCD,
                    porcentaje = IVR_ESCD * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "INFORMACION DE SERVICIOS",
                    numero = IVR_IS,
                    porcentaje = IVR_IS * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "COMPRAS WEB",
                    numero = IVR_CW,
                    porcentaje = IVR_CW * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "EJECUTIVO DE SERVICIOS",
                    numero = IVR_EDS,
                    porcentaje = IVR_EDS * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "APLICAR CREDITO DIRECTO",
                    numero = IVR_ACD,
                    porcentaje = IVR_ACD * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "COMUNICACION CON OFICINAS",
                    numero = IVR_CCC,
                    porcentaje = IVR_CCC * 100 / (total)
                });
                listconteo.Add(new ConteoTipifica
                {
                    nombre = "EJECUTIVO SAC",
                    numero = IVR_ES,
                    porcentaje = IVR_ES * 100 / (total)
                });

                promedio = total / Convert.ToDouble(logs.Count);
                listconteo = listconteo.OrderByDescending(o => o.numero).ToList();
                listconteoReporte = new List<ConteoTipifica>();
                listconteoReporte.Clear();
                listconteoReporte = listconteo;

                d.listconteo = listconteo;
                d.promedio = Math.Round(promedio, 2);
                d.totalConsultas = total;
                d.totalPerson = totalPersonas;
                d.promedioxPerson = Math.Round((total / totalPersonas), 2);
                d.totalInteraccion = logs.Count();
            }
            else
            {
                d.listconteo = listconteo;
                d.promedio = 0;
                d.totalConsultas = 0;
                d.totalPerson = 0;
                d.promedioxPerson = 0;
                d.totalInteraccion = 0;
            }

            das = new DashBoard();
            das.listconteo = new List<ConteoTipifica>();
            das.listconteo.Clear();
            das = d;
            return d;
        }

        public void ReporteTransacciones()
        {
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");
            var date = new DateTime(0001, 01, 01, 0, 0, 0);

            System.Diagnostics.Debug.WriteLine(date);

            ws.Cells["A1"].Value = "Report";
            ws.Cells["B1"].Value = "Reporte Transacciones";
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

            ws.Cells["A6"].Value = "OPCION IVR";
            ws.Cells["B6"].Value = "TOTAL";
            ws.Cells["C6"].Value = "PORCENTAJE";

            ws.Cells["D7"].Value = "# Total de Consultas";
            ws.Cells["E7"].Value = das.totalConsultas;
            ws.Cells["D8"].Value = "# Total de Clientes";
            ws.Cells["E8"].Value = das.totalPerson;
            ws.Cells["D9"].Value = "# Total de Interacciones";
            ws.Cells["E9"].Value = das.totalInteraccion;
            ws.Cells["D10"].Value = "# Consultas x Interacción";
            ws.Cells["E10"].Value = das.promedio;
            ws.Cells["D11"].Value = "# Consultas x Cliente";
            ws.Cells["E11"].Value = das.promedioxPerson;

            int row = 7;
            if (listconteoReporte != null)
            {
                foreach (var item in listconteoReporte)
                {

                    ws.Cells[string.Format("A{0}", row)].Value = item.nombre;
                    ws.Cells[string.Format("B{0}", row)].Value = item.numero;
                    ws.Cells[string.Format("C{0}", row)].Value = item.porcentaje;
                    row++;
                }
            }

            ws.Cells["A26"].Value = "CEDULA";
            ws.Cells["B26"].Value = "CONVERSATION ID";
            ws.Cells["C26"].Value = "FLUJO";
            ws.Cells["D26"].Value = "FECHA";
            ws.Cells["E26"].Value = "# CONSULTAS";
            ws.Cells["F26"].Value = "TRANSACCIONES";

            ws.Cells["A6:G6"].Style.Font.Bold = true;
            ws.Cells["A26:G26"].Style.Font.Bold = true;
            ws.Cells["D7:D11"].Style.Font.Bold = true;
            int rowStart = 27;
            if (logsReporte != null)
            {
                foreach (var item in logsReporte)
                {

                    ws.Cells[string.Format("A{0}", rowStart)].Value = item.Cedula;
                    ws.Cells[string.Format("B{0}", rowStart)].Value = item.ConversationID;
                    ws.Cells[string.Format("C{0}", rowStart)].Value = item.Flujo;
                    ws.Cells[string.Format("D{0}", rowStart)].Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", item.fecha);
                    ws.Cells[string.Format("E{0}", rowStart)].Value = item.Consultas;
                    ws.Cells[string.Format("F{0}", rowStart)].Value = item.Transaccion;

                    rowStart++;
                }
            }

            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "ExcelReport.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();
        }
    }
}
