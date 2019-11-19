using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
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
                                            ( @firstName, @lastName, @isSupervisor, @departmentId )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.Employee.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.Employee.LastName));
                        cmd.Parameters.Add(new SqlParameter("@isSupervisor", viewModel.Employee.IsSupervisor));
                        cmd.Parameters.Add(new SqlParameter("@departmentId", viewModel.Employee.DepartmentId));
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
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
            var employee = GetEmployeeById(id);
            var viewModel = new EmployeeEditViewModel()
            {
                Employee = employee,
                Departments = GetAllDepartments(),
            };
            if (employee.Computer != null)
            {
                viewModel.SelectedComputerId = employee.Computer.Id;
                viewModel.Computers = GetUnassignedComputersAndCurrentEmployeeComputer(id, employee.Computer.Id);
            }
            else
            {
                viewModel.Computers = GetUnassignedComputersAndCurrentEmployeeComputer(id, null);
            }
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

                        var employee = GetEmployeeById(id);
                        if (viewModel.SelectedComputerId.HasValue)
                        {
                            if (employee.Computer == null)
                            {
                                cmd.CommandText = @"INSERT INTO ComputerEmployee
                                                    ( ComputerId, EmployeeId, AssignDate )
                                                    VALUES ( @computerId, @employeeId, GETDATE() )";
                                cmd.Parameters.Add(new SqlParameter("@computerId", viewModel.SelectedComputerId));
                                cmd.Parameters.Add(new SqlParameter("@employeeId", id));
                                cmd.ExecuteNonQuery();
                            }
                            else if (employee.Computer.Id != viewModel.SelectedComputerId)
                            {
                                cmd.CommandText = @"UPDATE ComputerEmployee
                                                    SET UnassignDate = GETDATE()
                                                    WHERE ComputerId = @oldComputerId AND EmployeeId = @employeeId;
                                                    INSERT INTO ComputerEmployee
                                                    ( ComputerId, EmployeeId, AssignDate )
                                                    VALUES ( @computerId, @employeeId, GETDATE() )";
                                cmd.Parameters.Add(new SqlParameter("@oldComputerId", employee.Computer.Id));
                                cmd.Parameters.Add(new SqlParameter("@computerId", viewModel.SelectedComputerId));
                                cmd.Parameters.Add(new SqlParameter("@employeeId", id));
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult UnassignComputer(int employeeId, int computerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE ComputerEmployee
                                        SET UnassignDate = GETDATE()
                                        WHERE ComputerId = @computerId AND EmployeeId = @employeeId AND UnassignDate IS NULL
                                        ";
                    cmd.Parameters.Add(new SqlParameter("@computerId", computerId));
                    cmd.Parameters.Add(new SqlParameter("@employeeId", employeeId));
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction(nameof(Details), new { id = employeeId });
            }
        }

        // GET: Employee/AssignTrainingProgram/5
        public ActionResult AssignTrainingProgram(int id)
        {
            var employee = GetEmployeeById(id);
            var viewModel = new EmployeeAssignTrainingProgramViewModel()
            {
                Employee = employee,
                AllTrainingPrograms = GetAllFutureTrainingPrograms(id),
                SelectedTrainingProgramIds = employee.TrainingPrograms.Select(t => t.Id).ToList()
            };
            return View(viewModel);
        }
        // POST: Employee/AssignTrainingProgram/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTrainingProgram(int id, EmployeeAssignTrainingProgramViewModel viewModel)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM EmployeeTraining WHERE EmployeeId = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"INSERT INTO EmployeeTraining
                                            ( EmployeeId, TrainingProgramId )
                                            VALUES
                                            ( @employeeId, @trainingProgramId )
                                        ";
                    foreach (var trainingProgramId in viewModel.SelectedTrainingProgramIds)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqlParameter("@employeeId", id));
                        cmd.Parameters.Add(new SqlParameter("@trainingProgramId", trainingProgramId));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return RedirectToAction(nameof(Details), new { id });
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
                        if (!reader.IsDBNull(reader.GetOrdinal("TrainingProgramId")) && !anEmployee.TrainingPrograms.Any(tp => tp.Id == reader.GetInt32(reader.GetOrdinal("TrainingProgramId"))))
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
        private List<TrainingProgram> GetAllFutureTrainingPrograms(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT t.Id, t.Name, COUNT(et.EmployeeId)
                                            FROM TrainingProgram t
                                             LEFT JOIN EmployeeTraining et ON et.TrainingProgramId = t.Id
                                            WHERE StartDate > GETDATE()
                                            GROUP BY t.Id, t.Name, t.MaxAttendees
                                            HAVING COUNT(et.EmployeeId) < t.MaxAttendees";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();
                    while (reader.Read())
                    {
                        trainingPrograms.Add(new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                      
                        });
                    }

                    reader.Close();

                    var employee = GetEmployeeById(id);

                    foreach(var etp in employee.TrainingPrograms)
                    {
                        if (!trainingPrograms.Any(tp => tp.Id == etp.Id))
                        {
                            trainingPrograms.Add(etp);
                        }
                    }

                    return trainingPrograms;
                }
            }
        }
        private List<Computer> GetUnassignedComputersAndCurrentEmployeeComputer(int employeeId, int? computerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.Id AS 'ComputerId', c.PurchaseDate, c.DecomissionDate, c.Make, c.Manufacturer
                                            FROM Computer c
                                            LEFT JOIN ComputerEmployee ce ON ce.ComputerId = c.Id
                                            WHERE ce.id IS NULL
	                                            OR c.id IN (
		                                            SELECT ce.ComputerId
		                                            FROM ComputerEmployee ce
		                                            WHERE ce.UnassignDate IS NOT NULL AND c.DecomissionDate IS NULL
				                                            AND ce.ComputerId NOT IN (
					                                            SELECT ce.ComputerId
					                                            FROM ComputerEmployee ce
					                                            WHERE ce.UnassignDate IS NULL
				                                            )
		                                            )
                                        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    Computer computer = null;

                    while (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };
                        computers.Add(computer);
                    }
                    reader.Close();


                    if (computerId.HasValue)
                    {
                        cmd.CommandText = @"SELECT c.Id AS ComputerId, c.Make, c.Manufacturer
                                            FROM ComputerEmployee ce
                                            LEFT JOIN Computer c ON ce.ComputerId = c.Id
                                            WHERE ce.ComputerId = @computerId AND ce.EmployeeId = @employeeId AND ce.UnassignDate IS NULL";
                        cmd.Parameters.Add(new SqlParameter("@computerId", computerId));
                        cmd.Parameters.Add(new SqlParameter("@employeeId", employeeId));
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };
                            computers.Add(computer);
                        }
                    }
                    reader.Close();

                    return computers;
                }
            }
        }
    }
}
