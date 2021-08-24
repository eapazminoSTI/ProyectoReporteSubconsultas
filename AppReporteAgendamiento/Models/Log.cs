using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Models
{
    public class Log
    {
        public long formulario { get; set; }
        public string id { get; set; }
        public string nombre { get; set; }
        public string correo { get; set; }
        public string telefono { get; set; }
        public string Cedula { get; set; }
        public DateTime fecha { get; set; }
        public string empresa { get; set; }
        public string servicio { get; set; }
        public string categoria { get; set; }
        public string accion { get; set; }
        public string comentario { get; set; }
        public string tipo { get; set; }
        public string agente { get; set; }
        public string emisor_solicitud { get; set; }
        public string tienda { get; set; }
    }
}