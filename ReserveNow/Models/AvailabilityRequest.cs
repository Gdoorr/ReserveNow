using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow.Models
{
    class AvailabilityRequest
    {
        public int TableId { get; set; } // Идентификатор стола
        public string? Date { get; set; } // Дата в формате "yyyy-MM-dd"
        public string? StartTime { get; set; } // Время начала в формате "hh:mm:ss"
        public string? EndTime { get; set; }
    }
}
