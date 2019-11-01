using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

public class EventController : ApiController
{
    private EventContext EventContext = new EventContext();

    #region Events CRUD

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
            Event getEvent = EventContext.Get(eventId);

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

            if (!int.TryParse(json["EventId"]?.ToString(), out int eventId))
            {
                eventId = 0;
            }

            if (!int.TryParse(json["TypeId"]?.ToString(), out int typeId) ||
                !double.TryParse(json["Longitude"]?.ToString(), out double longitude) ||
                !double.TryParse(json["Latitude"]?.ToString(), out double latitude) ||
                !DateTime.TryParse(json["Date"]?.ToString(), out DateTime date) ||
                !string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            else
            {
                User user;

                using (UserContext userContext = new UserContext())
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
                bool retorno = false;
                user = userContext.Get(userKey);
                
                EventContext.Delete(eventId, user.Id);

                if (retorno) return Ok(retorno);
                else return BadRequest();
            }
        }

        return Ok("Unauthorized");
    }

    #endregion

    #region Events

    [HttpPost]
    [Route("Events/Participate({eventId})")]
    public IHttpActionResult Participate(int eventId)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth && eventId > 0)
        {

        }

        return Ok("Unauthorized");
    }


    #endregion

    #region Types

    [HttpGet]
    [Route("Events/Types")]
    public IHttpActionResult GetTypes()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            List<EventType> eventTypes = EventContext.GetTypes();

            return Ok(eventTypes);
        }

        return Ok("Unauthorized");
    }

    #endregion
}