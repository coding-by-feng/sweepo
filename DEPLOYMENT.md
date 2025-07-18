# Sweepo Server - Deployment & Testing Guide

This guide provides step-by-step instructions for deploying and testing the Sweepo Server on a Linux system with Docker.

## 🚀 Quick Start Deployment

### Prerequisites
- Linux system with Docker and Docker Compose installed
- Git (to clone the repository)
- curl (for testing API endpoints)

### 1. Clone and Setup

```bash
# Clone the repository
git clone <your-repo-url>
cd sweepo-server

# Create environment file from template
cp .env.example .env

# Edit the environment file with your email configuration
nano .env
```

### 2. Configure Email Settings

Edit the `.env` file with your SMTP configuration:

```bash
# Email Configuration
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
FROM_EMAIL=your-email@gmail.com
RECIPIENT_EMAIL_1=admin@sweepo.com
RECIPIENT_EMAIL_2=quotes@sweepo.com
```

**For Gmail SMTP:**
1. Enable 2-factor authentication on your Gmail account
2. Generate an App Password: Google Account → Security → App passwords
3. Use the App Password (not your regular password) in `SMTP_PASSWORD`

### 3. Deploy with Docker Compose

```bash
# Build and start the server
docker-compose up -d

# Check if the container is running
docker-compose ps

# View logs
docker-compose logs -f sweepo-server
```

### 4. Alternative: Docker Build & Run

```bash
# Build the Docker image
docker build -t sweepo-server .

# Run the container
docker run -d \
  --name sweepo-server \
  -p 8080:8080 \
  -e EmailSettings__SmtpUsername=your-email@gmail.com \
  -e EmailSettings__SmtpPassword=your-app-password \
  -e EmailSettings__FromEmail=your-email@gmail.com \
  -e EmailSettings__RecipientEmails__0=admin@sweepo.com \
  -e EmailSettings__RecipientEmails__1=quotes@sweepo.com \
  sweepo-server

# Check container status
docker ps

# View logs
docker logs -f sweepo-server
```

## 🧪 Testing Guide

### 1. Health Check Test

```bash
# Test server health
curl -X GET http://localhost:8080/api/quote/health

# Expected response:
# {"status":"healthy","timestamp":"2025-01-15T10:30:00Z"}
```

### 2. Root Endpoint Test

```bash
# Test root endpoint
curl -X GET http://localhost:8080/

# Expected response:
# {"service":"Sweepo Server","version":"1.0.0","status":"running","timestamp":"..."}
```

### 3. Quote Submission Test

```bash
# Test quote submission with valid data
curl -X POST http://localhost:8080/api/quote \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1234567890",
    "service": "home-cleaning",
    "address": "123 Main St, City, State",
    "message": "I need weekly cleaning service"
  }'

# Expected success response:
# {"success":true,"message":"Quote request submitted successfully! We'll contact you within 24 hours.","requestId":"abc12345"}
```

### 4. Validation Test

```bash
# Test with invalid data (missing required fields)
curl -X POST http://localhost:8080/api/quote \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "email": "invalid-email",
    "phone": "",
    "service": ""
  }'

# Expected error response:
# {"success":false,"message":"Validation failed: ..."}
```

### 5. CORS Test

```bash
# Test CORS headers
curl -X OPTIONS http://localhost:8080/api/quote \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type" \
  -v

# Check for CORS headers in response
```

## 📊 Monitoring & Logs

### View Application Logs

```bash
# Docker Compose
docker-compose logs -f sweepo-server

# Docker Run
docker logs -f sweepo-server

# View logs from host (if volume mounted)
tail -f ./logs/sweepo-server-*.txt
```

### Monitor Container Health

```bash
# Check container health
docker inspect sweepo-server | grep -A 10 "Health"

# Manual health check
curl -f http://localhost:8080/api/quote/health || echo "Health check failed"
```

## 🔧 Troubleshooting

### Common Issues

1. **Container won't start**
   ```bash
   # Check logs for errors
   docker-compose logs sweepo-server
   
   # Check if port is already in use
   netstat -tulpn | grep 8080
   ```

2. **Email sending fails**
   ```bash
   # Check email configuration in logs
   docker-compose logs sweepo-server | grep -i email
   
   # Verify SMTP settings
   echo $SMTP_USERNAME
   ```

3. **API returns 500 errors**
   ```bash
   # Check application logs
   docker-compose logs sweepo-server | grep -i error
   
   # Restart the container
   docker-compose restart sweepo-server
   ```

### Debug Commands

```bash
# Enter container shell
docker exec -it sweepo-server /bin/bash

# Check container environment variables
docker exec sweepo-server env | grep EmailSettings

# Test SMTP connection from container
docker exec -it sweepo-server curl -v telnet://smtp.gmail.com:587
```

## 🌐 Production Deployment

### Reverse Proxy Setup (Nginx)

```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### SSL/HTTPS Setup

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx

# Get SSL certificate
sudo certbot --nginx -d your-domain.com

# Auto-renewal
sudo crontab -e
# Add: 0 12 * * * /usr/bin/certbot renew --quiet
```

### Environment Variables for Production

```bash
# Production environment variables
ASPNETCORE_ENVIRONMENT=Production
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__EnableSsl=true
# ... other settings
```

## 📝 Testing Checklist

- [ ] Server starts successfully
- [ ] Health check endpoint responds
- [ ] Root endpoint returns server info
- [ ] Quote submission with valid data succeeds
- [ ] Email is sent to configured recipients
- [ ] Validation errors are returned for invalid data
- [ ] CORS headers are present for allowed origins
- [ ] Logs are being written correctly
- [ ] Container restarts automatically on failure

## 🔗 Integration with Frontend

Update your sweepo-ui frontend to use the deployed server:

```javascript
// In your frontend configuration
const API_BASE_URL = 'http://your-server-ip:8080';

// Quote submission
const response = await fetch(`${API_BASE_URL}/api/quote`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify(formData),
});
```

## 📞 Support

If you encounter issues:
1. Check the logs first: `docker-compose logs sweepo-server`
2. Verify your email configuration
3. Test individual components (health check, SMTP connection)
4. Check firewall and network settings

---

**Server Endpoints Summary:**
- Health Check: `GET /api/quote/health`
- Submit Quote: `POST /api/quote`
- Server Info: `GET /`
- Swagger UI: `GET /swagger` (Development only)

**Default Port:** 8080 (configurable via environment variables)
