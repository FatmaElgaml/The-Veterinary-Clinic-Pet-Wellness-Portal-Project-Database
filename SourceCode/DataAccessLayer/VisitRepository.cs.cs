using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class VisitRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to a Visit object.</summary>
        private List<Visit> ReadVisits(string sql, SqlParameter[] parameters = null)
        {
            List<Visit> visits = new List<Visit>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    visits.Add(new Visit
                    {
                        VisitId = Convert.ToInt32(reader["VISITID"]),
                        PetId = Convert.ToInt32(reader["PETID"]),
                        SlotId = reader["SLOTID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SLOTID"]),
                        NoteId = reader["NOTEID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOTEID"]),
                        VisitDate = reader["VISITDATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["VISITDATE"]),
                        VisitStatus = reader["VISITSTATUS"] == DBNull.Value ? null : reader["VISITSTATUS"].ToString().Trim()
                    });
                }
            }

            return visits;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Visit object with Pet and Owner info.</summary>
        private List<Visit> ReadVisitsWithDetails(string sql, SqlParameter[] parameters = null)
        {
            List<Visit> visits = new List<Visit>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Visit visit = new Visit
                    {
                        VisitId = Convert.ToInt32(reader["VISITID"]),
                        PetId = Convert.ToInt32(reader["PETID"]),
                        SlotId = reader["SLOTID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SLOTID"]),
                        NoteId = reader["NOTEID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOTEID"]),
                        VisitDate = reader["VISITDATE"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["VISITDATE"]),
                        VisitStatus = reader["VISITSTATUS"] == DBNull.Value ? null : reader["VISITSTATUS"].ToString().Trim()
                    };

                    // Add Pet information
                    if (reader["PETNAME"] != DBNull.Value)
                    {
                        visit.PetName = reader["PETNAME"].ToString().Trim();
                        visit.Species = reader["SPECIES"] == DBNull.Value ? null : reader["SPECIES"].ToString().Trim();
                    }

                    // Add Owner information
                    if (reader["OFRISTNAME"] != DBNull.Value)
                    {
                        visit.OwnerFirstName = reader["OFRISTNAME"].ToString().Trim();
                        visit.OwnerLastName = reader["OLASTNAME"].ToString().Trim();
                        visit.OwnerPhone = reader["OPHONE"] == DBNull.Value ? null : reader["OPHONE"].ToString().Trim();
                    }

                    // Add Vet information
                    if (reader["VETFIRSTNAME"] != DBNull.Value)
                    {
                        visit.VetFirstName = reader["VETFIRSTNAME"].ToString().Trim();
                        visit.VetLastName = reader["VETLASTNAME"] == DBNull.Value ? null : reader["VETLASTNAME"].ToString().Trim();
                        visit.VetSpecialty = reader["SPECIALTY"] == DBNull.Value ? null : reader["SPECIALTY"].ToString().Trim();
                    }

                    // Add Clinic information
                    if (reader["CLINICNAME"] != DBNull.Value)
                    {
                        visit.ClinicName = reader["CLINICNAME"].ToString().Trim();
                        visit.ClinicLocation = reader["LOCATION"] == DBNull.Value ? null : reader["LOCATION"].ToString().Trim();
                    }

                    // Add Clinical Note information
                    if (reader["DIAGNOSIS"] != DBNull.Value)
                    {
                        visit.Diagnosis = reader["DIAGNOSIS"].ToString().Trim();
                        visit.TreatmentPlan = reader["TREATMENTPLAN"] == DBNull.Value ? null : reader["TREATMENTPLAN"].ToString().Trim();
                    }

                    visits.Add(visit);
                }
            }

            return visits;
        }

        // ============================================================
        // INSERT (CREATE)
        // ============================================================

        /// <summary>Inserts a new medical visit. Returns the number of affected rows.</summary>
        public int InsertVisit(Visit visit)
        {
            string sql = @"INSERT INTO MEDICAL_VISIT (PETID, SLOTID, NOTEID, VISITDATE, VISITSTATUS) 
                           VALUES (@PetId, @SlotId, @NoteId, @VisitDate, @VisitStatus)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", visit.PetId),
                new SqlParameter("@SlotId", (object)visit.SlotId ?? DBNull.Value),
                new SqlParameter("@NoteId", (object)visit.NoteId ?? DBNull.Value),
                new SqlParameter("@VisitDate", visit.VisitDate),
                new SqlParameter("@VisitStatus", (object)visit.VisitStatus ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Inserts a new medical visit and returns the new VisitId (IDENTITY).</summary>
        public int InsertVisitGetId(Visit visit)
        {
            string sql = @"INSERT INTO MEDICAL_VISIT (PETID, SLOTID, NOTEID, VISITDATE, VISITSTATUS) 
                           VALUES (@PetId, @SlotId, @NoteId, @VisitDate, @VisitStatus);
                           SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", visit.PetId),
                new SqlParameter("@SlotId", (object)visit.SlotId ?? DBNull.Value),
                new SqlParameter("@NoteId", (object)visit.NoteId ?? DBNull.Value),
                new SqlParameter("@VisitDate", visit.VisitDate),
                new SqlParameter("@VisitStatus", (object)visit.VisitStatus ?? DBNull.Value)
            };

            object result = dbHandler.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        // ============================================================
        // SELECT (READ)
        // ============================================================

        /// <summary>Returns every medical visit in the database.</summary>
        public List<Visit> GetAllVisits()
        {
            string sql = "SELECT * FROM MEDICAL_VISIT ORDER BY VISITDATE DESC";
            return ReadVisits(sql);
        }

        /// <summary>Returns every medical visit with full details (Pet, Owner, Vet, Clinic).</summary>
        public List<Visit> GetAllVisitsWithDetails()
        {
            string sql = @"SELECT 
                           mv.VISITID,
                           mv.PETID,
                           mv.SLOTID,
                           mv.NOTEID,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           c.CLINICNAME,
                           c.LOCATION,
                           cn.DIAGNOSIS,
                           cn.TREATMENTPLAN
                           FROM MEDICAL_VISIT mv
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN APPOINTMENT_SLOT aps ON mv.SLOTID = aps.SLOTID
                           LEFT JOIN VET_CLINIC vc ON aps.ATTRIBUTE_70 = vc.ATTRIBUTE_70
                           LEFT JOIN VETERINARIAN v ON vc.VETID = v.VETID
                           LEFT JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           LEFT JOIN CLINICAL_NOTE cn ON mv.NOTEID = cn.NOTEID
                           ORDER BY mv.VISITDATE DESC";
            return ReadVisitsWithDetails(sql);
        }

        /// <summary>Returns the medical visit with the given ID, or null if not found.</summary>
        public Visit GetVisitById(int visitId)
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            List<Visit> visits = ReadVisits(sql, parameters);
            return visits.Count > 0 ? visits[0] : null;
        }

        /// <summary>Returns the medical visit with full details by ID.</summary>
        public Visit GetVisitByIdWithDetails(int visitId)
        {
            string sql = @"SELECT 
                           mv.VISITID,
                           mv.PETID,
                           mv.SLOTID,
                           mv.NOTEID,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           c.CLINICNAME,
                           c.LOCATION,
                           cn.DIAGNOSIS,
                           cn.TREATMENTPLAN
                           FROM MEDICAL_VISIT mv
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN APPOINTMENT_SLOT aps ON mv.SLOTID = aps.SLOTID
                           LEFT JOIN VET_CLINIC vc ON aps.ATTRIBUTE_70 = vc.ATTRIBUTE_70
                           LEFT JOIN VETERINARIAN v ON vc.VETID = v.VETID
                           LEFT JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           LEFT JOIN CLINICAL_NOTE cn ON mv.NOTEID = cn.NOTEID
                           WHERE mv.VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            List<Visit> visits = ReadVisitsWithDetails(sql, parameters);
            return visits.Count > 0 ? visits[0] : null;
        }

        /// <summary>Returns all medical visits for a specific pet.</summary>
        public List<Visit> GetVisitsByPetId(int petId)
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE PETID = @PetId ORDER BY VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns all medical visits for a specific pet with full details.</summary>
        public List<Visit> GetVisitsByPetIdWithDetails(int petId)
        {
            string sql = @"SELECT 
                           mv.VISITID,
                           mv.PETID,
                           mv.SLOTID,
                           mv.NOTEID,
                           mv.VISITDATE,
                           mv.VISITSTATUS,
                           p.PETNAME,
                           p.SPECIES,
                           o.OWNERID,
                           o.OFRISTNAME,
                           o.OLASTNAME,
                           o.OPHONE,
                           v.VETFIRSTNAME,
                           v.VETLASTNAME,
                           v.SPECIALTY,
                           c.CLINICNAME,
                           c.LOCATION,
                           cn.DIAGNOSIS,
                           cn.TREATMENTPLAN
                           FROM MEDICAL_VISIT mv
                           INNER JOIN PET p ON mv.PETID = p.PETID
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           LEFT JOIN APPOINTMENT_SLOT aps ON mv.SLOTID = aps.SLOTID
                           LEFT JOIN VET_CLINIC vc ON aps.ATTRIBUTE_70 = vc.ATTRIBUTE_70
                           LEFT JOIN VETERINARIAN v ON vc.VETID = v.VETID
                           LEFT JOIN CLINIC c ON vc.CLINICID = c.CLINICID
                           LEFT JOIN CLINICAL_NOTE cn ON mv.NOTEID = cn.NOTEID
                           WHERE mv.PETID = @PetId
                           ORDER BY mv.VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return ReadVisitsWithDetails(sql, parameters);
        }

        /// <summary>Returns all medical visits within a date range.</summary>
        public List<Visit> GetVisitsByDateRange(DateTime startDate, DateTime endDate)
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE VISITDATE BETWEEN @StartDate AND @EndDate ORDER BY VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns all medical visits with a specific status (e.g., "Scheduled", "Completed", "Cancelled").</summary>
        public List<Visit> GetVisitsByStatus(string status)
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE VISITSTATUS = @Status ORDER BY VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Status", status)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns all upcoming visits (today and future).</summary>
        public List<Visit> GetUpcomingVisits()
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE VISITDATE >= @Today AND VISITSTATUS = 'Scheduled' ORDER BY VISITDATE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Today", DateTime.Today)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns today's visits.</summary>
        public List<Visit> GetTodaysVisits()
        {
            string sql = "SELECT * FROM MEDICAL_VISIT WHERE CAST(VISITDATE AS DATE) = CAST(@Today AS DATE) ORDER BY VISITDATE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Today", DateTime.Today)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns visits for a specific vet.</summary>
        public List<Visit> GetVisitsByVetId(int vetId)
        {
            string sql = @"SELECT mv.* 
                           FROM MEDICAL_VISIT mv
                           INNER JOIN APPOINTMENT_SLOT aps ON mv.SLOTID = aps.SLOTID
                           INNER JOIN VET_CLINIC vc ON aps.ATTRIBUTE_70 = vc.ATTRIBUTE_70
                           WHERE vc.VETID = @VetId
                           ORDER BY mv.VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VetId", vetId)
            };
            return ReadVisits(sql, parameters);
        }

        /// <summary>Returns visits for a specific clinic.</summary>
        public List<Visit> GetVisitsByClinicId(int clinicId)
        {
            string sql = @"SELECT mv.* 
                           FROM MEDICAL_VISIT mv
                           INNER JOIN APPOINTMENT_SLOT aps ON mv.SLOTID = aps.SLOTID
                           INNER JOIN VET_CLINIC vc ON aps.ATTRIBUTE_70 = vc.ATTRIBUTE_70
                           WHERE vc.CLINICID = @ClinicId
                           ORDER BY mv.VISITDATE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@ClinicId", clinicId)
            };
            return ReadVisits(sql, parameters);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        /// <summary>Updates all fields for the visit identified by VisitId. Returns affected rows.</summary>
        public int UpdateVisit(Visit visit)
        {
            string sql = @"UPDATE MEDICAL_VISIT SET 
                           PETID = @PetId, 
                           SLOTID = @SlotId, 
                           NOTEID = @NoteId, 
                           VISITDATE = @VisitDate, 
                           VISITSTATUS = @VisitStatus 
                           WHERE VISITID = @VisitId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visit.VisitId),
                new SqlParameter("@PetId", visit.PetId),
                new SqlParameter("@SlotId", (object)visit.SlotId ?? DBNull.Value),
                new SqlParameter("@NoteId", (object)visit.NoteId ?? DBNull.Value),
                new SqlParameter("@VisitDate", visit.VisitDate),
                new SqlParameter("@VisitStatus", (object)visit.VisitStatus ?? DBNull.Value)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the visit status. Returns affected rows.</summary>
        public int UpdateVisitStatus(int visitId, string newStatus)
        {
            string sql = "UPDATE MEDICAL_VISIT SET VISITSTATUS = @NewStatus WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId),
                new SqlParameter("@NewStatus", newStatus)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the visit date. Returns affected rows.</summary>
        public int UpdateVisitDate(int visitId, DateTime newDate)
        {
            string sql = "UPDATE MEDICAL_VISIT SET VISITDATE = @NewDate WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId),
                new SqlParameter("@NewDate", newDate)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates the clinical note for a visit. Returns affected rows.</summary>
        public int UpdateVisitNote(int visitId, int noteId)
        {
            string sql = "UPDATE MEDICAL_VISIT SET NOTEID = @NoteId WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId),
                new SqlParameter("@NoteId", noteId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // DELETE
        // ============================================================

        /// <summary>Deletes the medical visit with the given ID. Returns affected rows.</summary>
        public int DeleteVisit(int visitId)
        {
            string sql = "DELETE FROM MEDICAL_VISIT WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all visits for a specific pet. Returns affected rows.</summary>
        public int DeleteVisitsByPetId(int petId)
        {
            string sql = "DELETE FROM MEDICAL_VISIT WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all visits with a specific status. Returns affected rows.</summary>
        public int DeleteVisitsByStatus(string status)
        {
            string sql = "DELETE FROM MEDICAL_VISIT WHERE VISITSTATUS = @Status";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Status", status)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // HELPER / EXISTENCE METHODS
        // ============================================================

        /// <summary>Returns true if a medical visit with the given ID exists.</summary>
        public bool VisitExists(int visitId)
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT WHERE VISITID = @VisitId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitId", visitId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a pet has any visits.</summary>
        public bool PetHasVisits(int petId)
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of medical visits in the database.</summary>
        public int GetVisitCount()
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of visits for a specific pet.</summary>
        public int GetVisitCountByPet(int petId)
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the number of visits with a specific status.</summary>
        public int GetVisitCountByStatus(string status)
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT WHERE VISITSTATUS = @Status";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Status", status)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the number of visits in a date range.</summary>
        public int GetVisitCountByDateRange(DateTime startDate, DateTime endDate)
        {
            string sql = "SELECT COUNT(*) FROM MEDICAL_VISIT WHERE VISITDATE BETWEEN @StartDate AND @EndDate";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the most recent visit date for a pet.</summary>
        public DateTime? GetLastVisitDateByPet(int petId)
        {
            string sql = "SELECT MAX(VISITDATE) FROM MEDICAL_VISIT WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            object result = dbHandler.ExecuteScalar(sql, parameters);
            return result == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(result);
        }

        /// <summary>Returns the total number of visits per month (for statistics).</summary>
        public Dictionary<string, int> GetVisitsPerMonth(int year)
        {
            Dictionary<string, int> visitsPerMonth = new Dictionary<string, int>();
            string sql = @"SELECT 
                           MONTH(VISITDATE) AS MonthNumber,
                           DATENAME(MONTH, VISITDATE) AS MonthName,
                           COUNT(*) AS VisitCount
                           FROM MEDICAL_VISIT
                           WHERE YEAR(VISITDATE) = @Year
                           GROUP BY MONTH(VISITDATE), DATENAME(MONTH, VISITDATE)
                           ORDER BY MONTH(VISITDATE)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Year", year)
            };

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    string monthName = reader["MonthName"].ToString();
                    int count = Convert.ToInt32(reader["VisitCount"]);
                    visitsPerMonth.Add(monthName, count);
                }
            }

            return visitsPerMonth;
        }
    }
}