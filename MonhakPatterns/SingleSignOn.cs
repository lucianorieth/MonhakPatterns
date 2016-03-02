using System.DirectoryServices.AccountManagement;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MonhakPatterns
{
    public class SingleSignOn
    {
        /// <summary>
        /// Verify user is member of group
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="someUserName">User name (not domain)</param>
        /// <param name="yourGropuName">Group name to validate</param>
        /// <returns>Is member of or not?</returns>
        public bool UserInGroup(string domain, string someUserName, string yourGroupName)
        {
            var userInGroup = false;

            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);

            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, someUserName);

            // find the group in question
            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, yourGroupName);

            if (user != null && group != null)
            {
                // check if user is member of that group
                if (user.IsMemberOf(group))
                {
                    userInGroup = true;
                }
            }

            ctx.Dispose();
            return userInGroup;
        }

        /// <summary>
        /// Verify groups that user is a member
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="someUserName">User name (not domain)</param>
        /// <param name="yourGropuName">Group name to validate</param>
        /// <returns>List grops with member of value</returns>
        public List<GroupPermission> GroupsMemberOf(string domain, string someUserName, List<GroupPermission> groups)
        {
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);

            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, someUserName);

            foreach(GroupPermission groupPermission in groups)
            {
                // find the group in question
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, groupPermission.GroupName);
                if (user != null && group != null)
                {
                    // check if user is member of that group
                   
                        groupPermission.isMemberOf = user.IsMemberOf(group);
                }
            }

            ctx.Dispose();
            return groups;
        }
    }

    public class GroupPermission
    {
        public string GroupName;
        public bool isMemberOf;
    }
}
