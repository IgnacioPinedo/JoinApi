using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Spatial;
using System.Web;

[Table("tb_Event")]
public class Event
{
    [Key]
    public int Id { get; private set; }
    public int TypeId { get; set; }
    public int AdministratorId { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}