using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service_Contract
{
    [ServiceContract]
    public interface IContract
    {
        [OperationContract]
        void AddMessage(string Obj);
        [OperationContract]
        bool RemoveMessage(string Obj);
        [OperationContract]
        void DumpQuery(object obj);
        [OperationContract]
        void RestoreQuery();
    }
}
