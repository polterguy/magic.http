
# Magic HTTP for .Net

[![Build status](https://travis-ci.org/polterguy/magic.http.svg?master)](https://travis-ci.org/polterguy/magic.http)

An _"opinionated"_, minimalistic, and super simple HTTP REST library for .Net built on .Net Standard. The whole idea
with the library, is to provide a single line invocation for HTTP REST invocations from C# and other CLR languages.
Basically, the library simply wraps `HttpClient`, allowing you to invoke REST endpoints, passing in your own types,
that are automatically serialized to JSON, and return types from your endpoints, automatically de-serialized as JSON.
Below you can see an example of usage.

```csharp
var result = await client.GetAsync<Blog[]>("https://my-json-server.typicode.com/typicode/demo/posts");
```

The idea is that you provide your request type (if any), and your response type as generic arguments to its
`IHttpClient` interface methods, and the library will perform automatic conversion on your behalf, reducing your
HTTP REST invocations to a single line of code, resembling _"normal method invocations"_. This provides an
extremely simply to use API, and also allows you to have large amount of cohesion where you consume the library.

The library supports the 4 most commonly HTTP verbs, below is a list.

* POST
* GET
* PUT
* DELETE

The `POST` and `PUT` verbs requires payload objects, that you provide as typed generic arguments, while the `DELETE` and `GET`
verbs does not allow you to supply payloads. In addition you can optionally supply a _"token"_, normally a JWT token, that
will be transmitted in the HTTP Authorize header of your request, as a _"Bearer"_ token. In addition the library will
_"intelligently"_ handle both `Stream` requests and `Stream` responses for POST, PUT and GET (not DELETE), allowing you to
either supply a `Stream` as a request object (PUT and POST), which will serialize your stream directly on to the HTTP request
stream, or by using the overload of the `GetAsync` method that requires you to supply an `Action` taking one stream, from
where you can directly access the HTTP response stream. This allows you to serialize large files, without first loading them
into memory, both as request payloads and as response return values.

Apart from the above features, the library does not really give you much options, and is to be considered an _"opinionated"_
HTTP REST library, and its purpose is not to support every possible configuration you can imagine, since its purpose
is first and foremost to be dead simple to use, and force creation of better HTTP REST endpoint consumption and creation.
However, if you have that _"one feature request you simply must have"_, feel free to supply a request in the
[issues](https://github.com/polterguy/magic.http/issues). If it makes the API more complex, I might not want to support
it though.

## Dependency injection

The library is created to be _"dependency injection friendly"_, which implies you can use its `IHttpClient` interface
for accessing its functionality, using something such as the following to wire up your interface to its implementation,
for then to retrieve instances to its implementation using dependency injection later in your own code.

```code
/*
 * Somewhere where you wire up your IoC provider.
 */
services.AddTransient<IHttpClient, HttpClient>();
```

Most methods in its `HttpClient` implementation class is also virtual, allowing you to extend it, by inheriting from
its base class, and override whatever method you simply must change for some reasons.

However, the library is first and foremost created to support JSON and/or large files as payloads and response values,
and the _"philosophy"_ of the library is to allow you to create HTTP REST requests with a _single line of code_, to
such facilitate for simplified code, and great cohesion in your own code. The library will not support every single
permutation of HTTP requests possible to create, due to that this would complicate its code, and result in that it over
time degradates.

## License

You are allowed to use the library under the terms of the MIT license, which allows you to use it as you see fit, for
any purpose, including in your own closed source software, without any restrictions, besides that you can't sue its
author over its code for any reasons.
