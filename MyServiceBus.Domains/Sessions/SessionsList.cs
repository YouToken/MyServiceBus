using System;
using System.Collections.Generic;
using System.Threading;
using DotNetCoreDecorators;

namespace MyServiceBus.Domains.Sessions
{

    public class SessionsList
    {

        private readonly ReaderWriterLockSlim _lockObject = new ();

        
        private readonly Dictionary<string, MyServiceBusSession> _sessions = new ();
        private IReadOnlyList<MyServiceBusSession> _sessionsAsList = Array.Empty<MyServiceBusSession>();

        public MyServiceBusSession NewSession(string sessionId, string name, SessionType sessionType)
        {
            _lockObject.EnterWriteLock();
            try
            {
                var grpcSession = new MyServiceBusSession(sessionId, name, sessionType, RemoveIfExists);

                _sessions.Add(sessionId, grpcSession);
                _sessionsAsList = _sessions.Values.AsReadOnlyList();
                return grpcSession;
            }
            finally
            {
                _lockObject.ExitWriteLock();
            }
        }

        public IReadOnlyList<MyServiceBusSession> GetSessions()
        {
            return _sessionsAsList;
        }


        public MyServiceBusSession TryGetSession(string sessionId)
        {
            _lockObject.EnterReadLock();
            try
            {
                return _sessions.TryGetOrDefault(sessionId);
            }
            finally
            {
                _lockObject.ExitReadLock();
            }
        }


        private void RemoveIfExists(MyServiceBusSession session)
        {
           _lockObject.EnterWriteLock();
           try
           {
               if (!_sessions.ContainsKey(session.Id))
                   _sessions.Remove(session.Id);
           }
           finally
           {
               _lockObject.ExitWriteLock();
           }
        }


        public void Timer(DateTime utcNow)
        {
            var list = _sessionsAsList;

            foreach (var mySession in list)
                mySession.Timer();
        }
    }
}