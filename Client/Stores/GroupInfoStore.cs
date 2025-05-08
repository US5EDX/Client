using Client.Models;
using Client.Services;
using System.IO;

namespace Client.Stores
{
    public class GroupInfoStore
    {
        public uint GroupId { get; set; }

        public string GroupCode { get; set; } = null!;

        public byte EduLevel { get; set; }

        public byte Course { get; set; }

        public byte DurationOfStudy { get; set; }

        public short AdmissionYear { get; set; }

        public byte Nonparsemester { get; set; }

        public byte Parsemester { get; set; }

        public bool HasEnterChoise { get; set; }

        public byte ChoiceDifference { get; set; }

        public bool IsActual { get; set; }

        public async Task LoadInfoAsync(ApiService apiService, uint groupId, string accessToken)
        {
            if (IsActual)
                return;

            (var errorMessage, var group) =
                    await apiService.GetAsync<GroupInfo>
                    ("Group", $"getGroupById/{groupId}", accessToken);

            if (!string.IsNullOrEmpty(errorMessage))
                throw new Exception(errorMessage);

            if (group is null)
                throw new InvalidDataException("Не вдалось завантажити дані про групу");

            GetInfoFromModel(group);
        }

        public void GetInfoFromModel(GroupInfo group)
        {
            GroupCode = group.GroupCode;
            EduLevel = group.EduLevel;
            Course = group.Course;
            DurationOfStudy = group.DurationOfStudy;
            AdmissionYear = group.AdmissionYear;
            Nonparsemester = group.Nonparsemester;
            Parsemester = group.Parsemester;
            HasEnterChoise = group.HasEnterChoise;
            ChoiceDifference = group.ChoiceDifference;

            IsActual = true;
        }
    }
}
