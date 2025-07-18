# Sweepo Server

A .NET 8 backend server for the [Sweepo UI](https://github.com/coding-by-feng/sweepo-ui) project. This server provides an API endpoint to receive quote requests from the frontend and send them via email to preconfigured recipients.

## Features

- **Quote Request API**: Accepts quote requests with validation
- **Email Integration**: Sends formatted emails using SMTP
- **Template System**: Dynamic email templates (HTML and text)
- **Logging**: Comprehensive logging with Serilog
- **Health Check**: Built-in health check endpoint
- **CORS Support**: Configured for frontend integration
- **Docker Support**: Ready for containerized deployment

## API Endpoints

### POST /api/quote
Submit a new quote request.

**Request Body:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "+1234567890",
  "service": "home-cleaning",
  "address": "123 Main St, City, State",
  "message": "I need weekly cleaning service"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Quote request submitted successfully! We'll contact you within 24 hours.",
  "requestId": "abc12345"
}
```

### GET /api/quote/health
Health check endpoint.

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Configuration

### Email Settings
Configure email settings in `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "EnableSsl": true,
    "FromEmail": "your-email@gmail.com",
    "FromName": "Sweepo Team",
    "RecipientEmails": [
      "admin@sweepo.com",
      "quotes@sweepo.com"
    ],
    "Subject": "New Quote Request from Sweepo"
  }
}
```

### Gmail Setup
For Gmail SMTP:
1. Enable 2-factor authentication
2. Generate an App Password
3. Use the App Password in `SmtpPassword`

## Local Development

### Prerequisites
- .NET 8 SDK
- Visual Studio Code or Visual Studio

### Setup
1. Clone the repository
2. Update email configuration in `appsettings.Development.json`
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

The server will start at `https://localhost:7000` and `http://localhost:5000`.

### Testing
- Swagger UI: `https://localhost:7000/swagger`
- Health check: `https://localhost:7000/api/quote/health`

## Docker Deployment

### Build and Run with Docker
```bash
# Build the image
docker build -t sweepo-server .

# Run the container
docker run -p 8080:8080 \
  -e EmailSettings__SmtpUsername=your-email@gmail.com \
  -e EmailSettings__SmtpPassword=your-app-password \
  -e EmailSettings__FromEmail=your-email@gmail.com \
  -e EmailSettings__RecipientEmails__0=admin@sweepo.com \
  sweepo-server
```

### Docker Compose
1. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` with your email configuration
3. Run with Docker Compose:
   ```bash
   docker-compose up -d
   ```

### Production Deployment
The server is configured to run on port 8080 in production. Make sure to:
- Set up proper SSL/TLS termination (nginx, load balancer)
- Configure environment variables securely
- Set up log rotation for the logs directory
- Monitor the health check endpoint

## Service Types
The API supports these service types:
- `home-cleaning` → Home Cleaning
- `commercial-cleaning` → Commercial Cleaning
- `pest-control` → Pest Control
- `garbage-removal` → Garbage Removal
- `lawn-garden` → Lawn & Garden
- `car-valet` → Car Valet

## Logging
Logs are written to:
- Console (development)
- `logs/sweepo-server-{date}.txt` files
- Structured logging with Serilog

## CORS Configuration
The server is configured to accept requests from:
- `http://localhost:3000` (React dev server)
- `http://localhost:5173` (Vite dev server)
- `https://sweepo-ui.vercel.app`
- `https://sweepo.com`

Update the CORS policy in `Program.cs` for additional origins.

## License
This project is part of the Sweepo application suite.
