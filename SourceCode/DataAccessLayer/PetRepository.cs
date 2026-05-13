using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryClinicProject.Models;

namespace VeterinaryClinicProject.DataAccessLayer
{
    public class PetRepository
    {
        private readonly DBHandler dbHandler = new DBHandler();

        /// <summary>Executes a SELECT and maps each row to a Pet object.</summary>
        private List<Pet> ReadPets(string sql, SqlParameter[] parameters = null)
        {
            List<Pet> pets = new List<Pet>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    pets.Add(new Pet
                    {
                        PetId = Convert.ToInt32(reader["PETID"]),
                        PetName = reader["PETNAME"].ToString().Trim(),
                        Species = reader["SPECIES"].ToString().Trim(),
                        Breed = reader["BREED"] == DBNull.Value ? null : reader["BREED"].ToString().Trim(),
                        Age = reader["AGE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AGE"]),
                        OwnerId = Convert.ToInt32(reader["OWNERID"])
                    });
                }
            }

            return pets;
        }

        /// <summary>Executes a SELECT with JOIN and maps each row to a Pet object with Owner info.</summary>
        private List<Pet> ReadPetsWithOwner(string sql, SqlParameter[] parameters = null)
        {
            List<Pet> pets = new List<Pet>();

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    Pet pet = new Pet
                    {
                        PetId = Convert.ToInt32(reader["PETID"]),
                        PetName = reader["PETNAME"].ToString().Trim(),
                        Species = reader["SPECIES"].ToString().Trim(),
                        Breed = reader["BREED"] == DBNull.Value ? null : reader["BREED"].ToString().Trim(),
                        Age = reader["AGE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AGE"]),
                        OwnerId = Convert.ToInt32(reader["OWNERID"])
                    };

                    // Add owner information if available
                    if (reader["OFRISTNAME"] != DBNull.Value)
                    {
                        pet.OwnerFirstName = reader["OFRISTNAME"].ToString().Trim();
                        pet.OwnerLastName = reader["OLASTNAME"].ToString().Trim();
                        pet.OwnerPhone = reader["OPHONE"] == DBNull.Value ? null : reader["OPHONE"].ToString().Trim();
                    }

                    pets.Add(pet);
                }
            }

            return pets;
        }

        // ============================================================
        // INSERT (CREATE)
        // ============================================================

        /// <summary>Inserts a new pet. Returns the number of affected rows.</summary>
        public int InsertPet(Pet pet)
        {
            string sql = @"INSERT INTO PET (PETNAME, SPECIES, BREED, AGE, OWNERID) 
                           VALUES (@PetName, @Species, @Breed, @Age, @OwnerId)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetName", pet.PetName),
                new SqlParameter("@Species", pet.Species),
                new SqlParameter("@Breed", (object)pet.Breed ?? DBNull.Value),
                new SqlParameter("@Age", (object)pet.Age ?? DBNull.Value),
                new SqlParameter("@OwnerId", pet.OwnerId)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Inserts a new pet and returns the new PetId (IDENTITY).</summary>
        public int InsertPetGetId(Pet pet)
        {
            string sql = @"INSERT INTO PET (PETNAME, SPECIES, BREED, AGE, OWNERID) 
                           VALUES (@PetName, @Species, @Breed, @Age, @OwnerId);
                           SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetName", pet.PetName),
                new SqlParameter("@Species", pet.Species),
                new SqlParameter("@Breed", (object)pet.Breed ?? DBNull.Value),
                new SqlParameter("@Age", (object)pet.Age ?? DBNull.Value),
                new SqlParameter("@OwnerId", pet.OwnerId)
            };

            object result = dbHandler.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        // ============================================================
        // SELECT (READ)
        // ============================================================

        /// <summary>Returns every pet in the database.</summary>
        public List<Pet> GetAllPets()
        {
            string sql = "SELECT * FROM PET ORDER BY PETNAME";
            return ReadPets(sql);
        }

        /// <summary>Returns every pet with owner information (JOIN).</summary>
        public List<Pet> GetAllPetsWithOwner()
        {
            string sql = @"SELECT p.*, o.OFRISTNAME, o.OLASTNAME, o.OPHONE 
                           FROM PET p
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           ORDER BY p.PETNAME";
            return ReadPetsWithOwner(sql);
        }

        /// <summary>Returns the pet with the given ID, or null if not found.</summary>
        public Pet GetPetById(int petId)
        {
            string sql = "SELECT * FROM PET WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            List<Pet> pets = ReadPets(sql, parameters);
            return pets.Count > 0 ? pets[0] : null;
        }

        /// <summary>Returns the pet with owner information by ID.</summary>
        public Pet GetPetByIdWithOwner(int petId)
        {
            string sql = @"SELECT p.*, o.OFRISTNAME, o.OLASTNAME, o.OPHONE 
                           FROM PET p
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           WHERE p.PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            List<Pet> pets = ReadPetsWithOwner(sql, parameters);
            return pets.Count > 0 ? pets[0] : null;
        }

        /// <summary>Returns all pets belonging to a specific owner.</summary>
        public List<Pet> GetPetsByOwnerId(int ownerId)
        {
            string sql = "SELECT * FROM PET WHERE OWNERID = @OwnerId ORDER BY PETNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return ReadPets(sql, parameters);
        }

        /// <summary>Returns all pets belonging to a specific owner with owner info.</summary>
        public List<Pet> GetPetsByOwnerIdWithOwner(int ownerId)
        {
            string sql = @"SELECT p.*, o.OFRISTNAME, o.OLASTNAME, o.OPHONE 
                           FROM PET p
                           INNER JOIN OWNER o ON p.OWNERID = o.OWNERID
                           WHERE p.OWNERID = @OwnerId
                           ORDER BY p.PETNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return ReadPetsWithOwner(sql, parameters);
        }

        /// <summary>Returns pets whose name contains the search term (case-insensitive).</summary>
        public List<Pet> SearchPetsByName(string searchTerm)
        {
            string sql = "SELECT * FROM PET WHERE PETNAME LIKE @SearchTerm ORDER BY PETNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@SearchTerm", "%" + searchTerm + "%")
            };
            return ReadPets(sql, parameters);
        }

        /// <summary>Returns pets of a specific species.</summary>
        public List<Pet> GetPetsBySpecies(string species)
        {
            string sql = "SELECT * FROM PET WHERE SPECIES = @Species ORDER BY PETNAME";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Species", species)
            };
            return ReadPets(sql, parameters);
        }

        /// <summary>Returns all unique species in the database (for dropdown menus).</summary>
        public List<string> GetAllSpecies()
        {
            List<string> species = new List<string>();
            string sql = "SELECT DISTINCT SPECIES FROM PET ORDER BY SPECIES";

            using (SqlDataReader reader = dbHandler.ExcuteReader(sql))
            {
                while (reader.Read())
                {
                    species.Add(reader["SPECIES"].ToString().Trim());
                }
            }

            return species;
        }

        /// <summary>Returns pets older than a certain age.</summary>
        public List<Pet> GetPetsOlderThan(int age)
        {
            string sql = "SELECT * FROM PET WHERE AGE > @Age ORDER BY AGE DESC";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Age", age)
            };
            return ReadPets(sql, parameters);
        }

        /// <summary>Returns pets younger than a certain age.</summary>
        public List<Pet> GetPetsYoungerThan(int age)
        {
            string sql = "SELECT * FROM PET WHERE AGE < @Age ORDER BY AGE";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Age", age)
            };
            return ReadPets(sql, parameters);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        /// <summary>Updates all fields for the pet identified by PetId. Returns affected rows.</summary>
        public int UpdatePet(Pet pet)
        {
            string sql = @"UPDATE PET SET 
                           PETNAME = @PetName, 
                           SPECIES = @Species, 
                           BREED = @Breed, 
                           AGE = @Age, 
                           OWNERID = @OwnerId 
                           WHERE PETID = @PetId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", pet.PetId),
                new SqlParameter("@PetName", pet.PetName),
                new SqlParameter("@Species", pet.Species),
                new SqlParameter("@Breed", (object)pet.Breed ?? DBNull.Value),
                new SqlParameter("@Age", (object)pet.Age ?? DBNull.Value),
                new SqlParameter("@OwnerId", pet.OwnerId)
            };

            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the pet's name. Returns affected rows.</summary>
        public int UpdatePetName(int petId, string newName)
        {
            string sql = "UPDATE PET SET PETNAME = @NewName WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId),
                new SqlParameter("@NewName", newName)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates only the pet's age. Returns affected rows.</summary>
        public int UpdatePetAge(int petId, int newAge)
        {
            string sql = "UPDATE PET SET AGE = @NewAge WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId),
                new SqlParameter("@NewAge", newAge)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Updates the pet's owner. Returns affected rows.</summary>
        public int UpdatePetOwner(int petId, int newOwnerId)
        {
            string sql = "UPDATE PET SET OWNERID = @NewOwnerId WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId),
                new SqlParameter("@NewOwnerId", newOwnerId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // DELETE
        // ============================================================

        /// <summary>Deletes the pet with the given ID. Returns affected rows.</summary>
        public int DeletePet(int petId)
        {
            string sql = "DELETE FROM PET WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        /// <summary>Deletes all pets belonging to an owner. Returns affected rows.</summary>
        public int DeletePetsByOwnerId(int ownerId)
        {
            string sql = "DELETE FROM PET WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return dbHandler.ExecuteNonQuery(sql, parameters);
        }

        // ============================================================
        // HELPER / EXISTENCE METHODS
        // ============================================================

        /// <summary>Returns true if a pet with the given ID exists.</summary>
        public bool PetExists(int petId)
        {
            string sql = "SELECT COUNT(*) FROM PET WHERE PETID = @PetId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@PetId", petId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if the owner has any pets.</summary>
        public bool OwnerHasPets(int ownerId)
        {
            string sql = "SELECT COUNT(*) FROM PET WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns true if a pet with the given name exists for the owner.</summary>
        public bool PetNameExistsForOwner(int ownerId, string petName)
        {
            string sql = "SELECT COUNT(*) FROM PET WHERE OWNERID = @OwnerId AND PETNAME = @PetName";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId),
                new SqlParameter("@PetName", petName)
            };
            int count = Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
            return count > 0;
        }

        /// <summary>Returns the total number of pets in the database.</summary>
        public int GetPetCount()
        {
            string sql = "SELECT COUNT(*) FROM PET";
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql));
        }

        /// <summary>Returns the number of pets for a specific owner.</summary>
        public int GetPetCountByOwner(int ownerId)
        {
            string sql = "SELECT COUNT(*) FROM PET WHERE OWNERID = @OwnerId";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@OwnerId", ownerId)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the number of pets of a specific species.</summary>
        public int GetPetCountBySpecies(string species)
        {
            string sql = "SELECT COUNT(*) FROM PET WHERE SPECIES = @Species";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Species", species)
            };
            return Convert.ToInt32(dbHandler.ExecuteScalar(sql, parameters));
        }

        /// <summary>Returns the average age of all pets.</summary>
        public double GetAveragePetAge()
        {
            string sql = "SELECT AVG(CAST(AGE AS FLOAT)) FROM PET WHERE AGE IS NOT NULL";
            object result = dbHandler.ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }

        /// <summary>Returns the oldest pet's age.</summary>
        public int GetMaxPetAge()
        {
            string sql = "SELECT MAX(AGE) FROM PET WHERE AGE IS NOT NULL";
            object result = dbHandler.ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        /// <summary>Returns the youngest pet's age.</summary>
        public int GetMinPetAge()
        {
            string sql = "SELECT MIN(AGE) FROM PET WHERE AGE IS NOT NULL";
            object result = dbHandler.ExecuteScalar(sql);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
    }
}