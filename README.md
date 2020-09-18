# Proge-software Teams Connector powered by Microsoft Graph API

[![NuGet Version](https://buildstats.info/nuget/Proge.Teams.Edu.Abstraction)](https://www.nuget.org/packages/Proge.Teams.Edu.Abstraction/)
[![NuGet Version](https://buildstats.info/nuget/Proge.Teams.Edu.GraphApi)](https://www.nuget.org/packages/Proge.Teams.Edu.GraphApi/)
[![NuGet Version](https://buildstats.info/nuget/Proge.Teams.Edu.Esse3)](https://www.nuget.org/packages/Proge.Teams.Edu.Esse3/)
[![NuGet Version](https://buildstats.info/nuget/Proge.Teams.Edu.DAL)](https://www.nuget.org/packages/Proge.Teams.Edu.DAL/)


Integrate Teams and its Education related capabilities in your application 
through the [Microsoft Graph API](https://graph.microsoft.io) 

The Proge-Software Teams Connector targets .NetStandard 2.0

## Installation via NuGet

To install the client library via NuGet:

Search in the NuGet Library for 
* `Proge.Teams.Edu.Abstraction` [required]
* `Proge.Teams.Edu.GraphApi` [required]
* `Proge.Teams.Edu.Esse3` [optional]
* `Proge.Teams.Edu.DAL` [optional]

or type 

* `Install-Package Proge.Teams.Edu.Abstraction` [required]
* `Proge.Teams.Edu.GraphApi` [required]
* `Proge.Teams.Edu.Esse3` [optional]
* `Install-Package Proge.Teams.Edu.DAL` [optional]

into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application to use Microsoft Graph API using the [Microsoft Application Registration Portal](https://aka.ms/appregistrations).

### 2. Authenticate for the Microsoft Graph service

The Teams Connector use the following Microsoft MSAL authentication implementations
* `ClientCredentialProvider` for Application-type integrations
* `UsernamePasswordProvider` for Delegated-type integration demanded by certain operation like "Join Code generation" for Team

### 4. Compose the requests to the graph to satisfy your needs

Once you have completed authentication , you can begin to make calls to the service

In `src` folder there's an example how to integrate the calls inside a job that can be
placed behind a Console Application, a WebJob or a WebApi project.

Based on the .net built in dependancy injection Engine


## License

Copyright (c) Microsoft Corporation. All Rights Reserved. Licensed under the MIT [license](LICENSE.txt). See [Third Party Notices](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/master/THIRD%20PARTY%20NOTICES) for information on the packages referenced via NuGet.
