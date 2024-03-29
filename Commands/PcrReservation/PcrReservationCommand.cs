﻿using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NepPure.Onebot.Commands.PcrReservation
{
    [CommandGroup]
    public class PcrReservationCommand
    {
        [GroupCommand(CommandExpressions = new[] { "^预约出刀.*", "^申请出刀.*", "^出刀.*" }, MatchType = Sora.Enumeration.MatchType.Regex)]
        public async ValueTask Reserve(GroupMessageEventArgs eventArgs)
        {
            var ps = string.Empty;
            var rawtext = eventArgs.Message.RawText;
            var reg = new Regex(".*出刀(.*)");
            var regResult = reg.Match(rawtext);
            if (regResult.Success
                && regResult.Groups.Count > 0)
            {
                ps = regResult.Groups[1].ToString().Trim();
            }

            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            message.Add(SegmentBuilder.At(sender.UserId));

            var userName = sender.Card;
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = sender.Nick;
            }

            PcrReservationManager.Enqueue(groupId, new PcrReservationModel(sender.UserId, userName, ps));

            var alluser = PcrReservationManager.PeekAll(groupId);
            var first = alluser.FirstOrDefault();
            if (first == null)
            {
                await eventArgs.Reply("奇怪，队列里面找不到人");
                return;
            }

            message.Add("预约成功，若需要出多刀请多次预约。");

            message.Add(SegmentBuilder.At(first.UserId));

            if (first.UserId == sender.UserId)
            {
                // 成功加入队列 
                message.Add("当前您在出刀队列第一位，请出刀。");
            }
            else
            {
                if (first.IsOnTree)
                {
                    message.Add("挂得很稳，如果您的\n【预约位置 ✖ 刀伤 < BOSS血量】\n可以考虑一起出刀。");
                }
                else
                {
                    message.Add("正在出刀，如果您的\n【预约位置 ✖ 刀伤 < BOSS血量】\n可以考虑一起出刀。");

                }
            }
            message.AddRange(GetWaitUserMessage(alluser));

            await eventArgs.Reply(message);
        }

        [GroupCommand(CommandExpressions = new[] { "^报刀.*", "^报🔪.*" }, MatchType = Sora.Enumeration.MatchType.Regex)]
        public async ValueTask Report(GroupMessageEventArgs eventArgs)
        {
            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            message.Add(SegmentBuilder.At(sender.UserId));

            var first = PcrReservationManager.Peek(groupId);
            if (first == null)
            {
                await eventArgs.Reply("你没约！");
                return;
            }

            if (sender.UserId != first.UserId)
            {
                var target = PcrReservationManager.SetCancel(groupId, sender.UserId);
                if (target == null)
                {
                    //出刀队列没有他
                    message.Add("好像没有您的预约记录。");
                }
                else
                {
                    message.Add("好的已经记录您提前出完刀啦~");
                }
            }
            else
            {
                PcrReservationManager.Dequeue(groupId);
                message.Add("(๑•̀ㅂ•́)و✧辛苦啦~");
            }

            var alluser = PcrReservationManager.PeekAll(groupId);
            first = alluser.FirstOrDefault();
            if (first != null)
            {
                message.Add("\n");
                message.Add(SegmentBuilder.At(first.UserId));
                if (first.IsOnTree)
                {
                    message.Add("你还在树上挂着！");
                }
                else
                {
                    message.Add("轮到您出刀了呢，出刀结束记得回复【报刀】!");
                }
            }
            message.AddRange(GetWaitUserMessage(alluser));

            await eventArgs.Reply(message);
        }

        [GroupCommand(CommandExpressions = new[] { "^挂树.*" }, MatchType = Sora.Enumeration.MatchType.Regex)]
        public async ValueTask OnTree(GroupMessageEventArgs eventArgs)
        {
            var ps = string.Empty;
            var rawtext = eventArgs.Message.RawText;
            var reg = new Regex(".*挂树(.*)");
            var regResult = reg.Match(rawtext);
            if (regResult.Success
                && regResult.Groups.Count > 0)
            {
                ps = regResult.Groups[1].ToString().Trim();
            }

            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            message.Add(SegmentBuilder.At(sender.UserId));

            var userName = sender.Card;
            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = sender.Nick;
            }

            PcrReservationManager.SetOnTree(groupId, sender.UserId, userName, ps);

            var alluser = PcrReservationManager.PeekAll(groupId);
            var first = alluser.FirstOrDefault();
            if (first == null)
            {
                await eventArgs.Reply("奇怪，队列里面找不到人");
                return;
            }

            message.Add("挂稳了~（<ゝω·）诶嘿~☆");
            message.AddRange(GetWaitUserMessage(alluser));
            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "下树" })]
        public async ValueTask OffTree(GroupMessageEventArgs eventArgs)
        {
            var sender = eventArgs.SenderInfo;
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            message.Add(SegmentBuilder.At(sender.UserId));

            var first = PcrReservationManager.SetOffTree(groupId, sender.UserId);
            if (first == null)
            {
                await eventArgs.Reply("树上没你。");
                return;
            }
            message.Add("已下树。");
            var alluser = PcrReservationManager.PeekAll(groupId);
            if (alluser.Count > 0)
            {
                message.AddRange(GetWaitUserMessage(alluser));
            }
            await eventArgs.Reply(message);
        }


        [GroupCommand(new string[] { "查看预约", "查看队列", "查看挂树" })]
        public async ValueTask View(GroupMessageEventArgs eventArgs)
        {
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            var sender = eventArgs.SenderInfo;
            message.Add(SegmentBuilder.At(sender.UserId));

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
            var message = new MessageBody();
            var sender = eventArgs.SenderInfo;
            message.Add(SegmentBuilder.At(sender.UserId));

            PcrReservationManager.Dequeue(groupId);
            var alluser = PcrReservationManager.PeekAll(groupId);
            var first = alluser.FirstOrDefault();
            message.Add("嗯！我就当小伙伴出完刀了~");
            if (first != null)
            {
                message.Add("\n");
                message.Add(SegmentBuilder.At(first.UserId));
                if (first.IsOnTree)
                {
                    message.Add("你还在树上挂着！");
                }
                else
                {
                    message.Add("轮到您出刀了呢，出刀结束记得回复【报刀】!");
                }
            }
            message.AddRange(GetWaitUserMessage(alluser));

            await eventArgs.Reply(message);
        }

        [GroupCommand(new string[] { "清空预约出刀", "清空出刀队列" },
        PermissionLevel = Sora.Enumeration.EventParamsType.MemberRoleType.Admin)]
        public async ValueTask ForceClear(GroupMessageEventArgs eventArgs)
        {
            var groupId = eventArgs.SourceGroup.Id;
            var message = new MessageBody();
            var sender = eventArgs.SenderInfo;
            message.Add(SegmentBuilder.At(sender.UserId));

            PcrReservationManager.ClearQueue(groupId);
            message.Add("队列已清空");
            await eventArgs.Reply(message);
        }

        private List<SoraSegment> GetWaitUserMessage(IList<PcrReservationModel> models)
        {
            if (models.Count == 0)
            {
                return new List<SoraSegment>();
            }

            var res = new List<SoraSegment>
            {
                SegmentBuilder.Text($"当前队列有{models.Count}位小伙伴，他们是：")
            };
            int index = 1;

            foreach (var model in models.Where(m => !m.IsOnTree))
            {
                var info = $"\n{index++}.[{model.ReserveTime.ToShortTimeString()}] {model.NickName}";
                if (!string.IsNullOrWhiteSpace(model.Ps))
                {
                    info += $"：{model.Ps}";
                }
                res.Add(SegmentBuilder.Text(info));
            }

            res.Add(SegmentBuilder.Text("\n在树上的："));
            foreach (var model in models.Where(m => m.IsOnTree))
            {
                var info = $"\n{index++}.[{(DateTime.Now - (model.TreeTime ?? model.ReserveTime)).TotalMinutes:0}分钟] {model.NickName}";
                if (!string.IsNullOrWhiteSpace(model.Ps))
                {
                    info += $"：{model.Ps}";
                }
                res.Add(SegmentBuilder.Text(info));
            }
            if (!models.Any(m => m.IsOnTree))
            {
                res.Add(SegmentBuilder.Text("无"));
            }
            return res;
        }
    }
}
