using System;
using System.Collections.Generic;
using System.Text;

namespace Pensees.Charon.OperationAPIs.Dto
{
    public class EnableFeatureDto
    {
        public int TenantId { get; set; }

        //public bool IsEnable { get; set; }

        public List<string> FeatureNames { get; set; }
    }
}
