using Client.Services;
using Client.Stores;
using Client.ViewModels;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Client.Handlers
{
    public static class ApiResponseStatusCodeHandler
    {
        public static async Task<string?> HandleApiResponse(
            HttpResponseMessage? responseMessage,
            UserStore userStore,
            NavigationService<LoginViewModel> navigationService,
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
                        return "Неправильна пошта або пароль";
                    else
                    {
                        //To do

                        //userStore.Clear();
                        //navigationViewModel.NavigateToLogin();
                        return "Сесія закінчилася, будь ласка, увійдіть знову";
                    }

                case HttpStatusCode.Forbidden:
                    return "У доступі відмовлено";

                case HttpStatusCode.BadRequest:
                    return await GetErrorMessage(responseMessage) ?? "Некоректний запит";

                case HttpStatusCode.NotFound:
                    {
                        var message = await GetErrorMessage(responseMessage);
                        return message is null ? "Дані не знайдено" : message == string.Empty ? "Ресурс не знайдено" : message;
                    }

                case HttpStatusCode.InternalServerError:
                    return "Сталася помилка сервера";

                case HttpStatusCode.ServiceUnavailable:
                    return "Сервер недоступний";

                default:
                    return "Щось пішло не так, спробуйте ще раз";
            }
        }

        private static async Task<string?> GetErrorMessage(HttpResponseMessage responseMessage)
        {
            return await responseMessage.Content.ReadAsStringAsync();
        }
    }
}
