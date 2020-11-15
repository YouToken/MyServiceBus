using System.Collections.Generic;
using System.Threading.Tasks;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Tests.GrpcMocks
{
    public class MyServiceBusQueuePersistenceGrpcServiceMock : IMyServiceBusQueuePersistenceGrpcService
    {

        private TopicAndQueuesSnapshotGrpcModel[] _snapshot;
        
        
        public ValueTask SaveSnapshotAsync(SaveQueueSnapshotGrpcRequest request)
        {
            _snapshot = request.QueueSnapshot;
            return new ValueTask();
        }

        private ValueTask<IEnumerable<TopicAndQueuesSnapshotGrpcModel>> GetSnapshotTaskAsync()
        {
            return new ValueTask<IEnumerable<TopicAndQueuesSnapshotGrpcModel>>(_snapshot);
        }
        

        public async IAsyncEnumerable<TopicAndQueuesSnapshotGrpcModel> GetSnapshotAsync()
        {
            foreach (var grpcModel in await GetSnapshotTaskAsync())
                yield return grpcModel;
        }
    }
}