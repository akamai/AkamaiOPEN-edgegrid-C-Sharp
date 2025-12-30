# Dockerfile for CI/CD testing
# Builds a minimal image with .NET SDK and project files
# Commands are executed at runtime by Jenkins

FROM docker.akamai.com/microsoft-mcr-upstream/dotnet/sdk:8.0

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
