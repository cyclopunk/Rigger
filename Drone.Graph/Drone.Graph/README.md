## GraphGL Drone Module

This is an add on module for Rigger. It allows for GraphQL APIs to be
built automatically by the container. 

### GraphQL Implementation

The reference implmentation uses GraphQL and EntityGraphQL to provide query
services.

#### Scalars
Scalars are simple types that do not have fields. Override
the ToString method in order to provide a result on a complex Object type.

```
[Scalar]
[SchemaInformation(Name = "Boo", Description = "Scalar Test Type")]
public class ScalarType
{
    private string Test { get; set; }

    public override string ToString()
    {
        return Test;
    }
}
```
#### Types
Custom querable / mutatable types. 

```
[External]
public class QueryReturn
{
    public string QueryReturnInfo { get; set; }
}
```

#### Queries

```
[Singleton]
public class QueryService
{
    [Query]
    [SchemaInformation(Name="queryByName", Description = "Query description here")]
    string QueryString(string param1, string param2)
    {
        .... Some return data ...
    }

    [Query]
    [SchemaInformation(Name = "testQuery2", 
           Description = "This is another test query")]
    QueryReturn QueryString2(string param1, string two)
    {
        // return a complex object that has bene registered
        // external
        return new QueryReturn { 
            QueryReturnInfo = $"Return some data ({one},{two})"
        };
    }
}
```

#### Mutators

```
public class ForceChokeArguments {
    public string TargetName {get;set;}
    public double LengthOfChoke {get; set;}
}

[Mutator]
public class JediService
{
    [Autowire]
    private IContainer container;
    [Autowire]
    private ForceService theForce;

    [GraphQLMutation("Force choak an opponent")]
    public QueryReturn ForceChoke(DbContext ctx, ForceChokeArguments args)
    {

        theForce.Use(args.TargetName, Powers.ForceChoke, args.LengthOfChoke);

        return new QueryReturn
        {
            QueryReturnInfo = $"{args.TargetName} Choked!"
        };
    }
}
```

#### Authorization

# Basic API Webserver

## Configuration
- WebServer.Start to true
- WebServer.Port - The port to run the server on
- WebServer.CertificateFile - the PFX file to use for a certificate
- WebServer.CertificatePassword - The PFX file password

## Create A Self Signed Cert
The following powershell commands can be used to create a self-signed cert for development purposes.
```
 New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\CurrentUser\My\"
 $PFXPass = ConvertTo-SecureString -String "abc123" -Force -AsPlainText
 Export-PfxCertificate -Cert cert:\CurrentUser\My\987217C4F58E4C6C8FB65C7D876F0EF5571D04D4 -Password $PFXPass -FilePath .\test.pfx
```
