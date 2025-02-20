using Client.Models;

namespace Client.Stores
{
    public class GroupInfoStore
    {
        public uint GroupId { get; set; }

        public string GroupCode { get; set; }

        public byte EduLevel { get; set; }

        public byte Course { get; set; }

        public byte Nonparsemester { get; set; }

        public byte Parsemester { get; set; }

        public bool IsLoadedFromGroupsPage { get; set; }

        public void GetInfoFromModel(GroupInfo group)
        {
            GroupCode = group.GroupCode;
            EduLevel = group.EduLevel;
            Course = group.Course;
            Nonparsemester = group.Nonparsemester;
            Parsemester = group.Parsemester;

            IsLoadedFromGroupsPage = false;
        }
    }
}
