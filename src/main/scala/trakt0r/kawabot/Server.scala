package trakt0r.kawabot

import akka.actor.ActorSystem
import akka.http.scaladsl.Http
import akka.http.scaladsl.server.Directives._
import akka.http.scaladsl.server.Route
import akka.stream.ActorMaterializer
import com.typesafe.scalalogging.LazyLogging
import trakt0r.kawabot.controller.HealthController

import scala.concurrent.duration._
import scala.concurrent.{Await, Future}
import scala.io.StdIn

class Server(host: String, port: Int, routes: Route*) extends LazyLogging {
  private implicit val system = ActorSystem("server")
  private implicit val materializer = ActorMaterializer()
  private implicit val executionContext = system.dispatcher

  var bindingFuture: Future[Http.ServerBinding] = _

  def start(): Unit = {
    bindingFuture = Http().bindAndHandle(concat(routes:_*), host, port)
    logger.info(s"Listening to $host:$port started")
  }

  def stop(): Unit = {
    Await.ready(bindingFuture.flatMap(_.unbind()), 1.second)
    logger.info(s"Listening to $host:$port stopped")
  }

  def dispose(): Unit = {
    system.terminate()
  }
}

object Server {
  private val Host = "localhost"
  private val Port = 50000

  def main(args: Array[String]): Unit = {
    val server = new Server(Host, Port, HealthController.getUrlHandler)
    server.start()
    StdIn.readLine()
    server.stop()
  }
}
