using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

public class SettingsController : ApiController
{
    [HttpGet]
    [Route("On")]
    public IHttpActionResult Get()
    {
        return Ok(new
        {
            success = true,
            message = "The Api is working properly!"
        });
    }
}