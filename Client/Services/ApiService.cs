using Client.API;
using Client.Handlers;
using Client.Stores;
using Client.ViewModels;
using Client.ViewModels.NavigationViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.Services
{
    public class ApiService
    {
        private readonly Endpoints _endpoints;
        private readonly UserStore _userStore;
        private readonly NavigationService<LoginViewModel> _navigationService;

        public ApiService(Endpoints endpoints, UserStore userStore, NavigationService<LoginViewModel> navigationService)
        {
            _endpoints = endpoints;
            _userStore = userStore;
            _navigationService = navigationService;
        }

        public async Task<(string? ErrorMessage, T? ResponseContent)> GetAsync<T>(
            string nav, string endpoint, string accessToken)
        {
            var response = await _endpoints.GetCall(nav, endpoint, accessToken);
            return await ProcessResponse<T>(response);
        }

        public async Task<(string? ErrorMessage, T? ResponseContent)> PostAsync<T>(
            string nav, string endpoint, object newObject, string? accessToken = null, bool isLoginViewModel = false)
        {
            var response = await _endpoints.PostCall(nav, endpoint, newObject, accessToken);
            return await ProcessResponse<T>(response, isLoginViewModel);
        }

        public async Task<(string? ErrorMessage, T? ResponseContent)> PutAsync<T>(
            string nav, string endpoint, object updateObject, string accessToken)
        {
            var response = await _endpoints.PutCall(nav, endpoint, updateObject, accessToken);
            return await ProcessResponse<T>(response);
        }

        public async Task<(string? ErrorMessage, T? ResponseContent)> DeleteAsync<T>(
            string nav, string endpoint, string accessToken)
        {
            var response = await _endpoints.DeleteCall(nav, endpoint, accessToken);
            return await ProcessResponse<T>(response);
        }

        private async Task<(string? ErrorMessage, T? ResponseContent)> ProcessResponse<T>(
            HttpResponseMessage? response, bool isLoginViewModel = false)
        {
            var errorMessage = await ApiResponseStatusCodeHandler.HandleApiResponse(
                response, _userStore, _navigationService, isLoginViewModel);

            if (errorMessage != null)
                return (errorMessage, default);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseContent))
                return (null, default);

            var deserializedContent = JsonSerializer.Deserialize<T>(responseContent);

            if (deserializedContent == null)
                return ("Некоректна відповідь від сервера", default);

            return (null, deserializedContent);
        }
    }
}
