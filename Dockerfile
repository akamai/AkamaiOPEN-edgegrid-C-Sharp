# Dockerfile for CI/CD testing
# Builds a minimal image with .NET SDK and project files
# Commands are executed at runtime

FROM docker.akamai.com/microsoft-mcr-upstream/dotnet/runtime:10.0 AS net10-runtime
FROM docker.akamai.com/microsoft-mcr-upstream/dotnet/sdk:11.0

# Run tests for both target frameworks with the .NET 11 SDK.
COPY --from=net10-runtime /usr/share/dotnet/shared/Microsoft.NETCore.App/ /usr/share/dotnet/shared/Microsoft.NETCore.App/

# Install git and SSL tools
RUN apt-get update && apt-get install -y \
    git \
    ca-certificates \
    openssl \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /workspace

# Copy project files
COPY . .

# Create results directory
RUN mkdir -p /results/reports

# Set volume for extracting results
VOLUME ["/results"]

# Default command (can be overridden by docker run)
CMD ["bash", "-c", "echo 'Use docker run with custom command to execute CI pipeline'"]
