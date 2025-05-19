using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow.Models
{
    public class Reservation
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } // Название ресторана
        public string RestaurantCity { get; set; }
        public int TableId { get; set; }
        public string Date { get; set; } // Дата в формате ISO 8601 (строка)
        public string StartTime { get; set; } // Время в формате строки hh:mm:ss
        public string EndTime { get; set; }   // Время в формате строки hh:mm:ss
        public int Guests { get; set; }
    }
}
