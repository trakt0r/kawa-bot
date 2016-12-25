namespace KawaBot.Server

open System
open System.Collections.Generic
open System.Net
open NLog

module Http =
    type HandlerFunc = HttpListenerRequest -> HttpListenerResponse -> Async<unit>

    type UrlHandler(url: string, handler: HandlerFunc) =
        member this.Url: string = url
        member this.Handler: HandlerFunc = handler

    let Logger = LogManager.GetLogger("KawaBot.Server")

    type HttpListener =
        val private _url: string
        val private _handlers: Dictionary<string, UrlHandler>
        val private _handlerNotFound: HandlerFunc
        
        [<DefaultValue>]
        val mutable private _listener: System.Net.HttpListener

        new(url: string) =
            {
                _url = url
                _handlers = Dictionary()
                _handlerNotFound = fun req resp -> async {
                    resp.StatusCode <- (int)HttpStatusCode.NotFound
                }
            }

        member this.Register(handler: UrlHandler): unit =
            this._handlers.Add(handler.Url, handler);
            
        member this.Run(): unit = 
            this._listener <- new System.Net.HttpListener()
            this._listener.Prefixes.Add this._url
            this._listener.Start()
            async {
                Logger.Info("Listening to {0} started", this._url)
                Logger.Info("Press any key to exit...")
                let mutable keepRunning = true
                while keepRunning do
                    match this.GetContext() with
                        | None -> keepRunning <- false
                        | Some(context) ->
                            let url = context.Request.RawUrl.Substring(1).Split('?').[0];
                            let invokeAsync = fun handle ->
                                this.Invoke(context.Request.RawUrl, handle, context)
                                |> Async.Start
                            match this._handlers.TryGetValue(url) with
                                | true, handler -> invokeAsync handler.Handler
                                | _ -> invokeAsync this._handlerNotFound
                Logger.Info("Listening to {0} stopped", this._url)
            }
            |> Async.Start
            |> ignore

        member this.Stop(): unit =
            this._listener.Stop()

        member private this.GetContext(): option<HttpListenerContext> =
            try
                Some(this._listener.GetContext())
            with
                | :? HttpListenerException as e ->
                    if e.ErrorCode = 995 then // ERROR_OPERATION_ABORTED
                        None
                    else
                        reraise()

        member private this.Invoke(url: string, handle: HandlerFunc, ctx: HttpListenerContext): Async<unit> =
            async {
                Logger.Info("{0} {1}", ctx.Request.HttpMethod, url)
                let! _ = handle ctx.Request ctx.Response
                ctx.Response.Close()
                Logger.Info("{0} {1}", ctx.Response.StatusCode, ctx.Response.StatusDescription)
            }
