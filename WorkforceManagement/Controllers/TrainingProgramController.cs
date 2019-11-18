using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WorkforceManagement.Models;
using WorkforceManagement.Models.ViewModels;

namespace WorkforceManagement.Controllers
{
    public class TrainingProgramController : Controller
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: TrainingProgram
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name, StartDate, EndDate, MaxAttendees
                                        FROM TrainingProgram
                                        WHERE StartDate > GETDATE()";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            // build out training program
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        };

                        trainingPrograms.Add(trainingProgram);
                    }
                    reader.Close();

                    return View(trainingPrograms);
                }
            }
        }

        // GET: TrainingProgram/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.Id, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees, 
                                                e.Id as 'EmployeeId', e.FirstName, e.LastName, e.DepartmentId, 
                                                d.Name as 'DeptName'
                                        FROM TrainingProgram tp LEFT JOIN EmployeeTraining et ON tp.Id = et.TrainingProgramId
						                LEFT JOIN Employee e on et.EmployeeId = e.Id
						                LEFT JOIN Department d on e.DepartmentId = d.Id
                                        WHERE tp.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    TrainingProgram trainingProgram = null;

                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            int employeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                            if (!trainingProgram.Employees.Any(e => e.Id == employeeId))
                            {
                                Employee employee = new Employee
                                {
                                    Id = employeeId,
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                                };

                                trainingProgram.Employees.Add(employee);
                            }
                        }
                        
                    }

                    reader.Close();
                    return View(trainingProgram);
                }
            }
        }

        // GET: TrainingProgram/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TrainingProgram/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TrainingProgramCreateView newTrainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)                       VALUES (@name, @startDate, @endDate, @maxAttendees)";
                        cmd.Parameters.Add(new SqlParameter("@name", newTrainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@startDate", newTrainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@endDate", newTrainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@maxAttendees", newTrainingProgram.MaxAttendees));
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TrainingProgram/Edit/5
        public ActionResult Edit(int id)
        {
            // Get the training program
            TrainingProgram trainingProgram = GetTrainingProgram(id);
            DateTime currentDate = DateTime.Now;

            // Check if training program start date is in the future. If it is, instatiate the EditView so it can be edited, otherwise take the user back to the index view 
            if (trainingProgram.StartDate > currentDate)
            {
                TrainingProgramEditView viewModel = new TrainingProgramEditView
                {
                    TrainingProgram = trainingProgram
                };
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: TrainingProgram/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, TrainingProgramEditView viewModel)
        {
            TrainingProgram trainingProgram = viewModel.TrainingProgram;
            try
            {
                var updatedStudent = viewModel.Student;
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                    }
                        
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TrainingProgram/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TrainingProgram/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private TrainingProgram GetTrainingProgram(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT tp.Id, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees                               FROM TrainingProgram tp
                                        WHERE tp.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    TrainingProgram trainingProgram = null;

                    if (reader.Read())
                    {
                        trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        };
                    }

                    reader.Close();
                    return trainingProgram;
                }
            }
        }
    }
}