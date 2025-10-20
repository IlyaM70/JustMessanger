import { jwtDecode } from 'jwt-decode';
import React, { useEffect, useState } from 'react';

interface TokenPayload {
    uid: string;
    email: string;
    expiration: number;
}


const Header: React.FC = () => {

  const [token, setToken] = useState(localStorage.getItem('token') || '');
  const [currentUserId, setCurrentUserId] = useState('');

  useEffect(() => {
    const handleStorageChange = () => {
      setToken(localStorage.getItem('token') || '');
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  useEffect(() => {
    if (token) {
      try {
        const decoded = jwtDecode<TokenPayload>(token);
        setCurrentUserId(decoded.uid);
      } catch {
        setCurrentUserId('');
      }
    } else {
      setCurrentUserId('');
    }
  }, [token]);

  const LogOut = () => {
    localStorage.removeItem('token');
    setToken('');
    window.location.href = '/login';
  };
  return (
    <header className="top-nav">
      <div className="nav-inner container d-flex align-items-center">
        <a className="brand" href="/">
          <div className="logo">JM</div>
          <div>
            <div style={{fontWeight:700}}>Just Messenger</div>
            <small className="small-muted">Lightweight chat for hackers & humans</small>
          </div>
        </a>

        <nav className="nav-links ms-auto d-none d-md-flex">
          {currentUserId && 
            <>
            <a href="/" className="small-muted">Home</a>
            </>
          }
          {!currentUserId && 
          <>
          <a href="/register" className="small-muted">Register</a>
          <a href="/login" className="small-muted">Login</a>
          </>}

          {currentUserId &&
          <div>
            <button onClick={LogOut} className="btn btn-pill btn-outline-secondary">Log out</button>
          </div>
          }
        </nav>

      </div>
    </header>
  );
};

export default Header;
