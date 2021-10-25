namespace TodoApi
{
    public class PostTest
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
