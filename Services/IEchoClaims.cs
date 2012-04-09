using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Services
{
    [ServiceContract]
    public interface IEchoClaims
    {
        [OperationContract]
        string Echo();
    }
}
