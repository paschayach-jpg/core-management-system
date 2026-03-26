using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IPermissionGroupRepository
    {
        int Insert(PermissionGroup group);
        PermissionGroup GetById(int groupId);
        IEnumerable<PermissionGroup> GetAll();
        void Deactivate(int groupId);

        // UserGroupMember operations
        void AddUserToGroup(int userId, int groupId, int assignedBy);
        void RemoveUserFromGroup(int userId, int groupId);
        IEnumerable<UserGroupMember> GetGroupsByUser(int userId);
        IEnumerable<UserGroupMember> GetUsersByGroup(int groupId);
    }
}
