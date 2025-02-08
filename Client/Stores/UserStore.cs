using Client.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.Stores
{
    public class UserStore
    {
        public bool IsTokenTriedForLogin { get; set; }

        public byte[] UserId { get; set; }

        public string Email { get; set; }

        public byte Role { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public StudentInfo? StudentInfo { get; set; }

        public WorkerInfo? WorkerInfo { get; set; }

        public void LoadUserStoreFromJson(JsonObject userInfo)
        {
            UserId = JsonSerializer.Deserialize<byte[]>(userInfo["userId"]);
            Email = JsonSerializer.Deserialize<string>(userInfo["email"]);
            Role = JsonSerializer.Deserialize<byte>(userInfo["role"]);
            AccessToken = JsonSerializer.Deserialize<string>(userInfo["accessToken"]);
            RefreshToken = JsonSerializer.Deserialize<string>(userInfo["refreshToken"]);

            if (Role == 1)
                return;

            if (Role == 4)
            {
                StudentInfo = JsonSerializer.Deserialize<StudentInfo>(userInfo["studentInfo"]);
                return;
            }

            WorkerInfo = JsonSerializer.Deserialize<WorkerInfo>(userInfo["workerInfo"]);

            return;
        }
    }
}
