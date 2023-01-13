using System;

namespace gamecrew.Models
{
    public class DashboardViewModel
    {
        public List<PlayerMemberships> Memberships { get; set; }
        public List<PlayerGroups> PlayerGroupsAsAdmin { get; set; }
        public List<GroupInvitesOfPlayer> Invites { get; set; }
    }


    public class GroupInvitesOfPlayer
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string InviterName { get; set; }
        public string GroupImage { get; set; }
        public DateTime InvitedDate { get; set; }
        public string InviteId { get; set; }
    }
}