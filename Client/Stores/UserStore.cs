using Client.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Client.Stores
{
    public class UserStore
    {
        public bool IsTokenTriedForLogin { get; set; }

        public string? UserId { get; set; }

        public string Email { get; set; } = null!;

        public byte Role { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public StudentInfo? StudentInfo { get; set; }

        public WorkerInfo? WorkerInfo { get; set; }

        public void LoadUserStoreFromJson(JsonObject userInfo)
        {
            UserId = JsonSerializer.Deserialize<string>(userInfo["userId"]);
            Email = JsonSerializer.Deserialize<string>(userInfo["email"]);
            Role = JsonSerializer.Deserialize<byte>(userInfo["role"]);
            AccessToken = JsonSerializer.Deserialize<string>(userInfo["accessToken"]);
            RefreshToken = JsonSerializer.Deserialize<string>(userInfo["refreshToken"]);

            if (Role == 1)
            {
                StudentInfo = null;
                WorkerInfo = null;
                return;
            }

            if (Role == 4)
            {
                WorkerInfo = null;
                StudentInfo = JsonSerializer.Deserialize<StudentInfo>(userInfo["studentInfo"]);
                return;
            }

            StudentInfo = null;
            WorkerInfo = JsonSerializer.Deserialize<WorkerInfo>(userInfo["workerInfo"]);
        }
    }
}
