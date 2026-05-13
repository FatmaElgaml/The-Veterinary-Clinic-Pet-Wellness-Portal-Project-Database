using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryClinicProject.DataAccessLayer
{
    /// Executes database commands using DBConnection
    public class DBHandler
    {
        private readonly DBConnection dbConnection = new DBConnection();

        /// For INSERT, UPDATE, DELETE (returns number of affected rows)
        public int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = dbConnection.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database operation failed: " + ex.Message, ex);
            }
        }

        /// For SELECT queries that return multiple rows
        public SqlDataReader ExcuteReader(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                SqlConnection connection = dbConnection.GetConnection();
                SqlCommand command = new SqlCommand(sql, connection);
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception("Database operation failed: " + ex.Message, ex);
            }
        }

        /// For SELECT queries that return a single value (COUNT, SUM, etc.)

        public object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = dbConnection.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database operation failed: " + ex.Message, ex);
            }
        }
    }
}
