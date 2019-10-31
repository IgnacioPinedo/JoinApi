using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

public class EventController : ApiController
{
    private EventContext EventContext = new EventContext();

    [HttpGet]
    [Route("Events({eventId})")]
    public IHttpActionResult Get(int eventId)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if(auth && eventId > 0)
        {
            Event getEvent;

            getEvent = EventContext.Get(eventId);

            return Ok(getEvent);
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Events")]
    public IHttpActionResult Post(JObject json)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }
        
        if(auth)
        {
            string name = json["Name"]?.ToString();

            if (!Int32.TryParse(json[" "]?.ToString(), out int eventId))
            {
                eventId = 0;
            }

            if (!int.TryParse(json["TypeId"]?.ToString(), out int typeId) ||
                !double.TryParse(json["Longitude"]?.ToString(), out double longitude) ||
                !double.TryParse(json["Latitude"]?.ToString(), out double latitude) ||
                !DateTime.TryParse(json["Date"]?.ToString(), out DateTime date) ||
                string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            else
            {
                User user;

                using (UserContext userContext = new UserContext())
                {
                    if (userContext.Authenticate(userKey))
                    {
                        Event getEvent;
                        user = userContext.Get(userKey);

                        if (eventId == 0)
                        {
                            getEvent = EventContext.Create(typeId, user.Id, name, date, longitude, latitude);
                        }
                        else
                        {
                            getEvent = EventContext.Update(eventId, typeId, user.Id, name, date, longitude, latitude);
                        }

                        if (getEvent != null) return Ok(getEvent);
                    }
                }
            }
        }

        return Ok("Unauthorized");
    }

    [HttpDelete]
    [Route("Events({eventId})")]
    public IHttpActionResult Delete(int eventId)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {

            User user;

            using (UserContext userContext = new UserContext())
            {
                if (userContext.Authenticate(userKey))
                {
                    bool retorno = false;
                    user = userContext.Get(userKey);

                    EventContext.Delete(eventId, user.Id);

                    if (retorno) return Ok(retorno);
                }
            }
        }

        return Ok("Unauthorized");
    }
}