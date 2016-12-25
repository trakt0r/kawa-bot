namespace KawaBot.Feeder

open System
open System.Net.Http
open Newtonsoft.Json
open System.Text

type Feeder =
    val private _client: HttpClient
    val private _port: int

    new(port: int) as this =
        {
            _client = new HttpClient()
            _port = port
        }
        then
            this._client.BaseAddress <- new Uri(String.Format("http://localhost:{0}", this._port))

    member this.Send(message: string, ?method: HttpMethod): Async<HttpResponseMessage> =
        new HttpRequestMessage(defaultArg method HttpMethod.Get, message)
        |> this._client.SendAsync
        |> Async.AwaitTask

    member this.PostJSON(message: string, body: 'a) =
        let msg = new HttpRequestMessage(HttpMethod.Post, message)
        let bodyJson = JsonConvert.SerializeObject(body, Formatting.Indented)
        let bac = new ByteArrayContent(Encoding.UTF8.GetBytes(bodyJson))
        bac.Headers.Add("Content-Type", "application/json")
        msg.Content <- bac
        msg
        |> this._client.SendAsync
        |> Async.AwaitTask
