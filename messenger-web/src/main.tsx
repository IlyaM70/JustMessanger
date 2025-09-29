import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import Regin from './pages/Regin'
import Chat from './pages/Chat'

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        {/*<Regin isRegister={true} />*/}
        <Chat token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJLYXJsYSIsImp0aSI6IjY5ZjI3YzI5LWJlNTItNGE2Ny05ZDA0LTE4ZmRkYzU5ZDkzMSIsImVtYWlsIjoia2FybGFAdGVzdC5jb20iLCJ1aWQiOiIwZDZlMGZkYS03Nzg3LTQzZjktOTBjNC02MzdlMGI2NmFiM2YiLCJleHAiOjE3NTk3ODE2NTgsImlzcyI6Ikp1c3RNZXNzZW5nZXIiLCJhdWQiOiJKdXN0TWVzc2VuZ2VyVXNlcnMifQ.G-1lUHuAEuzonGw5QhkKDRUzS5KilVY1yak7VlWLLn8" recipientId="9f535ff9-a2d2-4b2d-88ed-356dc3fc22b2" />
  </StrictMode> 
)
