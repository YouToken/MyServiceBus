namespace MyServiceBus.Domains.Sessions
{

    public class SessionsList
    {
        
        private readonly ConcurrentDictionaryWithNoLocksOnRead<string, MyServiceBusSession> _sessions = new ();

        public MyServiceBusSession NewSession(string sessionId, string name, SessionType sessionType)
        {
            var grpcSession = new MyServiceBusSession(sessionId, name, sessionType, RemoveIfExists);
            _sessions.Add(sessionId, ()=>grpcSession);
            return grpcSession;
        }

        public int Count => _sessions.Count;

        private void RemoveIfExists(MyServiceBusSession session)
        {
            _sessions.TryRemoveOrDefault(session.Id);
        }

        public void Timer()
        {
            foreach (var mySession in _sessions.GetAllValues())
                mySession.Timer();
        }
    }
}