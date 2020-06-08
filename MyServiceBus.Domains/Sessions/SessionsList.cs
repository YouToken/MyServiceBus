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
        
        private readonly Dictionary<long, MySession> _sessions = new Dictionary<long, MySession>();
        private IReadOnlyList<MySession> _sessionsAsList = Array.Empty<MySession>();

        public MySession NewSession(string name, string ip, DateTime nowTime, in TimeSpan sessionTimeout, int protocolVersion)
        {
            _lockObject.EnterWriteLock();
            try
            {
                var sessionId = GetNextSessionId();
                var grpcSession = MySession.Create(sessionId, name, ip, nowTime, sessionTimeout, RemoveIfExists, protocolVersion);

                _sessions.Add(sessionId, grpcSession);
                _sessionsAsList = _sessions.Values.AsReadOnlyList();
                return grpcSession;
            }
            finally
            {
                _lockObject.ExitWriteLock();
            }
        }

        public IReadOnlyList<MySession> GetSessions()
        {
            return _sessionsAsList;
        }


        public MySession GetSession(long sessionId, DateTime now)
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

        private IReadOnlyList<MySession> GetSessionsToGarbageCollect(DateTime now)
        {
            List<MySession> result = null;
            var sessions = _sessionsAsList;

            foreach (var mySession in sessions)
            {
                if (mySession.IsExpired(now))
                {
                    if (result == null)
                        result = new List<MySession>();
                    
                    result.Add(mySession);
                }
            }

            return result;
        }


        public IReadOnlyList<MySession> GarbageCollect(DateTime now)
        {
            var sessionsToGarbageCollect = GetSessionsToGarbageCollect(now);

            if (sessionsToGarbageCollect == null)
                return Array.Empty<MySession>();

            _lockObject.EnterWriteLock();
            try
            {
                foreach (var sessionToGc in sessionsToGarbageCollect)
                {
                    if (_sessions.ContainsKey(sessionToGc.Id))
                        _sessions.Remove(sessionToGc.Id);
                    
                    
                    Console.WriteLine($"Session with Id {sessionToGc.Id} Name {sessionToGc.Name} LastAccess:{sessionToGc.LastAccess} and Ip {sessionToGc.Ip} is expired at {now}");
                }

                _sessionsAsList = _sessions.Values.AsReadOnlyList();

            }
            finally
            {
                _lockObject.ExitWriteLock();
            }

            return sessionsToGarbageCollect;

        }

        public IReadOnlyList<MySession> GetByCondition(Func<MySession, bool> condition)
        {

            List<MySession> result = null; 
            _lockObject.EnterReadLock();
            try
            {
                foreach (var session in _sessions.Values.Where(condition))
                {
                    if (result == null)
                        result = new List<MySession>();
                        
                    result.Add(session);
                }
            }
            finally
            {
                _lockObject.ExitReadLock();
            }

            if (result == null)
                return Array.Empty<MySession>();

            return result;
        }

        public void Update(long sessionId, Action<MySession> sessionUpdater)
        {
            _lockObject.EnterReadLock();
            try
            {
                if (_sessions.ContainsKey(sessionId))
                    sessionUpdater(_sessions[sessionId]);
            }
            finally
            {
                _lockObject.ExitReadLock();
            }
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
        
        public void RemoveIfExists(MySession session)
        {
           RemoveIfExists(session.Id);
        }


        public void Timer()
        {

            var list = _sessionsAsList;

            foreach (var mySession in list)
            {
                mySession.Timer();
            }

        }
    }
}