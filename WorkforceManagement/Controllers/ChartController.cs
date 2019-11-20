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
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ChartController(IConfiguration config)
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

        // GET: api/Chart
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Chart/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<IActionResult> Get(int id)
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
	                                        COUNT(et.TrainingProgramId) as 'NumTrainingPrograms'
                                        FROM Employee e
                                        LEFT JOIN Department d on e.DepartmentId = d.Id
                                        LEFT JOIN EmployeeTraining et on et.EmployeeId = e.Id
                                        LEFT JOIN TrainingProgram t on et.TrainingProgramId = t.Id
                                        WHERE e.DepartmentId = @id
                                        GROUP BY e.Id, e.FirstName, e.LastName";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Employee> employees = new List<Employee>();

                    Employee anEmployee = null;
                    while (reader.Read())
                    {
                        anEmployee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            TotalTrainingPrograms = reader.GetInt32(reader.GetOrdinal("NumTrainingPrograms"))
                        };

                        employees.Add(anEmployee);
                        
                    }

                    reader.Close();

                    return Ok(employees);
                }
            }
        }

        // POST: api/Chart
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Chart/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
