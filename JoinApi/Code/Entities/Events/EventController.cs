using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

public class EventController : ApiController
{
    private EventContext EventContext = new EventContext();

    [HttpGet]
    [Route("Event")]
    public IHttpActionResult Get()
    {

        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        User user;

        using (UserContext userContext = new UserContext())
        {
            if (userContext.Authenticate(userKey))
            {
                user = userContext.Get(userKey);

                EventContext.Create
            }
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Event")]
    public IHttpActionResult Post()
    {
        return Ok("Unauthorized");
    }

    [HttpDelete]
    [Route("Event")]
    public IHttpActionResult Delete()
    {
        return Ok("Unauthorized");
    }
}