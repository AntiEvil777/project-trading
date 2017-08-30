using Antonyproject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Antonyproject.Controllers
{
    public class PointsController : ApiController
    {
        // GET api/<controller>
        public int Get()
        {
            return UserProfileModel.GetUserPoints(); 
        }

    }
}