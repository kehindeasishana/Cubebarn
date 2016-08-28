using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class Audit
    {
        //[Table("Audit")]
        // Audit Properties
        public Guid AuditID { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public string UrlAccessed { get; set; }
        public DateTime TimeAccessed { get; set; }

        // Default Constructor
        public Audit() { }
    }

}