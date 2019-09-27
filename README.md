
# Magic HTTP for .Net

[![Build status](https://travis-ci.org/polterguy/magic.http.svg?master)](https://travis-ci.org/polterguy/magic.http)

A minimalistic HTTP REST library for .Net built for .Net Standard. The whole idea with the library, is to provide a
single line invocation for HTTP REST invocations from C# and other CLR languages. Below you can see an example of usage.

```csharp
var result = await client.GetAsync<Blog[]>("https://my-json-server.typicode.com/typicode/demo/posts");
```

The idea is tha tyou provide your request type (if any), and your response type as generic arguments to its
`IHttpClient` interface, and the library will perform automatic conversion on your behalf, reducing your
HTTP REST invocations to a single line of code, resembling _"normal method invocations"_.

The library supports all 4 most commonly verbs, below is a list.

* POST
* GET
* PUT
* DELETE

The `POST` and `PUT` verbs requires payload objects, that you provide as typed generic arguments, while the `DELETE` and `GET`
verbs does not. In addition you can optionally supply a _"token"_, normally a JWT token, that will be transmitted in the
HTTP Authorize header of your request, as a _"Bearer"_ token. In addition the library will _"intelligently"_ handle both `Stream`
requests and `Stream` responses for POST, PUT and GET (not DELETE), allowing you to either supply a `Stream` as a request
object, which will serialize your stream directly on to the HTTP request stream, or by using the overload of the `GetAsync`
method that requires you to supply an `Action` taking one stream, from where you can directly access the HTTP response stream.
This allows you to serialize large files, without first loading them into memory.

Apart from the above features, the library does not really give you much options, and is to be considered an _"opinionated"_
HTTP REST library, and its purpose is not to support every possible configuration you can imagine, since its purpose
is first and foremost to be dead simple, yet still hyper optimal in usage, taking advantage of the _"newer"_ stuff
from .Net, such as `HttpClient` (that it's using internally), and async methods.
