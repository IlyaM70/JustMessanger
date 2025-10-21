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
| **Web Client** | React Frontend | User-facing interface for login and chatting. Built with React + Vite


## 🧱 Note on Architecture
While **microservices** and **containerization** are often reserved for large-scale distributed systems, they were deliberately used here **as a learning exercise**.  
This project could easily be implemented as a monolithic application, but separating components into independent services provided valuable hands-on experience with:
- Containerization and orchestration using **Docker Compose**  
- Service communication and API design  
- Deployment pipelines for multiple containerized services  
- Configuration management across multiple environments  

This architectural choice reflects a **focus on mastering DevOps and distributed systems concepts**, rather than a strict requirement for the app’s scale.


---

## ⚙️ Current Functionality (as of MVP)
- ✅ **User authentication** (register/login) using the Auth microservice  
- ✅ **Basic message sending and retrieval** via the Messenger service  
- ✅ **REST API endpoints** for both Auth and Messaging  
- ✅ **React-based frontend** with navigation, login page, and message UI skeleton  
 

---

## 🧠 Tech Stack

| Component                 | Choice                | Rationale                                                    |
| ------------------------- | --------------------- | ------------------------------------------------------------ |
| **Language & Framework**  | ASP.NET Core          | Proven performance, developer familiarity, strong ecosystem  |
| **Real-Time Messaging**   | SignalR               | High-level abstraction for WebSocket communication           |
| **Primary Storage**       | Azure SQL             | Scalable, cost-effective, reliable for structured data       |
| **Testing Storage**       | SQLite                | Light, no set up for quick testing       |
| **CI/CD (Planned)**                 | GitHub Actions        | Simple automation, seamless GitHub integration               |
| **Unit Testing**          | xUnit & Moq           | Commonly used .NET testing frameworks                        |
| **Integration Testing (Planned)**   | WebApplicationFactory | Native ASP.NET Core support for integration tests            |
| **Frontend (Planned)**    | React + React Native  | Strong ecosystem and developer familiarity                   |


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
You can view the live demo at:
🔗 https://just-messenger-app.azurewebsites.net

APIs are deployed separately as container services in Azure.

### 📚 Next Steps (Planned)
- Improve tests
- Set up CI/CD
- Improve frontend UI/UX
- Mobile client
- Notification service
