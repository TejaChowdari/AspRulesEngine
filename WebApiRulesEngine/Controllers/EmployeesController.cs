using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RulesEngine.Models;
using RulesEngine.Extensions;
namespace WebApiRulesEngine.Controllers
{
    public class EmployeesController : ApiController
    {
        [HttpGet]
        public IEnumerable<Employee1> Get()
        {
            string cs = ConfigurationManager.ConnectionStrings["TejaDB"].ConnectionString;
            try
            {
                List<Employee1> employees = new List<Employee1>();
                string Qry = @" select * from Employee";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand(Qry, con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable table = new DataTable();
                        da.Fill(table);
                        foreach (DataRow c in table.Rows)
                        {
                            employees.Add(new Employee1
                            {
                                ID = Convert.ToInt32(c["ID"]),
                                First_Name = c["First_Name"].ToString(),
                                Last_Name = c["Last_Name"].ToString(),
                                Gender = c["Gender"].ToString(),
                                Sal = Convert.ToInt32(c["Sal"])
                            });
                        }
                        return employees;
                    }
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        /*
        [HttpGet]
        public string GetId(int id, string employees)
        {
            string cs = ConfigurationManager.ConnectionStrings["TejaDB"].ConnectionString;
            try
            {
                List<Employee1> employees = new List<Employee1>();
                string Qry = @" select * from Employee";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand(Qry, con))
                    {
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable table = new DataTable();
                        da.Fill(table);
                        foreach (DataRow c in table.Rows)
                        {
                            employees.Add(new Employee1
                            {
                                ID = Convert.ToInt32(c["ID"]),
                                First_Name = c["First_Name"].ToString(),
                                Last_Name = c["Last_Name"].ToString(),
                                Gender = c["Gender"].ToString(),
                                Sal = Convert.ToInt32(c["Sal"])
                            });
                        }
                        return employees;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }*/
        [HttpPost]
        public HttpResponseMessage Post(Employee1 Emp)
        {
            string cs = ConfigurationManager.ConnectionStrings["TejaDB"].ConnectionString;
            try
            {
                string Qry = @"insert into Employee (ID,First_Name,Last_Name,Gender,Sal)values(@ID,@First_Name,@Last_Name,@Gender,@Sal) ";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(Qry, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", Emp.ID);
                        cmd.Parameters.AddWithValue("@First_Name", Emp.First_Name);
                        cmd.Parameters.AddWithValue("@Last_Name", Emp.Last_Name);
                        cmd.Parameters.AddWithValue("@Gender", Emp.Gender);
                        cmd.Parameters.AddWithValue("@Sal", Emp.Sal);
                        int a = cmd.ExecuteNonQuery();
                        var files = Directory.GetFiles(@"D:\Practice\WebApiRulesEngine\WebApiRulesEngine\Models","EmpCheck.json", SearchOption.AllDirectories);
                        if (files == null || files.Length == 0)
                        {
                            throw new Exception("Rules not found.");
                        }
                        var fileData = File.ReadAllText(files[0]);
                        var Workflows = JsonConvert.DeserializeObject<List<Workflow>>(fileData);
                        var bre = new RulesEngine.RulesEngine(Workflows.ToArray(), null);
                        // Console.WriteLine(bre);
                        foreach (var workflow in Workflows)
                        {
                            var resultList = bre.ExecuteAllRulesAsync(workflow.WorkflowName, Qry.ToArray()).Result;
                            //bre.ExecuteAllRulesAsync(workflow,ls.ToArray())
                            resultList.OnSuccess((eventname) =>
                            {
                                Console.WriteLine($"{workflow.WorkflowName} Evaluation was SUCCESS - {eventname}");
                            }).OnFail(() =>
                            {
                                Console.WriteLine($"{workflow.WorkflowName} Evaluation was FAILED");
                            });
                        }
                        if (a == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, a); ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}