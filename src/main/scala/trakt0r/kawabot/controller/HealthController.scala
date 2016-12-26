package trakt0r.kawabot.controller

import akka.http.scaladsl.model.{ContentTypes, HttpEntity}
import akka.http.scaladsl.server.Directives._
import akka.http.scaladsl.server.Route

object HealthController {
  def getUrlHandler: Route = {
    path("health") {
      get {
        complete(HttpEntity(ContentTypes.`text/plain(UTF-8)`, "OK"))
      }
    }
  }
}
