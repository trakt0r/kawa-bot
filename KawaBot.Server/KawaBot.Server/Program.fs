namespace KawaBot.Server

open System
open System.Net
open Utils
open Http

module Program =
    [<EntryPoint>]
    let main argv = 
        printfn "[%s] Listening to the http://localhost:80/ started" (Utils.ToDateTimeString DateTime.Now)

        HttpListener.Run("http://*:80/Health/", (fun req resp -> 
                async {
                    printfn "[%s] Received request to the /Health" (Utils.ToDateTimeString DateTime.Now)
                    let out = Text.Encoding.ASCII.GetBytes "OK"
                    resp.OutputStream.Write(out, 0, out.Length)
                    resp.OutputStream.Close()
                    printfn "[%s] Sent response" (Utils.ToDateTimeString DateTime.Now)
                }
            )) |> ignore

        printfn "Press any key to exit..."
        Console.ReadKey true |> ignore
        0
