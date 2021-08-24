using AppReporteAgendamiento.fonts;
using AppReporteAgendamiento.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AppReporteAgendamiento.Data
{
    public class DbContext
    {
        public string ServerProd = ConfigurationManager.ConnectionStrings["DB_ETAFASION"].ConnectionString;
        public Gestion ObtenerGestion(string conversationId)
        {

            Gestion ges = new Gestion();

            try
            {
                using (SqlConnection connection = new SqlConnection(ServerProd))
                {

                    string Consulta1 = "SELECT * " +
                       "FROM TBL_FORMULARIO  WHERE ID_INTERACTION=@id";
                    using (SqlCommand cmd = new SqlCommand(Consulta1))
                    {
                        cmd.Connection = connection;
                        connection.Open();
                        cmd.Parameters.AddWithValue("id", conversationId);

                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {

                            while (sdr.Read())
                            {
                            
                                ges.cedula = sdr[3].ToString();
                                ges.nombreAgente = sdr[13].ToString();
                                ges.nombreCliente = sdr[2].ToString();
                            }


                           

                        }
                        connection.Dispose();
                        connection.Close();
                       
                    }
                    return ges;

                }
            }
            catch (Exception)
            {

                
                ges.cedula = "";
                ges.nombreAgente = "";
                ges.nombreCliente = "";
                return ges;
            }
           
        }
    }
}