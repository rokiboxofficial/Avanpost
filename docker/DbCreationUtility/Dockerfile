FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime

WORKDIR /DbCreationUtility

COPY ./src/DbCreationUtility/ ./
COPY ./docker/DbCreationUtility/ ./

ENTRYPOINT ["/DbCreationUtility/entrypoint.sh"]