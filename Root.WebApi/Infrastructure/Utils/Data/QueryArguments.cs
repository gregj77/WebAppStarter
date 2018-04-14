using System.Collections.Generic;

namespace Utils.Data
{
    public class QueryArguments
    {
        public virtual uint? StartRow { get; set; }
        public virtual uint? RecordsPerPage { get; set; }
        public virtual string OrderBy { get; set; }
        public virtual string Filter { get; set; }

        public virtual IEnumerable<string> FilterProcessed { get; set; }
    }
}
