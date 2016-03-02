using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonhakPatterns;
using System.Collections.Generic;

namespace MonhakPatterns.Tests
{
    [TestClass]
    public class SingleSignOnTest
    {
        [TestMethod]
        public void UserInGroupTest()
        {
            SingleSignOn sso = new SingleSignOn();
            Assert.IsTrue(sso.UserInGroup("fsk", "lrieth", "HQT Developers"));
        }

        [TestMethod]
        public void GroupsMemberOfTest()
        {
            SingleSignOn sso = new SingleSignOn();

            GroupPermission group = new GroupPermission();
            group.GroupName = "HQT Developers";

            List<GroupPermission> groups = new List<GroupPermission>();
            groups.Add(group);

            List<GroupPermission> target = sso.GroupsMemberOf("fsk", "lrieth", groups);

            Assert.IsTrue(target[0].isMemberOf);
        }
    }
}
