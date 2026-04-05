using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations.Model;

namespace Validations
{
    public class InMemoryRequestRepository
    {
        private readonly Dictionary<string, Request> _requests = new Dictionary<string, Request>();

        private static readonly Lazy<InMemoryRequestRepository> _instance = new Lazy<InMemoryRequestRepository>(() => new InMemoryRequestRepository());
        public static InMemoryRequestRepository Instance => _instance.Value;

        private InMemoryRequestRepository() { }


        public void SaveRequest(Request request)
        {
            _requests[request.UserName] = request;
        }

        public Request GetRequest(string userName)
        {
            if(!_requests.TryGetValue(userName, out var request))
            {
                request = new Request { UserName = userName, ProcessCreationDate = DateTime.Now, ValidationMaps = new List<ValidationMap>()};
                _requests.Add(userName, request);
            }                
            return request;
        }

        public Request DeleteRequest(string userName)
        {
            if (_requests.TryGetValue(userName, out var request))
            {
                _requests.Remove(userName);
                return null; 
            }
            return null;
        }

        public Request GetFirstRequest()
        {
            var firstEntry = _requests.FirstOrDefault();
            return firstEntry.Value;
        }
    }
}
