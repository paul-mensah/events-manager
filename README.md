# Events Manager API

## Pre-requirements
* Jetbrains Rider or Visual Studio 2019 (or higher)
* .NET Core SDK (version 6.0 or higher)
* MySQL
* Redis
* Elasticsearch
* Docker

## Setup
* `Docker`: Download Docker Desktop [here](https://www.docker.com/products/docker-desktop/) if not
  installed on your pc and follow instructions [here](https://docs.docker.com/get-docker/)
  to set it up.
* `.NET 6`: Download .NET 6 [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and proceed to your OS link for installation guide
    * Windows Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60
    * Linux Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/linux
    * macOS Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/macos
* `IDEs`: Click [here](https://www.jetbrains.com/rider/download/) to download Jetbrains Rider and
  [here](https://visualstudio.microsoft.com/downloads/) for Visual Studio 2022

## How To Run
* Open solution in Jetbrains Rider or Visual Studio
* Build the solution.
* Navigate to the root folder in terminal/command prompt and execute this command `docker-compose up` which will pull and
install all required images for the project.
* Navigate to [http://localhost:5467/swagger/index.html](http://localhost:5467/swagger/index.html) in your browser to load up
the swagger to interact with the web APIS.

### NB
* When the database connection is setup and configured successfully in appsettings, the database and tables will be
  created automatically on the first start of the application.
* There is a Postman collection included in the <b>docs</b> folder to test the endpoints; also a swagger page will be 
loaded when the api runs
* Events API uses elasticsearch for storage of events and Event Invitations API uses Redis for caching and MySQL.
