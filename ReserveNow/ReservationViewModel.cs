using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReserveNow.Models;

namespace ReserveNow
{
    public class ReservationViewModel
    {
        public Reservation Reservation { get; set; }

        public ReservationViewModel(Reservation reservation)
        {
            Reservation = reservation;
        }
    }
}
