FROM mcr.microsoft.com/dotnet/sdk:6.0 AS runtime

WORKDIR /DbCreationUtility

COPY ./src/DbCreationUtility/ ./
COPY ./docker/DbCreationUtility/ ./

RUN chmod +x /DbCreationUtility/entrypoint.sh

ENTRYPOINT ["/DbCreationUtility/entrypoint.sh"]