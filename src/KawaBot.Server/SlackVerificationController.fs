namespace KawaBot.Server

open System.IO
open Http
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

[<DataContract>]
type SlackVerification =
    {
        [<field: DataMember(Name="token") >]
        Token : string;
        [<field: DataMember(Name="challenge") >]
        Challenge : string;
        [<field: DataMember(Name="type") >]
        Type : string;
    }

module SlackVerificationController =
    let GetUrlHandler() =
        new UrlHandler("slack/verify", fun req resp -> async {
            assert (req.HttpMethod = "POST")
            assert (req.ContentType = "application/json")
            use inputStream = req.InputStream
            let jss = new DataContractJsonSerializer(typedefof<SlackVerification>)
            let verification =  jss.ReadObject(inputStream) :?> SlackVerification
            printfn "Got verification, challenge='%s'" verification.Challenge
            use writer = new StreamWriter(resp.OutputStream)
            writer.Write(verification.Challenge)
        })
