package trakt0r.kawabot

import akka.actor.ActorSystem
import akka.http.scaladsl.Http
import akka.http.scaladsl.model.{HttpRequest, HttpResponse}
import akka.http.scaladsl.model.StatusCodes._
import akka.http.scaladsl.unmarshalling.Unmarshal
import akka.stream.ActorMaterializer
import org.scalatest._
import trakt0r.kawabot.controller.HealthController

import scala.concurrent.Await
import scala.concurrent.duration._

class BasicControllerSpec extends FlatSpec with Matchers with BeforeAndAfterAll {
  private implicit val system = ActorSystem("tests")
  private implicit val materializer = ActorMaterializer()
  private implicit val executionContext = system.dispatcher

  private val host = "localhost"
  private val port = 49999
  private val server = new Server(host, port, HealthController.getUrlHandler)

  override def afterAll(): Unit = {
    server.dispose()
  }

  private def testGet(url: String): HttpResponse = {
    try {
      server.start()
      Await.result(Http().singleRequest(HttpRequest(uri = s"http://$host:$port$url")), 1.second)
    } finally {
      server.stop()
    }
  }

  private def getBody(response: HttpResponse): String = {
    Await.result(Unmarshal(response.entity).to[String], 1.second)
  }

  "A server" should "respond to non-existing endpoints with error 404" in {
    val response = testGet("/non-existing")
    response.status shouldBe NotFound
    getBody(response) shouldBe "The requested resource could not be found."
  }

  it should "respond to /health with OK" in {
    val response = testGet("/health")
    response.status shouldBe OK
    getBody(response) shouldBe "OK"
  }
}
