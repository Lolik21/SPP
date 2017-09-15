using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Contract
{
    [DataContract]
    public class QClass : IBaseJob 
    {
        [DataMember]
        public int ObjID { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Params { get; set; }

        public QClass(int ObjID, string Message, string Params)
        {
            this.ObjID = ObjID;
            this.Message = Message;
            this.Params = Params;
        }

        public void Perform()
        {
            Console.WriteLine("Обьект c ID: {0} c сообщением: {1} и параметрами: {2} вышел.",
                        ObjID, Message, Params);
        }
    }
}
