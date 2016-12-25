namespace KawaBot.Tests

open System.Net
open KawaBot.Feeder
open KawaBot.Server
open NUnit.Framework

[<TestFixture>]
[<Category(Categories.Integration)>]
type BasicControllerTests =
    val private _server: Http.HttpListener
    val private _feeder: Feeder

    new() as this =
        {
            _server = new Http.HttpListener("http://*:80/")
            _feeder = new Feeder(80)
        } then
            HealthController.GetUrlHandler()
            |> this._server.Register
            SlackVerificationController.GetUrlHandler()
            |> this._server.Register

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
 