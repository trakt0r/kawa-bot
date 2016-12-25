namespace KawaBot.Tests

open System.Net
open KawaBot.Feeder
open KawaBot.Server
open NUnit.Framework

[<TestFixture>]
[<Category(Categories.Integration)>]
type BasicControllerTests =
    val private _server: Http.HttpListener
    [<DefaultValue>]
    val mutable private _feeder: Feeder

    new() as this =
        {
            _server = new Http.HttpListener("localhost", 49999)
        } then
            this._feeder <- new Feeder(this._server.Port)
            HealthController.GetUrlHandler()
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
