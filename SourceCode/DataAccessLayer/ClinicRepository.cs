using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class ClinicRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to a Clinic object.</summary>
        private List<Clinic> ReadClinics(string sql, SqlParameter[] parameters = null)
        {
            List<Clinic> clinics = new List<Clinic>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    clinics.Add(new Clinic
                    {
                        ClinicId = Convert.ToInt32(reader["CLINICID"]),
                        ClinicName = reader["CLINICNAME"].ToString().Trim(),
                        Location = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim(),
                        HasEmergencyFacility = reader["HASEMERGENCYFACILITY"] != DBNull.Value && Convert.ToBoolean(reader["HASEMERGENCYFACILITY"]),
                        Phone = reader["CLINICPHONE"] == DBNull.Value ? null : reader["CLINICPHONE"].ToString().Trim()
                    });
                }
            }

            return clinics;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Clinic object with Vet info.</summary>
        private List<Clinic> ReadClinicsWithVets(string sql, SqlParameter[] parameters = null)
        {
            List<Clinic> clinics = new List<Clinic>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Clinic clinic = new Clinic
                    {
                        ClinicId = Convert.ToInt32(reader["CLINICID"]),
                        ClinicName = reader["CLINICNAME"].ToString().Trim(),
                        Location = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim(),
                        HasEmergencyFacility = reader["HASEMERGENCYFACILITY"] != DBNull.Value && Convert.ToBoolean(reader["HASEMERGENCYFACILITY"]),
                        Phone = reader["CLINICPHONE"] == DBNull.Value ? null : reader["CLINICPHONE"].ToString().Trim()
                    };

                    // Add Vet information (from JOIN)
                    if (reader["VETID"] != DBNull.Value)
                    {
                        clinic.VetId = Convert.ToInt32(reader["VETID"]);
                        clinic.VetFirstName = reader["VETFIRSTNAME"] == DBNull.Value ? null : reader["VETFIRSTNAME"].ToString().Trim();
                        clinic.VetLastName = reader["VETLASTNAME"] == DBNull.Value ? null : reader["VETLASTNAME"].ToString().Trim();
                        clinic.VetSpecialty = reader["SPECIALTY"] == DBNull.Value ? null : reader["SPECIALTY"].ToString().Trim();
                        clinic.IsPrimaryVet = reader["ISPRIMARY"] != DBNull.Value && Convert.ToBoolean(reader["ISPRIMARY"]);
                    }

                    clinics.Add(clinic);
                }
            }

            return clinics;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Clinic object with statistics.</summary>
        private List<Clinic> ReadClinicsWithStats(string sql, SqlParameter[] parameters = null)
        {
            List<Clinic> clinics = new List<Clinic>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Clinic clinic = new Clinic
                    {
                        ClinicId = Convert.ToInt32(reader["CLINICID"]),
                        ClinicName = reader["CLINICNAME"].ToString().Trim(),
                        Location = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim(),
                        HasEmergencyFacility = reader["HASEMERGENCYFACILITY"] != DBNull.Value && Convert.ToBoolean(reader["HASEMERGENCYFACILITY"]),
                        Phone = reader["CLINICPHONE"] == DBNull.Value ? null : reader["CLINICPHONE"].ToString().Trim()
                    };

                    // Add statistics
                    if (reader["VETCOUNT"] != DBNull.Value)
                    {
                        clinic.VetCount = Convert.ToInt32(reader["VETCOUNT"]);
                    }
                    if (reader["VISITCOUNT"] != DBNull.Value)
                    {
                        clinic.VisitCount = Convert.ToInt32(reader["VISITCOUNT"]);
                    }
                    if (reader["TOTALVACCINATIONS"] != DBNull.Value)
                    {
                        clinic.TotalVaccinations = Convert.ToInt32(reader["TOTALVACCINATIONS"]);
                    }

                    clinics.Add(clinic);
                }
            }

            return clinics;
        }

        // ============================================================
        // INSERT (CREATE)
        // ============================================================

        /// <summary>Inserts a new clinic. Returns the number of affected rows.</summary>
        public int InsertClinic(Clinic clinic)
        {
            string sql = @"INSERT INTO CLINIC (CLINICNAME, LOCATION, HASEMERGENCYFACILITY, CLINICPHONE) 
                           VALUES (@ClinicName, @Location, @HasEmergencyFacility, @Phone)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicName", clinic.ClinicName),
                new SqlParameter("@Location", (object)clinic.Location ?? DBNull.Value),
                new SqlParameter("@HasEmergencyFacility", clinic.HasEmergencyFacility),
                new SqlParameter("@Phone", (object)clinic.Phone ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Inserts a new clinic and returns the new ClinicId (IDENTITY).</summary>
        public int InsertClinicGetId(Clinic clinic)
        {
            string sql = @"INSERT INTO CLINIC (CLINICNAME, LOCATION, HASEMERGENCYFACILITY, CLINICPHONE) 
                           VALUES (@ClinicName, @Location, @HasEmergencyFacility, @Phone);
                           SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicName", clinic.ClinicName),
                new SqlParameter("@Location", (object)clinic.Location ?? DBNull.Value),
                new SqlParameter("@HasEmergencyFacility", clinic.HasEmergencyFacility),
                new SqlParameter("@Phone", (object)clinic.Phone ?? DBNull.Value)
            };

            object result = dbHandler.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        // ============================================================
        // SELECT (READ)
        // ============================================================

        /// <summary>Returns every clinic in the database.</summary>
        public List<Clinic> GetAllClinics()
        {
            string sql = "SELECT * FROM CLINIC ORDER BY CLINICNAME";
            return ReadClinics(sql);
        }

        /// <summary>Returns every clinic with their veterinarians (JOIN).</summary>
        public List<Clinic> GetAllClinicsWithVets()
        {
            string sql = @"SELECT 
                           c.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           c.HASEMERGENCYFACILITY,
                           c.CLINICPHONE,
                           vc.VETID,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           vc.ISPRIMARY
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           LEFT JOIN VETERINARIAN v ON vc.VETID = v.VETID
                           ORDER BY c.CLINICNAME, v.VETLASTNAME";
            return ReadClinicsWithVets(sql);
        }

        /// <summary>Returns the clinic with the given ID, or null if not found.</summary>
        public Clinic GetClinicById(int clinicId)
        {
            string sql = "SELECT * FROM CLINIC WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            List<Clinic> clinics = ReadClinics(sql, parameters);
            return clinics.Count > 0 ? clinics[0] : null;
        }

        /// <summary>Returns the clinic with veterinarians by ID.</summary>
        public Clinic GetClinicByIdWithVets(int clinicId)
        {
            string sql = @"SELECT 
                           c.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           c.HASEMERGENCYFACILITY,
                           c.CLINICPHONE,
                           vc.VETID,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           vc.ISPRIMARY
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           LEFT JOIN VETERINARIAN v ON vc.VETID = v.VETID
                           WHERE c.CLINICID = @ClinicId
                           ORDER BY v.VETLASTNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            List<Clinic> clinics = ReadClinicsWithVets(sql, parameters);
            return clinics.Count > 0 ? clinics[0] : null;
        }

        /// <summary>Returns clinics whose name contains the search term (case-insensitive).</summary>
        public List<Clinic> SearchClinicsByName(string searchTerm)
        {
            string sql = "SELECT * FROM CLINIC WHERE CLINICNAME LIKE @SearchTerm ORDER BY CLINICNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SearchTerm", "%" + searchTerm + "%")
            };
            return ReadClinics(sql, parameters);
        }

        /// <summary>Returns clinics by location (city/area).</summary>
        public List<Clinic> GetClinicsByLocation(string location)
        {
            string sql = "SELECT * FROM CLINIC WHERE LOCATION LIKE @Location ORDER BY CLINICNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Location", "%" + location + "%")
            };
            return ReadClinics(sql, parameters);
        }

        /// <summary>Returns clinics that have emergency facilities.</summary>
        public List<Clinic> GetClinicsWithEmergencyFacility()
        {
            string sql = "SELECT * FROM CLINIC WHERE HASEMERGENCYFACILITY = 1 ORDER BY CLINICNAME";
            return ReadClinics(sql);
        }

        /// <summary>Returns clinics that have a specific veterinarian.</summary>
        public List<Clinic> GetClinicsByVetId(int vetId)
        {
            string sql = @"SELECT c.* 
                           FROM CLINIC c
                           INNER JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           WHERE vc.VETID = @VetId
                           ORDER BY c.CLINICNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            return ReadClinics(sql, parameters);
        }

        /// <summary>Returns clinics with statistics (vet count, visit count).</summary>
        public List<Clinic> GetAllClinicsWithStatistics()
        {
            string sql = @"SELECT 
                           c.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           c.HASEMERGENCYFACILITY,
                           c.CLINICPHONE,
                           COUNT(DISTINCT vc.VETID) AS VETCOUNT,
                           COUNT(DISTINCT mv.VISITID) AS VISITCOUNT,
                           COUNT(DISTINCT v.VACCINATIONID) AS TOTALVACCINATIONS
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           LEFT JOIN APPOINTMENT_SLOT aps ON vc.ATTRIBUTE_70 = aps.ATTRIBUTE_70
                           LEFT JOIN MEDICAL_VISIT mv ON aps.SLOTID = mv.SLOTID
                           LEFT JOIN VACCINATION v ON mv.VISITID = v.VISITID
                           GROUP BY c.CLINICID, c.CLINICNAME, c.LOCATION, c.HASEMERGENCYFACILITY, c.CLINICPHONE
                           ORDER BY c.CLINICNAME";
            return ReadClinicsWithStats(sql);
        }

        /// <summary>Returns clinic statistics by ID.</summary>
        public Clinic GetClinicStatisticsById(int clinicId)
        {
            string sql = @"SELECT 
                           c.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           c.HASEMERGENCYFACILITY,
                           c.CLINICPHONE,
                           COUNT(DISTINCT vc.VETID) AS VETCOUNT,
                           COUNT(DISTINCT mv.VISITID) AS VISITCOUNT,
                           COUNT(DISTINCT v.VACCINATIONID) AS TOTALVACCINATIONS
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           LEFT JOIN APPOINTMENT_SLOT aps ON vc.ATTRIBUTE_70 = aps.ATTRIBUTE_70
                           LEFT JOIN MEDICAL_VISIT mv ON aps.SLOTID = mv.SLOTID
                           LEFT JOIN VACCINATION v ON mv.VISITID = v.VISITID
                           WHERE c.CLINICID = @ClinicId
                           GROUP BY c.CLINICID, c.CLINICNAME, c.LOCATION, c.HASEMERGENCYFACILITY, c.CLINICPHONE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            List<Clinic> clinics = ReadClinicsWithStats(sql, parameters);
            return clinics.Count > 0 ? clinics[0] : null;
        }

        /// <summary>Returns all unique locations (for dropdown menus).</summary>
        public List<string> GetAllLocations()
        {
            List<string> locations = new List<string>();
            string sql = "SELECT DISTINCT LOCATION FROM CLINIC WHERE LOCATION IS NOT NULL ORDER BY LOCATION";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    locations.Add(reader["LOCATION"].ToString().Trim());
                }
            }

            return locations;
        }

        // ============================================================
        // VET_CLINIC JUNCTION TABLE OPERATIONS
        // ============================================================

        /// <summary>Assigns a veterinarian to a clinic. Returns affected rows.</summary>
        public int AssignVetToClinic(int vetId, int clinicId, bool isPrimary = false, DateTime? joinDate = null)
        {
            string sql = @"INSERT INTO VET_CLINIC (VETID, CLINICID, ISPRIMARY, JOINDATE, ATTRIBUTE_70) 
                           VALUES (@VetId, @ClinicId, @IsPrimary, @JoinDate, 
                           (SELECT ISNULL(MAX(ATTRIBUTE_70), 0) + 1 FROM VET_CLINIC))";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@IsPrimary", isPrimary),
                new SqlParameter("@JoinDate", (object)joinDate ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Removes a veterinarian from a clinic. Returns affected rows.</summary>
        public int RemoveVetFromClinic(int vetId, int clinicId)
        {
            string sql = "DELETE FROM VET_CLINIC WHERE VETID = @VetId AND CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@ClinicId", clinicId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates the primary status of a veterinarian at a clinic. Returns affected rows.</summary>
        public int UpdatePrimaryVetStatus(int vetId, int clinicId, bool isPrimary)
        {
            string sql = "UPDATE VET_CLINIC SET ISPRIMARY = @IsPrimary WHERE VETID = @VetId AND CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId),
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@IsPrimary", isPrimary)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Removes all veterinarians from a clinic. Returns affected rows.</summary>
        public int RemoveAllVetsFromClinic(int clinicId)
        {
            string sql = "DELETE FROM VET_CLINIC WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        /// <summary>Updates all fields for the clinic identified by ClinicId. Returns affected rows.</summary>
        public int UpdateClinic(Clinic clinic)
        {
            string sql = @"UPDATE CLINIC SET 
                           CLINICNAME = @ClinicName, 
                           LOCATION = @Location, 
                           HASEMERGENCYFACILITY = @HasEmergencyFacility, 
                           CLINICPHONE = @Phone 
                           WHERE CLINICID = @ClinicId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinic.ClinicId),
                new SqlParameter("@ClinicName", clinic.ClinicName),
                new SqlParameter("@Location", (object)clinic.Location ?? DBNull.Value),
                new SqlParameter("@HasEmergencyFacility", clinic.HasEmergencyFacility),
                new SqlParameter("@Phone", (object)clinic.Phone ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the clinic name. Returns affected rows.</summary>
        public int UpdateClinicName(int clinicId, string newName)
        {
            string sql = "UPDATE CLINIC SET CLINICNAME = @NewName WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@NewName", newName)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the clinic location. Returns affected rows.</summary>
        public int UpdateClinicLocation(int clinicId, string newLocation)
        {
            string sql = "UPDATE CLINIC SET LOCATION = @NewLocation WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@NewLocation", (object)newLocation ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the clinic phone. Returns affected rows.</summary>
        public int UpdateClinicPhone(int clinicId, string newPhone)
        {
            string sql = "UPDATE CLINIC SET CLINICPHONE = @NewPhone WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@NewPhone", (object)newPhone ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates emergency facility status. Returns affected rows.</summary>
        public int UpdateEmergencyFacilityStatus(int clinicId, bool hasEmergencyFacility)
        {
            string sql = "UPDATE CLINIC SET HASEMERGENCYFACILITY = @HasEmergencyFacility WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@HasEmergencyFacility", hasEmergencyFacility)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // DELETE
        // ============================================================

        /// <summary>Deletes the clinic with the given ID. Returns affected rows.</summary>
        public int DeleteClinic(int clinicId)
        {
            string sql = "DELETE FROM CLINIC WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all clinics in a specific location. Returns affected rows.</summary>
        public int DeleteClinicsByLocation(string location)
        {
            string sql = "DELETE FROM CLINIC WHERE LOCATION = @Location";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Location", location)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // HELPER / EXISTENCE METHODS
        // ============================================================

        /// <summary>Returns true if a clinic with the given ID exists.</summary>
        public bool ClinicExists(int clinicId)
        {
            string sql = "SELECT COUNT(*) FROM CLINIC WHERE CLINICID = @ClinicId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a clinic with the given name exists.</summary>
        public bool ClinicNameExists(string clinicName, int excludeClinicId = 0)
        {
            string sql = "SELECT COUNT(*) FROM CLINIC WHERE CLINICNAME = @ClinicName AND CLINICID != @ExcludeId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicName", clinicName),
                new SqlParameter("@ExcludeId", excludeClinicId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a veterinarian is assigned to the clinic.</summary>
        public bool ClinicHasVet(int clinicId, int vetId)
        {
            string sql = "SELECT COUNT(*) FROM VET_CLINIC WHERE CLINICID = @ClinicId AND VETID = @VetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId),
                new SqlParameter("@VetId", vetId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of clinics in the database.</summary>
        public int GetClinicCount()
        {
            string sql = "SELECT COUNT(*) FROM CLINIC";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of clinics with emergency facilities.</summary>
        public int GetEmergencyClinicCount()
        {
            string sql = "SELECT COUNT(*) FROM CLINIC WHERE HASEMERGENCYFACILITY = 1";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of clinics per location.</summary>
        public Dictionary<string, int> GetClinicCountByLocation()
        {
            Dictionary<string, int> countByLocation = new Dictionary<string, int>();
            string sql = "SELECT LOCATION, COUNT(*) AS Count FROM CLINIC WHERE LOCATION IS NOT NULL GROUP BY LOCATION ORDER BY LOCATION";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    string location = reader["LOCATION"].ToString().Trim();
                    int count = Convert.ToInt32(reader["Count"]);
                    countByLocation.Add(location, count);
                }
            }

            return countByLocation;
        }

        /// <summary>Returns the average number of veterinarians per clinic.</summary>
        public double GetAverageVetsPerClinic()
        {
            string sql = @"SELECT AVG(VetCount) 
                           FROM (SELECT c.CLINICID, COUNT(vc.VETID) AS VetCount
                                 FROM CLINIC c
                                 LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                                 GROUP BY c.CLINICID) AS Subquery";
            object result = dbHandler.ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        /// <summary>Returns the clinic with the most veterinarians.</summary>
        public Clinic GetClinicWithMostVets()
        {
            string sql = @"SELECT TOP 1 
                           c.CLINICID,
                           c.CLINICNAME,
                           c.LOCATION,
                           c.HASEMERGENCYFACILITY,
                           c.CLINICPHONE,
                           COUNT(vc.VETID) AS VETCOUNT
                           FROM CLINIC c
                           LEFT JOIN VET_CLINIC vc ON c.CLINICID = vc.CLINICID
                           GROUP BY c.CLINICID, c.CLINICNAME, c.LOCATION, c.HASEMERGENCYFACILITY, c.CLINICPHONE
                           ORDER BY VETCOUNT DESC";
            List<Clinic> clinics = ReadClinicsWithStats(sql);
            return clinics.Count > 0 ? clinics[0] : null;
        }
    }
}