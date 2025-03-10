﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace Client.Models
{
    public partial class GroupWithSpecialtyInfo : ObservableObject
    {
        [JsonPropertyName("groupId")]
        public uint GroupId { get; set; }

        [ObservableProperty]
        [property: JsonPropertyName("groupCode")]
        private string _groupCode;

        [ObservableProperty]
        [property: JsonPropertyName("specialty")]
        private SpecialtyInfo _specialty;

        [ObservableProperty]
        [property: JsonPropertyName("eduLevel")]
        private byte _eduLevel;

        [ObservableProperty]
        [property: JsonPropertyName("course")]
        private byte _course;

        [ObservableProperty]
        [property: JsonPropertyName("nonparsemester")]
        private byte _nonparsemester;

        [ObservableProperty]
        [property: JsonPropertyName("parsemester")]
        private byte _parsemester;

        public GroupInfo ToGroupInfo()
        {
            return new GroupInfo
            {
                GroupId = this.GroupId,
                GroupCode = this.GroupCode,
                EduLevel = this.EduLevel,
                Course = this.Course,
                Nonparsemester = this.Nonparsemester,
                Parsemester = this.Parsemester
            };
        }
    }
}
