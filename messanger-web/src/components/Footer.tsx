import React from "react";

const Footer: React.FC = () => (
  <footer className="py-3 text-center text-muted" style={{fontSize:13}}>
    © {new Date().getFullYear()} JustMessenger — built with ❤️.
  </footer>
);

export default Footer;
