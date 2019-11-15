using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WorkforceManagement.Models;

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
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Make, Manufacturer FROM Computer";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Computer> computers = new List<Computer>();
                    Computer computer = null;

                    while (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Manufacturer = reader.GetString(reader.GetOrdinal("Manufacturer"))
                        };


                        computers.Add(computer);
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
            return View(new Computer());
        }

        // POST: Computer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Computer 
                           (Make, Manufacturer, PurchaseDate, DecomissionDate)
                                              
                                                VALUES (@make, @manufacturer, @purchaseDate, null)";
                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@manufacturer", computer.Manufacturer));
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));

                        cmd.ExecuteNonQuery();
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
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT c.id AS 'Computer ID', e.id AS 'employeeId' FROM ComputerEmployee ce JOIN Employee e ON e.Id = ce.EmployeeId JOIN Computer c ON c.id = ce.ComputerId WHERE c.id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (!reader.Read())
                        {

                            cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return RedirectToAction(nameof(Index));
                            }
                            reader.Close();
                            throw new Exception("No rows affected");
                         
                        }
                        else
                        {
                            throw new Exception("This computer has a user");
                        }

                    }
                }
            }



            catch
            {
                return View();
            }
        }
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
