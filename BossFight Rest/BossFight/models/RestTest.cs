namespace BossFight.Models
{
    public class RestTest
    {
        public int Id { get; set; }
        public string TestString { get; set; }
        public string UnusedString { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, TestString: '{TestString}', UnusedString: '{UnusedString}'";
        }
    }
}
