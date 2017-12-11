namespace Logger
{
    using System.Collections.Generic;

    public class DistinctIpConteinerUrlComparer : IEqualityComparer<IpContainer>
    {
        public bool Equals(IpContainer x, IpContainer y)
        {
            return x.Ip == y.Ip;
        }

        public int GetHashCode(IpContainer obj)
        {
            return 1;
        }
    }
}