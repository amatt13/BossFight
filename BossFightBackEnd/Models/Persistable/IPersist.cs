namespace BossFight.Models
{
    public interface IPersist<T>
        where T : PersistableBase
    {
        T FindOne(int id);
    }
}