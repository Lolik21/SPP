namespace Logger
{
    using System.Collections.Generic;

    public class DistinctConteinerUrlComparer : IEqualityComparer<Container>
    {
        public bool Equals(Container x, Container y)
        {
            return x.Log.Url == y.Log.Url;
        }

        public int GetHashCode(Container obj)
        {
            return 1;
        }
    }
}