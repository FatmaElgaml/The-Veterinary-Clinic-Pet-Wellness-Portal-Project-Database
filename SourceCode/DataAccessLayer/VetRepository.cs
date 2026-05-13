using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class VetRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to a Veterinarian object.</summary>
        private List<Veterinarian> ReadVets(string sql, SqlParameter[] parameters = null)
        {
            List<Veterinarian> vets = new List<Veterinarian>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    vets.Add(new Veterinarian
                    {
                        VetId = Convert.ToInt32(reader["VETID"]),
                        FirstName = reader["VETFIRSTNAME"].ToString().Trim(),
                        LastName = reader["VETLASTNAME"].ToString().Trim(),
                        Specialty = reader["SPECIALTY"] == DBNull.Value ? null : reader["SPECIALTY"].ToString().Trim(),
                        LicenseNumber = reader["LICENSENUMBER"] == DBNull.Value ? null : reader["LICENSENUMBER"].ToString().Trim(),
                        Phone = reader["VETPHONE"] == DBNull.Value ? null : reader["VETPHONE"].ToString().Trim()
                    });
                }
            }

            return vets;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Veterinarian object with Clinic info.</summary>
        private List<Veterinarian> ReadVetsWithClinics(string sql, SqlParameter[] parameters = null)
        {
            List<Veterinarian> vets = new List<Veterinarian>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Veterinarian vet = new Veterinarian
                    {
                        VetId = Convert.ToInt32(reader["VETID"]),
                        FirstName = reader["VETFIRSTNAME"].ToString().Trim(),
                        LastName = reader["VETLASTNAME"].ToString().Trim(),
                        Specialty = reader["SPECIALTY"] == DBNull.Value ? null : reader["SPECIALTY"].ToString().Trim(),
                        LicenseNumber = reader["LICENSENUMBER"] == DBNull.Value ? null : reader["LICENSENUMBER"].ToString().Trim(),
                        Phone = reader["VETPHONE"] == DBNull.Value ? null : reader["VETPHONE"].ToString().Trim()
                    };

                    // Add Clinic information (from JOIN)
                    if (reader["CLINICID"] != DBNull.Value)
                    {
                        vet.ClinicId = Convert.ToInt32(reader["CLINICID"]);
                        vet.ClinicName = reader["CLINICNAME"] == DBNull.Value ? null : reader["CLINICNAME"].ToString().Trim();
                        vet.ClinicLocation = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim();
                        vet.IsPrimaryAtClinic = reader["ISPRIMARY"] != DBNull.Value && Convert.ToBoolean(reader["ISPRIMARY"]);
                        vet.JoinDate = reader["JOINDATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["JOINDATE"]);
                    }

                    vets.Add(vet);
                }
            }

            return vets;
        }

        // ============================================================
        // INSERT (CREATE)
        // ============================================================

        /// <summary>Inserts a new veterinarian. Returns the number of affected rows.</summary>
        public int InsertVet(Veterinarian vet)
        {
            string sql = @"INSERT INTO VETERINARIAN (VETFIRSTNAME, VETLASTNAME, SPECIALTY, LICENSENUMBER, VETPHONE) 
                           VALUES (@FirstName, @LastName, @Specialty, @LicenseNumber, @Phone)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FirstName", vet.FirstName),
                new SqlParameter("@LastName", vet.LastName),
                new SqlParameter("@Specialty", (object)vet.Specialty ?? DBNull.Value),
                new SqlParameter("@LicenseNumber", (object)vet.LicenseNumber ?? DBNull.Value),
                new SqlParameter("@Phone", (object)vet.Phone ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Inserts a new veterinarian and returns the new VetId (IDENTITY).</summary>
        public int InsertVetGetId(Veterinarian vet)
        {
            string sql = @"INSERT INTO VETERINARIAN (VETFIRSTNAME, VETLASTNAME, SPECIALTY, LICENSENUMBER, VETPHONE) 
                           VALUES (@FirstName, @LastName, @Specialty, @LicenseNumber, @Phone);
                           SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@FirstName", vet.FirstName),
                new SqlParameter("@LastName", vet.LastName),
                new SqlParameter("@Specialty", (object)vet.Specialty ?? DBNull.Value),
                new SqlParameter("@LicenseNumber", (object)vet.LicenseNumber ?? DBNull.Value),
                new SqlParameter("@Phone", (object)vet.Phone ?? DBNull.Value)
            };

            object result = dbHandler.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        // ============================================================
        // SELECT (READ)
        // ============================================================

        /// <summary>Returns every veterinarian in the database.</summary>
        public List<Veterinarian> GetAllVets()
        {
            string sql = "SELECT * FROM VETERINARIAN ORDER BY VETLASTNAME, VETFIRSTNAME";
            return ReadVets(sql);
        }

        /// <summary>Returns every veterinarian with their clinic information (JOIN).</summary>
        public List<Veterinarian> GetAllVetsWithClinics()
        {
            string sql = @"SELECT 
                           v.VETID,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           v.LICENSENUMBER,
                           v.VETPHONE,
                           vc.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           vc.ISPRIMARY,
                           vc.JOINDATE
                           FROM VETERINARIAN v
                           LEFT JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           LEFT JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           ORDER BY v.VETLASTNAME, v.VETFIRSTNAME";
            return ReadVetsWithClinics(sql);
        }

        /// <summary>Returns the veterinarian with the given ID, or null if not found.</summary>
        public Veterinarian GetVetById(int vetId)
        {
            string sql = "SELECT * FROM VETERINARIAN WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            List<Veterinarian> vets = ReadVets(sql, parameters);
            return vets.Count > 0 ? vets[0] : null;
        }

        /// <summary>Returns the veterinarian with clinic information by ID.</summary>
        public Veterinarian GetVetByIdWithClinics(int vetId)
        {
            string sql = @"SELECT 
                           v.VETID,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           v.LICENSENUMBER,
                           v.VETPHONE,
                           vc.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           vc.ISPRIMARY,
                           vc.JOINDATE
                           FROM VETERINARIAN v
                           LEFT JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           LEFT JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           WHERE v.VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            List<Veterinarian> vets = ReadVetsWithClinics(sql, parameters);
            return vets.Count > 0 ? vets[0] : null;
        }

        /// <summary>Returns veterinarians by specialty.</summary>
        public List<Veterinarian> GetVetsBySpecialty(string specialty)
        {
            string sql = "SELECT * FROM VETERINARIAN WHERE SPECIALTY = @Specialty ORDER BY VETLASTNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Specialty", specialty)
            };
            return ReadVets(sql, parameters);
        }

        /// <summary>Returns veterinarians whose name contains the search term (case-insensitive).</summary>
        public List<Veterinarian> SearchVetsByName(string searchTerm)
        {
            string sql = @"SELECT * FROM VETERINARIAN 
                           WHERE VETFIRSTNAME LIKE @SearchTerm 
                           OR VETLASTNAME LIKE @SearchTerm
                           ORDER BY VETLASTNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SearchTerm", "%" + searchTerm + "%")
            };
            return ReadVets(sql, parameters);
        }

        /// <summary>Returns all veterinarians working at a specific clinic.</summary>
        public List<Veterinarian> GetVetsByClinicId(int clinicId)
        {
            string sql = @"SELECT v.* 
                           FROM VETERINARIAN v
                           INNER JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           WHERE vc.CLINICID = @ClinicId
                           ORDER BY v.VETLASTNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            return ReadVets(sql, parameters);
        }

        /// <summary>Returns all veterinarians working at a specific clinic with details.</summary>
        public List<Veterinarian> GetVetsByClinicIdWithDetails(int clinicId)
        {
            string sql = @"SELECT 
                           v.VETID,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           v.LICENSENUMBER,
                           v.VETPHONE,
                           vc.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           vc.ISPRIMARY,
                           vc.JOINDATE
                           FROM VETERINARIAN v
                           INNER JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           INNER JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           WHERE vc.CLINICID = @ClinicId
                           ORDER BY v.VETLASTNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            return ReadVetsWithClinics(sql, parameters);
        }

        /// <summary>Returns the primary veterinarian for a specific clinic.</summary>
        public Veterinarian GetPrimaryVetByClinicId(int clinicId)
        {
            string sql = @"SELECT v.* 
                           FROM VETERINARIAN v
                           INNER JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           WHERE vc.CLINICID = @ClinicId AND vc.ISPRIMARY = 1";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            List<Veterinarian> vets = ReadVets(sql, parameters);
            return vets.Count > 0 ? vets[0] : null;
        }

        /// <summary>Returns all unique specialties (for dropdown menus).</summary>
        public List<string> GetAllSpecialties()
        {
            List<string> specialties = new List<string>();
            string sql = "SELECT DISTINCT SPECIALTY FROM VETERINARIAN WHERE SPECIALTY IS NOT NULL ORDER BY SPECIALTY";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    specialties.Add(reader["SPECIALTY"].ToString().Trim());
                }
            }

            return specialties;
        }

        /// <summary>Returns veterinarians with no clinic assignment.</summary>
        public List<Veterinarian> GetVetsWithoutClinic()
        {
            string sql = @"SELECT v.* 
                           FROM VETERINARIAN v
                           LEFT JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                           WHERE vc.VETID IS NULL
                           ORDER BY v.VETLASTNAME";
            return ReadVets(sql);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        /// <summary>Updates all fields for the veterinarian identified by VetId. Returns affected rows.</summary>
        public int UpdateVet(Veterinarian vet)
        {
            string sql = @"UPDATE VETERINARIAN SET 
                           VETFIRSTNAME = @FirstName, 
                           VETLASTNAME = @LastName, 
                           SPECIALTY = @Specialty, 
                           LICENSENUMBER = @LicenseNumber, 
                           VETPHONE = @Phone 
                           WHERE VETID = @VetId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vet.VetId),
                new SqlParameter("@FirstName", vet.FirstName),
                new SqlParameter("@LastName", vet.LastName),
                new SqlParameter("@Specialty", (object)vet.Specialty ?? DBNull.Value),
                new SqlParameter("@LicenseNumber", (object)vet.LicenseNumber ?? DBNull.Value),
                new SqlParameter("@Phone", (object)vet.Phone ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the veterinarian's specialty. Returns affected rows.</summary>
        public int UpdateVetSpecialty(int vetId, string newSpecialty)
        {
            string sql = "UPDATE VETERINARIAN SET SPECIALTY = @NewSpecialty WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@NewSpecialty", (object)newSpecialty ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the veterinarian's phone number. Returns affected rows.</summary>
        public int UpdateVetPhone(int vetId, string newPhone)
        {
            string sql = "UPDATE VETERINARIAN SET VETPHONE = @NewPhone WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@NewPhone", (object)newPhone ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the veterinarian's license number. Returns affected rows.</summary>
        public int UpdateVetLicenseNumber(int vetId, string newLicenseNumber)
        {
            string sql = "UPDATE VETERINARIAN SET LICENSENUMBER = @NewLicenseNumber WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@NewLicenseNumber", (object)newLicenseNumber ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // DELETE
        // ============================================================

        /// <summary>Deletes the veterinarian with the given ID. Returns affected rows.</summary>
        public int DeleteVet(int vetId)
        {
            string sql = "DELETE FROM VETERINARIAN WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all veterinarians with a specific specialty. Returns affected rows.</summary>
        public int DeleteVetsBySpecialty(string specialty)
        {
            string sql = "DELETE FROM VETERINARIAN WHERE SPECIALTY = @Specialty";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Specialty", specialty)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // HELPER / EXISTENCE METHODS
        // ============================================================

        /// <summary>Returns true if a veterinarian with the given ID exists.</summary>
        public bool VetExists(int vetId)
        {
            string sql = "SELECT COUNT(*) FROM VETERINARIAN WHERE VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a veterinarian with the given license number exists.</summary>
        public bool LicenseNumberExists(string licenseNumber, int excludeVetId = 0)
        {
            string sql = "SELECT COUNT(*) FROM VETERINARIAN WHERE LICENSENUMBER = @LicenseNumber AND VETID != @ExcludeId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@LicenseNumber", licenseNumber),
                new SqlParameter("@ExcludeId", excludeVetId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if the veterinarian works at a specific clinic.</summary>
        public bool VetWorksAtClinic(int vetId, int clinicId)
        {
            string sql = "SELECT COUNT(*) FROM VET_CLINIC WHERE VETID = @VetId AND CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@ClinicId", clinicId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of veterinarians in the database.</summary>
        public int GetVetCount()
        {
            string sql = "SELECT COUNT(*) FROM VETERINARIAN";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of veterinarians by specialty.</summary>
        public Dictionary<string, int> GetVetCountBySpecialty()
        {
            Dictionary<string, int> countBySpecialty = new Dictionary<string, int>();
            string sql = "SELECT SPECIALTY, COUNT(*) AS Count FROM VETERINARIAN GROUP BY SPECIALTY ORDER BY SPECIALTY";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    string specialty = reader["SPECIALTY"] == DBNull.Value ? "Not Specified" : reader["SPECIALTY"].ToString().Trim();
                    int count = Convert.ToInt32(reader["Count"]);
                    countBySpecialty.Add(specialty, count);
                }
            }

            return countBySpecialty;
        }

        /// <summary>Returns the number of veterinarians per clinic.</summary>
        public Dictionary<string, int> GetVetCountPerClinic()
        {
            Dictionary<string, int> countPerClinic = new Dictionary<string, int>();
            string sql = @"SELECT c.CLINICNAME, COUNT(vc.VETID) AS VetCount
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           GROUP BY c.CLINICID, c.CLINICNAME
                           ORDER BY VetCount DESC";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    string clinicName = reader["CLINICNAME"].ToString().Trim();
                    int count = Convert.ToInt32(reader["VetCount"]);
                    countPerClinic.Add(clinicName, count);
                }
            }

            return countPerClinic;
        }

        /// <summary>Returns the average number of clinics per veterinarian.</summary>
        public double GetAverageClinicsPerVet()
        {
            string sql = @"SELECT AVG(ClinicCount) 
                           FROM (SELECT v.VETID, COUNT(vc.CLINICID) AS ClinicCount
                                 FROM VETERINARIAN v
                                 LEFT JOIN VET_CLINIC vc ON v.VETID = vc.VETID
                                 GROUP BY v.VETID) AS Subquery";
            object result = dbHandler.ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
    }
}