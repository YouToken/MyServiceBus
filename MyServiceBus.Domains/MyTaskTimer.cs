using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyServiceBus.Domains
{
    public class MyTaskTimer
    {
        private readonly int _delay;

        private readonly Dictionary<string, Func<ValueTask>> _items = new Dictionary<string, Func<ValueTask>>();

        public MyTaskTimer(int delay)
        {
            _delay = delay;
        }

        public void Register(string name, Func<ValueTask> callback)
        {
            _items.Add(name, callback);
        }

        private bool _working;
        private Task _task;

        private static async Task ExecuteAsync(KeyValuePair<string, Func<ValueTask>> item)
        {
            try
            {
                await item.Value();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception executing timer: " + item.Key);
                Console.WriteLine(e);
            }
        }

        private async Task LoopAsync()
        {
            var tasks = new List<Task>();

            while (_working)
            {
                try
                {
                    foreach (var item in _items)
                        tasks.Add(ExecuteAsync(item));

                    foreach (var task in tasks)
                        await task;
                }
                finally
                {
                    tasks.Clear();
                    await Task.Delay(_delay);
                }
            }
        }

        public void Start()
        {
            _working = true;
            _task = Task.Run(LoopAsync);
        }
        
        public void Stop()
        {
            _working = false;
            _task.Wait();
        }

    }
}