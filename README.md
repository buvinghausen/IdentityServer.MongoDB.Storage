# IdentityServer.MongoDB.Storage
MongoDB based persistence for IdentityServer's configuration and operational stores

Here is a screen shot of the telemetry produced from hitting the .well-known/openid-configuration endpoint. As you can see the call to GetAllResources hits the Resources collection once in Mongo because of the polymorphic storage capabilities. Also of note is the traceId of c3e791884cbc114a8e503787ce0d3f18 carries correctly across the telemetry from both ASP.NET Core and Mongo.  Export that into your favorite endpoint (Jaeger, Zipkin, Prometheus, etc) and you're off to the races.
![image](https://user-images.githubusercontent.com/1130210/127032137-e82c47b4-3644-4a5a-9528-de19e6b5cc25.png)
