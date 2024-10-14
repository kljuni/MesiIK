MesiIK is a desktop application built with Avalonia UI that provides a HTTP server and client functionality. It allows users to configure server and client settings, send HTTP requests, and view responses.

## Features

- Configurable server inbound and client outbound addresses and ports
- Draggable configuration items
- Send POST HTTP requests with optional headers
- View received requests
- Save and load settings

## Requirements

- .NET 8.0 SDK or later

## Build instructions

1. Clone the repository:
   ```
   git clone https://github.com/kljuni/MesiIK.git
   cd MesiIK
   ```

2. Build the project:
   ```
   dotnet build src/MesiIK.csproj
   ```

## Running the Application

To run the application, use the following command in the project root directory:

```
dotnet run --project src/MesiIK.csproj
```

## Usage

1. Start the application.
2. Configure the server and client settings using the draggable text boxes on the canvas.
3. Click the \"Start\" button to start the server.
4. Enter a message body and optional HTTP headers in the provided text boxes.
5. Click the \"Send\" button to send a POST request.
6. View the received requests in the text box on the right side of the window.

## Configuration

The application allows you to configure the following settings:

- Server Inbound Address
- Server Inbound Port
- Client Outbound Address
- Client Outbound Port

These settings can be dragged around the canvas for easy organization.

## HTTP Server

- For all requests, the server will echo back the received content with a timestamp.

## Saving and Loading Settings

- Click the \"Save Settings\" button to save the current configuration.
- The application saves your configuration settings to a `settings.json` file in the application's root directory.
- Settings are automatically loaded when the application starts.
- Click the \"Reset to Defaults\" button to revert to default settings.

