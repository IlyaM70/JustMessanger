import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import Regin from './pages/Regin'

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <Regin />
  </StrictMode>
)
