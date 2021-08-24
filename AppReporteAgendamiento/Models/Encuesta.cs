using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Models
{
    public class Encuesta
    {
        public string conversationId { get; set; }
        public string pregunta1 { get; set; }
        public string pregunta2 { get; set; }
        public DateTime date { get; set; }

        public string cedula { get; set; }
        public string nombreCliente { get; set; }

        public string nombreAgente { get; set; }
    }
}