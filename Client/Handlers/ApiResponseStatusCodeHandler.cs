using Client.Services;
using Client.Stores;
using Client.ViewModels;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.Handlers
{
    public static class ApiResponseStatusCodeHandler
    {
        public static async Task<string?> HandleApiResponse(
            HttpResponseMessage? responseMessage,
            bool isLoginViewModel)
        {
            if (responseMessage == null)
                return "Сервер не відповідає";

            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.Accepted:
                    return null; // Success

                case HttpStatusCode.Unauthorized:
                    if (isLoginViewModel)
                        return await GetErrorMessage(responseMessage);
                    else
                    {
                        //To do

                        //userStore.Clear();
                        //navigationViewModel.NavigateToLogin();
                        return "Сесія закінчилася, будь ласка, увійдіть знову";
                    }

                case HttpStatusCode.Forbidden:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.NotFound:
                    return await GetErrorMessage(responseMessage);

                case HttpStatusCode.InternalServerError:
                    return "Сталася помилка сервера";

                case HttpStatusCode.ServiceUnavailable:
                    return "Сервер недоступний";

                default:
                    return "Щось пішло не так, спробуйте ще раз";
            }
        }

        private static async Task<string> GetErrorMessage(HttpResponseMessage responseMessage)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (content is null) return "Некоректний запит";

            if (content.Length == 0) return "Ресурс не знайдено";

            var jsonObject = JsonSerializer.Deserialize<JsonObject>(content);

            if (jsonObject is null) return "Некоректний запит";

            return JsonSerializer.Deserialize<string?>(jsonObject["detail"]) ?? "Сталась помилка, але деталі не було надано";
        }
    }
}
