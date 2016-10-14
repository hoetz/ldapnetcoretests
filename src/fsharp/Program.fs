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
    let host= (config.Item("host"))
    
    let con = new LdapConnection()
    
    con.Connect(host, 389);
    con.Bind("CN=Administrator,CN=Users,DC=flo,DC=loc", password);
    let searchBase = "CN=Users,DC=flo,DC=loc"
    let searchFilter = "(objectclass=*)"
    
    let isNotNull (r:LdapMessage)=
        match r with
        | null -> false
        | _ -> true

    let processLdapResult (r:LdapMessage)=
        match r with
        | :? LdapSearchResult as s ->
            let entry = s.Entry
            printfn "\n%s" entry.DN
        | _ -> ()

    let queue = con.Search(searchBase,
                    LdapConnection.SCOPE_SUB,
                    searchFilter,
                    [| "sAMAccountName"|],
                    false,
                    null,
                    null)

    let message _ = (queue.getResponse())
    Seq.initInfinite message
       |> Seq.takeWhile isNotNull
       |> Seq.iter processLdapResult 

    con.Disconnect()
    0 // return an integer exit code
