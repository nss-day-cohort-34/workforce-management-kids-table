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
