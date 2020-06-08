using MyDependencies;

namespace MyServiceBus.Persistence.AzureStorage
{
    public static class BindTableStorageAsPersistentData
    {
        public static void BindAzureStorageAsPersistence(this IServiceRegistrator services, string connectionString)
        {


 //           var messagesStorage =
//                new MessagesPersistentStorageAzureBlob(new MyPageBlob(cloudStorageAccount, "queues"));
//            services.Register<IMessagesPersistentStorage>(messagesStorage);

        }
    }
}