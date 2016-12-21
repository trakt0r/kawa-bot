open System
open System.Net

type HttpListener with
        static member Run (url:string,handler: (HttpListenerRequest -> HttpListenerResponse -> Async<unit>)) = 
            let listener = new HttpListener()
            listener.Prefixes.Add url
            listener.Start()
            let asynctask = Async.FromBeginEnd(listener.BeginGetContext,listener.EndGetContext)
            async {
                while true do 
                    let! context = asynctask
                    Async.Start (handler context.Request context.Response)
            } |> Async.Start 
            listener

let toDateTimeString (date:DateTime) = date.ToString("dd/MM/yyyy HH:mm:ss:fffff")

[<EntryPoint>]
let main argv = 
    printfn "[%s] Listening to the http://localhost:80/ started" (toDateTimeString DateTime.Now)

    HttpListener.Run("http://*:80/Health/",(fun req resp -> 
            async {
                printfn "[%s] Recieved request to the /Health" (toDateTimeString DateTime.Now)
                let out = Text.Encoding.ASCII.GetBytes "OK"
                resp.OutputStream.Write(out,0,out.Length)
                resp.OutputStream.Close()
                printfn "[%s] Sent response" (toDateTimeString DateTime.Now)
            }
        )) |> ignore

    printfn "Press Enter to exit..."
    Console.Read () |> ignore
    0
