using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReserveNow.Models
{
    public static class AppData
    {
        public static Client CurrentUser { get; set; } = null!;
    }
}
