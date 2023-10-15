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

    public class InvalidPlayerClassException : System.Exception
    {
        public InvalidPlayerClassException(string message) : base(message) { }
    }
}
