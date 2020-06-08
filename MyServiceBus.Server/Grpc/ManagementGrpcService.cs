using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Grpc;
using MyServiceBus.Grpc.Contracts;
using MyServiceBus.Grpc.Models;

namespace MyServiceBus.Server.Grpc
{
    public class ManagementGrpcService : ControllerBase, IManagementGrpcService
    {
        public async ValueTask<CreateTopicGrpcResponse> CreateTopicAsync(CreateTopicGrpcRequest request)
        {
            var session = ServiceLocatorApi.SessionsList.GetSession(request.SessionId, DateTime.UtcNow);

            if (session == null)
                return new CreateTopicGrpcResponse
                {
                    SessionId = -1,
                    Status = GrpcResponseStatus.SessionExpired
                };

            
            Console.WriteLine($"Creating topic {request.TopicId} for connection: "+session.Name);
            
            await ServiceLocatorApi.TopicsManagement.AddIfNotExistsAsync(request.TopicId);

            session.PublishToTopic(request.TopicId);
            
            return new CreateTopicGrpcResponse
            {
                SessionId = session.Id,
                Status = GrpcResponseStatus.Ok
            };
        }

        public ValueTask<CreateQueueGrpcResponse> CreateQueueAsync(CreateQueueGrpcRequest request)
        {

            var session = ServiceLocatorApi.SessionsList.GetSession(request.SessionId, DateTime.UtcNow);

            if (session == null)
            {
                var resultNoSession = new CreateQueueGrpcResponse
                {
                    SessionId = -1,
                    Status = GrpcResponseStatus.SessionExpired
                };
                return new ValueTask<CreateQueueGrpcResponse>(resultNoSession);
            }

            var topic = ServiceLocatorApi.TopicsList.Get(request.TopicId);

            topic.CreateQueueIfNotExists(request.QueueId, request.DeleteOnNoConnections);

            var result = new CreateQueueGrpcResponse
            {
                SessionId = request.SessionId,
                Status = GrpcResponseStatus.Ok

            };
            return new ValueTask<CreateQueueGrpcResponse>(result);
            
        }

        public ValueTask<GreetingGrpcResponse> GreetingAsync(GreetingGrpcRequest request)
        {

            var session = ServiceLocatorApi.SessionsList.NewSession(request.Name, "127.0.0.1", DateTime.UtcNow, Startup.SessionTimeout, 0);

            var result = new GreetingGrpcResponse
            {
                SessionId = session.Id,
                Status = GrpcResponseStatus.Ok
            };

            return new ValueTask<GreetingGrpcResponse>(result);
        }

        public ValueTask<PingGrpcResponse> PingAsync(PingGrpcRequest request)
        {
            var session = ServiceLocatorApi.SessionsList.GetSession(request.SessionId, DateTime.UtcNow);

            if (session != null)
                return new ValueTask<PingGrpcResponse>(new PingGrpcResponse
                {
                    Status = GrpcResponseStatus.Ok,
                    SessionId = request.SessionId
                });

            var noSession = new PingGrpcResponse
            {
                SessionId = -1,
                Status = GrpcResponseStatus.SessionExpired
            };

            return new ValueTask<PingGrpcResponse>(noSession);
        }

        public ValueTask<LogoutGrpcResponse> LogoutAsync(LogoutGrpcRequest request)
        {
            ServiceLocatorApi.SessionsList.RemoveIfExists(request.SessionId);
            
            return new ValueTask<LogoutGrpcResponse>(new LogoutGrpcResponse
            {
                SessionId = request.SessionId
            });
        }
    }
}