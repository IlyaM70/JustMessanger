import { BrowserRouter, Routes, Route } from "react-router-dom";
import Layout from "./components/Layout";
import Home from "./pages/Home";
import Chat from "./pages/Chat";
import Regin from "./pages/Regin";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="register" element={<Regin isRegister={true} />} />
          <Route path="login" element={<Regin isRegister={false} />} />
          <Route path="chat" element={<Chat/>} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
