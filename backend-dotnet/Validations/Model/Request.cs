using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Validations.Model
{
    public class Request
    {
        public string UserName {get; set;}
        public DateTime ProcessCreationDate { get; set; }
        public List<ValidationMap> ValidationMaps { get; set; }
        public string RecoveryNextHandlerName { get; set; }
        
}
}
