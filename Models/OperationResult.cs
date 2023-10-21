namespace Dorbit.Models
{
    public class OperationResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; } = true;

        public OperationResult()
        {

        }

        public OperationResult(bool success)
        {
            Success = success;
        }

        public OperationResult(string message)
        {
            Message = message;
        }

        public static OperationResult Succeed()
        {
            return new OperationResult()
            {
                Success = true
            };
        }

        public static OperationResult Failed(object message)
        {
            return new OperationResult()
            {
                Success = false,
                Message = message.ToString()
            };
        }
    }
}
