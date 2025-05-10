using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow.Models
{
    public class Client
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CityId { get; set; }
        public string Role { get; set; } = "User";
        public string? City { get; set; }
    }
}

