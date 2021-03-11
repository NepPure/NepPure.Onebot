using System;
using System.Threading.Tasks;
using Sora.Net;
using Sora.OnebotModel;
using YukariToolBox.Extensions;
using YukariToolBox.FormatLog;


//设置log等级
Log.SetLogLevel(LogLevel.Debug);

//实例化服务器
SoraWebsocketServer server = new(new ServerConfig { Port = 9200 });

#region 服务器事件处理

//服务器连接事件
server.ConnManager.OnOpenConnectionAsync += (connectionInfo, eventArgs) =>
                                        {
                                            Log.Debug("Sora",
                                                             $"connectionId = {connectionInfo} type = {eventArgs.Role}");
                                            return ValueTask.CompletedTask;
                                        };
//服务器连接关闭事件
server.ConnManager.OnCloseConnectionAsync += (connectionInfo, eventArgs) =>
{
    Log.Debug("Sora",
                     $"connectionId = {connectionInfo} type = {eventArgs.Role}");
    return ValueTask.CompletedTask;
};
//服务器心跳包超时事件
server.ConnManager.OnHeartBeatTimeOut += (connectionInfo, eventArgs) =>
{
    Log.Debug("Sora",
                     $"Get heart beat time out from[{connectionInfo}] uid[{eventArgs.SelfId}]");
    return ValueTask.CompletedTask;
};

server.Event.OnSelfMessage += (type, eventArgs) =>
{
    Log.Info("test", $"self msg {eventArgs.Message.MessageId}");
    return ValueTask.CompletedTask;
};
#endregion

//启动服务器并捕捉错误
await server.StartServer().RunCatch(e => Log.Error("Server Error", Log.ErrorLogBuilder(e)));
