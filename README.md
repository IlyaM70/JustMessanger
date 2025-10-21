# JustMessanger
# 📬 JustMessenger – MVP Documentation

## Overview
**JustMessenger** is a microservices-based messaging web application designed to demonstrate secure user authentication, message exchange, and modular architecture.  
This MVP (Minimum Viable Product) implements the essential backend and frontend components required for a functioning, containerized chat platform deployed via Docker and Azure.

---

## 🧩 Architecture

The system is composed of **three services**, orchestrated using **Docker Compose**:

| Service | Type | Description |
|----------|------|-------------|
| **Auth Service** | ASP.NET Core Web API | Handles user registration, login, and JWT-based authentication. Connects to Azure SQL Database.
| **Messenger Service** | ASP.NET Core Web API | Manages user messages (sending, retrieving). Connects to a separate Azure SQL Database.
| **Web Client** | React Frontend | User-facing interface for login and chatting. Built with React + Vite and deployed in Docker.

Each service is published as a **container image** on GitHub Container Registry (`ghcr.io/ilyam70/...`) and connected through the internal Docker network.

---

## ⚙️ Current Functionality (as of MVP)
- ✅ **User authentication** (register/login) using the Auth microservice  
- ✅ **Basic message sending and retrieval** via the Messenger service  
- ✅ **REST API endpoints** for both Auth and Messaging  
- ✅ **React-based frontend** with navigation, login page, and message UI skeleton  
- ✅ **Azure SQL Database integration** for persistent user and message data  
- ✅ **Dockerized deployment** for all three components  

---

## 🧠 Tech Stack

### Backend
- C#, .NET 8 (ASP.NET Core Web API)
- Entity Framework Core (Code First Migrations)
- Azure SQL Database
- JWT Authentication

### Frontend
- React + Vite
- React Router

### DevOps
- Docker & Docker Compose
- GitHub Container Registry (GHCR)
- Azure Web App (for deployment)

---

## 🚀 Running Locally

### Prerequisites
- Docker & Docker Compose installed  
- Valid connection strings to your Azure SQL Databases (or local SQL instances)

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/IlyaM70/JustMessanger.git
   cd JustMessanger
   
2. Update database connection strings in docker-compose.yml if needed.

3. Build and start the services:

```bash
docker compose up -d
```
4. Access the services:

  Auth API: http://localhost:5027/swagger  
  Messenger API: http://localhost:5091/swagger  
  Web Client: http://localhost:5173

### 🌐 Deployed Version
You can view the live frontend at:
🔗 https://just-messenger-app.azurewebsites.net

APIs are deployed separately as container services in Azure.

### 📚 Next Steps (Planned)
Improve tests
Set up CI/CD
Improve frontend UI/UX
Mobile client
Notification service
