# Azure Deployment Scripts

This directory contains deployment scripts ready to use. You need the Azure CLI (`az`) installed and must log in to the account you want to deploy with.

## Setting up your Azure Resources

To run this application you need a web server that runs .NET 7, that's it. You do not need Node because `dotnet build` builds all of the front end components, and this also happens when you run `dotnet publish`.

For convenience, we've added a setup script in this here directory called `app_service.sh`, which is a script that creates the necessary services on Azure for you. 

From the server project root:

```
source ./Deployment/Azure/app_service.sh
```

Please have a read and change things as you need **before you run this script**. It will run in Powershell as well as bash and the only thing you need to do is **change the variables at the top**.

## Deploying

When you run the `app_service.sh` script, it creates a second script for you called `zip.sh`. This sends your code to Azure hardware by zipping and pushing it:

```
source ./Deployment/Azure/zip.sh
```

For fun and convenience we've added a Makefile which will execute this for you:

```
make app_service && make
```

Yay for Make!

## What's going on during deployment

Once everything is ready (after `dotnet publish`), find your deployment artifacts in `/bin/Release/net7.0/publish`. This directory includes the Svelte application, which `dotnet publish` builds along with the ASP.NET application.

The built Svelte application lives in `wwwroot` in that directory, which is where it needs to be.

The deployment process zips up this entire directory and pushes it to Azure. Once complete, a browser opens to your site's directory and streams the application logs so you can monitor progress.

If there are any problems you can drop all of your resources using `az group delete -n [NAME]` where `NAME` is the resource group name you came up with (`RG` in the script).

Have fun!

