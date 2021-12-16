#!/bin/sh
dotnet test --filter DisplayName=FluentNest.Tests.TestServer.TestForever

# exclude
# dotnet test --filter "FullyQualifiedName!=FluentNest.Tests.TestServer.TestForever" --collect:"XPlat Code Coverage"
