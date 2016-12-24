namespace KawaBot.Server

open System.IO
open Http
open Newtonsoft.Json
open System.Collections.Generic
open Newtonsoft.Json.Linq

[<JsonObject>]
type SlackVerification =
    {
        [<field: JsonProperty("token") >]
        Token : string;
        [<field: JsonProperty("challenge") >]
        Challenge : string;
        [<field: JsonProperty("type") >]
        Type : string;
    }

module SlackVerificationController =
    let GetUrlHandler() =
        new UrlHandler("slack", fun req resp -> async {
            assert (req.HttpMethod = "POST")
            assert (req.ContentType = "application/json")
            use inputReader = new StreamReader(req.InputStream)
            use writer = new StreamWriter(resp.OutputStream)
            let responseStr = inputReader.ReadToEnd()
            let jobj = JObject.Parse(responseStr)
            match jobj.TryGetValue("challenge") with
                | true, challenge -> 
                    //TODO check (verification) token field
                    let verification = JsonConvert.DeserializeObject<SlackVerification>(responseStr)
                    printfn "Got verification, challenge='%s'" verification.Challenge
                    writer.Write(verification.Challenge)
                | false, _ -> 
                   printfn "got event: %s" responseStr
        })
