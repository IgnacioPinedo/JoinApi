using Newtonsoft.Json;
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
            string address = json["Address"]?.ToString();
            string description = json["Description"]?.ToString();

            if (!int.TryParse(json["EventId"]?.ToString(), out int eventId))
            {
                eventId = 0;
            }

            if (!int.TryParse(json["TypeId"]?.ToString(), out int typeId) ||
                !double.TryParse(json["Longitude"]?.ToString(), out double longitude) ||
                !double.TryParse(json["Latitude"]?.ToString(), out double latitude) ||
                !DateTime.TryParse(json["Date"]?.ToString(), out DateTime date) ||
                string.IsNullOrEmpty(name) &&
                string.IsNullOrEmpty(address) &&
                string.IsNullOrEmpty(description))
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
                        getEvent = EventContext.Create(typeId, user.Id, name, description, address, date, longitude, latitude);
                    }
                    else
                    {
                        getEvent = EventContext.Update(eventId, typeId, user.Id, name, description, address, date, longitude, latitude);
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
            User user = new UserContext().Get(userKey);

            return Ok(EventContext.Participate(user.Id, eventId));
        }

        return Ok("Unauthorized");
    }

    [HttpGet]
    [Route("Events/Participants({eventId})")]
    public IHttpActionResult Participants(int eventId)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth && eventId > 0)
        {
            return Ok(EventContext.Participants(eventId));
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Events/Leave({eventId})")]
    public IHttpActionResult Leave(int eventId)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth && eventId > 0)
        {
            User user = new UserContext().Get(userKey);

            return Ok(EventContext.Leave(user.Id, eventId));
        }

        return Ok("Unauthorized");
    }


    [HttpGet]
    [Route("Events/ToHappen")]
    public IHttpActionResult ToHappen()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            User user = new UserContext().Get(userKey);

            var events = EventContext.ToHappen(user.Id);

            if (events != null) return Ok(events);
            else return BadRequest();
        }

        return Ok("Unauthorized");
    }

    [HttpGet]
    [Route("Events/HasHappen")]
    public IHttpActionResult HasHappen()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            User user = new UserContext().Get(userKey);

            var events = EventContext.HasHappen(user.Id);

            if (events != null) return Ok(events);
            else return BadRequest();
        }

        return Ok("Unauthorized");
    }

    [HttpGet]
    [Route("Events/Own")]
    public IHttpActionResult Own()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            User user = new UserContext().Get(userKey);

            var events = EventContext.Own(user.Id);

            if (events != null) return Ok(events);
            else return BadRequest();
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Events/Filter")]
    public IHttpActionResult Filter(JObject json)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            string name = json["Name"]?.ToString();
            Location location = JsonConvert.DeserializeObject<Location>(json["Location"]?.ToString());
            List<int> types = JsonConvert.DeserializeObject<List<int>>(json["Types"] != null ? json["Types"].ToString() : "");
            List<DateTime> dates = JsonConvert.DeserializeObject<List<DateTime>>(json["Dates"] != null ? json["Dates"].ToString() : "");
            int radius;

            int.TryParse(json["Radius"]?.ToString(), out radius);

            radius = radius == 0 ? 2000 : radius;


            if (location == null)
            {
                return BadRequest();
            }
            else
            {
                var eventsFiltered = EventContext.GetFiltered(location, radius, name, types, dates);

                return Ok(eventsFiltered);
            }
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