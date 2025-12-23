FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link *.csproj .
RUN dotnet restore 

# Copy source code and publish app
COPY --link . .
RUN dotnet publish MCBA.csproj  --no-restore -o /app


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
EXPOSE 8080
EXPOSE 8081
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["./MCBA"]