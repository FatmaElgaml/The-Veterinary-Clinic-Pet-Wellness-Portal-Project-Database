using System;
using System.Data.SqlClient;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class DBConnection
    {
        private string GetconnectionString()
        {
            return "Data Source=(local);Initial Catalog=\"Veterinary network\";Integrated Security=True;Encrypt=True";
        }
        public SqlConnection GetConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(GetconnectionString());
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception("Database connection failed: " + ex.Message, ex);
            }
        }
    }
}
