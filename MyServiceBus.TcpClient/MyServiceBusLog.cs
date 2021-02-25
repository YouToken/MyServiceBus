using System;
using System.Collections.Generic;

namespace MyServiceBus.TcpClient
{

    public interface IMyServiceBusLogInvoker
    {
        void InvokeLogInfo(string message);

        void InvokeLogException(Exception ex);

    }
    
    public class MyServiceBusLog<T> : IMyServiceBusLogInvoker
    {
        private readonly T _master;

        public MyServiceBusLog(T master)
        {
            _master = master;
        }

        private readonly List<Action<string>> _logInfo = new List<Action<string>>();

        public T AddLogInfo(Action<string> logInfo)
        {
            _logInfo.Add(logInfo);
            return _master;
        }
        
        private readonly List<Action<Exception>> _logException = new List<Action<Exception>>();

        public T AddLogException(Action<Exception> logException)
        {
            _logException.Add(logException);
            return _master;
        }


        public void InvokeLogInfo(string message)
        {
            if (_logInfo.Count == 0)
            {
                Console.WriteLine($"{DateTime.UtcNow}. MyServiceBus Info: "+message);
                return;
            }

            foreach (var action in _logInfo)
            {
                try
                {
                    action(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        public void InvokeLogException(Exception ex)
        {
            if (_logInfo.Count == 0)
            {
                Console.WriteLine($"{DateTime.UtcNow}. MyServiceBus Exception: "+ex);
                return;
            }

            foreach (var action in _logException)
            {
                try
                {
                    action(ex);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}