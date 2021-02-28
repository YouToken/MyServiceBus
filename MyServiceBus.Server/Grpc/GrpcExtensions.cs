using System;

namespace MyServiceBus.Server.Grpc
{
    public static class GrpcExtensions
    {

        public static void GrpcPreExecutionCheck()
        {
                        
            if (ServiceLocator.MyGlobalVariables.ShuttingDown)
                throw new Exception("Application is shutting down");
        }
        
    }
}