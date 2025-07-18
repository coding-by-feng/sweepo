# Sweepo Server

A modern ASP.NET Core web API for handling cleaning service quote requests with automated email notifications and Docker deployment.

## 🚀 Features

- **Quote Request API**: RESTful API for submitting cleaning service quotes
- **Email Notifications**: Automated email sending with HTML and text templates
- **Gmail Integration**: Secure Gmail SMTP with App Password authentication
- **Comprehensive Logging**: Structured logging with Serilog and correlation IDs
- **CORS Support**: Cross-origin resource sharing for frontend integration
- **Swagger Documentation**: Interactive API documentation
- **Docker Support**: One-click containerized deployment with automated script
- **Production Ready**: Optimized for Linux deployment with port 1969

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Email**: MailKit for SMTP
- **Logging**: Serilog with file and console output
- **Documentation**: Swagger/OpenAPI
- **Containerization**: Docker with multi-stage builds
- **Templates**: HTML and text email templates

## 🏃‍♂️ Quick Start

### Prerequisites

- .NET 8.0 SDK (for local development)
- Docker (for containerized deployment)
- Gmail account with 2-Factor Authentication enabled
- Gmail App Password (for email functionality)

### 🐳 Docker Deployment (Recommended)

The fastest way to get Sweepo Server running:

```bash
# 1. Set your Gmail App Password
export SWEEPO_FROM_EMAIL_PASSWORD="your-16-char-gmail-app-password"

# 2. Deploy with one command
./deploy.sh
```

That's it! Your server will be running at **http://localhost:1969**

#### Deployment Script Commands

```bash
./deploy.sh          # Build and deploy
./deploy.sh logs     # View real-time logs
./deploy.sh stop     # Stop the container
./deploy.sh restart  # Restart the container
./deploy.sh status   # Check container status
./deploy.sh clean    # Remove everything (container + image)
```

### 💻 Local Development

```bash
# 1. Clone and navigate
git clone <repository-url>
cd windsurf-project

# 2. Set environment variable
export SWEEPO_FROM_EMAIL_PASSWORD="your-gmail-app-password"

# 3. Run with environment loading
source ~/.zshrc && dotnet run

# 4. Access the API
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
```

## 📧 Gmail App Password Setup

1. **Enable 2-Factor Authentication** on your Gmail account
2. Go to **Google Account Settings** → **Security** → **2-Step Verification** → **App passwords**
3. Generate a new app password for **"Mail"** or **"Other (Custom name)"**
4. Copy the **16-character password** (format: `abcdabcdabcdabcd`)
5. Set it as your environment variable:
   ```bash
   export SWEEPO_FROM_EMAIL_PASSWORD="abcdabcdabcdabcd"
   ```

## 🔌 API Endpoints

### POST /api/quote

Submit a new cleaning service quote request.

**Request:**
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "021-123-4567",
  "address": "123 Main St, Auckland",
  "propertyType": "House",
  "service": "Regular Cleaning",
  "frequency": "Weekly",
  "bedrooms": 3,
  "bathrooms": 2,
  "additionalInfo": "Additional requirements..."
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "Quote request submitted successfully! We'll contact you within 24 hours.",
  "requestId": "abc123"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Failed to process quote request. Please try again later.",
  "requestId": "abc123"
}
```

### GET /

Health check endpoint.

**Response:**
```json
{
  "service": "Sweepo Server",
  "version": "1.0.0",
  "status": "running",
  "timestamp": "2025-07-18T10:30:00.000Z"
}
```

## ⚙️ Configuration

### Email Settings (`appsettings.json`)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "",
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

**⚠️ Security Note**: Never put your Gmail password in the config file. Always use the `SWEEPO_FROM_EMAIL_PASSWORD` environment variable.

### Development vs Production

- **Development**: Uses `appsettings.Development.json` with debug logging
- **Production**: Uses `appsettings.json` with optimized logging
- **Docker**: Automatically uses Production environment

## 📁 Project Structure

```
sweepo-server/
├── 📁 Controllers/
│   └── QuoteController.cs      # API endpoints and request handling
├── 📁 Models/
│   ├── QuoteRequest.cs         # Request/response data models
│   └── EmailConfiguration.cs   # Email configuration model
├── 📁 Services/
│   ├── EmailService.cs         # Email sending with SMTP
│   └── TemplateService.cs      # Email template processing
├── 📁 Templates/
│   ├── quote-email.html        # HTML email template
│   └── quote-email.txt         # Plain text email template
├── 📁 logs/                    # Application log files
├── 🐳 Dockerfile              # Docker container configuration
├── 🚀 deploy.sh               # One-click deployment script
├── 📋 docker-compose.yml      # Docker Compose setup
├── ⚙️ appsettings.json        # Production configuration
├── ⚙️ appsettings.Development.json # Development configuration
└── 📖 README.md               # This file
```

## 📊 Logging & Monitoring

### Structured Logging
- **Console Output**: Real-time logs with colors
- **File Logging**: Daily rolling files in `logs/sweepo-server-YYYYMMDD.txt`
- **Correlation IDs**: Each request gets a unique 8-character ID for tracing
- **Log Levels**: Debug, Information, Warning, Error, Fatal

### Log Examples
```
[22:30:15 INF] [abc12345] === QUOTE REQUEST STARTED ===
[22:30:15 INF] [abc12345] Step 1: ✅ Request validation passed
[22:30:15 INF] [abc12345] Step 2: ✅ Generated request ID: def67890
[22:30:15 INF] [abc12345] Step 3: Processing quote request from John Doe
[22:30:16 INF] [def67890] Step 4: ✅ Connected to SMTP server successfully
[22:30:17 INF] [def67890] Step 5: ✅ SMTP authentication successful
[22:30:18 INF] [def67890] Step 6: ✅ Email sent successfully
```
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
