using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Models
{
    public class LogTransaccion
    {
        public int id { get; set; }
        public string ConversationID { get; set; }
        public string Transaccion { get; set; }
        public string[] Transacciones { get; set; }
        public string Cedula { get; set; }
        public string Flujo { get; set; }
        public int Consultas { get; set; }
        public DateTime fecha { get; set; }
    }
}