using Sora.Command.Attributes;
using Sora.Entities.CQCodes;
using Sora.Entities.CQCodes.CQCodeModel;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NepPure.Onebot.Commands.PcrReservation
{
    [CommandGroup]
    public class PcrReservationCommand
    {
        [GroupCommand(new string[] { "预约出刀" })]
        public async ValueTask Reserve(GroupMessageEventArgs eventArgs)
        {
            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;


            await eventArgs.Reply(CQCode.CQAt(sender.UserId),
                CQCode.CQText("不出 爪巴"));
        }
    }
}
