using MyServiceBus.Grpc.Contracts;
using MyServiceBus.Grpc.Models;

namespace MyServiceBus.Server.Grpc
{
    public static class ErrorGrpcResponses
    {
        public static readonly PublishMessageGrpcResponse TopicNotFoundGrpcResponse = new PublishMessageGrpcResponse
        {
            Status  = GrpcResponseStatus.TopicNotFound
        };
        
        public static readonly PublishMessageGrpcResponse SessionExpired = new PublishMessageGrpcResponse
        {
            Status  = GrpcResponseStatus.SessionExpired
        };
        
        public static readonly SubscribeGrpcResponse SubscribeSessionExpired = new SubscribeGrpcResponse
        {
            Status  = GrpcResponseStatus.SessionExpired
        };

        public static readonly SubscribeGrpcResponse SubscribeTopicNotFound = new SubscribeGrpcResponse
        {
            Status  = GrpcResponseStatus.TopicNotFound
        };
    }
}