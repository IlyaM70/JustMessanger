import React from "react";
import { Outlet } from "react-router-dom";
import Header from "./Header";
import Footer from "./Footer";

const Layout: React.FC = () => {
  return (
    <div className="app-shell">
      <Header />
      <div className="page-container">
        <main style={{minHeight: '60vh'}}>
          <Outlet /> {/* where page content loads */}
        </main>
      </div>
      <Footer />
    </div>
  );
};

export default Layout;
