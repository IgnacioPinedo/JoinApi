using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

[System.ComponentModel.DataAnnotations.Schema.Table("tb_Event")]
public class EventType
{
    [Key]
    public int Id { get; private set; }
    public string Type { get; set; }
}