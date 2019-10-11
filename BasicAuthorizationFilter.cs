using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Dashboard;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using Microsoft.AspNetCore.Http;

namespace Hangfire.Dashboard
{
    public class BasicAuthenticationFilter : IDashboardAuthorizationFilter
    {
        public IEnumerable<UserCredentials> Users { get; }

        public BasicAuthenticationFilter(IEnumerable<UserCredentials> users)
        {
            Users = users;
        }

        public bool Authorize(DashboardContext dashboardContext)
        {
            var context = dashboardContext.GetHttpContext();

            string header = context.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header) == false)
            {
                AuthenticationHeaderValue authValues = AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
                    var parts = parameter.Split(':');

                    if (parts.Length > 1)
                    {
                        string username = parts[0];
                        string password = parts[1];

                        if ((string.IsNullOrWhiteSpace(username) == false) && (string.IsNullOrWhiteSpace(password) == false))
                        {
                            return Users.Any(user => user.Validate(username, password)) || Challenge(context);
                        }
                    }
                }
            }

            return Challenge(context);
        }

        private bool Challenge(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");

            context.Response.WriteAsync("Authentication is required.");

            return false;
        }
    }

    public class UserCredentials
    {
        public string Username { get; set; }

        /// <summary>
        /// Setter for password as a SHA1 hash.
        /// </summary>
        public byte[] PasswordSha1Hash { get; set; }

        /// <summary>
        /// Setter for password as plain text.
        /// </summary>
        public string Password
        {
            set
            {
                using (var cryptoProvider = SHA1.Create())
                {
                    PasswordSha1Hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(value));
                }
            }
        }

        public bool Validate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) == true)
                throw new ArgumentNullException("login");

            if (string.IsNullOrWhiteSpace(password) == true)
                throw new ArgumentNullException("password");

            if (username == Username)
            {
                using (var cryptoProvider = SHA1.Create())
                {
                    byte[] passwordHash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return StructuralComparisons.StructuralEqualityComparer.Equals(passwordHash, PasswordSha1Hash);
                }
            }
            else
                return false;
        }
    }
}
