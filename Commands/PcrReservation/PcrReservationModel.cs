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

        public PcrReservationModel(long userId, string nickName, string ps)
        {
            UserId = userId;
            NickName = nickName;
            ReserveTime = DateTime.Now;
            Ps = ps;
        }

        public long UserId { get; set; }

        public string NickName { get; set; }

        public bool IsCancel { get; set; }

        public bool IsOnTree { get; set; }

        public DateTime ReserveTime { get; set; }

        public DateTime? TreeTime { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        public string Ps { get; set; } = string.Empty;
    }
}