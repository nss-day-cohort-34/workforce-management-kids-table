using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WorkforceManagement.Models;
using WorkforceManagement.Models.ViewModels;

namespace WorkforceManagement.Controllers
{
    public class ComputerController : Controller
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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
        // GET: Computer
        public ActionResult Index(string searchString)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        cmd.CommandText = @"SELECT c.Id as 'ComputerId', c.Make, c.Manufacturer, 
                         e.id as 'EmployeeId', e.FirstName, e.LastName, ce.AssignDate, ce.UnassignDate
                        FROM Computer c LEFT JOIN ComputerEmployee ce ON CE.ComputerId = c.Id
                         LEFT JOIN Employee e ON ce.employeeId = e.Id WHERE Make LIKE @searchString OR Manufacturer LIKE @searchString";
                        cmd.Parameters.Add(new SqlParameter("@searchString", searchString));
                    }
                    else
                    {
                        cmd.CommandText = @"SELECT c.Id as 'ComputerId', c.Make, c.Manufacturer, 
                         e.id as 'EmployeeId', e.FirstName, e.LastName, ce.AssignDate, ce.UnassignDate
                        FROM Computer c LEFT JOIN ComputerEmployee ce ON CE.ComputerId = c.Id
                         LEFT JOIN Employee e ON ce.employeeId=e.Id";
                    }
                        SqlDataReader reader = cmd.ExecuteReader();
                        List<Computer> computers = new List<Computer>();
                        Computer computer = null;
                        Employee employee = null;
                    

                        while (reader.Read())
                        {
                            computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                            };


                            computers.Add(computer);
                            if (reader.IsDBNull(reader.GetOrdinal("UnassignDate")) && !reader.IsDBNull(reader.GetOrdinal("assignDate")))
                            {
                                employee = new Employee
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName"))
                                };
                                computer.employee = employee;
                            }
                        }
                        reader.Close();
                        return View(computers);
                    }
                }
            }
        


        // GET: Computer/Details/5
        public ActionResult Details(int id)
        {
            Computer computer = GetSingleComputer(id);
            return View(computer);
        }

        // GET: Computer/Create
        public ActionResult Create()
        {
            var viewModel = new ComputerCreateViewModel();
            var employees = GetEmployees();
           var items = employees
                .Select(employee => new SelectListItem
                 {
                     Text = $"{employee.FirstName } { employee.LastName}",
                     Value = employee.Id.ToString()
                 })
                .ToList();

            items.Insert(0, new SelectListItem
            {
                Text = "Assign employee",
                Value = "0"
            });
            viewModel.Employees = items;
            return View(viewModel);
        }

        // POST: Computer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ComputerCreateViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer 
                           (Make, Manufacturer, PurchaseDate)
                                              OUTPUT INSERTED.Id
                                                VALUES (@make, @manufacturer, @purchaseDate)";
                        cmd.Parameters.Add(new SqlParameter("@make", viewModel.computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@manufacturer", viewModel.computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", viewModel.computer.PurchaseDate));
                        int i = 0;
                        object a = cmd.ExecuteScalar();
                        if (a != null)
                            i = (int)a;
                        //int compId = (int?)await cmd.ExecuteScalarAsync();
                        viewModel.computer.Id = i;


                     if (viewModel.employeeId != 0)
                        {
                            cmd.CommandText = @"INSERT INTO ComputerEmployee (EmployeeId, ComputerId, AssignDate, UnassignDate)
                                                OUTPUT INSERTED.Id
                                                VALUES (@employeeId, @computerId, @assignDate, null)";
                            DateTime currentDate = DateTime.Now;
                            cmd.Parameters.Add(new SqlParameter("@employeeId", viewModel.employeeId));
                            cmd.Parameters.Add(new SqlParameter("@computerId", i));
                            cmd.Parameters.Add(new SqlParameter("@assignDate", currentDate));


                            int newCEId = (int)cmd.ExecuteScalar();

                            cmd.CommandText = @"UPDATE ComputerEmployee SET UnassignDate = @unassignDate WHERE employeeID = @employeeId AND computerId != @computerId";

                            cmd.Parameters.Add(new SqlParameter("@unassignDate", currentDate));

                            cmd.ExecuteScalar();
                        }
                       

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Computer/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Computer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Computer/Delete/5
        public ActionResult Delete(int id)
        {
            Computer ChosenComputer = GetSingleComputer(id);
            return View(ChosenComputer);
        }

        // POST: Computer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Computer computer)
        {
            try
            {
                var errMsg = TempData["ErrorMessage"] as string;

                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE Computer
                                            FROM Computer c
                                            LEFT JOIN ComputerEmployee ce
                                            ON c.Id = ce.ComputerId
                                            WHERE ce.ComputerId IS NULL
                                            AND c.Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();


                        if (rowsAffected > 0)
                        {
                            TempData["ErrorMessage"] = "This computer cannot be deleted because it is currently or previously assigned to an employee";
                        }

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }
        private List<Employee> GetEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName FROM Employee";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))
                        });
                    }

                    reader.Close();

                    return employees;
                } } } 
        private Computer GetSingleComputer(int id)
        {
            Computer computer = null;

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                      SELECT Id, Make, Manufacturer, PurchaseDate, DecomissionDate FROM Computer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        computer = new Computer()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecommissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecommissionDate = null;

                        }




                    }


                    reader.Close();

                    return computer;
                }
            }
        }
    }
}
