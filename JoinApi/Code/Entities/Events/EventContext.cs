using Microsoft.SqlServer.Types;
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


    public bool Create(int typeId, int admId, string name, DateTime date, double longitude, double latitude)
    {
        Event newEvent = new Event()
        {
            TypeId = typeId,
            AdministratorId = admId,
            Name = name,
            Date = date,
            Location = SqlGeography.Point(latitude, longitude, DefaultSRID)
        };

        Event.Add(newEvent);
        
        SaveChanges();
        
        return true;
    }

    public bool Update(int eventId, int typeId, int admId, string name, DateTime date, double longitude, double latitude)
    {
        Event updateEvent = Event.Where(s => s.Id == eventId).FirstOrDefault();
        if (updateEvent != null)
        {
            //update
            return true;
        }
        else
            return false;
    }
}