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
            var message = new List<CQCode>();
            var userName = sender.Card;
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = sender.Nick;
            }

            var enqueuRes = PcrReservationManager.Enqueue(groupId, new PcrReservationModel(sender.UserId, userName));

            var alluser = PcrReservationManager.PeekAll(groupId);
            var first = alluser.FirstOrDefault();
            if (first == null)
            {
                await eventArgs.Reply("奇怪，队列里面找不到人");
                return;
            }

            if (enqueuRes)
            {
                // 成功加入队列               
                message.Add(CQCode.CQText("预约成功，"));
            }
            else
            {
                // 已经正在队列
                message.Add(CQCode.CQText("您已经预约过啦，"));
            }

            message.Add(CQCode.CQAt(first.UserId));

            if (first.UserId == sender.UserId)
            {
                // 成功加入队列 
                message.Add(CQCode.CQText("当前无人出刀，请出刀，出刀结束记得回复【报刀】哦！"));
            }
            else
            {
                message.Add(CQCode.CQText("正在出刀，请等待他回复【报刀】"));
            }
            message.AddRange(GetWaitUserMessage(alluser.Skip(1).ToList()));

            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "报刀" })]
        public async ValueTask Report(GroupMessageEventArgs eventArgs)
        {
            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;
            var message = new List<CQCode>();

            var first = PcrReservationManager.Peek(groupId);
            if (first == null)
            {
                await eventArgs.Reply("奇怪，队列里面找不到人");
                return;
            }

            if (sender.UserId != first.UserId)
            {
                await eventArgs.Reply("当前没有轮到您出刀呢，管理员可以通过【强制报刀】命令帮助小伙伴报刀");
                return;
            }

            PcrReservationManager.Dequeue(groupId);
            var alluser = PcrReservationManager.PeekAll(groupId);
            first = alluser.FirstOrDefault();
            message.Add(CQCode.CQText("辛苦啦~"));
            if (first != null)
            {
                message.Add(CQCode.CQText("\n"));
                message.Add(CQCode.CQAt(first.UserId));
                message.Add(CQCode.CQText("轮到您出刀了呢，出刀结束记得回复【报刀】哦"));
            }
            message.AddRange(GetWaitUserMessage(alluser.Skip(1).ToList()));

            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "查看预约" })]
        public async ValueTask View(GroupMessageEventArgs eventArgs)
        {
            var groupId = eventArgs.SourceGroup.Id;
            var message = new List<CQCode>();

            var alluser = PcrReservationManager.PeekAll(groupId);
            if (alluser.Count == 0)
            {
                await eventArgs.Reply("当前无人预约出刀");
                return;
            }

            message.AddRange(GetWaitUserMessage(alluser));
            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "强制报刀" },
            PermissionLevel = Sora.Enumeration.EventParamsType.MemberRoleType.Admin)]
        public async ValueTask ForceReport(GroupMessageEventArgs eventArgs)
        {
            var groupId = eventArgs.SourceGroup.Id;
            var message = new List<CQCode>();

            PcrReservationManager.Dequeue(groupId);
            var alluser = PcrReservationManager.PeekAll(groupId);
            var first = alluser.FirstOrDefault();
            message.Add(CQCode.CQText("嗯！我就当小伙伴出完刀了~"));
            if (first != null)
            {
                message.Add(CQCode.CQText("\n"));
                message.Add(CQCode.CQAt(first.UserId));
                message.Add(CQCode.CQText("轮到您出刀了呢，出刀结束记得回复【报刀】哦"));
            }
            message.AddRange(GetWaitUserMessage(alluser.Skip(1).ToList()));

            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "清空预约出刀" },
        PermissionLevel = Sora.Enumeration.EventParamsType.MemberRoleType.Admin)]
        public async ValueTask ForceClear(GroupMessageEventArgs eventArgs)
        {
            var groupId = eventArgs.SourceGroup.Id;
            var message = new List<CQCode>();

            PcrReservationManager.ClearQueue(groupId);
            message.Add(CQCode.CQText("出刀队列已清空"));
            await eventArgs.Reply(message);
        }

        private List<CQCode> GetWaitUserMessage(IList<PcrReservationModel> models)
        {
            if (models.Count == 0)
            {
                return new List<CQCode>();
            }

            var res = new List<CQCode>
            {
                CQCode.CQText($"当前还有{models.Count}位小伙伴等待出刀，他们是：\n")
            };
            int index = 1;

            foreach (var model in models)
            {
                res.Add(CQCode.CQText($"{index++}. {model.NickName}\n"));
            }

            return res;
        }
    }
}
