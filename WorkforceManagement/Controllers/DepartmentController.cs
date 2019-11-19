using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WorkforceManagement.Models;

namespace WorkforceManagement.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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
        // GET: Department
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                       SELECT d.Id, d.Name, d.Budget, Count(e.Id) as EmployeeCount
                                       FROM Department d LEFT JOIN Employee e on d.Id = e.DepartmentId
                                       Group By d.Id, d.Name, d.Budget
                                        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Department> departments = new List<Department>();
                    while (reader.Read())
                    {
                        Department department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount"))
                        };

                        departments.Add(department);
                    }

                    reader.Close();

                    return View(departments);
                }
            };
        }

        //GET: Department/Details/5
        public ActionResult Details(int id, string orderby)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (orderby == "firstname") {
                        cmd.CommandText = @"SELECT d.Id as DepId, 
                                                d.Name, 
                                                d.Budget, 
                                                e.Id as EmployeeId, 
                                                e.FirstName,   
                                                e.LastName 
                                        FROM Department d LEFT JOIN Employee e on d.Id = e.DepartmentId
                                        WHERE d.Id = @id
                                        Order By e.FirstName";
                    }
                    else if (orderby == "lastname")
                    {
                        cmd.CommandText = @"SELECT d.Id as DepId, 
                                                d.Name, 
                                                d.Budget, 
                                                e.Id as EmployeeId, 
                                                e.FirstName,   
                                                e.LastName 
                                        FROM Department d LEFT JOIN Employee e on d.Id = e.DepartmentId
                                        WHERE d.Id = @id
                                        Order By e.LastName";
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT d.Id as DepId, 
                                                d.Name, 
                                                d.Budget, 
                                                e.Id as EmployeeId, 
                                                e.FirstName,   
                                                e.LastName 
                                        FROM Department d LEFT JOIN Employee e on d.Id = e.DepartmentId
                                        WHERE d.Id = @id";
                    }
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    var reader = cmd.ExecuteReader();

                    Department department = null;
                    while (reader.Read())
                    {
                        if (department == null)
                        {
                            department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            };
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            var emp = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            };
                            department.Employees.Add(emp);
                        }
                    }
                    reader.Close();
                    return View(department);
                }
            }
        }

        //GET: Department/Create
        public ActionResult Create()
        {
            var department = new Department();
            return View(department);
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department newDepartment)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Department (Name, Budget) Values (@name, @budget)";
                        cmd.Parameters.Add(new SqlParameter("@name", newDepartment.Name));
                        cmd.Parameters.Add(new SqlParameter("@budget", newDepartment.Budget));
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

        // GET: Department/Edit/5
        public ActionResult Edit(int id)
        {
            Department dep = GetDepartmentById(id);
            return View(dep);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department SET Name = @name, Budget = @budget
                                                WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@budget", department.Budget));

                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                department = new Department();
                
                return View(department);
            }
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int id)
        {
            var department = GetDepartmentById(id);
            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            DELETE FROM Department WHERE Id = @id;";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var errMsg = TempData["ErrorMessage"] as string;
                TempData["ErrorMessage"] = "You cannot delete a department that currently has employees!";
                return RedirectToAction(nameof(Delete));
            }
        }


        private Department GetDepartmentById(int id)
        {
            Department department = null;

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                      SELECT d.Id as depId, d.Name, d.Budget, Count(e.Id) as EmployeeCount
                                       FROM Department d LEFT JOIN Employee e on d.Id = e.DepartmentId
                                       WHERE d.Id = @id
                                       Group By d.Id, d.Name, d.Budget";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        department = new Department()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("depId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount")),
                        };
                    }


                    reader.Close();

                    return department;
                }
            }
        }
    }
}