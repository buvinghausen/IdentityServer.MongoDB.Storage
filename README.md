# IdentityServer.MongoDB.Storage
MongoDB based persistence for IdentityServer's configuration and operational stores

Here is a screen shot of the telemetry produced from hitting the /.well-known/openid-configuration endpoint. As you can see the call to GetAllResources hits the Resources collection once in Mongo because of the polymorphic storage capabilities. Also of note is the traceId of c720610851587e4ab48e4115064cf1fb carries correctly across the telemetry from both ASP.NET Core and Mongo.  Export that into your favorite endpoint (Jaeger, Zipkin, Prometheus, etc) and you're off to the races.
![image](https://user-images.githubusercontent.com/1130210/127034176-4d51c4bb-7f92-4c17-b2b4-18d0a8a6a0c1.png)

Here's another telemetry screenshot from a token validation request to /connect/token you can actually see the traceId 08609f5e026b9642806c02fb2dfb67a1 yielded five separate calls to Mongo (once to get the client, once to get the matching ApiResources, once to get the matching IdentityResources, and twice to get the ApiScopes) the last of which appears very much to be a bug in IdentityServer...  The good thing about observability is the instrumentation doesn't lie.
![image](https://user-images.githubusercontent.com/1130210/127046771-859d98ac-07d7-4205-88db-7cf73408a2ad.png)
