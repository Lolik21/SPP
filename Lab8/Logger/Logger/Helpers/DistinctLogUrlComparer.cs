namespace Logger
{
    using System.Collections.Generic;

    class DistinctLogUrlComparer : IEqualityComparer<Log>
    {

        public bool Equals(Log x, Log y)
        {
            return x.Url == y.Url;
        }

        public int GetHashCode(Log obj)
        {
            return 1;
        }
    }
}