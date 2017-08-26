using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Antonyproject.Models
{
    public class UserProfileModel
    {
        
        public static int GetUserPoints()
        {
            string email = HttpContext.Current.User.Identity.GetUserName();

            const string constr = @"data source=(LocalDB)\MSSQLLocalDB;attachdbfilename=|DataDirectory|\MyDatabase.mdf;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";

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

                return points;
            }
        }
    }
}