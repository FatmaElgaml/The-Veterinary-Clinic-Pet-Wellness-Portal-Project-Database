using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class VaccinationRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to a Vaccination object.</summary>
        private List<Vaccination> ReadVaccinations(string sql, SqlParameter[] parameters = null)
        {
            List<Vaccination> vaccinations = new List<Vaccination>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    vaccinations.Add(new Vaccination
                    {
                        VaccinationId = Convert.ToInt32(reader["VACCINATIONID"]),
                        VisitId = Convert.ToInt32(reader["VISITID"]),
                        InventoryId = Convert.ToInt32(reader["INVENTORYID"]),
                        VaccineType = reader["VACCINETYPE"].ToString().Trim(),
                        AdministeredDate = reader["ADMINISTEREDDATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ADMINISTEREDDATE"]),
                        NextBoosterDue = reader["NEXTBOOSTERDUE"] == DBNull.Value ? null : Convert.ToDateTime(reader["NEXTBOOSTERDUE"])
                    });
                }
            }

            return vaccinations;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Vaccination object with Pet, Owner, and Visit info.</summary>
        private List<Vaccination> ReadVaccinationsWithDetails(string sql, SqlParameter[] parameters = null)
        {
            List<Vaccination> vaccinations = new List<Vaccination>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Vaccination vaccination = new Vaccination
                    {
                        VaccinationId = Convert.ToInt32(reader["VACCINATIONID"]),
                        VisitId = Convert.ToInt32(reader["VISITID"]),
                        InventoryId = Convert.ToInt32(reader["INVENTORYID"]),
                        VaccineType = reader["VACCINETYPE"].ToString().Trim(),
                        AdministeredDate = reader["ADMINISTEREDDATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["ADMINISTEREDDATE"]),
                        NextBoosterDue = reader["NEXTBOOSTERDUE"] == DBNull.Value ? null : Convert.ToDateTime(reader["NEXTBOOSTERDUE"])
                    };

                    // Add Pet information
                    if (reader["PETNAME"] != DBNull.Value)
                    {
                        vaccination.PetName = reader["PETNAME"].ToString().Trim();
                        vaccination.PetId = Convert.ToInt32(reader["PETID"]);
                        vaccination.Species = reader["SPECIES"] == DBNull.Value ? null : reader["SPECIES"].ToString().Trim();
                    }

                    // Add Owner information
                    if (reader["OFRISTNAME"] != DBNull.Value)
                    {
                        vaccination.OwnerFirstName = reader["OFRISTNAME"].ToString().Trim();
                        vaccination.OwnerLastName = reader["OLASTNAME"].ToString().Trim();
                        vaccination.OwnerPhone = reader["OPHONE"] == DBNull.Value ? null : reader["OPHONE"].ToString().Trim();
                        vaccination.OwnerEmail = reader["OEMAIL"] == DBNull.Value ? null : reader["OEMAIL"].ToString().Trim();
                    }

                    // Add Visit information
                    if (reader["VISITDATE"] != DBNull.Value)
                    {
                        vaccination.VisitDate = Convert.ToDateTime(reader["VISITDATE"]);
                        vaccination.VisitStatus = reader["VISITSTATUS"] == DBNull.Value ? null : reader["VISITSTATUS"].ToString().Trim();
                    }

                    // Add Clinic information
                    if (reader["CLINICNAME"] != DBNull.Value)
                    {
                        vaccination.ClinicName = reader["CLINICNAME"].ToString().Trim();
                        vaccination.ClinicLocation = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim();
                    }

                    // Add Inventory information
                    if (reader["BATCHNUMBER"] != DBNull.Value)
                    {
                        vaccination.BatchNumber = reader["BATCHNUMBER"].ToString().Trim();
                        vaccination.SupplierName = reader["SUPPLIERNAME"] == DBNull.Value ? null : reader["SUPPLIERNAME"].ToString().Trim();
                    }

                    vaccinations.Add(vaccination);
                }
            }

            return vaccinations;
        }

        // ============================================================
        // INSERT (CREATE)
        // ============================================================

        /// <summary>Inserts a new vaccination. Returns the number of affected rows.</summary>
        public int InsertVaccination(Vaccination vaccination)
        {
            string sql = @"INSERT INTO VACCINATION (VISITID, INVENTORYID, VACCINETYPE, ADMINISTEREDDATE, NEXTBOOSTERDUE) 
                           VALUES (@VisitId, @InventoryId, @VaccineType, @AdministeredDate, @NextBoosterDue)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", vaccination.VisitId),
                new SqlParameter("@InventoryId", vaccination.InventoryId),
                new SqlParameter("@VaccineType", vaccination.VaccineType),
                new SqlParameter("@AdministeredDate", vaccination.AdministeredDate),
                new SqlParameter("@NextBoosterDue", (object)vaccination.NextBoosterDue ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Inserts a new vaccination and returns the new VaccinationId (IDENTITY).</summary>
        public int InsertVaccinationGetId(Vaccination vaccination)
        {
            string sql = @"INSERT INTO VACCINATION (VISITID, INVENTORYID, VACCINETYPE, ADMINISTEREDDATE, NEXTBOOSTERDUE) 
                           VALUES (@VisitId, @InventoryId, @VaccineType, @AdministeredDate, @NextBoosterDue);
                           SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", vaccination.VisitId),
                new SqlParameter("@InventoryId", vaccination.InventoryId),
                new SqlParameter("@VaccineType", vaccination.VaccineType),
                new SqlParameter("@AdministeredDate", vaccination.AdministeredDate),
                new SqlParameter("@NextBoosterDue", (object)vaccination.NextBoosterDue ?? DBNull.Value)
            };

            object result = dbHandler.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        // ============================================================
        // SELECT (READ)
        // ============================================================

        /// <summary>Returns every vaccination in the database.</summary>
        public List<Vaccination> GetAllVaccinations()
        {
            string sql = "SELECT * FROM VACCINATION ORDER BY ADMINISTEREDDATE DESC";
            return ReadVaccinations(sql);
        }

        /// <summary>Returns every vaccination with full details (Pet, Owner, Visit, Clinic).</summary>
        public List<Vaccination> GetAllVaccinationsWithDetails()
        {
            string sql = @"SELECT 
                           v.VACCINATIONID,
                           v.VISITID,
                           v.INVENTORYID,
                           v.VACCINETYPE,
                           v.ADMINISTEREDDATE,
                           v.NEXTBOOSTERDUE,
                           p.PETID,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           o.OEMAIL,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           c.CLINICNAME,
                           c.LOCATION,
                           vi.BATCHNUMBER,
                           vi.SUPPLIERNAME
                           FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN VACCINE_INVENTORY vi ON v.INVENTORYID = vi.INVENTORYID
                           LEFT JOIN CLINIC c ON vi.CLINICID = c.CLINICID
                           ORDER BY v.ADMINISTEREDDATE DESC";
            return ReadVaccinationsWithDetails(sql);
        }

        /// <summary>Returns the vaccination with the given ID, or null if not found.</summary>
        public Vaccination GetVaccinationById(int vaccinationId)
        {
            string sql = "SELECT * FROM VACCINATION WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId)
            };
            List<Vaccination> vaccinations = ReadVaccinations(sql, parameters);
            return vaccinations.Count > 0 ? vaccinations[0] : null;
        }

        /// <summary>Returns the vaccination with full details by ID.</summary>
        public Vaccination GetVaccinationByIdWithDetails(int vaccinationId)
        {
            string sql = @"SELECT 
                           v.VACCINATIONID,
                           v.VISITID,
                           v.INVENTORYID,
                           v.VACCINETYPE,
                           v.ADMINISTEREDDATE,
                           v.NEXTBOOSTERDUE,
                           p.PETID,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           o.OEMAIL,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           c.CLINICNAME,
                           c.LOCATION,
                           vi.BATCHNUMBER,
                           vi.SUPPLIERNAME
                           FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN VACCINE_INVENTORY vi ON v.INVENTORYID = vi.INVENTORYID
                           LEFT JOIN CLINIC c ON vi.CLINICID = c.CLINICID
                           WHERE v.VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId)
            };
            List<Vaccination> vaccinations = ReadVaccinationsWithDetails(sql, parameters);
            return vaccinations.Count > 0 ? vaccinations[0] : null;
        }

        /// <summary>Returns all vaccinations for a specific pet.</summary>
        public List<Vaccination> GetVaccinationsByPetId(int petId)
        {
            string sql = @"SELECT v.* 
                           FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId
                           ORDER BY v.ADMINISTEREDDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations for a specific pet with full details.</summary>
        public List<Vaccination> GetVaccinationsByPetIdWithDetails(int petId)
        {
            string sql = @"SELECT 
                           v.VACCINATIONID,
                           v.VISITID,
                           v.INVENTORYID,
                           v.VACCINETYPE,
                           v.ADMINISTEREDDATE,
                           v.NEXTBOOSTERDUE,
                           p.PETID,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           o.OEMAIL,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           c.CLINICNAME,
                           c.LOCATION,
                           vi.BATCHNUMBER,
                           vi.SUPPLIERNAME
                           FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN VACCINE_INVENTORY vi ON v.INVENTORYID = vi.INVENTORYID
                           LEFT JOIN CLINIC c ON vi.CLINICID = c.CLINICID
                           WHERE mv.PETID = @PetId
                           ORDER BY v.ADMINISTEREDDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return ReadVaccinationsWithDetails(sql, parameters);
        }

        /// <summary>Returns all vaccinations for a specific owner.</summary>
        public List<Vaccination> GetVaccinationsByOwnerId(int ownerId)
        {
            string sql = @"SELECT v.* 
                           FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           WHERE p.OWNERID = @OwnerId
                           ORDER BY v.ADMINISTEREDDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations of a specific type (e.g., "Rabies", "Distemper").</summary>
        public List<Vaccination> GetVaccinationsByType(string vaccineType)
        {
            string sql = "SELECT * FROM VACCINATION WHERE VACCINETYPE = @VaccineType ORDER BY ADMINISTEREDDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccineType", vaccineType)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations that are due for booster in the next X days.</summary>
        public List<Vaccination> GetUpcomingBoosters(int daysAhead = 30)
        {
            string sql = @"SELECT * FROM VACCINATION 
                           WHERE NEXTBOOSTERDUE IS NOT NULL 
                           AND NEXTBOOSTERDUE BETWEEN @Today AND DATEADD(DAY, @DaysAhead, @Today)
                           ORDER BY NEXTBOOSTERDUE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Today", DateTime.Today),
                new SqlParameter("@DaysAhead", daysAhead)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations that are overdue for booster.</summary>
        public List<Vaccination> GetOverdueBoosters()
        {
            string sql = @"SELECT * FROM VACCINATION 
                           WHERE NEXTBOOSTERDUE IS NOT NULL 
                           AND NEXTBOOSTERDUE < @Today
                           ORDER BY NEXTBOOSTERDUE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Today", DateTime.Today)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations administered in a date range.</summary>
        public List<Vaccination> GetVaccinationsByDateRange(DateTime startDate, DateTime endDate)
        {
            string sql = "SELECT * FROM VACCINATION WHERE ADMINISTEREDDATE BETWEEN @StartDate AND @EndDate ORDER BY ADMINISTEREDDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all vaccinations for a specific visit.</summary>
        public List<Vaccination> GetVaccinationsByVisitId(int visitId)
        {
            string sql = "SELECT * FROM VACCINATION WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            return ReadVaccinations(sql, parameters);
        }

        /// <summary>Returns all unique vaccine types (for dropdown menus).</summary>
        public List<string> GetAllVaccineTypes()
        {
            List<string> vaccineTypes = new List<string>();
            string sql = "SELECT DISTINCT VACCINETYPE FROM VACCINATION ORDER BY VACCINETYPE";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    vaccineTypes.Add(reader["VACCINETYPE"].ToString().Trim());
                }
            }

            return vaccineTypes;
        }

        // ============================================================
        // UPDATE
        // ============================================================

        /// <summary>Updates all fields for the vaccination identified by VaccinationId. Returns affected rows.</summary>
        public int UpdateVaccination(Vaccination vaccination)
        {
            string sql = @"UPDATE VACCINATION SET 
                           VISITID = @VisitId, 
                           INVENTORYID = @InventoryId, 
                           VACCINETYPE = @VaccineType, 
                           ADMINISTEREDDATE = @AdministeredDate, 
                           NEXTBOOSTERDUE = @NextBoosterDue 
                           WHERE VACCINATIONID = @VaccinationId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccination.VaccinationId),
                new SqlParameter("@VisitId", vaccination.VisitId),
                new SqlParameter("@InventoryId", vaccination.InventoryId),
                new SqlParameter("@VaccineType", vaccination.VaccineType),
                new SqlParameter("@AdministeredDate", vaccination.AdministeredDate),
                new SqlParameter("@NextBoosterDue", (object)vaccination.NextBoosterDue ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the vaccine type. Returns affected rows.</summary>
        public int UpdateVaccineType(int vaccinationId, string newVaccineType)
        {
            string sql = "UPDATE VACCINATION SET VACCINETYPE = @NewVaccineType WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId),
                new SqlParameter("@NewVaccineType", newVaccineType)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the next booster due date. Returns affected rows.</summary>
        public int UpdateNextBoosterDue(int vaccinationId, DateTime? newBoosterDue)
        {
            string sql = "UPDATE VACCINATION SET NEXTBOOSTERDUE = @NewBoosterDue WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId),
                new SqlParameter("@NewBoosterDue", (object)newBoosterDue ?? DBNull.Value)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates the inventory ID for a vaccination. Returns affected rows.</summary>
        public int UpdateInventoryId(int vaccinationId, int newInventoryId)
        {
            string sql = "UPDATE VACCINATION SET INVENTORYID = @NewInventoryId WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId),
                new SqlParameter("@NewInventoryId", newInventoryId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // DELETE
        // ============================================================

        /// <summary>Deletes the vaccination with the given ID. Returns affected rows.</summary>
        public int DeleteVaccination(int vaccinationId)
        {
            string sql = "DELETE FROM VACCINATION WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all vaccinations for a specific visit. Returns affected rows.</summary>
        public int DeleteVaccinationsByVisitId(int visitId)
        {
            string sql = "DELETE FROM VACCINATION WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all vaccinations for a specific pet. Returns affected rows.</summary>
        public int DeleteVaccinationsByPetId(int petId)
        {
            string sql = @"DELETE v FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all expired vaccinations (older than a date). Returns affected rows.</summary>
        public int DeleteVaccinationsOlderThan(DateTime date)
        {
            string sql = "DELETE FROM VACCINATION WHERE ADMINISTEREDDATE < @Date";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Date", date)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // HELPER / EXISTENCE METHODS
        // ============================================================

        /// <summary>Returns true if a vaccination with the given ID exists.</summary>
        public bool VaccinationExists(int vaccinationId)
        {
            string sql = "SELECT COUNT(*) FROM VACCINATION WHERE VACCINATIONID = @VaccinationId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VaccinationId", vaccinationId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a visit has any vaccinations.</summary>
        public bool VisitHasVaccinations(int visitId)
        {
            string sql = "SELECT COUNT(*) FROM VACCINATION WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a pet has any vaccinations.</summary>
        public bool PetHasVaccinations(int petId)
        {
            string sql = @"SELECT COUNT(*) FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of vaccinations in the database.</summary>
        public int GetVaccinationCount()
        {
            string sql = "SELECT COUNT(*) FROM VACCINATION";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of vaccinations for a specific pet.</summary>
        public int GetVaccinationCountByPet(int petId)
        {
            string sql = @"SELECT COUNT(*) FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the number of vaccinations by vaccine type.</summary>
        public Dictionary<string, int> GetVaccinationCountByType()
        {
            Dictionary<string, int> countByType = new Dictionary<string, int>();
            string sql = "SELECT VACCINETYPE, COUNT(*) AS Count FROM VACCINATION GROUP BY VACCINETYPE ORDER BY VACCINETYPE";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    string vaccineType = reader["VACCINETYPE"].ToString();
                    int count = Convert.ToInt32(reader["Count"]);
                    countByType.Add(vaccineType, count);
                }
            }

            return countByType;
        }

        /// <summary>Returns the number of vaccinations per month (for statistics).</summary>
        public Dictionary<string, int> GetVaccinationsPerMonth(int year)
        {
            Dictionary<string, int> vaccinationsPerMonth = new Dictionary<string, int>();
            string sql = @"SELECT 
                           MONTH(ADMINISTEREDDATE) AS MonthNumber,
                           DATENAME(MONTH, ADMINISTEREDDATE) AS MonthName,
                           COUNT(*) AS VaccinationCount
                           FROM VACCINATION
                           WHERE YEAR(ADMINISTEREDDATE) = @Year
                           GROUP BY MONTH(ADMINISTEREDDATE), DATENAME(MONTH, ADMINISTEREDDATE)
                           ORDER BY MONTH(ADMINISTEREDDATE)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Year", year)
            };

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    string monthName = reader["MonthName"].ToString();
                    int count = Convert.ToInt32(reader["VaccinationCount"]);
                    vaccinationsPerMonth.Add(monthName, count);
                }
            }

            return vaccinationsPerMonth;
        }

        /// <summary>Returns the most recent vaccination date for a pet.</summary>
        public DateTime? GetLastVaccinationDateByPet(int petId)
        {
            string sql = @"SELECT MAX(ADMINISTEREDDATE) FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            object result = dbHandler.ExecuteScalar(sql, parameters);
            return result == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(result);
        }

        /// <summary>Returns the next upcoming booster date for a pet.</summary>
        public DateTime? GetNextBoosterDateByPet(int petId)
        {
            string sql = @"SELECT MIN(NEXTBOOSTERDUE) FROM VACCINATION v
                           INNER JOIN MEDICAL_VISIT mv ON v.VISITID = mv.VISITID
                           WHERE mv.PETID = @PetId 
                           AND NEXTBOOSTERDUE IS NOT NULL 
                           AND NEXTBOOSTERDUE >= @Today";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId),
                new SqlParameter("@Today", DateTime.Today)
            };
            object result = dbHandler.ExecuteScalar(sql, parameters);
            return result == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(result);
        }
    }
}