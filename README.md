# Hangfire.Dashboard.BasicAuthenticationFilter

This is a simpler replacement for the [`Hangfire.Dashboard.Authorization`](https://github.com/HangfireIO/Hangfire.Dashboard.Authorization) Nuget package which at its 2.1 version is not compatible with Hangfire Core 1.7. It's based on the [`BasicAuthAuthorizationFilter`](https://github.com/HangfireIO/Hangfire.Dashboard.Authorization/blob/master/src/Hangfire.Dashboard.Authorization/BasicAuthAuthorizationFilter.cs) class provided in that same package.

Please note that contrary to the original package this doesn't intend to support HTTPS enforcement/redirection, but just basic HTTP authentication. Hopefully the `Hangfire.Dashboard.Authorization` will eventually be upgraded to be compatible with the latest Hangfire Core version so it can be used again instead.

### Usage

Just copy to your project the `BasicAuthorizationFilter` file provided in this repository and add the following code to the `Configure()` method in your project's `Startup` class (be sure to change the `Username` and `Password` to the desired values):

```cs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] {
        new BasicAuthorizationFilter(new[]
        {
            new BasicAuthAuthorizationUser
            {
                Username = "SET YOUR USERNAME HERE",
                Password = "SET YOUR PASSWORD HERE"
            }
        })
    }
});
```

**IMPORTANT:** It's strongly recommended that you don't put your password in plain text in your codebase, but store the credentials in your environment-specific `appsettings.{ENVIRONMENT}.json` file and reference them through the injected `IConfiguration Configuration` instead (or use a proper secrets manager like Azure Key Vault or AWS Secrets Manager):

```cs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] {
        new BasicAuthorizationFilter(new[]
        {
            new BasicAuthAuthorizationUser
            {
                Username = Configuration["Hangfire:Dashboard:Authentication:Username"],
                Password = Configuration["Hangfire:Dashboard:Authentication:Password"]
            }
        })
    }
});
```

Alternatively, if for some reason you can't or don't want to use the approach recommended above, you can specify the password as a SHA-1 byte hash instead for reducing the inherent risk of storing your password in plain text in your codebase and version control history:

```cs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] {
        new BasicAuthorizationFilter(new[]
        {
            new BasicAuthAuthorizationUser
            {
                Username = "SET YOUR USERNAME HERE",
                PasswordSha1Hash = new byte[]{ 0xa9,
                    0x4a, 0x8f, 0xe5, 0xcc, 0xb1, 0x9b,
                    0xa6, 0x1c, 0x4c, 0x08, 0x73, 0xd3,
                    0x91, 0xe9, 0x87, 0x98, 0x2f, 0xbb,
                    0xd3 }
            }
        })
    }
});
```

For help with generating the byte hash for your chosen password, please refer to:
https://github.com/HangfireIO/Hangfire.Dashboard.Authorization#how-to-generate-password-hash
