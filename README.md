# Dapr Workflow with Stripe

## Running locally

### 1a. Pre-requisites to use this repo with the devcontainer

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [VSCode](https://code.visualstudio.com/)
- [DevContainers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) Extension for VSCode

The devcontainer configuration that is included in this repo contains the .NET SDK, installs the Dapr CLI, and initializes Dapr.

### 1b. Pre-requisites to use this repo without a devcontainer

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/)
- [Initialize Dapr](https://docs.dapr.io/getting-started/install-dapr-selfhost/)

### 2. Create a Stipe API key



### 3. Run the Workflow application

Open a terminal in the root of the repository and start the Workflow application using the Dapr CLI:

```bash
dapr run --app-id workflow-app --resources-path ./Resources --app-port 5253 --dapr-http-port 3516 -- dotnet run --project ./SetupStripeWorkflowApp/
```

## 4. Starting the SetupStripeWorkflow

If you're using the devcontainer, open the [setupstripe.http](/LocalTests/setupstripe.http) file and use the REST Client extension to make requests to the WorkflowApp.

## 5. Starting the SetupStripeWorkflow
