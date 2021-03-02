using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyServiceBus.Abstractions;
using MyServiceBus.Grpc;
using MyServiceBus.Grpc.Contracts;
using MyServiceBus.Grpc.Models;

namespace MyServiceBus.Server.Grpc
{
    public class ManagementGrpcService : ControllerBase, IManagementGrpcService
    {
        public async ValueTask<CreateTopicGrpcResponse> CreateTopicAsync(CreateTopicGrpcRequest request)
        {
            GrpcExtensions.GrpcPreExecutionCheck();

            var grpcSession = ServiceLocator.GrpcSessionsList.TryGetSession(request.SessionId, DateTime.UtcNow);

            if (grpcSession == null)
                return new CreateTopicGrpcResponse
                {
                    SessionId = -1,
                    Status = GrpcResponseStatus.SessionExpired
                };
            
            Console.WriteLine($"Creating topic {request.TopicId} for connection: "+grpcSession.Name);
            
            await ServiceLocator.TopicsManagement.AddIfNotExistsAsync(request.TopicId);

            grpcSession.SessionContext.PublisherInfo.AddIfNotExists(request.TopicId);
            
            return new CreateTopicGrpcResponse
            {
                SessionId = grpcSession.Id,
                Status = GrpcResponseStatus.Ok
            };
        }

        public ValueTask<CreateQueueGrpcResponse> CreateQueueAsync(CreateQueueGrpcRequest request)
        {
            GrpcExtensions.GrpcPreExecutionCheck();
            
            var session = ServiceLocator.GrpcSessionsList.TryGetSession(request.SessionId, DateTime.UtcNow);

            if (session == null)
            {
                var resultNoSession = new CreateQueueGrpcResponse
                {
                    SessionId = -1,
                    Status = GrpcResponseStatus.SessionExpired
                };
                return new ValueTask<CreateQueueGrpcResponse>(resultNoSession);
            }

            var topic = ServiceLocator.TopicsList.TryGet(request.TopicId);

            if (topic == null)
            {
                var topicNotFoundResult = new CreateQueueGrpcResponse
                {
                    SessionId = request.SessionId,
                    Status = GrpcResponseStatus.TopicNotFound
                };
                return new ValueTask<CreateQueueGrpcResponse>(topicNotFoundResult); 
            }

            topic.CreateQueueIfNotExists(request.QueueId, ToTopicQueueType(request.QueueType), false);

            var okResult = new CreateQueueGrpcResponse
            {
                SessionId = request.SessionId,
                Status = GrpcResponseStatus.Ok

            };
            
            return new ValueTask<CreateQueueGrpcResponse>(okResult);
        }

        private static TopicQueueType ToTopicQueueType(QueueTypeGrpcEnum queueType)
        {
            return queueType switch
            {
                QueueTypeGrpcEnum.Permanent => TopicQueueType.Permanent,
                QueueTypeGrpcEnum.PermanentWithSingleConnect => TopicQueueType.PermanentWithSingleConnection,
                _ => TopicQueueType.DeleteOnDisconnect
            };
        }

        public ValueTask<GreetingGrpcResponse> GreetingAsync(GreetingGrpcRequest request)
        {
            
            GrpcExtensions.GrpcPreExecutionCheck();

            var grpcSession = ServiceLocator.GrpcSessionsList.GenerateNewSession(request.Name);

            var result = new GreetingGrpcResponse
            {
                SessionId = grpcSession.Id,
                Status = GrpcResponseStatus.Ok
            };

            return new ValueTask<GreetingGrpcResponse>(result);
        }

        public ValueTask<PingGrpcResponse> PingAsync(PingGrpcRequest request)
        {
            GrpcExtensions.GrpcPreExecutionCheck();
            
            var session = ServiceLocator.GrpcSessionsList.TryGetSession(request.SessionId, DateTime.UtcNow);

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
            ServiceLocator.GrpcSessionsList.TryRemoveSession(request.SessionId);
            
            return new ValueTask<LogoutGrpcResponse>(new LogoutGrpcResponse
            {
                SessionId = request.SessionId
            });
        }
    }
}