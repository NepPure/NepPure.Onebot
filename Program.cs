using Sora.Net;
using System.Threading.Tasks;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.Net.Config;
using YukariToolBox.FormatLog;

//设置log等级
Log.SetLogLevel(LogLevel.Info);

//实例化Sora服务
var service = SoraServiceFactory.CreateService(new ServerConfig()
{
    Port = 9200
});

#region 事件处理

//连接事件
service.ConnManager.OnOpenConnectionAsync += (connectionInfo, eventArgs) =>
{
    Log.Debug("Sora_Test|OnOpenConnectionAsync",
              $"connectionId = {connectionInfo} type = {eventArgs.Role}");
    return ValueTask.CompletedTask;
};
//连接关闭事件
service.ConnManager.OnCloseConnectionAsync += (connectionInfo, eventArgs) =>
{
    Log.Debug("Sora_Test|OnCloseConnectionAsync",
              $"uid = {eventArgs.SelfId} connectionId = {connectionInfo} type = {eventArgs.Role}");
    return ValueTask.CompletedTask;
};
//连接成功元事件
service.Event.OnClientConnect += (type, eventArgs) =>
{
    Log.Debug("Sora_Test|OnClientConnect",
              $"uid = {eventArgs.LoginUid}");
    return ValueTask.CompletedTask;
};
#endregion

//启动服务并捕捉错误
await service.StartService();
//.RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));
await Task.Delay(-1);