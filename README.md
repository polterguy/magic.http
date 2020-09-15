
# Magic HTTP

[![Build status](https://travis-ci.com/polterguy/magic.http.svg?master)](https://travis-ci.com/polterguy/magic.http)

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
extremely simply to use API, and also allows you to have large amount of cohesion in your own code.

The library supports the 4 most commonly HTTP verbs, below is a list.

* POST
* GET
* PUT
* DELETE

The library provides 5 dead simple methods throughs its `IHttpClient` interface, which maps to the above HTTP verbs
somehow. These methods should be fairly easily understood by most C# developers.

* PostAsync(url, request)
* PutAsync(url, request)
* GetAsync(url)
* GetAsync(url, callback)
* DeleteAsync(url)

The `POST` and `PUT` verbs requires payload objects, that you provide as typed generic arguments, while the `DELETE` and `GET`
verbs does not allow you to supply payloads. In addition you can optionally supply a dictionary of HTTP headers, that
will correctly decorate your HTTP content and your HTTP request message, depending upon where it belongs. There are also
convenience methods for invoking HTTP REST endpoints with _"Bearer"_ authorization tokens, automatically taking care of
adding your tokens correctly to your requests.

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

The `HttpClient` implementation class, also provides multiple CTORs, allowing you to pass in `ILogger`, and
a _"factory function"_ for creating .Net `HttpClient` instances, which allows you to use among other things
the `IHttpClientFactory` in .Net Core projects, without bringing in dependencies upon it, which would break
.Net Framework compatibility.

* [Main documentation](https://polterguy.github.io/)

## Quality gates

- [![Build status](https://travis-ci.com/polterguy/magic.http.svg?master)](https://travis-ci.com/polterguy/magic.http)
- [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=alert_status)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=bugs)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=code_smells)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=coverage)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=ncloc)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=security_rating)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=sqale_index)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
- [![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=polterguy_magic.http&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=polterguy_magic.http)
