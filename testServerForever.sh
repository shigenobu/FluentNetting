#!/bin/sh
dotnet test --filter DisplayName=FluentNetting.Tests.TestServer.TestForever

# exclude
# dotnet test --filter "FullyQualifiedName!=FluentNetting.Tests.TestServer.TestForever" --collect:"XPlat Code Coverage"
