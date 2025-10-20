import { useEffect, useState } from "react";
import { jwtDecode } from 'jwt-decode';
import {useNavigate } from "react-router-dom";

type Contact = {
  userId: string;
  email: string;
  userName: string;
  lastMessage: string;
  lastMessageAt?: string;
};

interface TokenPayload {
    sub: string;
    email: string;
    uid: string;
    expiration: number;
}

const ContactList: React.FC = () => {

  const messageApiBase = 'https://just-messenger-messages.azurewebsites.net/';
  const messageApiUrl = `${messageApiBase}/api/Message`;
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [currentUserId, setCurrentUserId] = useState<string>('');
  const [currentUserName, setCurrentUserName] = useState<string>('JM');
  const token = localStorage.getItem('token') || '';
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);

useEffect(() => {
  if (!loading && (!currentUserId || currentUserId === '')) {
    navigate('/login');
  }
}, [loading, currentUserId, navigate]);




  //get user id from token
  useEffect(() => {
        try
        {
            const decoded = jwtDecode<TokenPayload>(token);            
            setCurrentUserId(decoded.uid);
            setCurrentUserName(decoded.sub);
        }
        catch (error)
        {
            console.error("Invalid token:", error);
        }
        finally {
        setLoading(false);
      }
  }, [token]);

  
  // Fetch contacts from an API
  useEffect(() => {
  if (!currentUserId) return; // Don't call API if userId isn't ready

  const fetchContacts = async () => {
    try {
      const response = await fetch(
        `${messageApiUrl}/contacts?userId=${currentUserId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error("Failed to fetch contacts");
      }

      const data = await response.json();
      setContacts(data.value);
    } catch (err) {
      console.error("Error fetching contacts:", err);
    }
  };

  fetchContacts();
}, [currentUserId, token]);

  const openChat = (recipientId: string, recipientName: string) => {
    navigate('/chat', { state: { recipientId, recipientName } });
  }

  const newChat = () => {
    navigate('/chat');
  }

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <div className="contact-card">
      <div className="head">
        <div style={{display:'flex', alignItems:'center', gap:12}}>
          <div className="avatar-sm">{currentUserName?.slice(0,2).toUpperCase()}</div>
          <div>
            <div style={{fontWeight:700}}>You</div>
            <small className="small-muted">Online</small>
          </div>
        </div>

        {/* for fuuuuture */}
        {/* <div className="search ms-auto">
          <input className="form-control input-rounded" placeholder="Search contacts" />
        </div> */}
      </div>

      <div className="p-2 d-flex justify-content-between align-items-center">
        <div className="small-muted">Messages</div>
        <button className="btn btn-sm btn-outline-primary btn-pill" onClick={newChat}>New chat</button>
      </div>

      <div className="contact-list-area">
        {contacts.length === 0 && (
          <div className="center-placeholder">No contacts yet</div>
        )}

        {contacts.map((c) => (        
          <div
            onClick={() => openChat(c.userId, c.userName)}
            key={c.userId}
            className={`contact-item ${/* maybe add active detection later */ ''}`}
            role="button"
          >
            <div className="avatar-sm">{c.userName?.slice(0,2).toUpperCase()}</div>
            <div className="contact-meta">
              <div className="contact-name">{c.userName}</div>
              <div className="contact-preview">{c.lastMessage}</div>
            </div>
            {c.lastMessageAt && (
              <div className="small-muted" style={{minWidth:60, textAlign:'right'}}>
                {new Date(c.lastMessageAt).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default ContactList;
