using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Collections;

namespace MonhakPatterns
{
    /// <summary>
    /// Classe para consulta ao AD e recuperação de Grupos.
    /// Autor: Luciano Rieth
    /// Data: 01/03/2016
    /// </summary>
    public class LdapAuthentication
    {
        public string ldap { get; set; }
        public string LdapPath { get; set; }
        public string FilterAtribute { get; set; }
        public DirectoryEntry entry { get; set; }
        public string domain { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string domainAndUsername { get; set; }

        /// <summary>
        /// Construtor para receber o caminho Ldap.
        /// </summary>
        /// <param name="ldapPath">Caminho LDAP do AD</param>
        public LdapAuthentication(string ldapPath)
        {
            this.LdapPath = ldapPath;
        }

        /// <summary>
        /// Construtor para receber o caminho Ldap parametrizado.
        /// </summary>
        /// <param name="ldap">LDAP</param>
        /// <param name="ldapPath">Path Ldap</param>
        /// <param name="domain">Domínio</param>
        /// <param name="username">Login</param>
        /// <param name="password">Senha</param>
        public LdapAuthentication(string ldap, string ldapPath, string domain, string username, string password)
        {
            this.ldap = ldap + "/";
            this.LdapPath = ldapPath;
            this.domain = domain;
            this.userName = username;
            this.password = password;

            this.domainAndUsername = domain + @"\" + username;

            this.entry = new DirectoryEntry(LdapPath, domainAndUsername, password);
        }

        /// <summary>
        /// Verificar se usuário está autenticado ou pode ser autenticado no AD segundo o caminho LDAP fornecido na propriedade <paramref name="LdapPath"/>.
        /// </summary>
        /// <returns>Verdadeiro se usuário for autenticável.</returns>
        public bool IsAuthenticated()
        {
            try
            {
                // Bind to the native AdsObject to force authentication. 
                //Object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + this.userName + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return false;
                }

                // Update the new path to the user in the directory
                LdapPath = result.Path;
                FilterAtribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na autenticação do usuário. Descrição: " + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// Verificar se usuário está autenticado ou pode ser autenticado no AD segundo o caminho LDAP fornecido na propriedade <paramref name="LdapPath"/>.
        /// </summary>
        /// <param name="domain">Domínio</param>
        /// <param name="username">Login do usuário</param>
        /// <param name="pwd">Password do usuário</param>
        /// <returns>Verdadeiro se usuário for autenticável.</returns>
        public bool IsAuthenticated(string domain, string username, string pwd)
        {
            string domainAndUsername = domain + @"\" + username;

            //DirectoryEntry entry = new DirectoryEntry(LdapPath, domainAndUsername, pwd);

            try
            {
                // Bind to the native AdsObject to force authentication. 
                //Object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.Add("cn");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    return false;
                }

                // Update the new path to the user in the directory
                LdapPath = result.Path;
                FilterAtribute = (String)result.Properties["cn"][0];
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na autenticação do usuário. Descrição: " + ex.Message);
            }
            return true;
        }

        /// <summary>
        /// Buscar os grupos principais que o usuário é menbro
        /// </summary>
        /// <returns></returns>
        public string GetGroups()
        {
            DirectorySearcher search = new DirectorySearcher(this.entry);
            search.Filter = "(cn=" + FilterAtribute + ")";
            search.PropertiesToLoad.Add("memberOf");
            StringBuilder groupNames = new StringBuilder();

            try
            {
                SearchResult result = search.FindOne();
                int propertyCount = result.Properties["memberOf"].Count;
                String dn;
                int equalsIndex, commaIndex;

                for (int propertyCounter = 0; propertyCounter < propertyCount;
                     propertyCounter++)
                {
                    dn = (String)result.Properties["memberOf"][propertyCounter];

                    equalsIndex = dn.IndexOf("=", 1, System.StringComparison.Ordinal);
                    commaIndex = dn.IndexOf(",", 1, System.StringComparison.Ordinal);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    groupNames.Append(dn.Substring((equalsIndex + 1),
                                      (commaIndex - equalsIndex) - 1));
                    groupNames.Append("|");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter os grupos do usuário. Descrição: " + ex.Message);
            }
            return groupNames.ToString();
        }

        /// <summary>
        /// Buscar o e-mail do usuário configurado no Active Directory.
        /// </summary>
        /// <param name="domain">Domínio</param>
        /// <param name="username">Login do usuário</param>
        /// <param name="pwd">Password do usuário</param>
        /// <returns>E-mail do usuário</returns>
        public string GetUserEmail(string domain, string username, string pwd)
        {
            try
            {
                string email = string.Empty;

                string domainAndUsername = domain + @"\" + username;

                DirectoryEntry directoryEntry = new DirectoryEntry(LdapPath, domainAndUsername, pwd);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = String.Format("(&(objectClass=user)(SAMAccountName=*{0}*))", username);
                SearchResultCollection searchResults = directorySearcher.FindAll();

                try
                {
                    if (searchResults.Count > 0 && searchResults[0].Properties["mail"][0] != null)
                        email = searchResults[0].Properties["mail"][0].ToString();
                }
                catch
                {
                    //Se não encontrar e-mail vai gerar erro.
                }

                return email;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserEmail() - " + ex.Message, ex.InnerException);
            }

        }

        /// <summary>
        /// Buscar o display name (nome completo) do usuário no  Active Directory.
        /// </summary>
        /// <param name="domain">Domínio</param>
        /// <param name="username">Login do usuário</param>
        /// <param name="pwd">Password do usuário</param>
        /// <returns>Nome do usuário</returns>
        public string GetUserDisplayName(string domain, string username, string pwd)
        {
            try
            {
                string displayName = string.Empty;

                string domainAndUsername = domain + @"\" + username;

                DirectoryEntry directoryEntry = new DirectoryEntry(LdapPath, domainAndUsername, pwd);
                DirectorySearcher directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.Filter = String.Format("(&(objectClass=user)(SAMAccountName=*{0}*))", username);
                SearchResultCollection searchResults = directorySearcher.FindAll();

                try
                {
                    if (searchResults.Count > 0 && searchResults[0].Properties["mail"][0] != null)
                        displayName = searchResults[0].Properties["displayName"][0].ToString();
                }
                catch (Exception)
                {
                }

                return displayName;
            }
            catch (Exception ex)
            {
                throw new Exception("GetUserDisplayName() - " + ex.Message, ex.InnerException);
            }

        }

        /// <summary>
        /// Buscar todos os grupos que usuário pertence mesmo quando o grupo estiver dentro de outros grupos (recursivo).
        /// </summary>
        /// <param name="userDn">Path do AD com atributo DN do usuário</param>
        /// <param name="recursive">Se deve ser recursivo (grupos dentro de grupos)</param>
        /// <returns>Lista de todos os grupos do usuário</returns>
        public ArrayList GroupsRecursive(string userDn, bool recursive)
        {
            try
            {
                ArrayList groupMemberships = new ArrayList();

                var groupsNested = AttributeValuesMultiString("memberOf", userDn, groupMemberships, recursive);

                var arrayGroups = new ArrayList();
                foreach (string groupName in groupsNested)
                {
                    string[] groupsSplit = groupName.Replace(",", "=").Split('=');

                    arrayGroups.Add(groupsSplit[1].ToUpper());
                }

                return arrayGroups;
            }
            catch (Exception ex)
            {
                throw new Exception("GroupsRecursive() - " + ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Buscar os atributos para possível conexão e pesquisa em grupos do Active Directory.
        /// </summary>
        /// <param name="attributeName">Nome do atributo</param>
        /// <param name="objectDn">Objeto Dn</param>
        /// <param name="valuesCollection">Array</param>
        /// <param name="recursive">Se deve ser recursivo</param>
        /// <returns>Lista de atributos</returns>
        private ArrayList AttributeValuesMultiString(string attributeName, string objectDn,
                                                  ArrayList valuesCollection, bool recursive)
        {
            try
            {
                string domainAndUsername = this.domain + @"\" + this.userName;

                this.entry = new DirectoryEntry(objectDn, domainAndUsername, password);
                //DirectoryEntry ent = new DirectoryEntry(objectDn);
                PropertyValueCollection propertyValueCollection = entry.Properties[attributeName];
                IEnumerator en = propertyValueCollection.GetEnumerator();

                while (en.MoveNext())
                {
                    if (en.Current != null)
                    {
                        if (!valuesCollection.Contains(en.Current.ToString()))
                        {
                            valuesCollection.Add(en.Current.ToString());
                            if (recursive)
                            {
                                AttributeValuesMultiString(attributeName, this.ldap + en.Current, valuesCollection, true);
                            }
                        }
                    }
                }

                entry.Close();
                entry.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("AttributeValuesMultiString() - " + ex.Message, ex.InnerException);
            }

            return valuesCollection;
        }

        /// <summary>
        /// Verificar se usuário está dentro de um determinado grupo.
        /// </summary>
        /// <param name="group">Nome do grupo</param>
        /// <returns>Verdadeiro ou falso</returns>
        public bool UserInGroup(string group)
        {
            string groups = this.GetGroups();

            if (groups.Contains(group))
                return true;

            return false;
        }
    }
}
