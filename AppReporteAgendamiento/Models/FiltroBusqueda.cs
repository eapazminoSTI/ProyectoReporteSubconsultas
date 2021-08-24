using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Models
{
    public class FiltroBusqueda
    {

        [Display(Name = "Filtrar por Fecha: ")]
        public DateTime fechaDesde { get; set; }
        public DateTime fechaHasta { get; set; }
        public string flujoLlamada { get; set; }
        public string opcionFlujo { get; set; }

    }
}