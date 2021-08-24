using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Models
{
    public class DashBoard
    {
        public List<ConteoTipifica> listconteo { get; set; }
        public double promedio { get; set; }
        public double totalConsultas { get; set; }
        public double totalPerson { get; set; }
        public double promedioxPerson { get; set; }
        public double totalInteraccion { get; set; }
        public double transferidasACD { get; set; }

    }
}