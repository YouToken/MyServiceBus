using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotNetCoreDecorators;

namespace MyServiceBus.Domains.Sessions
{

    public class SessionsList
    {

        private int _lastSessionId;
        private readonly ReaderWriterLockSlim _lockObject = new ReaderWriterLockSlim();

        private int GetNextSessionId()
        {
            lock (_lockObject)
            {
                _lastSessionId++;
                return _lastSessionId;
            }
        }
        
        private readonly Dictionary<long, MyServiceBusSession> _sessions = new Dictionary<long, MyServiceBusSession>();
        private IReadOnlyList<MyServiceBusSession> _sessionsAsList = Array.Empty<MyServiceBusSession>();

        public MyServiceBusSession NewSession(string name, string ip, DateTime nowTime, in TimeSpan sessionTimeout, int protocolVersion, SessionType sessionType)
        {
            _lockObject.EnterWriteLock();
            try
            {
                var sessionId = GetNextSessionId();
                var grpcSession = MyServiceBusSession.Create(sessionId, name, ip, nowTime, sessionTimeout, RemoveIfExists, protocolVersion, sessionType);

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


        public MyServiceBusSession GetSession(long sessionId, DateTime now)
        {
            _lockObject.EnterReadLock();
            try
            {
                if (!_sessions.ContainsKey(sessionId)) 
                    return null;
                
                var result = _sessions[sessionId];
                result.LastAccess = now;
                return result;
            }
            finally
            {
                _lockObject.ExitReadLock();
            }
        }

        private IReadOnlyList<MyServiceBusSession> GetSessionsToGarbageCollect(DateTime now)
        {
            List<MyServiceBusSession> result = null;
            var sessions = _sessionsAsList;

            foreach (var mySession in sessions.Where(itm => itm.SessionType == SessionType.Http))
            {
                if (!mySession.IsExpired(now)) continue;
                result ??= new List<MyServiceBusSession>();

                result.Add(mySession);
            }

            return result;
        }

        public IReadOnlyList<MyServiceBusSession> GetByCondition(Func<MyServiceBusSession, bool> condition)
        {

            List<MyServiceBusSession> result = null; 
            _lockObject.EnterReadLock();
            try
            {
                foreach (var session in _sessions.Values.Where(condition))
                {
                    result ??= new List<MyServiceBusSession>();

                    result.Add(session);
                }
            }
            finally
            {
                _lockObject.ExitReadLock();
            }

            if (result == null)
                return Array.Empty<MyServiceBusSession>();

            return result;
        }

        public void RemoveIfExists(in long sessionId)
        {
            _lockObject.EnterWriteLock();
             try
             {
                 if (_sessions.ContainsKey(sessionId))
                     _sessions.Remove(sessionId);

                 _sessionsAsList = _sessions.Values.AsReadOnlyList();
             }
             finally
             {
                 _lockObject.ExitWriteLock();
             }
        }

        private void RemoveIfExists(MyServiceBusSession session)
        {
           RemoveIfExists(session.Id);
        }


        public void Timer(DateTime utcNow)
        {
            var list = _sessionsAsList;

            foreach (var mySession in list)
                mySession.Timer();

            var garbageCollectedSessions = GetSessionsToGarbageCollect(utcNow);
            
            if (garbageCollectedSessions != null)
                foreach (var session in garbageCollectedSessions)
                    session.Disconnect();   
        }
    }
}