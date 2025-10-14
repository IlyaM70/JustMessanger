import React from 'react';

const Footer: React.FC = () => {
    return (
        <footer className="app-footer">
            <div>© {new Date().getFullYear()} Just Messenger — built with ❤️</div>
        </footer>
    );
};

export default Footer;
