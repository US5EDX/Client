using System.Net;
using System.Net.Http;

namespace Client.Handlers
{
    public static class ApiResponseStatusCodeHandler
    {
        public static string? HandleApiResponse(HttpResponseMessage? responseMessage, string? unauthorizedMessage = null)
        {
            if (responseMessage == null)
                return "Сервер не відповідає";

            if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                return "У доступі відмовлено";

            if (unauthorizedMessage is not null && responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                return unauthorizedMessage;

            if (!responseMessage.IsSuccessStatusCode)
                return "Щось пішло не так спробуйте ще раз";

            return null;
        }
    }
}
