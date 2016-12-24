namespace KawaBot.Server

open System.IO
open Http
open System.Net.Http
open System.Net
open Newtonsoft.Json
open Environment
open System.Collections.Generic
open Newtonsoft.Json.Linq

[<JsonObject>]
type SlackError =
    {
        [<field: JsonProperty("ok") >]
        Ok: bool;
        [<field: JsonProperty("error") >]
        Error: string;
    }

[<JsonObject>]
type OAuthBot = {
    [<field: JsonProperty("bot_user_id") >]
    BotUserId: string;
    [<field: JsonProperty("bot_access_token") >]
    BotAccessToken: string;
}

[<JsonObject>]
type OAuthSuccessResponse = 
    {
        [<field: JsonProperty("ok") >]
        Ok: bool;
        [<field: JsonProperty("access_token") >]
        AccessToken: string;
        [<field: JsonProperty("scope") >]
        Scope: string;
        [<field: JsonProperty("user_id") >]
        UserId: string;
        [<field: JsonProperty("team_name") >]
        TeamName: string;
        [<field: JsonProperty("team_id") >]
        TeamId: string;
        [<field: JsonProperty("bot") >]
        Bot: OAuthBot;
    }

module OAuthController =
    open System.Text

    let GetUrlHandler() =
        new UrlHandler("oauth", fun req resp -> async {
            let urlArgs = [
                "https://slack.com/api/oauth.access";
                sprintf "?client_id=%s" Environment.ClientID;
                sprintf  "&client_secret=%s" Environment.ClientSecret;
                sprintf "&code=%s" (req.QueryString.Get("code"));
            ] 
            let url = urlArgs |> String.concat ""
            use authReq = new HttpClient()
            Logger.Debug("performing request GET %s\n", url)
            use! authResp = authReq.GetAsync(url) |> Async.AwaitTask
            Logger.Debug("success, status %s\n", (authResp.StatusCode.ToString()))
            let! body = authResp.Content.ReadAsStringAsync() |> Async.AwaitTask
            Logger.Debug("response: %s\n", body)
            let jobj = JObject.Parse(body)
            match jobj.TryGetValue("ok") with
                | true, ok -> 
                    match ok.Value<bool>() with
                        | false -> 
                            let err = JsonConvert.DeserializeObject<SlackError>(body)
                            Logger.Error(err.Error)
                        | true -> 
                            let oauthResp = JsonConvert.DeserializeObject<OAuthSuccessResponse>(body)
                            assert (oauthResp.Bot.BotAccessToken <> "")
                            Environment.BotToken <- oauthResp.Bot.BotAccessToken
                            Logger.Info("successfully obtain the bot token\n")
                | false, _ -> 
                    Logger.Error("Invalid OAuth format")
                    resp.StatusCode <- 400
            resp.Close()
        })