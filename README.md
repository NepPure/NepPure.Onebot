# NepPure.Onebot
[![.NET](https://github.com/NepPure/NepPure.Onebot/actions/workflows/dotnet.yml/badge.svg)](https://github.com/NepPure/NepPure.Onebot/actions/workflows/dotnet.yml)

本项目基于https://github.com/Yukari316/Sora

自用 QQ BOT

### 目前PCR会战预约功能开发中
**成员命令：**
```
A：预约出刀
BOT: @A当前无人出刀，请出刀，出刀结束记得回复"报刀"哦！

B：预约出刀
BOT 当前@A正在出刀，请等待他回复“报刀”，当前还有1位小伙伴等待出刀，他们是：
1. B

C：预约出刀
BOT 当前@A正在出刀，请等待他回复“报刀”，当前还有2位小伙伴等待出刀，他们是：
1. B
2. C

A：报刀
BOT：辛苦啦，@B 轮到您出刀啦~还有1位小伙伴等您出刀结束，他们是：
1. C
出刀结束记得回复"报刀"哦！

C：预约出刀
BOT：您已经预约过啦， 当前@B正在出刀，请等待他回复“报刀”，当前还有1位小伙伴等待出刀，他们是：
1. C
```

**管理命令：**

```

管理员：强制报刀
BOT：好的，就当@B出完刀了，@C 轮到你出刀啦~

管理员：清空预约出刀
BOT：出刀队列已清空
```
