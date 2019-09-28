
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

The library provides 5 dead simple methods throughs its `IHttpClient`, which maps to the above HTTP verbs somehow.
These methods should be fairly easily understood by most C# developers.

* PostAsync(url, request)
* PutAsync(url, request)
* GetAsync(url)
* GetAsync(url, callback)
* DeleteAsync(url)

The `POST` and `PUT` verbs requires payload objects, that you provide as typed generic arguments, while the `DELETE` and `GET`
verbs does not allow you to supply payloads. In addition you can optionally supply a _"token"_, normally a JWT token, that
will be transmitted in the HTTP Authorize header of your request, as a _"Bearer"_ token. You can also explicitly declare
which type of `Content-Type` your PUT and POST requests are having as their payloads.

In addition the library will _"intelligently"_ handle `Stream` requests for POST and PUT, allowing you to
supply a `Stream` as a request object, which will serialize your stream directly on to the HTTP request
stream, without loading its content into memory first. The `GetAsync` method also has an overload allowing
you to access the response stream directly, allowing you to download large files, without first loading them
into memory.

Apart from the above features, the library does not really give you much options, and is to be considered an _"opinionated"_
HTTP REST library, and its purpose is not to support every possible configuration you can imagine, since its purpose
is first and foremost to be dead simple to use, and force creation of better HTTP REST endpoint consumption and creation.
However, if you have that _"one feature request you simply must have"_, feel free to supply a request in the
[issues](https://github.com/polterguy/magic.http/issues). If it makes the API more complex, I might not want to support
it though.

## Installation

You can either download the latest release, or install it directly through NuGet using the following installation
command.

```
Install-Package magic.http
```

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

Most methods in its `HttpClient` implementation class is also virtual, allowing you to extend the base `HttpClient`
implementation, by inheriting from its base class, and override whatever method you simply must change for some reasons.

However, the library is first and foremost created to support JSON and/or large files as payloads and response values,
and the _"philosophy"_ of the library is to allow you to create HTTP REST requests with a _single line of code_, to
such facilitate for simplified code, and great cohesion in your own code. The library will not support every single
permutation of HTTP requests possible to create, due to that this would complicate its code, and results in that it
degradates over time.

## License

Copyright (c) 2019 Thomas Hansen, thomas@gaiasoul.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.