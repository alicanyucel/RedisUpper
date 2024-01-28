using StackExchange.Redis;

namespace Redis.Sentinel.Services
{
    public class RedisService
    {
        static ConfigurationOptions sentinelOptions => new()
        {
            EndPoints =
            {
                { "localhost", 6393 },
                { "localhost", 6394 },
                { "localhost", 6395 }
            },
            CommandMap = CommandMap.Sentinel,
            AbortOnConnectFail = false
        };
        static ConfigurationOptions masterOptions => new()
        {
            AbortOnConnectFail = false
        };
        static public async Task<IDatabase> RedisMasterDatabase()
        {
            ConnectionMultiplexer sentinelConnection = await ConnectionMultiplexer.SentinelConnectAsync(sentinelOptions);

            System.Net.EndPoint masterEndPoint = null;
            foreach (System.Net.EndPoint endpoint in sentinelConnection.GetEndPoints())
            {
                IServer server = sentinelConnection.GetServer(endpoint);
                if (!server.IsConnected)
                    continue;
                masterEndPoint = await server.SentinelGetMasterAddressByNameAsync("mymaster");
                break;
            }

            var localMasterIP = masterEndPoint.ToString() switch
            {
                "0.0.0.0:6379" => "localhost:6379",
                "0.0.0.0:6379" => "localhost:6390",
                "0.0.0.0:6379" => "localhost:6391",
                "0.0.0.0:6379" => "localhost:6392",
            };

            ConnectionMultiplexer masterConnection = await ConnectionMultiplexer.ConnectAsync(localMasterIP);
            IDatabase database = masterConnection.GetDatabase();
            return database;
        }
    }
}
