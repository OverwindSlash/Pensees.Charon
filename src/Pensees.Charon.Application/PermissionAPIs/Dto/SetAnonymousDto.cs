using System;
using System.Collections.Generic;
using System.Text;

namespace Pensees.Charon.PermissionAPIs.Dto
{
    public class SetAnonymousDto
    {
        public string ServiceName { get; set; }
        public List<string> Urls { get; set; }
    }
}
