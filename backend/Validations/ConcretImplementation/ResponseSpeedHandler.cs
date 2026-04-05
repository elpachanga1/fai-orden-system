using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations.Model;

namespace Validations.ConcretImplementation
{
    public class ResponseSpeedHandler : Handler
    {
        public override string HandlerName => "ResponseSpeed";
        public override void Handle(Request incomingRequest)
        {
            Request request = InMemoryRequestRepository.Instance.GetRequest(incomingRequest.UserName);
            var validationMapEntry = GetDataFromRequestInfo(incomingRequest);
            InMemoryRequestRepository.Instance.SaveRequest(request);

            if (NextHandler != null)
            {
                if (validationMapEntry.State == true)
                {
                    request.RecoveryNextHandlerName = NextHandler.HandlerName;
                    InMemoryRequestRepository.Instance.SaveRequest(request);
                    NextHandler.Handle(request);
                }
            }
            else
            {
                request.RecoveryNextHandlerName = "The Process is Finished";
                InMemoryRequestRepository.Instance.SaveRequest(request);
            }
        }

        private ValidationMap GetDataFromRequestInfo(Request incomingRequest)
        {
            Request request = InMemoryRequestRepository.Instance.GetRequest(incomingRequest.UserName);
            var validationMapEntry = base.GetValidationMap(request);
            var incomingValidationMapEntry = incomingRequest.ValidationMaps.FirstOrDefault(x => x.ValidationName == HandlerName);
            if (incomingValidationMapEntry != null && incomingValidationMapEntry.State == true)
            {
                validationMapEntry.State = incomingValidationMapEntry.State;
                validationMapEntry.CreationDate = incomingValidationMapEntry.CreationDate;
                validationMapEntry.DetailData = DummyDetailData();
            }
            return validationMapEntry;
        }

        private Dictionary<string, string> DummyDetailData()
        {
            Dictionary<string, string> detailData = new Dictionary<string, string>();
            detailData.Add("CachedResponse", "true");
            detailData.Add("CacheHitTime", DateTime.Now.ToString());
            return detailData;
        }
    }
}
