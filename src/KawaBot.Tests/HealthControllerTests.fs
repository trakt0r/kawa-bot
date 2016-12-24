namespace KawaBot.Tests

open System.Net
open KawaBot.Feeder
open KawaBot.Server
open NUnit.Framework

[<TestFixture>]
[<Category(Categories.Integration)>]
type HealthControllerTests =
    val private _server: Http.HttpListener
    val private _feeder: Feeder

    new() as this =
        {
            _server = new Http.HttpListener("http://*:80/")
            _feeder = new Feeder(80)
        } then
            HealthController.GetUrlHandler()
            |> this._server.Register

    [<SetUp>]
    member this.SetUp() =
        this._server.Run()

    [<TearDown>]
    member this.TearDown() =
        this._server.Stop()

    [<Test>]
    member this.TestServerRespondsWithOK() =
        let response = this._feeder.Send("/health") |> Async.RunSynchronously
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK))
        Assert.That(response.Content.ReadAsStringAsync().Result, Is.EqualTo("OK"))
