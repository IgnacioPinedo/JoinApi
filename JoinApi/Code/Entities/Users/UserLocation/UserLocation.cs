using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

[Table("tb_Location")]
public class UserLocation
{
    [Key]
    public int Id { get; private set; }
    public bool IsHome { get; set; }
    public bool IsWork { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}