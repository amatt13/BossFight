namespace BossFight.CustemExceptions
{
    public class MyException : System.Exception
    {
        public MyException() { }
        public MyException(string message) : base(message) { }
        public MyException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class WTFException : System.Exception
    {
        public WTFException() { }
        public WTFException(string message) : base(message) { }
        public WTFException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class PlayerClassNotFoundException : System.Exception
    {
        public PlayerClassNotFoundException() { }
        public PlayerClassNotFoundException(string message) : base(message) { }
        public PlayerClassNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }

    public class LootNotFoundException : System.Exception
    {
        public LootNotFoundException() { }
        public LootNotFoundException(string message) : base(message) { }
        public LootNotFoundException(string message, System.Exception inner) : base(message, inner) { }
    }
}
