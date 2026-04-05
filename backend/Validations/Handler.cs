using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validations.Interface;
using Validations.Model;

namespace Validations
{
    public abstract class Handler: IHandler
    {
        protected IHandler NextHandler;
        public abstract string HandlerName { get; }

        public virtual IHandler SetNext(IHandler handler)
        {
            NextHandler = handler;
            return handler;
        }

        public abstract void Handle(Request request);

        protected ValidationMap GetValidationMap(Request request)
        {
            var validationMapEntry = request.ValidationMaps.FirstOrDefault(vm => vm.ValidationName == this.HandlerName);
            if (validationMapEntry == null)
            {
                validationMapEntry = new ValidationMap { ValidationName = this.HandlerName, State = false };
                request.ValidationMaps.Add(validationMapEntry);
            }
            return validationMapEntry;
        }
    }
}
