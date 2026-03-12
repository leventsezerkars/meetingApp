# Stage 1: Build Angular
FROM node:22-alpine AS angular-build
WORKDIR /app/web
COPY src/ToplantiApp.Web/package*.json ./
RUN npm ci
COPY src/ToplantiApp.Web/ .
RUN npx ng build --configuration production

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS api-build
WORKDIR /app
COPY ToplantiCaseApp.slnx ./
COPY src/ToplantiApp.Domain/*.csproj src/ToplantiApp.Domain/
COPY src/ToplantiApp.Application/*.csproj src/ToplantiApp.Application/
COPY src/ToplantiApp.Infrastructure/*.csproj src/ToplantiApp.Infrastructure/
COPY src/ToplantiApp.API/*.csproj src/ToplantiApp.API/
RUN dotnet restore ToplantiCaseApp.slnx
COPY src/ src/
RUN dotnet publish src/ToplantiApp.API/ToplantiApp.API.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=api-build /app/publish .
COPY --from=angular-build /app/web/dist/ToplantiApp.Web/browser ./wwwroot

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ToplantiApp.API.dll"]
