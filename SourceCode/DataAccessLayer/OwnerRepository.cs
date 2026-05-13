using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class OwnerRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to an Owner object.</summary>
        private List<Owner> ReadOwners(string sql, SqlParameter[] parameters = null)
        {
            List<Owner> owners = new List<Owner>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    owners.Add(new Owner
                    {
                        OwnerId = Convert.ToInt32(reader["OWNERID"]),
                        FirstName = reader["OFRISTNAME"].ToString().Trim(),
                        LastName = reader["OLASTNAME"].ToString().Trim(),
                        Phone = reader["OPHONE"] == DBNull.Value ? null : reader["OPHONE"].ToString().Trim(),
                        Email = reader["OEMAIL"] == DBNull.Value ? null : reader["OEMAIL"].ToString().Trim(),
                        BillingAddress = reader["BILLINGADDRESS"] == DBNull.Value ? null : reader["BILLINGADDRESS"].ToString().Trim(),
                        EmergencyContact = reader["EMERGENCYCONTACT"] == DBNull.Value ? null : reader["EMERGENCYCONTACT"].ToString().Trim()
                    });
                }
            }

            return owners;
        }

        /// <summary>Inserts a new owner. Returns the number of affected rows.</summary>
        public int InsertOwner(Owner owner)
        {
            string sql = @"INSERT INTO OWNERS (OFRISTNAME, OLASTNAME, OPHONE, OEMAIL, BILLINGADDRESS, EMERGENCYCONTACT) 
                         VALUES (@FirstName, @LastName, @Phone, @Email, @BillingAddress, @EmergencyContact)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FirstName", owner.FirstName),
                new SqlParameter("@LastName", owner.LastName),
                new SqlParameter("@Phone", (object)owner.Phone ?? DBNull.Value),
                new SqlParameter("@Email", (object)owner.Email ?? DBNull.Value),
                new SqlParameter("@BillingAddress", (object)owner.BillingAddress ?? DBNull.Value),
                new SqlParameter("@EmergencyContact", (object)owner.EmergencyContact ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Returns every owner in the database.</summary>
        public List<Owner> GetAllOwners()
        {
            string sql = "SELECT * FROM OWNERS";
            return ReadOwners(sql);
        }

        /// <summary>Returns the owner with the given ID, or null if not found.</summary>
        public Owner GetOwnerById(int ownerId)
        {
            string sql = "SELECT * FROM OWNERS WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            List<Owner> owners = ReadOwners(sql, parameters);
            return owners.Count > 0 ? owners[0] : null;
        }

        /// <summary>Returns owners whose last name contains the search term (case-insensitive).</summary>
        public List<Owner> SearchOwnersByLastName(string searchTerm)
        {
            string sql = "SELECT * FROM OWNERS WHERE OLASTNAME LIKE @SearchTerm";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SearchTerm", "%" + searchTerm + "%")
            };
            return ReadOwners(sql, parameters);
        }

        /// <summary>Updates all fields for the owner identified by OwnerId. Returns affected rows.</summary>
        public int UpdateOwner(Owner owner)
        {
            string sql = @"UPDATE OWNERS SET OFRISTNAME = @FirstName, OLASTNAME = @LastName, OPHONE = @Phone, 
                         OEMAIL = @Email, BILLINGADDRESS = @BillingAddress, EMERGENCYCONTACT = @EmergencyContact 
                         WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FirstName", owner.FirstName),
                new SqlParameter("@LastName", owner.LastName),
                new SqlParameter("@Phone", (object)owner.Phone ?? DBNull.Value),
                new SqlParameter("@Email", (object)owner.Email ?? DBNull.Value),
                new SqlParameter("@BillingAddress", (object)owner.BillingAddress ?? DBNull.Value),
                new SqlParameter("@EmergencyContact", (object)owner.EmergencyContact ?? DBNull.Value),
                new SqlParameter("@OwnerId", owner.OwnerId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);

        }

        /// <summary>Deletes the owner with the given ID. Returns affected rows.</summary>
        public int DeleteOwner(int ownerId)
        {
            string sql = "DELETE FROM OWNERS WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Returns true if an owner with the given ID exists.</summary>
        public bool OwnerExists(int ownerId)
        {
            string sql = "SELECT COUNT(*) FROM OWNERS WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of owners in the database.</summary>
        public int GetOwnerCount()
        {
            string sql = "SELECT COUNT(*) FROM OWNERS";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }
    }
}
