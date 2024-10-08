using System.Collections.Generic;

namespace BossFight.Models
{
    public interface IPersist<T>
        where T : PeristableProperties<T>
    {
        T FindOne(int? id = null);
        IEnumerable<T> FindAll(int? id = null);
        IEnumerable<T> FindTop(uint pRowsToRetrieve, string pOrderByColumn, bool pOrderByDescending = true);
    }
}