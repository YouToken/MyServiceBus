using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyServiceBus.Server.Controllers
{
    public static class ControllerExt
    {
        public static async Task<byte[]> GetBodyAsync(this HttpRequest httpRequest)
        {
            var res = await httpRequest.BodyReader.ReadAsync();
            httpRequest.BodyReader.AdvanceTo(res.Buffer.Start);
            if (res.Buffer.IsSingleSegment)
                return res.Buffer.First.ToArray();

            var pos = res.Buffer.Start;
            var listResult = new List<byte>();

            while (res.Buffer.TryGet(ref pos, out var mem))
            {
                listResult.AddRange(mem.ToArray());
            }

            return listResult.ToArray();
        }
        
    }
    
}