import React from "react";
import { Outlet, Link } from "react-router-dom";
import Header from "./Header";
import Footer from "./Footer";

const Layout: React.FC = () => {

    const LogOut = () => {
        localStorage.removeItem("token");
        window.location.href = "/login";
    }


  return (
    <div className="flex flex-col min-h-screen">
      <Header />

      <nav className="bg-gray-100 p-4">
        <Link to="/" className="mr-4">Home</Link>        
        <Link to="/register">Register</Link>
        <Link to="/login">Login</Link>
        <Link to="/chat">Chat</Link>
        <button onClick={LogOut} className="btn-primary">Log out</button>
      </nav>

      <main className="flex-grow p-4">
        <Outlet /> {/* where page content loads */}
      </main>

      <Footer />
    </div>
  );
};

export default Layout;
