FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /source

COPY *.sln .
COPY PaymentService.Console/*.csproj ./PaymentService.Console/

RUN dotnet restore "PaymentService.Console/PaymentService.Console.csproj"

COPY . .

WORKDIR /source/PaymentService.Console
RUN dotnet publish -c Release -o /app/publish /p:EnvironmentName=Docker

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_ENVIRONMENT=Docker
ENTRYPOINT ["dotnet", "PaymentService.Console.dll"]