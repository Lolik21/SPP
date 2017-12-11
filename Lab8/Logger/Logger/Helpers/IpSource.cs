namespace Logger
{
    using System;
    using System.Collections.Generic;
    using DataGenerator;
    using DataGenerator.Sources;

    public class IpSource : DataSourcePropertyType
    {
        Random random = new Random();

        public IpSource() : base(new Type[1])
        {
            
        }

        public IpSource(IEnumerable<Type> types)
            : base(types)
        {
        }

        public IpSource(int priority, IEnumerable<Type> types)
            : base(priority, types)
        {
        }

        public override object NextValue(IGenerateContext generateContext)
        {
            return this.GetRandomIpAddress();
        }

        private string GetRandomIpAddress()
        {
            return $"{random.Next(1, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}";
        }
    }
}