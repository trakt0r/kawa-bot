namespace KawaBot.Tests

open System.Net
open KawaBot.Feeder
open KawaBot.Server
open NUnit.Framework
open Moq
open System.Net.Http
open System.Text
open Newtonsoft.Json

[<TestFixture>]
[<Category(Categories.Integration)>]
type BasicControllerTests =
    val private _server: Http.HttpListener
    val private _feeder: Feeder
    val private _httpRequsterMock: Mock<IHTTPRequester>

    new() as this =
        {
            _server = new Http.HttpListener("http://*:80/")
            _feeder = new Feeder(80)
            _httpRequsterMock = Mock<IHTTPRequester>()
        } then
            HealthController.GetUrlHandler()
            |> this._server.Register
            SlackVerificationController.GetUrlHandler()
            |> this._server.Register
            let oauth = new OAuthController(this._httpRequsterMock.Object)
            oauth.GetUrlHandler() |> this._server.Register

    member this.StubOAuthController(body: string) =
        let resp = new HttpResponseMessage()
        resp.Content <- new ByteArrayContent(Encoding.UTF8.GetBytes(body))
        this._httpRequsterMock.Setup(fun req -> req.HTTPGet(It.IsAny<string>()))
            .Returns(fun _ -> resp) |> ignore

    [<SetUp>]
    member this.SetUp() =
        this._server.Run()

    [<TearDown>]
    member this.TearDown() =
        this._server.Stop()

    [<Test>]
    member this.TestNonExistingEndpoint() =
        let response = this._feeder.Send("/non-existing") |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound))

    [<Test>]
    member this.TestHealthEndpoint() =
        let response = this._feeder.Send("/health") |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK))
        Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("OK"))

    [<Test>]
    member this.TestVerification() = 
        let v = {
            Token = "";
            Challenge = "challenge";
            Type = "";
        }
        let response = this._feeder.PostJSON("/slack", v) |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK))
        Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo(v.Challenge))

    [<Test>]
    member this.TestOAuthURL() =
        let r = this.StubOAuthController("{}")
        let response = this._feeder.Send("/oauth?code=123") |> Async.RunSynchronously
        this._httpRequsterMock.Verify(fun req -> req.HTTPGet(OAuthHelper.GetOAuthURL("123")))

    [<Test>]
    member this.TestOAuthInvalidResponse() =
        this.StubOAuthController("{}") |> ignore
        let response = this._feeder.Send("/oauth?code=123") |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest))

    [<Test>]
    member this.TestOAuthGetsBotToken() =
        let res = {
            Ok = true;
            AccessToken = "";
            Scope = "";
            UserId = "";
            TeamName = "";
            TeamId = "";
            Bot = {
                BotUserId = "";
                BotAccessToken = "VerySecretBotToken";
            }
        }
        this.StubOAuthController(JsonConvert.SerializeObject(res, Formatting.Indented)) |> ignore
        let response = this._feeder.Send("/oauth?code=123") |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK))
        Assert.That(Environment.BotToken, Is.EqualTo(res.Bot.BotAccessToken))