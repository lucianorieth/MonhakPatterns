using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonhakPatterns;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MonhakPatterns.Tests
{
    [TestClass]
    public class DataAccessTest
    {
        public const string CONNECTION_STRING_KEY = "AzureConnectionString";

        public List<Parameters> GetParameters(string commandText, CommandType commandType)
        {
            DataAccess dal = new DataAccess(CONNECTION_STRING_KEY, true);
            List<Parameters> lstParameters = dal.GetCommandParameters(commandText, commandType);
            if (lstParameters.Count > 0)
            {
                foreach (Parameters insParameter in lstParameters)
                {
                    switch (insParameter.ParName)
                    {
                        case "@id":
                            {
                                insParameter.ParValue = 1;
                                break;
                            }
                    }
                }
            }

            return lstParameters;
        }

        [TestMethod]
        public void GetConnectionStringTest()
        {
            DataAccess dataAccess = new DataAccess(CONNECTION_STRING_KEY, true);
            Assert.IsFalse(dataAccess.ConnectionString.Equals(""));
        }

        [TestMethod]
        public void GetParametersTest()
        {
            string commandText = "SPTeste";
            DataAccess dal = new DataAccess(CONNECTION_STRING_KEY, true);
            List<Parameters> lstParameters = dal.GetCommandParameters(commandText, CommandType.StoredProcedure);
            Assert.IsTrue(lstParameters.Count > 0);
        }

        [TestMethod]
        public void GetDataSetTest()
        {
            string commandText = "SPTeste";
            DataAccess dal = new DataAccess(CONNECTION_STRING_KEY, true);
            List<Parameters> lstParameters = GetParameters(commandText, CommandType.StoredProcedure);
            System.Data.DataSet target = dal.SelectDataSet(commandText, CommandType.StoredProcedure, lstParameters);
            Assert.IsTrue(target.Tables[0].Rows.Count > 0);
        }

        [TestMethod]
        public void GetDataReaderTest()
        {
            string commandText = "SPTeste";
            DataAccess dal = new DataAccess(CONNECTION_STRING_KEY, true);
            List<Parameters> lstParameters = GetParameters(commandText, CommandType.StoredProcedure);
            SqlDataReader target = dal.SelectDataReader(commandText, CommandType.StoredProcedure, lstParameters);
            Assert.IsTrue(target.HasRows);
            target.Close();
        }

        [TestMethod]
        public void ExecuteTest()
        {
            string commandText = "Insert into teste values ('teste valor 01')";
            DataAccess dal = new DataAccess(CONNECTION_STRING_KEY, true);
            //List<Parameters> lstParameters = dal.GetCommandParameters(commandText, CommandType.Text);
            var target = dal.Execute(commandText, CommandType.Text, null);
            Assert.IsTrue(target > 0);
        }

    }
}
