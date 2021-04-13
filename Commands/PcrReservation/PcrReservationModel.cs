using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepPure.Onebot.Commands.PcrReservation
{
    public class PcrReservationModel
    {
        public PcrReservationModel()
        {

        }

        public PcrReservationModel(long userId, string nickName)
        {
            UserId = userId;
            NickName = nickName;
            ReserveTime = DateTime.Now;
        }

        public long UserId { get; set; }

        public string NickName { get; set; }

        public bool IsCancel { get; set; }
        public DateTime ReserveTime { get; set; }
    }
}
