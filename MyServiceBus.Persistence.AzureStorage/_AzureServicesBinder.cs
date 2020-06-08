using Microsoft.WindowsAzure.Storage;
using MyDependencies;
using MyServiceBus.Domains.MessagesContent;
using MyServiceBus.Domains.Persistence;
using MyServiceBus.Persistence.AzureStorage.PageBlob;
using MyServiceBus.Persistence.AzureStorage.TopicMessages;

namespace MyServiceBus.Persistence.AzureStorage
{
    public static class AzureServicesBinder
    {

        public static void BindTopicsPersistentStorage(this IServiceRegistrator sr, CloudStorageAccount cloudStorageAccount)
        {
            sr.Register<ITopicPersistenceStorage>(new TopicsPersistentStorageAzureBlobs(
                new MyPageBlob(cloudStorageAccount, "topics", "topicsdata")));
        }


        public static void BindMessagesPersistentStorage(this IServiceRegistrator sr, CloudStorageAccount cloudStorageAccount)
        {
            sr.Register<IMessagesPersistentStorage>(new MessagesPersistentStorage((topicId, pageId) =>
            {
                var fileName = pageId.Value.ToString("0000000000000000000");
                return new MyPageBlob(cloudStorageAccount, topicId, fileName);
            }));

        }
        
    }
}