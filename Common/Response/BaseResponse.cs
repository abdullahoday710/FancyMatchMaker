using Newtonsoft.Json;

namespace Common.Response
{
    public class BaseResponse
    {
        public bool IsSuccess { get; set; }

        public int Status { get; set; }

        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Record { get; set; }

        public BaseResponse()
        {
        }

        public BaseResponse(bool isSuccess, int status = 200, string msg = "")
        {
            IsSuccess = isSuccess;
            Status = status;
            Message = msg;
        }

        public BaseResponse(bool isSuccess, int status = 200, string msg = "", object data = null)
        {
            IsSuccess = isSuccess;
            Status = status;
            Message = msg;
            Record = data;
        }
    }
}
