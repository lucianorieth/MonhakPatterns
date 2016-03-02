using System.DirectoryServices.AccountManagement;
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
        public bool UserInGropu(string domain, string someUserName, string yourGropuName)
        {
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);

            // find a user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, someUserName);

            // find the group in question
            GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, yourGropuName);

            if (user != null)
            {
                // check if user is member of that group
                if (user.IsMemberOf(group))
                {
                    return true;
                }
                else return false;
            }
            else return false;
        }
    }
}
