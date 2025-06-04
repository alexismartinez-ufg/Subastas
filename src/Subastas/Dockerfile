# syntax=docker/dockerfile:1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

COPY . /source
WORKDIR /source/Subastas

ARG TARGETARCH

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a ${TARGETARCH/amd64/x64} --use-current-runtime --self-contained false -o /app

# 🔄 USAMOS SDK AQUÍ TAMBIÉN PARA DOTNET EF
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS final

WORKDIR /app

# ICU para globalización
RUN apk add --no-cache icu-libs

COPY --from=build /app .

# 🔧 Instalamos dotnet-ef y exportamos path
ENV DOTNET_TOOLS=/root/.dotnet/tools
ENV PATH="${PATH}:${DOTNET_TOOLS}"

RUN dotnet tool install --global dotnet-ef

EXPOSE 8080

ENTRYPOINT ["dotnet", "Subastas.dll"]
