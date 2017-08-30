using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antonyproject;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using Antonyproject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Antonyproject.Controllers
{
    public class UploadedFilesController : Controller
    {
        private MyDatabaseEntities db = new MyDatabaseEntities();
        //private UserLoginInfo user;

        private string GetEmail()
        {
            return HttpContext.GetOwinContext().Authentication.User.Identity.GetUserName();

        }

        // GET: Home
        [Authorize]
        public ActionResult Index()
        {
            return View(GetFiles(GetEmail()));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile)
        {
            var email = GetEmail();
            byte[] bytes;
            using (BinaryReader br = new BinaryReader(postedFile.InputStream))
            {
                bytes = br.ReadBytes(postedFile.ContentLength);
            }
            const string constr = @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "INSERT INTO UploadedFile(FileName,ContentType, FileContent, FileSize, FileExtension, UserName) VALUES (@Name, @ContentType, @Data, @Size, @Extension, @UserName)";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Name", Path.GetFileName(postedFile.FileName));
                    cmd.Parameters.AddWithValue("@ContentType", postedFile.ContentType);
                    cmd.Parameters.AddWithValue("@Data", bytes);
                    cmd.Parameters.AddWithValue("@Size", postedFile.ContentLength);
                    cmd.Parameters.AddWithValue("@Extension", Path.GetExtension(postedFile.FileName));
                    cmd.Parameters.AddWithValue("@UserName", email);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

            }
            using (SqlConnection con = new SqlConnection(constr))
            {
                //Insert a new record into UserProfile table
                //Add Points to the current user.

                var query = "SELECT Points FROM UserProfile WHERE Email = @Email";
                int points = 0;

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();
                    points = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }

                points += 20;

                query = @"IF EXISTS(SELECT 1 FROM UserProfile WHERE Email = @Email)
                            BEGIN
	                            UPDATE UserProfile SET Points = @Points WHERE Email = @Email;
                            END
                            ELSE
                            BEGIN
	                            INSERT INTO UserProfile(Email, Points) VALUES (@Email, @Points);
                            END";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Points", points);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

            }

            return View(GetFiles(email));
        }

        [HttpPost]
        public FileResult DownloadFile(int? fileId)
        {
            byte[] bytes;
            string fileName, contentType;
            string constr = @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT FileName, FileContent, ContentType FROM UploadedFile WHERE FileId=@Id";
                    cmd.Parameters.AddWithValue("@Id", fileId);
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        sdr.Read();
                        bytes = (byte[])sdr["FileContent"];
                        contentType = sdr["ContentType"].ToString();
                        fileName = sdr["FileName"].ToString();
                    }
                    con.Close();
                }
            }

            return File(bytes, contentType, fileName);
        }

        [HttpGet]
        public void DeleteFile(int id)
        {
            var email = GetEmail();

            string constr = @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "DELETE FROM UploadedFile WHERE FileId=@Id";
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();

                    con.Close();
                }

                var query = "UPDATE UserProfile SET Points = @Points WHERE Email = @Email";

                int points = UserProfileModel.GetUserPoints();
                points -= 20;

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Points", points);
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();
                    points = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }

            }

            Response.Redirect("~/UploadedFiles/Index");
        }

        private static List<UploadedFilesModels> GetFiles(String email)
        {
            List<UploadedFilesModels> files = new List<UploadedFilesModels>();
            string constr = NewMethod();
            
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT FileId, FileName FROM UploadedFile where UserName= @Email"))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            files.Add(new UploadedFilesModels
                            {
                                FileId = Convert.ToInt32(sdr["FileId"]),
                                FileName = sdr["FileName"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return files;
        }

        private static string NewMethod()
        {
            return @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        }

        private static List<UploadedFilesModels> AllFiles(string Email)
        {
            List<UploadedFilesModels> files = new List<UploadedFilesModels>();
            string constr = NewMethod();

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT FileId, FileName FROM UploadedFile where UserName != @Email"))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Email", Email);
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            files.Add(new UploadedFilesModels
                            {
                                FileId = Convert.ToInt32(sdr["FileId"]),
                                FileName = sdr["FileName"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }
            return files;
        }

        [HttpGet]
        [Authorize]
        public ActionResult OtherFiles()
        {
            return View(AllFiles(GetEmail()));
        }

        [HttpGet]
        [Authorize]
        public ActionResult DownloadSomeonesFile(int Id)
        {
            
            int points = UserProfileModel.GetUserPoints();
            points -= 20;
            if (points < 0)
            {
                return View("Error");
            }


            byte[] bytes;
            string fileName, contentType;
            string constr = @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "SELECT FileName, FileContent, ContentType FROM UploadedFile Where FileId = @Id";
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        sdr.Read();
                        bytes = (byte[])sdr["FileContent"];
                        contentType = sdr["ContentType"].ToString();
                        fileName = sdr["FileName"].ToString();
                    }
                    con.Close();
                }

                var query = "UPDATE UserProfile SET Points = @Points WHERE Email = @Email";

               

                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@Points", points);
                    cmd.Parameters.AddWithValue("@Email", GetEmail());
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            return File(bytes, contentType, fileName);
        }

        public ActionResult Error()
        {
            return View();
        }





    }

}

