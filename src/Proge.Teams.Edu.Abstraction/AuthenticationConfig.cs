using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;

namespace Proge.Teams.Edu.Abstraction
{
    public class AuthenticationConfig
    {
        public int RetryDelay { get; set; }
        
        //public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string Instance { get; set; } 

        /// <summary>
        /// The Tenant is:
        /// - either the tenant ID of the Azure AD tenant in which this application is registered (a guid)
        /// or a domain name associated with the tenant
        /// - or 'organizations' (for a multi-tenant application)
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Guid used by the application to uniquely identify itself to Azure AD
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// URL of the authority
        /// </summary>
        public string Authority
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Instance, TenantId);
            }
        }

        /// <summary>
        /// Client secret (application password)
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: this property)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by the CertificateName property belows)
        /// <remarks> 
        public string ClientSecret { get; set; }

        /// <summary>
        /// Name of a certificate in the user certificate store
        /// </summary>
        /// <remarks>Daemon applications can authenticate with AAD through two mechanisms: ClientSecret
        /// (which is a kind of application password: the property above)
        /// or a certificate previously shared with AzureAD during the application registration 
        /// (and identified by this CertificateName property)
        /// <remarks> 
        public string CertificateName { get; set; }

        /// <summary>
        /// Web Api base URL
        /// </summary>
        public string BaseAddressList { get; set; }

        /// <summary>
        /// Web Api scope. With client credentials flows, the scopes is ALWAYS of the shape "resource/.default"
        /// </summary>
        public string[] ScopeList { get; set; }

        /// <summary>
        /// User credentials
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// User credentials
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// User credentials
        /// </summary>
        public SecureString SecurePassword
        {
            get
            {
                SecureString securePwd = new SecureString();

                if (string.IsNullOrWhiteSpace(Password))
                    return securePwd;

                foreach (char c in Password)    
                    securePwd.AppendChar(c);    
                return securePwd;
            }
        }
       
    }
}
