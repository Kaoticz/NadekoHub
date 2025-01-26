# Please, see the following url: https://github.com/Kaoticz/NadekoHub/wiki/Docker
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /
RUN apt-get update \
    && apt-get install -y libsm-dev libice-dev libx11-dev fontconfig \
    && rm -rf /var/lib/apt/lists/* \
    && useradd -m -u 1000 nadekohub_user

FROM base AS publish
ARG DOTNET_BUILD=Debug
WORKDIR /home/nadekohub_user/src
COPY . .
ADD https://raw.githubusercontent.com/Kaoticz/NadekoHub/refs/heads/main/.editorconfig .editorconfig
RUN dotnet publish "NadekoHub.csproj" -c $DOTNET_BUILD -o /home/nadekohub_user/app /p:SelfContained=false /p:PublishSingleFile=false /p:IncludeNativeLibrariesForSelfExtract=false \
    && chown -R nadekohub_user:nadekohub_user /home/nadekohub_user

FROM base AS final
USER nadekohub_user
WORKDIR /home/nadekohub_user/app
COPY --from=publish /home/nadekohub_user/app .
ENTRYPOINT ["dotnet", "NadekoHub.dll"]
