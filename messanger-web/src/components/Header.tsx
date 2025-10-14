import React from "react";

const Header: React.FC = () => {
  return (
    <header className="py-3 bg-white shadow-sm">
      <div className="container d-flex align-items-center">
        <div className="d-flex align-items-center gap-3">
          <div style={{width:44, height:44}} className="rounded d-flex align-items-center justify-content-center" >
            <div style={{background:"linear-gradient(135deg,#5568ff,#7b61ff)", width:44, height:44, borderRadius:12}} className="d-flex align-items-center justify-content-center text-white fw-bold">JM</div>
          </div>
          <div>
            <h5 className="mb-0">JustMessenger</h5>
            <small className="text-muted">Lightweight chat for hackers & humans</small>
          </div>
        </div>
        <div className="ms-auto d-flex gap-2 align-items-center">
          <button className="btn btn-outline-secondary btn-sm">Account</button>
        </div>
      </div>
    </header>
  );
};

export default Header;
