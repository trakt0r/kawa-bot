namespace KawaBot.Server

open System.Net

module Http =
    type HttpListener with
        static member Run (url: string, handler: (HttpListenerRequest -> HttpListenerResponse -> Async<unit>)) = 
            let listener = new HttpListener()
            listener.Prefixes.Add url
            listener.Start()
            let asyncTask = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
            async {
                while true do 
                    let! context = asyncTask
                    Async.Start (handler context.Request context.Response)
            } |> Async.Start 
            listener
