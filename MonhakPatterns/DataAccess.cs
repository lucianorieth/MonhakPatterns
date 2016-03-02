using System.Collections.Generic;

using System.Data;
using System.Data.SqlClient;
using System;

namespace MonhakPatterns
{
    public class DataAccess
    {
        //Nome da Stored Procedure que retorna os parâmetros do comando.
        public const string PROCEDURE_GET_PARAMETERS = "SPGETPARAMETERS";
        public const string COMMAND_TIMEOUT = "9999";
        public string ConnectionString { get; set; }
        public SqlConnection connection { get; set; }
        public SqlCommand command { get; set; }

        /// <summary>
        /// Default Constructor - Busca connection string usando a chave passada como parâmetro que fica em <connectionStrings> no web.config ou app.config
        /// </summary>
        /// <param name="connectionStringKey">Chave da connection string em arquivos de configuração</param>
        public DataAccess(string connectionString, bool connectionStringIsKey)
        {
            if (connectionStringIsKey)
                this.ConnectionString = ConfigurationManager.ConnectionStrings[connectionString].ToString();
            else
                this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Retorna um inteiro demonstrado o tipo do dado para SQL Server.
        /// </summary>
        /// <param name="dataType">Descrição do tipo do dado</param>
        /// <returns>inteiro demonstrado o tipo do dado para SQL Server</returns>
        public int GetDataType(string dataType)
        {
            int iReturnType = 0;
            Dictionary<string, int> dicDados = new Dictionary<string, int>();

            dicDados.Add("Binary", 1);
            dicDados.Add("Bit", 2);
            dicDados.Add("Char", 3);
            dicDados.Add("DateTime", 4);
            dicDados.Add("Decimal", 5);
            dicDados.Add("Float", 6);
            dicDados.Add("Image", 7);
            dicDados.Add("Int", 8);
            dicDados.Add("Money", 9);
            dicDados.Add("NChar", 10);
            dicDados.Add("NText", 11);
            dicDados.Add("NVarChar", 12);
            dicDados.Add("Real", 13);
            dicDados.Add("UniqueIdentifier", 14);
            dicDados.Add("SmallDateTime", 15);
            dicDados.Add("SmallInt", 16);
            dicDados.Add("SmallMoney", 17);
            dicDados.Add("Text", 18);
            dicDados.Add("Timestamp", 19);
            dicDados.Add("TinyInt", 20);
            dicDados.Add("VarBinary", 21);
            dicDados.Add("VarChar", 22);
            dicDados.Add("Variant", 23);
            dicDados.Add("Xml", 25);
            dicDados.Add("Date", 31);
            dicDados.Add("Time", 32);

            foreach (string sKeys in dicDados.Keys)
            {
                if (sKeys.ToUpper().Equals(dataType.ToUpper()))
                {
                    iReturnType = dicDados[sKeys];
                    break;
                }
            }

            return iReturnType;
        }

        /// <summary>
        /// Recebe uma lista de parâmetros (List<PARAMETERS>) com a propriedade OValue preenchida com o valor do parâmetro de um Stored Procedure.
        /// </summary>
        /// <param name="listParameters">List<PARAMETERS> com os valores dos parâmetros preenchidos</param>
        public void SetParameters(List<Parameters> listParameters)
        {
            if (listParameters.Count > 0)
            {
                foreach (Parameters insParameters in listParameters)
                {
                    string parameterName = insParameters.ParName;

                    SqlDbType parType = (SqlDbType)GetDataType(insParameters.ParType);
                    int length = insParameters.Length;
                    object value = insParameters.ParValue;
                    bool isOutPut = insParameters.IsOutPut;

                    //Verifica se o valor foi fornecido
                    if (value != null)
                    {
                        SqlParameter objParameter = new SqlParameter();
                        objParameter.ParameterName = parameterName;
                        objParameter.SqlDbType = parType;
                        objParameter.Value = value;
                        if (isOutPut)
                            objParameter.Direction = ParameterDirection.Output;
                        command.Parameters.Add(objParameter);
                    }
                }
            }
        }

        /// <summary>
        /// Criar objetos de banco como conexão e command.
        /// </summary>
        /// <param name="commandText">Comando SQL ou nome de Stored Procedure</param>
        /// <param name="commandType">Tipo do Commando Usar System.Data.SqlClient.CommandType</param>
        /// <param name="listParameters">List<Parameters> de parâmetros a serem passados para o comando</param>
        public void CreateDataBaseObjects(string commandText, CommandType commandType, List<Parameters> listParameters)
        {
            connection = new SqlConnection(ConnectionString);
            command = new SqlCommand();
            command.CommandType = commandType;
            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandTimeout = Convert.ToInt32(COMMAND_TIMEOUT);

            if (listParameters != null)
            {
                try
                {
                    SetParameters(listParameters);
                }
                catch (Exception ex)
                {
                    throw new Exception("All parameters for the command to be executed must be provided. Exception: " + ex.Message);
                }
            }
        }

        /// <summary>
        ///  Busca uma lista de parâmetros (List<Parameters>) que são necessários para uma Stored Procedure. 
        /// </summary>
        /// <param name="commandText">Nome da procedure ou comando SQL</param>
        /// <returns>List<Parameters> com a lista dos parâmetros docomando</returns>
        public List<Parameters> GetCommandParameters(string commandText, CommandType commandType)
        {
            //Lista de classes parâmetros.
            List<Parameters> lstParameters = new List<Parameters>();

            CreateDataBaseObjects(PROCEDURE_GET_PARAMETERS, CommandType.StoredProcedure, null);
            //Set default parameter
            command.Parameters.Add(new SqlParameter("@OBJNAME", SqlDbType.VarChar, 255)).Value = commandText;

            SqlDataAdapter objDataAdapter = new SqlDataAdapter(command);
            DataSet objDataSet = new DataSet();
            objDataAdapter.Fill(objDataSet);

            if (objDataSet.Tables.Count > 0)
            {
                if (objDataSet.Tables[0].Rows.Count > 0)
                {
                    DataTable objDataTable = objDataSet.Tables[0];

                    foreach (DataRow dtRow in objDataTable.Rows)
                    {
                        Parameters insParameters = new Parameters();

                        insParameters.ParName = dtRow[0].ToString();
                        insParameters.ParType = dtRow[1].ToString();
                        insParameters.Length = Convert.ToInt16(dtRow[2]);
                        insParameters.Scale = Convert.ToInt16(dtRow[3]);
                        insParameters.Precision = Convert.ToInt16(dtRow[4]);
                        insParameters.IsOutPut = Convert.ToBoolean(dtRow[5]);

                        lstParameters.Add(insParameters);
                    }
                }
            }

            return lstParameters;
        }

        /// <summary>
        /// Executar consulta retornando um DataSet
        /// </summary>
        /// <param name="commandText">Comando ou Stored Procedure</param>
        /// <param name="commandType">Tipo do comando</param>
        /// <param name="lstParameters">lista de parâmetros</param>
        /// <returns>DataSet com o resultado</returns>
        public DataSet SelectDataSet(string commandText, CommandType commandType, List<Parameters> lstParameters)
        {
            CreateDataBaseObjects(commandText, commandType, lstParameters);

            //Cria o dataset
            DataSet dtsResult = new DataSet();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(dtsResult);
            if (connection.State == ConnectionState.Open)
                connection.Close();

            return dtsResult;
        }

        /// <summary>
        /// Executar consulta retornando SqlDataReader - Leitura somnte para frente. OBS: Não esquecer de fechar o DataReader após execução.
        /// </summary>
        /// <param name="commandText">Comando ou Stored Procedure</param>
        /// <param name="commandType">Tipo do comando</param>
        /// <param name="lstParameters">lista de parâmetros</param>
        /// <returns>SqlDataReader com o resultado</returns>
        public SqlDataReader SelectDataReader(string commandText, CommandType commandType, List<Parameters> lstParameters)
        {
            CreateDataBaseObjects(commandText, commandType, lstParameters);
            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            SqlDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return dataReader;
        }

        /// <summary>
        /// Executar comando SQL ou Stored Procedure. Se necessário retornar o ID inserido, o primeiro parâmetro tem que ser Output. 
        /// </summary>
        /// <param name="commandText">Comando ou Stored Procedure</param>
        /// <param name="commandType">Tipo do comando</param>
        /// <param name="lstParameters">lista de parâmetros</param>
        /// <returns>Retorna linhas afetadas ou ID inserido se primeiro parâmetro for output</returns>
        public int Execute(string commandText, CommandType commandType, List<Parameters> lstParameters)
        {
            int executeReturn = 0;
            CreateDataBaseObjects(commandText, commandType, lstParameters);

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            executeReturn = command.ExecuteNonQuery();

            if (command.Parameters != null)
            {
                if (command.Parameters.Count > 0)
                {
                    if (command.Parameters[0].Direction == ParameterDirection.Output)
                        executeReturn = Convert.ToInt32(command.Parameters[0].Value);
                }
            }

            if (command.Connection.State == ConnectionState.Open)
                command.Connection.Close();

            command.Dispose();

            return executeReturn;
        }
    }

    public class Parameters
    {
        #region [ Properties ]
        public string ParName { get; set; }
        public string ParType { get; set; }
        public Int16 Length { get; set; }
        public Int16 Scale { get; set; }
        public Int16 Precision { get; set; }
        public bool IsOutPut { get; set; }
        public object ParValue { get; set; }
        #endregion [ Properties ]

        #region [ Constructors ]
        public Parameters()
        {
        }
        public Parameters(string psParName, string psParType, Int16 piLength)
        {
            ParName = psParName;
            ParType = psParType;
            Length = piLength;
        }
        public Parameters(string psParName, string psParType, Int16 piLength, Int16 piScale, Int16 piPrecision, object poValue)
        {
            ParName = psParName;
            ParType = psParType;
            Length = piLength;
            Scale = piScale;
            Precision = piPrecision;
            ParValue = poValue;
        }
        #endregion [ Constructors ]
    }
}

