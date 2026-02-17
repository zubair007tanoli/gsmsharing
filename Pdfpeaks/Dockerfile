# Pdfpeaks - Modernized .NET 10 Application with AI Integration
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY Pdfpeaks/*.csproj Pdfpeaks/
RUN dotnet restore Pdfpeaks/Pdfpeaks.csproj

# Copy all source files
COPY Pdfpeaks/ Pdfpeaks/

# Build the application
WORKDIR /src/Pdfpeaks
RUN dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Install additional dependencies for PDF processing
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    fonts-liberation \
    fonts-noto \
    fonts-noto-cjk \
    fonts-opensymbol \
    fonts-ubuntu \
    fonts-dejavu \
    fonts-freefont \
    fonts-ghostscript \
    fonts-linux-libertine \
    fonts-sil-nanum \
    fonts-wqy-microhei \
    fonts-arphic-uming \
    fonts-arphic-ukai \
    libreoffice \
    libreoffice-writer \
    libreoffice-calc \
    libreoffice-impress \
    && rm -rf /var/lib/apt/lists/* \
    && fc-cache -f -v

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -g 1000 appgroup && \
    useradd -u 1000 -g appgroup -m -s /bin/bash appuser

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Configure Kestrel
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Pdfpeaks.dll"]
