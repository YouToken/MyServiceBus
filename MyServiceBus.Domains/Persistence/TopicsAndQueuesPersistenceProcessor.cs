using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyServiceBus.Domains.Topics;
using MyServiceBus.Persistence.Grpc;

namespace MyServiceBus.Domains.Persistence
{
    
    public class TopicsAndQueuesPersistenceProcessor
    {
        private readonly IMyServiceBusQueuePersistenceGrpcService _grpcService;


        public TopicsAndQueuesPersistenceProcessor(IMyServiceBusQueuePersistenceGrpcService grpcService)
        {
            _grpcService = grpcService;
        }

        public async Task PersistTopicsAndQueuesInBackgroundAsync(IReadOnlyList<MyTopic> topics)
        {
            var toSave
                = topics
                    .Select(itm => itm.GetQueuesSnapshot())
                    .ToList();

            await _grpcService.SaveSnapshotAsync(toSave);
            
        }
        
    }
    
}