using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Query
    {
        public string RedisKey { get; set; }
        public int Priority { get; set; }

        public Query(string RedisKey, int Priority)
        {
            this.RedisKey = RedisKey;
            this.Priority = Priority;
        }

    }
}
