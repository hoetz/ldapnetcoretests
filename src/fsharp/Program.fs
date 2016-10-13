// Learn more about F# at http://fsharp.org

open System
open Microsoft.Extensions.Configuration
open Novell.Directory.Ldap
open System.IO

[<EntryPoint>]
let main argv = 
    let config = (new ConfigurationBuilder())
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("secret.json").Build()
    let password= (config.Item("password"))
    
    let con = new LdapConnection()
    
    con.Connect("172.16.250.139", 389);
    con.Bind("CN=Administrator,CN=Users,DC=flo,DC=loc", password);
    let searchBase = "CN=Users,DC=flo,DC=loc"
    let searchFilter = "(objectclass=*)"

    let isValidLdapResult (r:LdapMessage) =
        match r with
        | null -> false
        | :? LdapSearchResult -> true
        | _ -> false

    let processLdapResult (r:LdapSearchResult)=
        let entry = r.Entry
        System.Console.Out.WriteLine("\n" + entry.DN);
        System.Console.Out.WriteLine("\tAttributes: ");
                    


    let queue = con.Search(searchBase,
                    LdapConnection.SCOPE_SUB,
                    searchFilter,
                    [| "sAMAccountName"|],
                    false,
                    null,
                    null)

    let message _ = (queue.getResponse())
    Seq.initInfinite message
       |> Seq.takeWhile isValidLdapResult
       |> Seq.cast<LdapSearchResult>
       |> Seq.iter processLdapResult 

    con.Disconnect()
    0 // return an integer exit code
