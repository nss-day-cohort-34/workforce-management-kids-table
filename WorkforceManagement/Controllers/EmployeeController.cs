using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using WorkforceManagement.Models;
using WorkforceManagement.Models.ViewModels;

namespace WorkforceManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
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
        // GET: Employee
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT e.Id,
                                            e.FirstName,
                                            e.LastName,
                                            e.IsSupervisor,
                                            e.DepartmentId,
                                            d.Name AS DepartmentName
                                        FROM Employee e
                                        LEFT JOIN Department d on e.DepartmentId = d.Id
                                      ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                            Department = new Department()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                            }
                        };

                        employees.Add(employee);
                    }

                    reader.Close();

                    return View(employees);
                }
            }
        }

        // GET: Employee/Details/5
        public ActionResult Details(int id)
        {
            Employee anEmployee = GetEmployeeById(id);
            return View(anEmployee);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            var viewModel = new EmployeeCreateViewModel()
            {
                Departments = GetAllDepartments()
            };

            return View(viewModel);
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Employee
                                            ( FirstName, LastName, IsSupervisor, DepartmentId )
                                            VALUES
                                            ( @firstName, @lastName, @departmentId, @isSupervisor )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.Employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.Employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", viewModel.Employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", viewModel.Employee.DepartmentId));
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

        // GET: Employee/Edit/5
        public ActionResult Edit(int id)
        {
            var viewModel = new EmployeeEditViewModel()
            {
                Employee = GetEmployeeById(id),
                Departments = GetAllDepartments()
            };
            return View(viewModel);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EmployeeEditViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE Employee
                                            SET FirstName = @firstName, LastName = @lastName, 
                                            IsSupervisor = @isSupervisor, DepartmentId = @departmentId
                                            WHERE Id = @id;
                                           ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.Employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.Employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", viewModel.Employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", viewModel.Employee.DepartmentId));
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

        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Employee/Delete/5
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
        private Employee GetEmployeeById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT e.Id AS EmployeeId,
                                            e.FirstName,
                                            e.LastName,
                                            e.IsSupervisor,
                                            e.DepartmentId,
                                            d.Name AS DepartmentName,
	                                        ce.ComputerId, c.Make, c.Manufacturer, c.PurchaseDate,
	                                        ce.AssignDate, ce.UnassignDate,
	                                        et.TrainingProgramId, t.Name AS TrainingProgramName,
	                                        t.StartDate, t.EndDate
                                        FROM Employee e
                                        LEFT JOIN Department d on e.DepartmentId = d.Id
                                        LEFT JOIN ComputerEmployee ce on ce.EmployeeId = e.Id
                                        LEFT JOIN Computer c on c.Id = ce.ComputerId
                                        LEFT JOIN EmployeeTraining et on et.EmployeeId = e.Id
                                        LEFT JOIN TrainingProgram t on et.TrainingProgramId = t.Id
                                        WHERE e.Id = @id
                                      ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Employee anEmployee = null;
                    Computer aComputer = null;
                    while (reader.Read())
                    {
                        if (anEmployee == null)
                        {
                            anEmployee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Department = new Department()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                                }
                            };
                        }
                        if (aComputer == null && !reader.IsDBNull(reader.GetOrdinal("AssignDate")) && reader.IsDBNull(reader.GetOrdinal("UnassignDate")))
                        {
                            aComputer = new Computer()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate"))
                            };
                            anEmployee.Computer = aComputer;
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")))
                        {
                            TrainingProgram aTrainingProgram = new TrainingProgram()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramId")),
                                Name = reader.GetString(reader.GetOrdinal("TrainingProgramName")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
                            };
                            anEmployee.TrainingPrograms.Add(aTrainingProgram);
                        }
                    }

                    reader.Close();

                    return anEmployee;
                }
            }
        }
        private List<Department> GetAllDepartments()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name FROM Department";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                        });
                    }

                    reader.Close();

                    return departments;
                }
            }
        }
    }
}