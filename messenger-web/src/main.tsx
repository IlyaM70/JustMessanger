import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import Regin from './pages/Regin'
import Chat from './pages/Chat'

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        {/*<Regin isRegister={true} />*/}
        <Chat />
  </StrictMode> 
)
