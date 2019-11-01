﻿using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
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

    public Event Get(int eventId)
    {
        return Event.Where(s => s.Id == eventId).FirstOrDefault();
    }

    public Event Create(int typeId, int admId, string name, DateTime date, double longitude, double latitude)
    {
        Event newEvent = new Event()
        {
            TypeId = typeId,
            AdministratorId = admId,
            Name = name,
            Date = date,
            Latitude = latitude,
            Longitude = longitude
        };

        Event.Add(newEvent);
        
        SaveChanges();
        
        return newEvent;
    }

    public Event Update(int eventId, int typeId, int admId, string name, DateTime date, double longitude, double latitude)
    {
        Event updateEvent = Event.Where(s => s.Id == eventId).FirstOrDefault();
        if (updateEvent != null)
        {
            updateEvent.Date = date != null ? date : updateEvent.Date;
            updateEvent.Latitude = (latitude != 0) ? latitude : updateEvent.Latitude;
            updateEvent.Longitude = (longitude != 0) ? longitude : updateEvent.Longitude;
            updateEvent.Name = (name != null && name != "") ? name : updateEvent.Name;
            updateEvent.TypeId = typeId != 0 ? typeId : updateEvent.TypeId;

            Entry(updateEvent.Date).State = EntityState.Modified;
            Entry(updateEvent.Latitude).State = EntityState.Modified;
            Entry(updateEvent.Longitude).State = EntityState.Modified;
            Entry(updateEvent.Name).State = EntityState.Modified;
            Entry(updateEvent.TypeId).State = EntityState.Modified;

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
            Event.Remove(deleteEvent);

            SaveChanges();
        }
        return false;
    }

    public bool Participate(int userId, int eventId)
    {
        if(Event.Where(s => s.Id == eventId).Count() > 0 && 
           Event.Where(s => s.AdministratorId != userId && s.Id == eventId).Count() == 0 &&
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

    public List<EventType> GetTypes()
    {
        return EventType.Select(s => s).ToList();
    }
}