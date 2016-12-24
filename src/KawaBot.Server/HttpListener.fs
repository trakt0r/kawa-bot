namespace KawaBot.Server

open System
open System.Collections.Generic
open System.Net

module Http =
    type HandlerFunc = HttpListenerRequest -> HttpListenerResponse -> Async<unit>

    type UrlHandler(url: string, handler: HandlerFunc) =
        member this.Url: string = url
        member this.Handle: HandlerFunc = handler

    type HttpListener =
        val private _url: string
        val private _handlers: Dictionary<string, UrlHandler>

        new(url: string) =
            {
                _url = url;
                _handlers = Dictionary();
            }

        member this.Register(handler: UrlHandler) =
            this._handlers.Add(handler.Url, handler);
            
        member this.Run() = 
            let listener = new System.Net.HttpListener()
            listener.Prefixes.Add this._url
            listener.Start()
            async {
                printfn "[%s] Listening to %s started" (Utils.ToDateTimeString DateTime.Now) this._url
                printfn "[%s] Press any key to exit..." (Utils.ToDateTimeString DateTime.Now)
                while true do 
                    let! context = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
                    let url = context.Request.RawUrl.Substring(1).Split('?').[0];
                    match this._handlers.TryGetValue(url) with
                        | true, handler -> this.Invoke(handler, context) |> Async.Start
                        | _ -> ()
            }
            |> Async.Start
            |> ignore

        member private this.Invoke(handler: UrlHandler, ctx: HttpListenerContext): Async<unit> =
            async {
                printfn "[%s] GET /%s" (Utils.ToDateTimeString DateTime.Now) handler.Url
                let! _ = handler.Handle ctx.Request ctx.Response
                printfn "[%s] %d %s" (Utils.ToDateTimeString DateTime.Now) ctx.Response.StatusCode ctx.Response.StatusDescription
            }
