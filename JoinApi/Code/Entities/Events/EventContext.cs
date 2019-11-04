using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer;
using System.Device.Location;
using System.Linq;
using System.Spatial;
using System.Web;

public class EventContext : DbContext
{
    private readonly int DefaultSRID = 4326;
    public EventContext() : base("name=JoinDB") { }
    public DbSet<Event> Event { get; set; }
    public DbSet<EventType> EventType { get; set; }
    public DbSet<UserEvent> UserEvent { get; set; }

    #region Events 

    public Event Get(int eventId)
    {
        return Event.Where(s => s.Id == eventId).FirstOrDefault();
    }

    public Event Create(int typeId, int admId, string name, string description, DateTime date, double longitude, double latitude)
    {
        if(EventType.Where(s => s.Id == typeId).Count() > 0)
        {

            Event newEvent = new Event()
            {
                TypeId = typeId,
                AdministratorId = admId,
                Name = name,
                Description = description,
                Date = date,
                Latitude = latitude,
                Longitude = longitude
            };

            Event.Add(newEvent);

            SaveChanges();

            return newEvent;
        }

        return null;
    }

    public Event Update(int eventId, int typeId, int admId, string name, string description, DateTime date, double longitude, double latitude)
    {
        Event updateEvent = Event.Where(s => s.Id == eventId && s.AdministratorId == admId).FirstOrDefault();
        if (updateEvent != null)
        {
            updateEvent.Date = date != null ? date : updateEvent.Date;
            updateEvent.Latitude = (latitude != 0) ? latitude : updateEvent.Latitude;
            updateEvent.Longitude = (longitude != 0) ? longitude : updateEvent.Longitude;
            updateEvent.Name = !string.IsNullOrEmpty(name) ? name : updateEvent.Name;
            updateEvent.Description = !string.IsNullOrEmpty(description) ? description : updateEvent.Description;
            updateEvent.TypeId = typeId != 0 && EventType.Where(s => s.Id == typeId).Count() > 0 ? typeId : updateEvent.TypeId;

            SaveChanges();

            return updateEvent;
        }
        else
            return null;
    }

    public bool Delete(int eventid, int userId)
    {
        Event deleteEvent = Event.Where(s => s.Id == eventid && s.AdministratorId == userId).FirstOrDefault();

        if(deleteEvent != null)
        {
            RemoveEventParticipants(eventid);

            Event.Remove(deleteEvent);

            SaveChanges();

            return true;
        }
        return false;
    }
    

    #endregion

    #region Events
    public bool Participate(int userId, int eventId)
    {
        if(Event.Where(s => s.Id == eventId).Count() > 0 && 
           Event.Where(s => s.AdministratorId == userId && s.Id == eventId).Count() == 0 &&
           UserEvent.Where(s => s.UserId == userId && s.EventId == eventId).Count() == 0)
        {
            UserEvent newUserEvent = new UserEvent
            {
                UserId = userId,
                EventId = eventId
            };

            UserEvent.Add(newUserEvent);

            SaveChanges();

            return true;
        }
        return false;
    }

    public List<int> Participants(int eventId)
    {
        List<int> participants = UserEvent.Where(w => w.EventId == eventId).Select(s => s.UserId).ToList();

        return participants;
    }

    public bool Leave(int userId, int eventId)
    {
        if (Event.Where(s => s.Id == eventId).Count() > 0 &&
           Event.Where(s => s.AdministratorId == userId && s.Id == eventId).Count() == 0 &&
           UserEvent.Where(s => s.UserId == userId && s.EventId == eventId).Count() > 0)
        {
            UserEvent deletedUserEvent = UserEvent.Where(s => s.EventId == eventId && s.UserId == userId).FirstOrDefault();

            UserEvent.Remove(deletedUserEvent);

            SaveChanges();

            return true;
        }
        return false;
    }

    public object ToHappen(int userId)
    {
        List<int> participationEvents = UserEvent.Where(w => w.UserId == userId).Select(s => s.EventId).ToList();

        var eventsToHappen = Event.Where(w => (w.AdministratorId == userId || participationEvents.Contains(w.Id)) && w.Date > DateTime.Now).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId }).ToList();

        return eventsToHappen;
    }

    public object HasHappen(int userId)
    {
        List<int> participationEvents = UserEvent.Where(w => w.UserId == userId).Select(s => s.EventId).ToList();

        var eventsToHappen = Event.Where(w => (w.AdministratorId == userId || participationEvents.Contains(w.Id)) && w.Date < DateTime.Now).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId }).ToList();

        return eventsToHappen;
    }

    public object Own(int userId)
    {
        var eventsToHappen = Event.Where(w => w.AdministratorId == userId).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId }).ToList();

        return eventsToHappen;
    }

    public object GetFiltered(Location location, int radius, string name = "", List<int> types = null, List<DateTime> dates = null)
    {

        if (dates != null && types != null)
        {
            DateTime fromDate = dates.First();
            DateTime toDate = dates.Last();

            return Event.Where(w => (SqlFunctions.SquareRoot(Math.Pow(w.Longitude - location.Longitude, 2) + Math.Pow(w.Latitude - location.Latitude, 2)) <= radius / 111139) &&
                                              (string.IsNullOrEmpty(name) || w.Name == name) &&
                                              (types.Contains(w.TypeId)) &&
                                              (w.Date > fromDate && w.Date < toDate)
                                 ).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId, s.Longitude, s.Latitude }).ToList();
        }
        else if(dates == null && types != null)
        {
            return Event.Where(w => (SqlFunctions.SquareRoot(Math.Pow(w.Longitude - location.Longitude, 2) + Math.Pow(w.Latitude - location.Latitude, 2)) <= radius / 111139) &&
                                              (string.IsNullOrEmpty(name) || w.Name == name) &&
                                              (types.Count == 0 || types.Contains(w.TypeId)) &&
                                              (w.Date > SqlFunctions.GetDate())
                                 ).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId, s.Longitude, s.Latitude }).ToList();
        }
        else if(dates != null && types == null)
        {
            DateTime fromDate = dates.First();
            DateTime toDate = dates.Last();

            return Event.Where(w => (SqlFunctions.SquareRoot(Math.Pow(w.Longitude - location.Longitude, 2) + Math.Pow(w.Latitude - location.Latitude, 2)) <= radius / 111139) &&
                                              (string.IsNullOrEmpty(name) || w.Name == name) &&
                                              (w.Date > fromDate && w.Date < toDate)
                                 ).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId, s.Longitude, s.Latitude }).ToList();
        }
        else
        {
            return Event.Where(w => (SqlFunctions.SquareRoot(Math.Pow(w.Longitude - location.Longitude, 2) + Math.Pow(w.Latitude - location.Latitude, 2)) <= radius / 111139) &&
                                              (string.IsNullOrEmpty(name) || w.Name == name) &&
                                              (w.Date > SqlFunctions.GetDate())
                                 ).Select(s => new { s.Id, s.Name, s.Description, s.Date, s.TypeId, s.Longitude, s.Latitude }).ToList();
        }
    }

    #endregion

    #region Event Types

    public List<EventType> GetTypes()
    {
        return EventType.Select(s => s).ToList();
    }

    #endregion

    #region Other Functions

    private void RemoveEventParticipants(int eventId)
    {
        IEnumerable<UserEvent> deletedUserEvents = UserEvent.Where(s => s.EventId == eventId);

        UserEvent.RemoveRange(deletedUserEvents);

        SaveChanges();
    }

    #endregion
}