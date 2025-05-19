using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow.Models
{
    public class Table
    {
        public int ID { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }

        public Restaurant Restaurant { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
