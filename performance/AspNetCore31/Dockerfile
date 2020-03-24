# https://hub.docker.com/_/microsoft-dotnet-core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY Directory.Build.props Directory.Build.props
COPY src/Directory.Build.props src/Directory.Build.props
COPY performance/Directory.Build.props performance/Directory.Build.props

COPY src/Datadog.Trace/*.csproj src/Datadog.Trace/
COPY performance/Datadog.RuntimeMetrics/*.csproj performance/Datadog.RuntimeMetrics/
COPY performance/Datadog.RuntimeMetrics.Hosting/*.csproj performance/Datadog.RuntimeMetrics.Hosting/
COPY performance/AspNetCore31/*.csproj performance/AspNetCore31/

RUN dotnet restore performance/AspNetCore31/

# copy everything else and build app
COPY src/Datadog.Trace/ src/Datadog.Trace/
COPY performance/Datadog.RuntimeMetrics/ performance/Datadog.RuntimeMetrics/
COPY performance/Datadog.RuntimeMetrics.Hosting/ performance/Datadog.RuntimeMetrics.Hosting/
COPY performance/AspNetCore31/ performance/AspNetCore31/
COPY Datadog.Trace.snk Datadog.Trace.snk
COPY stylecop.json stylecop.json
COPY GlobalSuppressions.cs GlobalSuppressions.cs
COPY src/GlobalSuppressions.cs src/GlobalSuppressions.cs
RUN dotnet publish performance/AspNetCore31/ -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./

ARG TRACER_VERSION=1.15.0
RUN mkdir -p /opt/datadog
RUN mkdir -p /var/log/datadog/dotnet
RUN curl -L https://github.com/DataDog/dd-trace-dotnet/releases/download/v$TRACER_VERSION/datadog-dotnet-apm-$TRACER_VERSION.tar.gz | tar xzf - -C /opt/datadog

ENV CORECLR_ENABLE_PROFILING=0
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH_64=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
ENV DD_INTEGRATIONS=/opt/datadog/integrations.json

ENTRYPOINT ["dotnet", "AspNetCore31.dll"]